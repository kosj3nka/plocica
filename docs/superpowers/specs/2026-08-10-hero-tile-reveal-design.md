# Hero tile-reveal hover effect

Replaces the current circular "spotlight" hover mask on the hero background
(`.hero-tiles`) with a grid-quantized reveal that lights up individual tiles,
splashes unevenly, leaves a fading trail, and desaturates with distance from
the cursor.

## Why

The current effect (`components.css` `.hero-tiles::after` + `radial-gradient`
mask) is a single smooth circle that fades the full-color photo in/out. It
reads as a generic "spotlight," not as revealing the individual ceramic tiles
in the photo. [[feedback_hero_tilesHero_simplicity]] still applies: the photo
(`tilesHero.jpg`) is never re-sliced or reassembled — only the reveal mask
changes from a smooth gradient to a grid-quantized, animated one.

## Grid

The photo is a real photograph of a **27×6 grid of tiles** (27 columns
across the width, 6 rows down the height — matches the photo's actual grout
lines, not an arbitrary overlay). At native size (2560×1640) each cell is
~94.8px × ~273.3px.

- `cellWidth = renderedImageWidth / 27`, where `renderedImageWidth =
  naturalWidth * (heroHeight / naturalHeight)` (mirrors the existing
  `background-size: auto 100%` scaling).
- `cellHeight = heroHeight / 6`.
- Column index for any x position: `floor(x / cellWidth)` — no modulo needed;
  since the underlying image already repeats seamlessly (`repeat-x`,
  grout continues), the canvas draw loop tiles the same source columns
  (`sourceCol = col % 27`) across however many columns fit the hero's
  current width.

## Architecture

- `.hero-tiles::before` (dimmed/desaturated full photo) is unchanged — it's
  always the visible base layer.
- `.hero-tiles::after` (the old spotlight mask) is removed.
- A `<canvas>` element is added as a real child of `.hero-tiles`, absolutely
  positioned to fill it, sitting above `::before`.
- `hero-tiles.js` loads `tilesHero.jpg` into an off-DOM `Image()` once, sizes
  the canvas to the hero's current box (accounting for `devicePixelRatio`),
  and runs a `requestAnimationFrame` loop that, per frame:
  1. Decays every active cell's heat: `heat *= 0.90` (tuned for a ~0.8–1.2s
     fade — see Trail below), dropping cells below a small epsilon from the
     active set.
  2. On pointer move, stamps heat into nearby cells (see Splash below).
  3. Clears the canvas and redraws: for each active cell, `drawImage(img,
     sourceRect, destRect)` with `globalAlpha = heat`, where `sourceRect`
     maps `(sourceCol, row)` back to the original image's pixel rect and
     `destRect` is the cell's on-screen rect.
  4. Stops the rAF loop when no cells are active and the pointer isn't
     currently over the hero, to avoid a perpetual idle loop.

This is a direct-draw compositing approach (not a CSS `mask-image` fed by a
canvas) — better cross-browser consistency and simpler alpha-per-cell math.

## Splash shape (unevenness)

On pointer move, find the cell under the cursor. For every cell within a
5×5 window centered on it:

- `dist` = Euclidean distance in cell units from the cursor's cell to the
  candidate cell (0 to ~3.5).
- Each cell gets a small fixed per-cell jitter (seeded from its column/row
  index via a cheap hash, stable across frames — not re-randomized every
  move, so the splash shape doesn't shimmer) added to `dist` before falloff,
  e.g. `jitteredDist = dist + jitter(col, row) * 0.9` where jitter is in
  [-1, 1].
- `targetHeat = clamp(1 - jitteredDist / 2.5, 0, 1)`.
- New heat is the max of the cell's current heat and `targetHeat` (so the
  trail from Trail below isn't stomped by a lower falloff value as the
  cursor moves past).

The jitter is what breaks the splash from a clean circle/diamond into an
uneven, organic-looking cluster — some cells 2 rings out occasionally light
up brighter than a cell 1 ring out.

## Trail

Decay factor `0.90`/frame at 60fps gives roughly a 0.8–1.2s fade from full
heat to invisible, matching the "medium" lingering trail: recently-visited
tiles stay lit as a short streak behind the cursor rather than snapping off
instantly. `pointerleave` does **not** zero the heat map — it just stops new
stamping, so the existing trail decays out naturally.

## Saturation falloff

Handled implicitly by the compositing model: cells at `heat = 1` show the
full-color photo un-obscured; cells at partial heat blend toward the dimmed
`::before` layer showing through beneath the canvas, which is already
desaturated (`grayscale(45%) brightness(1.05)`) — so distance from the
cursor naturally reads as "less saturated," with no separate saturation
logic needed.

## Touch

Same event wiring as today (`pointerdown`/`pointermove` stamp, `pointerup`
on touch does not force-clear — same reasoning as Trail above: let it decay
rather than snapping off). No separate touch-specific splash/trail tuning.

## Reduced motion

Under `prefers-reduced-motion: reduce`, skip the rAF decay loop entirely:
stamp cells to full heat immediately under the cursor (no jitter growth
over time, single instant splash) and clear them immediately on
`pointerleave`/`pointercancel`, mirroring how the current CSS disables the
`::after` transition for reduced motion.

## Resize

On hero resize (ResizeObserver on `.hero-tiles`), recompute `cellWidth` /
`cellHeight` and the canvas's backing size; clear the active heat map (a
resize invalidates in-flight cell coordinates).

## Out of scope

- No changes to the dimmed base layer's filter values.
- No changes to `hero.js` (unrelated hero image swap script) or other hero
  markup beyond adding the `<canvas>` child.
