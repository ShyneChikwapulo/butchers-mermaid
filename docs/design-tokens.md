# Design Tokens — The Butcher's Mermaid

Decisions locked before any CSS is written. Changes here ripple through
`wwwroot/app.css` and every component.

## Colour palette

Derived from the logo [CONFIRM exact hex values — extract from the vector
file using a tool like Figma, Sketch, or an online SVG colour picker].

Proposed direction: dark, warm, steakhouse-adjacent. Butchery + sea.

| Token              | Value     | Use                                      |
|--------------------|-----------|------------------------------------------|
| `--color-bg`       | `#0F0D0C` | Page background (near-black, warm)       |
| `--color-surface`  | `#1A1614` | Cards, panels                            |
| `--color-border`   | `#2E2725` | Dividers, card borders                   |
| `--color-text`     | `#F5F0E8` | Primary text (warm off-white)            |
| `--color-muted`    | `#A89B8C` | Secondary text                           |
| `--color-accent`   | `#8B1E1E` | Primary CTA — deep oxblood red           |
| `--color-accent-2` | `#C9A961` | Secondary accent — brass/gold            |
| `--color-success`  | `#4A7C59` | Form success                             |
| `--color-error`    | `#B23A3A` | Form errors                              |

**Flag for Phase 0 review:** pull the real hex values from the logo vector
and replace the placeholders above. The palette above is a defensible
starting point, not final.

## Typography

| Token               | Value                               | Use                  |
|---------------------|-------------------------------------|----------------------|
| `--font-display`    | `'Cormorant Garamond', serif`       | Headings, hero       |
| `--font-body`       | `'Inter', system-ui, sans-serif`    | Body, UI             |

Both are free, both on Google Fonts, both fast. If the logo uses a
distinctive typeface, substitute.

## Type scale (rem)

| Token          | Value  |
|----------------|--------|
| `--fs-xs`      | 0.75   |
| `--fs-sm`      | 0.875  |
| `--fs-base`    | 1      |
| `--fs-lg`      | 1.25   |
| `--fs-xl`      | 1.5    |
| `--fs-2xl`     | 2      |
| `--fs-3xl`     | 3      |
| `--fs-4xl`     | 4      |
| `--fs-hero`    | 5      |

## Spacing scale (rem)

| Token        | Value |
|--------------|-------|
| `--sp-1`     | 0.25  |
| `--sp-2`     | 0.5   |
| `--sp-3`     | 0.75  |
| `--sp-4`     | 1     |
| `--sp-6`     | 1.5   |
| `--sp-8`     | 2     |
| `--sp-12`    | 3     |
| `--sp-16`    | 4     |
| `--sp-24`    | 6     |

## Layout

| Token                | Value    |
|----------------------|----------|
| `--container-max`    | 1200px   |
| `--container-narrow` | 760px    |
| `--radius`           | 2px      |
| `--radius-lg`        | 6px      |

## Rationale

- **Dark background, light text:** steakhouse and upscale butchery aesthetic.
  Also differentiates from the generic white Squarespace look that most local
  restaurants use.
- **Oxblood red as primary accent:** reads as "meat" without being literal
  blood-red. Brass/gold secondary reinforces "premium" without going luxury-
  cliché.
- **Serif display, sans body:** classic restaurant pattern. The serif adds
  weight and ceremony; the sans keeps menus and forms legible.
- **Tight radii:** 2px reads as precision and craft. Round pills feel
  consumer-app, not butcher-shop.