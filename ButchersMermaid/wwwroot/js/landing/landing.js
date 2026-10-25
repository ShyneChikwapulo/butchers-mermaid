/* The Butcher's Mermaid: cinematic landing page.
 *
 * ES module started from Components/Shared/CinematicLanding.razor.
 *   init(root, id)  starts the experience inside the component's root element (returns immediately)
 *   dispose(id)     tears everything down (called when the Blazor component is disposed)
 *
 * Modes (root.dataset.mode): boot -> loading -> cinematic, or static (reduced motion / no WebGL / load failure).
 * three.js r128 and GSAP are vendored in /lib and loaded on demand, so other pages never pay for them.
 * The full opening plays once per browser session; later visits get a short entrance. ?intro forces the opening.
 * Add ?tbmdebug to the URL to log the lifecycle to the console.
 */

const MODELS = '/models/';
const SEEN_KEY = 'tbm-intro-seen';
const seenIntro = () => { try { return sessionStorage.getItem(SEEN_KEY) === '1'; } catch (e) { return false; } };
const markSeen = () => { try { sessionStorage.setItem(SEEN_KEY, '1'); } catch (e) { } };

/* ---------- on-demand libraries (shared between instances) ---------- */
const addScript = src => new Promise((ok, no) => {
  const s = document.createElement('script');
  s.src = src; s.async = false; s.onload = ok; s.onerror = () => no(new Error('failed to load ' + src));
  document.head.appendChild(s);
});
let libsPromise = null;
function loadLibs() {
  if (window.THREE && window.THREE.GLTFLoader && window.gsap && window.ScrollTrigger) return Promise.resolve();
  if (libsPromise) return libsPromise;
  libsPromise = Promise.all([
    (async () => {
      if (!window.THREE) await addScript('/lib/three/three.min.js');
      if (!window.THREE.GLTFLoader) await addScript('/lib/three/GLTFLoader.js');
    })(),
    (async () => {
      if (!window.gsap) await addScript('/lib/gsap/gsap.min.js');
      if (!window.ScrollTrigger) await addScript('/lib/gsap/ScrollTrigger.min.js');
    })()
  ]).catch(e => { libsPromise = null; throw e; });
  return libsPromise;
}

/* ---------- instance registry (the component identifies itself by id, not by DOM node) ---------- */
const instances = new Map();
export function init(root, id) {
  // only one landing can exist: clean up anything left over from a previous visit first
  for (const [k, old] of [...instances]) { instances.delete(k); old.dispose(); }
  const inst = create(root, id, () => instances.delete(id));
  instances.set(id, inst);
  inst.start().catch(e => { console.warn('[landing] using the static layout:', e); inst.fallback(); });
}
export function dispose(id) {
  const inst = instances.get(id);
  if (inst) { instances.delete(id); inst.dispose(); }
}

function create(root, id, forget) {
  const q = s => root.querySelector(s);
  const el = { stage: q('[data-stage]'), gl: q('[data-gl]'), brand: q('[data-brand]'), hint: q('[data-hint]'), skip: q('[data-skip]'), story: q('[data-story]') };
  const html = document.documentElement, R = Math.random;
  const RM = matchMedia('(prefers-reduced-motion:reduce)').matches;
  const QS = new URLSearchParams(location.search);
  const DBG = QS.has('tbmdebug');
  const dbg = (...a) => { if (DBG) console.log('[landing ' + id.slice(0, 6) + ']', ...a); };
  const mobile = Math.min(innerWidth, innerHeight) < 700;

  let dead = false, D = 1, Dd = 0, done = false, inView = true;
  let T = null, st = null, io = null, renderer = null, scene = null, pmrem = null;
  const offs = [], live = [], ticks = new Set();
  const on = (t, e, f, o) => { t.addEventListener(e, f, o); offs.push(() => t.removeEventListener(e, f, o)); };
  const track = a => (live.push(a), a);
  const setMode = m => { dbg('mode ->', m); root.dataset.mode = m; };
  const navOn = () => html.classList.add('tbm-nav-on');
  // scroll lock: only released by the instance that took it
  let locked = false;
  const lock = v => {
    if (v) { locked = true; html.style.overflow = 'hidden'; }
    else if (locked) { locked = false; html.style.overflow = ''; }
  };
  // the site sets scroll-behavior:smooth, which fights scrubbed scroll animation and slows scrollTo
  let sbPrev = null;
  const noSmooth = () => { if (sbPrev === null) { sbPrev = html.style.scrollBehavior; html.style.scrollBehavior = 'auto'; } };
  const restoreSmooth = () => { if (sbPrev !== null) { html.style.scrollBehavior = sbPrev; sbPrev = null; } };
  const tick = f => { gsap.ticker.add(f); ticks.add(f); return f; };
  const untick = f => { gsap.ticker.remove(f); ticks.delete(f); };

  function hasGL() {
    try {
      const c = document.createElement('canvas'), g = c.getContext('webgl2') || c.getContext('webgl');
      if (!g) return false;
      const x = g.getExtension('WEBGL_lose_context'); if (x) x.loseContext(); // do not leak the probe context
      return true;
    } catch (e) { return false; }
  }

  /* ---------- static layout: reduced motion, no WebGL, or anything failing to load ---------- */
  function teardownGL() {
    if (key && key.shadow && key.shadow.map) key.shadow.map.dispose();
    if (scene) {
      if (scene.environment) scene.environment.dispose();
      scene.traverse(o => {
        if (o.geometry) o.geometry.dispose();
        const ms = o.material ? (Array.isArray(o.material) ? o.material : [o.material]) : [];
        ms.forEach(m => { ['map', 'normalMap', 'roughnessMap', 'metalnessMap'].forEach(k => m[k] && m[k].dispose()); m.dispose(); });
      });
    }
    if (pmrem) pmrem.dispose();
    if (renderer) { renderer.dispose(); renderer.forceContextLoss(); }
    scene = pmrem = renderer = null;
  }
  function fallback() {
    if (dead) return;
    dbg('fallback to static layout');
    untickAll(); teardownGL();
    setMode('static'); lock(false); restoreSmooth(); navOn();
    if (el.skip) el.skip.hidden = true;
  }
  function untickAll() { if (window.gsap) ticks.forEach(f => gsap.ticker.remove(f)); ticks.clear(); }

  /* ---------- asset fetching (starts immediately, decoded once the libraries are ready) ---------- */
  const DEF = {
    steak: { glb: 'steak', tex: ['base', 'mr', 'normal'], small: true },
    block: { glb: 'block', tex: ['base', 'normal'], small: true },
    blockcut: { glb: 'block-cut', tex: [], small: false },
    cleaver: { glb: 'cleaver', tex: ['base', 'mr', 'normal'], small: false }
  };
  const loadImg = u => new Promise((ok, no) => { const i = new Image(); i.onload = () => ok(i); i.onerror = () => no(new Error('image ' + u)); i.src = u; });
  async function fetchModel(k) {
    const d = DEF[k], base = k === 'blockcut' ? 'block' : k, sfx = d.small && mobile ? '-1k' : '';
    const [buf, ...im] = await Promise.all([
      fetch(MODELS + d.glb + '.glb').then(r => { if (!r.ok) throw new Error('model ' + k); return r.arrayBuffer(); }),
      ...d.tex.map(t => loadImg(MODELS + base + '-' + t + sfx + '.jpg'))
    ]);
    return { buf, im: Object.fromEntries(d.tex.map((t, i) => [t, im[i]])) };
  }
  function parseModel(f) {
    return new Promise((ok, no) => new THREE.GLTFLoader().parse(f.buf, MODELS, g => {
      const T = {};
      for (const n in f.im) {
        const t = new THREE.Texture(f.im[n]);
        t.flipY = false; t.encoding = n === 'base' ? THREE.sRGBEncoding : THREE.LinearEncoding;
        t.wrapS = t.wrapT = THREE.RepeatWrapping; t.anisotropy = mobile ? 4 : 8; t.needsUpdate = true; T[n] = t;
      }
      ok({ scene: g.scene, T });
    }, no));
  }

  /* ---------- scene state ---------- */
  let camera, rig, key, rimL, flash, steak, cleaver, blockG, blockI = null, blockC = null, cone, dust, chips, FP;
  const FY = .819, CX = .45, CY = .2454, CZ = -.07;
  const S = { z: 7, y: 2.5, orbit: 0, sx: 0, sy: 0 };
  const JU = { uSq: { value: 0 }, uW: { value: 0 } }, JE = [];
  let mx = 0, my = 0, tx = 0, ty = 0, M = null;
  /* adaptive resolution: starts at the normal cap (or the level remembered for this device), drops straight to the level that
     should hit ~22 ms when animated frames are slow, never below 1.25, and only climbs back to just under a level that failed */
  const dprCapFor = () => Math.min(devicePixelRatio, innerWidth < 800 ? 1.5 : 1.75);
  const DPR_FLOOR = 1.25, DPR_KEY = 'tbm-dpr';
  let dprCap = 1, dprNow = 0, dprMax = 0, lastAnim = 0, gSum = 0, gN = 0, gGood = 0, gHold = 0;
  const dprLoad = () => { try { const o = JSON.parse(localStorage.getItem(DPR_KEY) || 'null'); return o && Math.abs(o.cap - dprCapFor()) < .01 ? o.dpr : 0; } catch (e) { return 0; } };
  const dprSave = () => { try { localStorage.setItem(DPR_KEY, JSON.stringify({ dpr: dprNow, cap: dprCap })); } catch (e) { } };

  function layout() {
    const w = innerWidth, h = innerHeight, asp = w / h, mob = asp < .9;
    return { mob, z: Math.max(7, 2.7 / (asp * .3153)), hx: mob ? 0 : 1.9 * Math.min(1, asp / 1.5), hy: mob ? -.7 : 0, side: mob ? 0 : 1.9 * Math.min(1, asp / 1.5), ys: mob ? .9 : -.3, cy: mob ? .8 : 0 };
  }
  function resize() {
    if (!renderer) return;
    dprCap = dprCapFor();
    if (!dprNow) { dprNow = Math.min(dprCap, Math.max(DPR_FLOOR, dprLoad() || dprCap)); dprMax = dprCap; }
    dprNow = Math.min(dprNow, dprCap); dprMax = Math.min(dprMax, dprCap);
    renderer.setPixelRatio(dprNow);
    renderer.setSize(innerWidth, innerHeight, false);
    camera.aspect = innerWidth / innerHeight; camera.updateProjectionMatrix(); D = 1;
  }
  function kick(A) { JE.push({ t: performance.now() / 1000, A }); D = 1; }

  function applyMat(Mo, o, jig) {
    Mo.scene.traverse(n => {
      if (!n.isMesh) return;
      const old = n.material; let m;
      if (old && old.name === 'cutwall') m = new THREE.MeshStandardMaterial({ color: old.color, roughness: .88, metalness: 0 });
      else {
        const P0 = Object.assign({ map: Mo.T.base, normalMap: Mo.T.normal || null, roughness: 1, metalness: 0 }, o);
        if (jig) Object.assign(P0, { clearcoat: .5, clearcoatRoughness: .12, emissive: new THREE.Color(0x2a0508), emissiveIntensity: .12 });
        m = jig ? new THREE.MeshPhysicalMaterial(P0) : new THREE.MeshStandardMaterial(P0);
        if (jig) m.normalScale.set(1.25, 1.25);
        if (Mo.T.mr) { m.roughnessMap = m.metalnessMap = Mo.T.mr; m.metalness = 1; }
        if (jig) jiggleMat(m);
      }
      n.material = m; n.castShadow = n.receiveShadow = true; n.frustumCulled = false;
    });
  }
  /* soft-body style wobble: damped spring displacement driven by uSq / uW */
  function jiggleMat(m) {
    m.onBeforeCompile = sh => {
      sh.uniforms.uSq = JU.uSq; sh.uniforms.uW = JU.uW;
      sh.vertexShader = 'uniform float uSq;uniform float uW;\n' + sh.vertexShader.replace('#include <begin_vertex>',
        'vec3 transformed=vec3(position);float hb=clamp((position.y+0.282)/0.564,0.,1.);transformed.y=-0.282+(position.y+0.282)*(1.0-uSq*(0.55+0.45*hb));transformed.xz*=1.0+uSq*0.45*(1.0-hb*0.4);transformed.y+=uW*sin(position.x*3.2+position.z*2.1)*hb*0.5;');
    };
  }
  function jigUpdate() {
    if (!JE.length) { if (JU.uSq.value || JU.uW.value) { JU.uSq.value = JU.uW.value = 0; D = 1; } return; }
    const now = performance.now() / 1000; let c = 0, w = 0;
    for (let i = JE.length - 1; i >= 0; i--) {
      const e = JE[i], t = now - e.t;
      if (t > 1.8) { JE.splice(i, 1); continue; }
      const env = Math.exp(-3.2 * t); c += e.A * env * Math.cos(6.2832 * 5.2 * t); w += e.A * env * Math.sin(6.2832 * 7.5 * t);
    }
    JU.uSq.value = c; JU.uW.value = w; D = 1;
  }

  /* ---------- build the three.js scene ---------- */
  function buildScene(A) {
    const dn = mobile ? 90 : 170;
    renderer = new THREE.WebGLRenderer({ canvas: el.gl, antialias: !mobile, powerPreference: 'high-performance' });
    renderer.shadowMap.enabled = true; renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    renderer.toneMapping = THREE.ACESFilmicToneMapping; renderer.toneMappingExposure = 1.15; renderer.outputEncoding = THREE.sRGBEncoding;
    renderer.setClearColor(0x050404);
    scene = new THREE.Scene(); camera = new THREE.PerspectiveCamera(35, 1, .1, 60);
    rig = new THREE.Group(); scene.add(rig);
    FP = new THREE.Vector3(-.1763, FY, .1842);

    // studio reflections for the steel
    {
      const c = document.createElement('canvas'); c.width = 512; c.height = 256; const g = c.getContext('2d');
      g.fillStyle = '#060505'; g.fillRect(0, 0, 512, 256); g.fillStyle = '#ffd9a8'; g.fillRect(60, 30, 120, 60);
      g.fillStyle = '#9fb4c8'; g.fillRect(330, 60, 14, 150); g.fillStyle = '#553'; g.fillRect(0, 150, 512, 106);
      const t = new THREE.CanvasTexture(c); t.mapping = THREE.EquirectangularReflectionMapping;
      pmrem = new THREE.PMREMGenerator(renderer); scene.environment = pmrem.fromEquirectangular(t).texture; t.dispose();
    }

    // block (intact + cut), levelled so its top sits at y=.55
    const dy = .55 - .791;
    applyMat(A.block, { roughness: .85 }); A.block.scene.position.y = dy;
    blockG = new THREE.Group(); blockG.add(A.block.scene); blockI = A.block.scene;
    if (A.blockcut) {
      A.blockcut.T = A.block.T; applyMat(A.blockcut, { roughness: .85 });
      A.blockcut.scene.position.y = dy; A.blockcut.scene.visible = false; blockG.add(A.blockcut.scene); blockC = A.blockcut.scene;
    }
    rig.add(blockG);
    const plate = new THREE.Mesh(new THREE.CircleGeometry(18, 96), new THREE.MeshStandardMaterial({ color: 0x0a0908, roughness: .94, metalness: 0, envMapIntensity: .03 }));
    plate.rotation.x = -Math.PI / 2; plate.position.y = dy - .001; plate.receiveShadow = true; rig.add(plate);

    // steak
    steak = new THREE.Group(); applyMat(A.steak, {}, true); steak.add(A.steak.scene);
    steak.position.copy(FP); steak.rotation.y = .43; steak.visible = false; rig.add(steak);

    // cleaver (posed in Blender: phi / scale / offset)
    const c = { flip: false, phi: 0.07568188807504805, s: 1.9585318467875408, t: [0.6904140190343675, -0.017163432818577525] };
    cleaver = new THREE.Group(); applyMat(A.cleaver, {});
    A.cleaver.scene.scale.setScalar(c.s); A.cleaver.scene.rotation.order = 'ZYX'; A.cleaver.scene.rotation.set(0, c.flip ? Math.PI : 0, c.phi);
    A.cleaver.scene.position.set(c.t[0], c.t[1], 0); cleaver.add(A.cleaver.scene);
    cleaver.position.set(CX, CY, -.75); cleaver.rotation.z = CZ; cleaver.rotation.y = .2; cleaver.visible = false; rig.add(cleaver);

    // lights
    key = new THREE.SpotLight(0xffc890, 0, 22, .38, .9, 1.2); key.position.set(-4, 7, 4); key.castShadow = true;
    const sm = mobile ? 1024 : 2048; key.shadow.mapSize.set(sm, sm); key.shadow.bias = -.0003; key.shadow.normalBias = .02; key.shadow.camera.near = 4; key.shadow.camera.far = 22;
    scene.add(key, key.target);
    rimL = new THREE.DirectionalLight(0x8fa4bd, .5); rimL.position.set(3, 3, -5); scene.add(rimL);
    scene.add(new THREE.HemisphereLight(0x332211, 0x050404, .18));
    flash = new THREE.PointLight(0xffd9a0, 0, 7); flash.position.set(.5, 1.4, 1.2); rig.add(flash);

    // visible spotlight beam
    const CONE_L = 12;
    cone = new THREE.Mesh((() => { const g = new THREE.ConeGeometry(Math.tan(.38) * CONE_L, CONE_L, 64, 1, true); g.translate(0, -CONE_L / 2, 0); g.rotateX(-Math.PI / 2); return g; })(),
      new THREE.ShaderMaterial({
        transparent: true, depthWrite: false, blending: THREE.AdditiveBlending, side: THREE.DoubleSide, uniforms: { uI: { value: 0 } },
        vertexShader: 'varying vec3 vN;varying vec3 vV;varying float vA;void main(){vN=normalize(normalMatrix*normal);vec4 mv=modelViewMatrix*vec4(position,1.);vV=normalize(-mv.xyz);vA=clamp(position.z/12.,0.,1.);gl_Position=projectionMatrix*mv;}',
        fragmentShader: 'uniform float uI;varying vec3 vN;varying vec3 vV;varying float vA;void main(){float f=pow(abs(dot(normalize(vN),normalize(vV))),1.6);float a=uI*f*pow(1.-vA,1.4)*smoothstep(0.,.08,vA);gl_FragColor=vec4(vec3(1.,.78,.5)*a,a);}'
      }));
    cone.frustumCulled = false; scene.add(cone);

    // wood chips thrown by the cleaver
    chips = new THREE.InstancedMesh(new THREE.BoxGeometry(.06, .012, .014), new THREE.MeshStandardMaterial({ color: 0xa9763f, roughness: .8 }), 44);
    chips.count = 0; chips.frustumCulled = false; chips.castShadow = true; rig.add(chips);

    // dust motes that only glow inside the spotlight cone
    const dpos = new Float32Array(dn * 3), dvel = new Float32Array(dn * 3), dph = new Float32Array(dn);
    for (let i = 0; i < dn; i++) {
      const a = R() * 6.283, r = Math.sqrt(R()) * 3.7;
      dpos[i * 3] = Math.cos(a) * r; dpos[i * 3 + 1] = -.3 + R() * 6.9; dpos[i * 3 + 2] = Math.sin(a) * r;
      dvel[i * 3] = (R() - .5) * .03; dvel[i * 3 + 1] = .012 + R() * .04; dvel[i * 3 + 2] = (R() - .5) * .03; dph[i] = R();
    }
    const dg = new THREE.BufferGeometry(); dg.setAttribute('position', new THREE.BufferAttribute(dpos, 3)); dg.setAttribute('ph', new THREE.BufferAttribute(dph, 1));
    dust = new THREE.Points(dg, new THREE.ShaderMaterial({
      transparent: true, depthWrite: false, blending: THREE.AdditiveBlending,
      uniforms: { uL: { value: new THREE.Vector3() }, uA: { value: new THREE.Vector3(0, -1, 0) }, uT: { value: Math.tan(.38) }, uRig: { value: new THREE.Vector3() }, uTime: { value: 0 }, uI: { value: 0 } },
      vertexShader: 'uniform vec3 uL;uniform vec3 uA;uniform float uT;uniform vec3 uRig;uniform float uTime;attribute float ph;varying float vA;void main(){vec3 wp=position+uRig;vec3 rel=wp-uL;float t=dot(rel,uA);vec3 pp=rel-uA*t;float k=length(pp)/(max(t,.001)*uT);float cone=(t>.5)?smoothstep(1.,.55,k):0.;float tw=.55+.45*sin(uTime*1.3+ph*40.);vA=cone*tw;vec4 mv=viewMatrix*vec4(wp,1.);gl_PointSize=clamp(30./(-mv.z)*(.6+.9*fract(ph*7.3)),1.2,5.5);gl_Position=projectionMatrix*mv;}',
      fragmentShader: 'uniform float uI;varying float vA;void main(){float d=length(gl_PointCoord-.5);float a=smoothstep(.5,.0,d)*vA*uI;gl_FragColor=vec4(vec3(1.,.82,.58)*a,a);}'
    }));
    dust.frustumCulled = false; scene.add(dust);
    let dtick = 0; const dclock = performance.now();
    dustUpdate = () => {
      const u = dust.material.uniforms, I = .9 * Math.min(1, key.intensity / 5.9); u.uI.value = I;
      if (I < .02 || (dtick++ & 1)) return;
      const t = (performance.now() - dclock) / 1000, dt = .033;
      u.uTime.value = t; u.uL.value.copy(key.position); u.uA.value.copy(rig.position).sub(key.position).normalize(); u.uRig.value.copy(rig.position);
      for (let i = 0; i < dn; i++) {
        const k = i * 3;
        dpos[k] += dvel[k] * dt + Math.sin(t * .7 + dph[i] * 9) * .0014; dpos[k + 1] += dvel[k + 1] * dt; dpos[k + 2] += dvel[k + 2] * dt + Math.cos(t * .6 + dph[i] * 7) * .0014;
        if (dpos[k + 1] > 6.6) dpos[k + 1] = -.3;
      }
      dg.attributes.position.needsUpdate = true; Dd = 1;
    };
    resize();
  }
  let dustUpdate = () => {};

  /* ---------- per-frame render (on demand: only when something changed) ---------- */
  function frame() {
    if (dead || !renderer) return;
    if (!root.isConnected) { dbg('root left the DOM: cleaning up'); dispose(); return; } // Blazor never told us
    mx += (tx - mx) * .05; my += (ty - my) * .05;
    if (Math.abs(tx - mx) > .0004 || Math.abs(ty - my) > .0004) D = 1;
    jigUpdate();
    if (!inView || document.hidden) return;
    dustUpdate();
    if (!D && !Dd) return;
    const animated = !!D;
    renderer.shadowMap.autoUpdate = animated; D = 0; Dd = 0;
    const a = S.orbit + mx * .16;
    camera.position.set(Math.sin(a) * S.z + S.sx, S.y - my * .4 + S.sy, Math.cos(a) * S.z); camera.lookAt(0, .6, 0);
    key.target.position.copy(rig.position); cone.position.copy(key.position); cone.lookAt(key.target.position);
    cone.material.uniforms.uI.value = .10 * Math.min(1, key.intensity / 5.9);
    renderer.render(scene, camera);
    // resolution governor: judge only consecutive animated frames, ignore one-off hitches
    const now = performance.now();
    if (animated) {
      const dt = now - lastAnim; lastAnim = now;
      if (dt < 100) { gSum += dt; gN++; }
      if (gN >= 24) {
        const mean = gSum / gN; gSum = 0; gN = 0;
        if (gHold > 0) gHold--;
        else if (mean > 26 && dprNow > DPR_FLOOR) {
          const want = Math.round(dprNow * Math.sqrt(22 / mean) * 20) / 20;      // frame time scales with pixel count
          dprMax = Math.min(dprMax, dprNow - .1);
          dprNow = Math.max(DPR_FLOOR, Math.min(dprNow - .1, want)); gGood = 0; gHold = 4; applyDpr('down', mean);
        } else if (mean < 15 && dprNow < dprMax) {
          if (++gGood >= 3) { dprNow = Math.min(dprMax, +(dprNow + .15).toFixed(2)); gGood = 0; gHold = 4; applyDpr('up', mean); }
        } else gGood = 0;
      }
    } else lastAnim = 0;
  }
  function applyDpr(dir, mean) {
    dbg('resolution', dir, '->', dprNow, '(mean frame', mean.toFixed(1), 'ms)');
    renderer.setPixelRatio(dprNow); renderer.setSize(innerWidth, innerHeight, false); dprSave(); D = 1;
  }

  /* ---------- the opening: flickering light, thrown steak, cleaver strike, block slides aside ---------- */
  function splinters() {
    const CH = [], dum = new THREE.Object3D();
    for (let i = 0; i < 44; i++) {
      const a = R() * Math.PI * 2, sp = .5 + R() * 1.1;
      CH.push({ p: new THREE.Vector3(.3 + R() * .6, .5, -.74 - R() * .07), v: new THREE.Vector3(Math.cos(a) * sp, 2.0 + R() * 2.4, Math.sin(a) * sp), r: new THREE.Vector3(R() * 6 - 3, R() * 6 - 3, R() * 6 - 3), q: new THREE.Euler(R() * 6, R() * 6, R() * 6), s: .6 + R() * 1.1, rest: false });
    }
    chips.count = 44; const t0 = performance.now(); let last = t0;
    const f = () => {
      const now = performance.now(), dt = Math.min((now - last) / 1000, .05), age = (now - t0) / 1000; last = now;
      CH.forEach((c, i) => {
        if (!c.rest) {
          c.v.y -= 12 * dt; c.p.addScaledVector(c.v, dt); c.q.x += c.r.x * dt; c.q.y += c.r.y * dt; c.q.z += c.r.z * dt;
          const fl = Math.hypot(c.p.x, c.p.z) < 1.5 ? .555 : -.235; if (c.p.y < fl && c.v.y < 0) { c.p.y = fl; c.rest = true; }
        }
        const sc = c.s * (age > 1.6 ? Math.max(0, 1 - (age - 1.6) / .6) : 1);
        dum.position.copy(c.p); dum.rotation.copy(c.q); dum.scale.setScalar(sc); dum.updateMatrix(); chips.setMatrixAt(i, dum.matrix);
      });
      chips.instanceMatrix.needsUpdate = true; D = 1;
      if (age > 2.3) { chips.count = 0; untick(f); D = 1; }
    };
    tick(f);
  }
  function impact() {
    track(gsap.fromTo(S, { sx: 0 }, { duration: 1.5, onUpdate() { const k = Math.pow(1 - this.progress(), 2) * .11; S.sx = (R() - .5) * k; S.sy = (R() - .5) * k; D = 1; }, ease: 'none' }));
    track(gsap.fromTo(flash, { intensity: 7 }, { intensity: 0, duration: .6, ease: 'power2.out' }));
    track(gsap.fromTo(key, { intensity: 9.9 }, { intensity: 5.9, duration: .9, ease: 'power2.out' }));
    if (blockC) { blockI.visible = false; blockC.visible = true; }
    splinters(); kick(.03);
    const o = { v: 0 };
    track(gsap.to(o, {
      v: 1, duration: 1.8, ease: 'none', onUpdate() {
        const d = Math.pow(1 - o.v, 2.2), by = Math.sin(o.v * 80) * .008 * d;
        blockG.position.y = by; blockG.rotation.z = Math.sin(o.v * 58) * .004 * d; cleaver.position.y = CY + by; cleaver.rotation.z = CZ + Math.sin(o.v * 90) * .012 * d;
        steak.position.y = FY + by + Math.max(0, Math.sin(o.v * 26)) * .015 * d; D = 1;
      }
    }));
  }
  const DR = { t: 0 }, DH = { t: 0 };
  function dropFn() {
    const t = DR.t, e = t * t * (3 - 2 * t), x0 = -5.8, y0 = 1.9, z0 = 1.0;
    steak.position.set(x0 + (FP.x - x0) * t, y0 * (1 - t) + FP.y * t + 1.05 * 4 * t * (1 - t), z0 + (FP.z - z0) * t);
    steak.rotation.set((1 - e) * (-Math.PI * 2.2), .43 * e, (1 - e) * .4); D = 1;
  }
  function hopFn() {
    const t = DH.t; steak.position.set(FP.x, FP.y + .075 * Math.sin(Math.PI * t) * (1 - t) * (1 - t * .3), FP.z);
    steak.rotation.set(.025 * Math.sin(Math.PI * 2 * t) * (1 - t), .43, 0); D = 1;
  }
  function initIntro() {
    M = layout(); rig.position.set(0, M.hy, 0); S.z = M.z + 2.6; gsap.set(el.brand, { autoAlpha: 0, y: 24 });
    T.to(key, { intensity: 1.4, duration: .05, ease: 'none' }, .55).to(key, { intensity: 0, duration: .06, ease: 'none' }, .68)
      .to(key, { intensity: 3.6, duration: .05, ease: 'none' }, 1.0).to(key, { intensity: .5, duration: .09, ease: 'none' }, 1.12)
      .to(key, { intensity: 2.2, duration: .04, ease: 'none' }, 1.42).to(key, { intensity: 0, duration: .05, ease: 'none' }, 1.52)
      .to(key, { intensity: 5.9, duration: 1.5, ease: 'power2.out' }, 1.78).to(S, { z: M.z, y: 2.5, duration: 8, ease: 'power1.inOut' }, 0);
    T.call(() => { steak.visible = true; dropFn(); }, null, 2.7)
      .fromTo(DR, { t: 0 }, { t: 1, duration: 1.05, ease: 'none', immediateRender: false, onUpdate: dropFn }, 2.7)
      .call(() => kick(.05), null, 3.75)
      .fromTo(DH, { t: 0 }, { t: 1, duration: .55, ease: 'none', immediateRender: false, onUpdate: hopFn }, 3.75)
      .call(() => kick(.07), null, 4.15)
      .call(() => { steak.position.copy(FP); steak.rotation.set(0, .43, 0); }, null, 4.32);
    T.call(() => { cleaver.visible = true; }, null, 5.5)
      .fromTo(cleaver.position, { x: CX + 1.4, y: 1.4, z: -.75 }, { x: CX + 1.8, y: 2.1, duration: .9, ease: 'power2.out' }, 5.5)
      .fromTo(cleaver.rotation, { z: -.2 }, { z: -.75, duration: .9, ease: 'power2.out' }, 5.5)
      .to(cleaver.position, { x: CX, y: CY, duration: .17, ease: 'power4.in' }, 6.55)
      .to(cleaver.rotation, { z: CZ, duration: .17, ease: 'power4.in' }, 6.55)
      .call(impact, null, 6.72)
      .to(rig.position, { x: M.hx, y: M.hy, duration: 1.8, ease: 'power3.inOut' }, 8.2)
      .to(el.brand, { autoAlpha: 1, y: 0, duration: 1.8, ease: 'power2.out' }, 8.8);
  }
  /* returning visitors: the finished composition with a short light-up instead of the full opening */
  function returnEntrance() {
    steak.visible = true; steak.position.copy(FP); steak.rotation.set(0, .43, 0);
    cleaver.visible = true; cleaver.position.set(CX, CY, -.75); cleaver.rotation.z = CZ;
    if (blockC) { blockI.visible = false; blockC.visible = true; }
    key.intensity = 0;
    unlock(); build();
    gsap.set(el.brand, { autoAlpha: 0, y: 24 });
    track(gsap.to(key, { intensity: 5.9, duration: 1.6, ease: 'power2.out' }));
    track(gsap.to(el.brand, { autoAlpha: 1, y: 0, duration: 1.4, ease: 'power2.out', delay: .5 }));
    D = 1;
  }
  function unlock() {
    done = true; lock(false); navOn(); el.skip.hidden = true;
    track(gsap.to(el.hint, { opacity: 1, duration: 1 }));
  }
  function finish() { if (done) return; dbg('intro finished'); markSeen(); unlock(); build(); }

  /* ---------- scroll-controlled story: the block travels between the three panels, then leaves ---------- */
  function build() {
    if (dead) return;
    if (st) { st.scrollTrigger && st.scrollTrigger.kill(); st.kill(); }
    M = layout(); const L = M.side;
    gsap.set(el.brand, { autoAlpha: 1, y: 0 }); gsap.set(el.stage, { autoAlpha: 1 });
    rig.position.set(M.hx, M.hy, 0); rig.rotation.y = 0; Object.assign(S, { z: M.z, y: 2.5, orbit: 0 }); key.position.set(-4, 7, 4); rimL.intensity = .5;
    st = gsap.timeline({ defaults: { ease: 'power1.inOut' }, onUpdate: () => D = 1, scrollTrigger: { trigger: el.story, start: 'top top', end: 'bottom bottom', scrub: QS.has('scroll') ? true : 1.2 } });
    st.to(el.brand, { autoAlpha: 0, duration: .45, ease: 'none' }, 0).to(el.hint, { opacity: 0, duration: .12, ease: 'none' }, 0)
      .to(rig.position, { x: -L, y: M.ys, duration: 1 }, 0).to(rig.rotation, { y: 1.3, duration: 1 }, 0).to(S, { z: M.z + 1.4, y: 2.2, duration: 1 }, 0)
      .to(rig.position, { x: L, y: M.ys, duration: 1 }, 1).to(rig.rotation, { y: 2.8, duration: 1 }, 1).to(S, { z: M.z + .3, y: 2.9, orbit: -.18, duration: 1 }, 1)
      .to(key.position, { x: 3.5, y: 6, z: 4, duration: 1 }, 1)
      .to(rig.position, { x: 0, y: M.cy, duration: 1 }, 2).to(rig.rotation, { y: 4.5, duration: 1 }, 2).to(S, { z: M.z - .8, y: 2.2, orbit: .22, duration: 1 }, 2)
      .to(key.position, { x: -1, y: 7, z: 5.5, duration: 1 }, 2).to(rimL, { intensity: 1.5, duration: 1 }, 2)
      .to(rig.position, { y: -4.6, duration: 1, ease: 'power2.out' }, 3).to(rig.rotation, { y: 5.1, duration: 1, ease: 'power2.out' }, 3)
      .to(S, { orbit: .05, y: 2.6, duration: 1, ease: 'power2.out' }, 3).to(el.stage, { autoAlpha: 0, duration: .6, ease: 'none' }, 3.4);
  }

  /* compile every shader and upload every texture behind the loading bar, a little per frame, instead of mid-shot */
  async function warmUp() {
    const t0 = performance.now(), k0 = key.intensity;
    const parts = [blockI, blockC, steak, cleaver].filter(Boolean), vis = parts.map(p => p.visible), cc = chips.count;
    const nextFrame = () => new Promise(r => { const to = setTimeout(r, 120); requestAnimationFrame(() => { clearTimeout(to); r(); }); });
    const tex = new Set();
    scene.traverse(o => { if (o.isMesh && o.material) [].concat(o.material).forEach(m => ['map', 'normalMap', 'roughnessMap', 'metalnessMap'].forEach(k => m[k] && tex.add(m[k]))); });
    key.intensity = .01;
    try {
      for (const t of tex) { renderer.initTexture(t); await nextFrame(); if (dead) return; }      // one texture upload per frame
      for (const p of parts) {                                                                    // one object's shaders per frame
        parts.forEach(q => q.visible = (q === p));
        chips.count = p === steak ? 1 : 0;
        renderer.shadowMap.autoUpdate = true; renderer.render(scene, camera);
        await nextFrame(); if (dead) return;
      }
    } catch (e) { dbg('warm-up failed', e); }
    parts.forEach((p, i) => p.visible = vis[i]); chips.count = cc; key.intensity = k0; D = 1;
    dbg('warm-up took', Math.round(performance.now() - t0), 'ms (staged)');
  }

  /* ---------- go: everything is loaded ---------- */
  function go() {
    setMode('cinematic');
    gsap.registerPlugin(ScrollTrigger);
    // panel text reveal
    root.querySelectorAll('.tbm-txt').forEach(t => {
      gsap.set(t.children, { opacity: 0, y: 34 });
      track(gsap.to(t.children, { opacity: 1, y: 0, stagger: .12, duration: 1, ease: 'power3.out', scrollTrigger: { trigger: t.parentElement, start: 'top 55%', end: 'bottom 45%', toggleActions: 'play reverse play reverse' } }));
    });
    // the full opening plays once per browser session; ?intro, ?seek and ?scroll force it
    const returning = seenIntro() && !QS.has('intro') && !QS.has('seek') && !QS.has('scroll');
    if (!returning) {
      T = gsap.timeline({ paused: true, onUpdate: () => D = 1, onComplete: finish });
      initIntro();
      el.skip.hidden = false; el.skip.onclick = () => T.timeScale(25);
    }
    tick(frame);
    io = new IntersectionObserver(es => { inView = es[0].isIntersecting; D = 1; dbg('story in view:', inView); }, { threshold: 0 }); io.observe(el.story);
    if (matchMedia('(pointer:fine)').matches) on(window, 'pointermove', e => { tx = e.clientX / innerWidth - .5; ty = e.clientY / innerHeight - .5; D = 1; });
    let rt, lw = innerWidth, lh = innerHeight;
    on(window, 'resize', () => {
      clearTimeout(rt);
      rt = setTimeout(() => {
        // ignore the mobile address-bar resize (height-only change)
        if (innerWidth === lw && Math.abs(innerHeight - lh) < lh * .25) return;
        lw = innerWidth; lh = innerHeight; resize();
        if (done) build(); else M = layout();
      }, 200);
    });
    D = 1;
    if (returning) { dbg('returning visitor: short entrance'); returnEntrance(); return; }
    if (QS.has('seek')) { T.pause(); T.time(parseFloat(QS.get('seek')), false); D = 1; setTimeout(() => window.__tbmReady = 1, 500); return; }
    if (QS.has('scroll')) {
      T.pause(); T.time(T.duration(), false); D = 1;
      setTimeout(() => { const s = st.scrollTrigger; scrollTo(0, s.start + parseFloat(QS.get('scroll')) * (s.end - s.start)); D = 1; setTimeout(() => window.__tbmReady = 1, 900); }, 2500);
      return;
    }
    setTimeout(() => { if (!dead) { dbg('intro starts'); T.play(); } }, 400);
  }

  async function start() {
    dbg('start | reduced motion:', RM, '| webgl:', hasGL(), '| mobile:', mobile);
    if (RM || QS.has('static') || !hasGL()) { fallback(); return; }
    setMode('loading'); noSmooth(); lock(true); scrollTo({ top: 0, left: 0, behavior: 'instant' });
    // start downloading the models while the libraries load
    const pre = Object.fromEntries(Object.keys(DEF).map(k => [k, fetchModel(k)]));
    Object.values(pre).forEach(p => p.catch(() => {}));
    await loadLibs(); dbg('libraries ready'); if (dead) return;
    const need = ['steak', 'block', 'cleaver'];
    const A = {};
    await Promise.all(Object.keys(DEF).map(async k => {
      try { A[k] = await parseModel(await pre[k]); }
      catch (e) { if (need.includes(k)) throw e; }
    }));
    dbg('models ready');
    if (dead) return;
    if (!root.isConnected) { dbg('root left the DOM before the scene was built'); dispose(); return; }
    buildScene(A); await warmUp(); if (dead) return; go();
  }

  function dispose() {
    if (dead) return;
    dead = true; dbg('dispose');
    offs.forEach(f => f());
    if (window.gsap) {
      ticks.forEach(f => gsap.ticker.remove(f)); ticks.clear();
      live.forEach(a => { try { a.scrollTrigger && a.scrollTrigger.kill(); a.kill(); } catch (e) { } });
      if (T) T.kill();
      if (st) { st.scrollTrigger && st.scrollTrigger.kill(); st.kill(); }
    }
    if (io) io.disconnect();
    teardownGL(); lock(false); restoreSmooth();
    forget();
  }

  return { start, fallback, dispose };
}