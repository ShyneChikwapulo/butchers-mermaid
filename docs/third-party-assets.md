# Third-party assets

## 3D models (CC BY 4.0)

All models are used under the [Creative Commons Attribution 4.0 licence](https://creativecommons.org/licenses/by/4.0/),
sourced via Sketchfab and **modified** for this project (geometry simplified, textures resized,
re-graded or re-packed, and, for the block, sliced, levelled, recoloured and cut).

| File(s) in `wwwroot/models/` | Title | Author | Modifications |
|---|---|---|---|
| `steak.glb`, `steak-*.jpg` | "bone-in meat cut 3d model" | grodan | Decimated to about 70k triangles; textures regraded (lean, fat, moisture) and resized |
| `block.glb`, `block-cut.glb`, `block-*.jpg` | "Tree Stump 1" | Alexandre González Rivas | Sliced flat, levelled, recoloured; second version has the cleaver cut |
| `cleaver.glb`, `cleaver-*.jpg` | "Cleaver - Butcher's Best Friend" | 3dsofsan | Textures resized; repositioned for the scene |

Before launch, confirm each title, author and link against the original Sketchfab page.
The same credit is shown in the site footer on the landing page.

## JavaScript libraries (vendored in `wwwroot/lib/`)

| Library | Version | Licence |
|---|---|---|
| three.js (`lib/three/three.min.js`, `GLTFLoader.js`) | r128 | MIT |
| GSAP + ScrollTrigger (`lib/gsap/`) | 3.12.5 | GreenSock "Standard No Charge" licence (see https://gsap.com/standard-license) |