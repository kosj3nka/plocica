# Hero redesign — hexagon ShapeGrid (vanilla JS port)

Date: 2026-07-29

## Goal

Replace the homepage hero's photo-crossfade visual with an interactive hexagon grid
(ported from react-bits' `ShapeGrid`), recolored to the site's ceramic-glaze palette.
Keep the real process photography, but give it its own section instead of the hero slot.

## Why a port, not the React component

This project is ASP.NET Core Razor Pages with vanilla JS/CSS and zero npm/React/bundler
tooling anywhere in the repo — a deliberate choice recorded in `docs/02-DESIGN.md`
("Bez JS frameworka"). `react-bits`' `ShapeGrid-JS-CSS` component is still a React
component (JSX + hooks), so it cannot be installed as-is.

The actual component was fetched via `npx shadcn@latest add @react-bits/ShapeGrid-JS-CSS`
in a disposable scratch Vite+React project (not part of this repo) purely to read the
real source. Its logic turned out to be plain `<canvas>` 2D drawing wrapped in
`useRef`/`useEffect` — nothing React-specific about the rendering itself — so it ports
mechanically to a plain JS module with no behavior loss.

## Architecture

```
Pages/Index.cshtml          — hero markup updated; new "Proces izrade" section added
wwwroot/css/components.css  — .hero-grid panel styles; relocated .hero-frame styles
                               renamed for the new process section
wwwroot/js/hero-grid.js     — new: ported ShapeGrid canvas module (vanilla JS)
wwwroot/js/hero.js          — updated: now drives the relocated process-photo
                               crossfade instead of the hero
Pages/Index.cshtml.cs       — HeroShape property and its query removed (dead code
                               once .plate-caption is gone)
```

No build step, no new dependencies. `hero-grid.js` is a self-contained IIFE/module
loaded via `<script src="~/js/hero-grid.js">`, same pattern as the existing `hero.js`
and `craft-video.js`.

## Components

### `.hero-grid` (new, replaces `.plate-frame` in the hero)

- Square panel (`aspect-ratio: 1`), hairline border in `var(--line)` — reuses the
  site's existing "technical drawing frame" signature element instead of inventing
  a new container style.
- Contains a single `<canvas>` filling the panel.
- `.plate-caption` (shape name + price, tied to `Model.HeroShape`) is deleted along
  with the DB lookup that fed it — it doesn't apply to an abstract visual.

### `hero-grid.js` — ported canvas module

Config (fixed, matches the requested snippet):
```js
{
  shape: "hexagon",
  direction: "down",
  speed: 0,            // effective speed floors at 0.1 in the original algorithm —
                        // kept as-is; produces slow, near-imperceptible drift
  squareSize: 45,
  hoverTrailAmount: 6,
  borderColor: "var(--line)",     // #C9C4B8, resolved via getComputedStyle
  hoverFillColor: "var(--glz-sun)" // #E3B23C
}
```

Ported 1:1 from the fetched source: hex-grid drawing, wrap-around offset animation,
mouse-hover cell targeting, trailing-cell opacity lerp. React lifecycle (`useRef`/
`useEffect`/cleanup on unmount) becomes a plain `init(canvas)` function; there's no
unmount case since the canvas lives for the page's lifetime.

Two behavioral branches, decided once at init:

1. **Hover-capable devices** (`matchMedia('(hover: hover)').matches` and no
   `prefers-reduced-motion`): full animation loop — ambient drift + mouse-driven
   hover fill + trail, as in the original.
2. **Touch devices** (`matchMedia('(hover: none)').matches`) **or**
   `prefers-reduced-motion: reduce`: no `requestAnimationFrame` loop, no pointer
   listeners. Draw a single static frame. For touch specifically, pre-seed 4–6
   hexagon cells at fixed opacity in `--glz-sun` (scattered, not a regular pattern)
   so it reads as a partially-glazed sample instead of a dead grid. Redraw once on
   resize/orientation change only.

Reduced-motion note: hover-fill response on a hover-capable-but-reduced-motion
device (rare, e.g. some desktop settings) is treated as direct interaction feedback,
not autoplay, and stays enabled — only the continuous ambient drift is suppressed.

### New section: "Izbliza / Iz naše ponude" (relocated photo crossfade)

Placed immediately after the hero, before the existing "Sudjelovanje u stvaranju"
manifesto section. Reuses the exact crossfade mechanics already in `hero.js` and
`.plate-frame`/`.hero-frame`/`.is-active` CSS (interval-driven, `prefers-reduced-motion`
shows last frame only) — no class renames needed, `.hero-grid` doesn't collide with
anything. Correction from the initial draft of this spec: the six images already
wired up in `Index.cshtml` (`arabeskHERO`, `curveHERO`, `curve1HERO`,
`kombinacijaOblikaHERO`, `lineaHERO`, `moduleHERO`) are a rotating showcase of
different finished tile designs, not a literal sketch→glaze→done process sequence
as `docs/03-HERO.md` originally envisioned — so the section is framed as a product
showcase ("Iz naše ponude"), not a "process" narrative.

## Data flow

No new data. The config object above is a fixed constant inside `hero-grid.js` itself
(not DB-driven, no admin panel involvement, no data-attribute indirection needed since
there is exactly one instance on the page). The process-photo section uses the same
static image list currently hardcoded in `Index.cshtml`.

## Error handling / edge cases

- No canvas support (extremely unlikely in target browsers): hairline panel still
  renders as an empty bordered square — acceptable, no JS error since `getContext`
  is guarded.
- Resize: canvas backing store resized on `window resize`, recomputing grid columns/
  rows from the container's current size — no hardcoded 1080px anywhere (the
  original snippet's fixed `1080x1080px` wrapper is a demo-only value; production
  uses a responsive square via CSS).
- `prefers-reduced-motion` and `(hover: none)` are both re-checked only at page
  load (matches the existing site pattern in `hero.js`), not live-tracked mid-session.

## Testing / verification

- Manual: run the site locally (`dotnet run`), verify hero hexagon grid renders,
  hover produces a trailing fill in the sun-glaze color, resizing the window
  reflows the grid without layout shift.
- Manual: throttle to a mobile viewport / use browser device toolbar with touch
  emulation, confirm the static pre-filled variant appears with no console errors
  from missing pointer events.
- Manual: enable "reduce motion" at the OS level, confirm no ambient drift but
  (on desktop) hover-fill still responds.
- Manual: confirm the new "Proces izrade" section shows the same crossfade
  previously in the hero, unchanged in behavior.
- No automated test suite exists in this project currently; this stays consistent
  with that (no tests added).
