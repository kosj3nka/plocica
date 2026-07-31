# Foundation + Nav — motion system, spacing scale, header polish

Date: 2026-07-31

## Goal

First of four sub-projects toward an "agency-tier" visual/motion upgrade of the public
site (Foundation+Nav → Homepage → Catalog pages → Content pages — each its own spec).
This sub-project:

1. Establishes shared tokens every later sub-project will build on: motion easing/
   duration, a spacing scale, and a nested-hairline-frame utility.
2. Elevates the header/nav's craft: hamburger→X morph, blurred/staggered mobile menu,
   magnetic hover, scroll-compact behavior.
3. Fixes reported spacing problems ("everything seems too big and unorganized, spaces
   between elements are either too small or too big") by normalizing every layout
   gap/padding/margin in the shared and page CSS onto the new scale, site-wide — ahead
   of the later per-page sub-projects, so they inherit consistent rhythm from the start.
4. Corrects the hero's oversized type scale (shipped just before this spec, in the same
   session) so the H1 doesn't wrap to four lines.

## Why "elevate craft," not "adopt agency tropes wholesale"

`docs/02-DESIGN.md` documents a deliberate identity — "tehnički crtež koji se ispunjava
glazurom" (technical drawing filled with glaze): paper-white/ink palette, hairline
borders, minimal/zero radius, one glaze accent per section, tabular numbering — and
explicitly rejects "AI-default" warm-cream+serif and, by extension, generic SaaS/glass
tropes. So this sub-project translates agency-tier *technique* (spring motion, nested
framing, magnetic hover, macro whitespace) into that existing visual language instead
of importing Tailwind/React-archetype surface styling (OLED glass, floating pill nav,
drop shadows). Confirmed with the project owner before starting:

- Keep the full-width "title block" header (not a floating pill) — elevate its
  interactions instead of changing its structure.
- No shadow-based button lift — use an on-brand "drafting-pen offset outline" instead.
- No icon-in-circle on the primary CTA — text-only, motion does the elevating.
- Backdrop-blur is acceptable on the mobile menu overlay specifically (fixed, opaque
  content behind it, satisfies the performance guardrail) since it's a UI mechanic, not
  a palette/material change.
- Admin panel (`admin.css`) is out of scope for the whole four-sub-project arc — internal
  tool, not the client-facing surface this upgrade targets.

## Architecture

```
Plocica/wwwroot/css/tokens.css       — motion tokens, spacing scale (fixed + fluid),
                                        --fs-hero correction, .section padding → fluid-xl
Plocica/wwwroot/css/layout.css       — header/nav spacing snapped to scale, hamburger
                                        icon styles, mobile menu blur + staggered reveal,
                                        scroll-compact header state
Plocica/wwwroot/css/components.css   — spacing snapped to scale across every section;
                                        .hero-copy/.hero type-scale fix; .btn/.btn-cta
                                        offset-outline hover physics; new .hairline-frame
                                        utility (defined, not yet applied anywhere)
Plocica/wwwroot/css/onama.css        — spacing snapped to scale
Plocica/Pages/Shared/_Layout.cshtml  — nav-toggle markup → icon spans + aria-label;
                                        1px sentinel element for the scroll-compact
                                        IntersectionObserver
Plocica/wwwroot/js/nav.js            — aria-label swap on hamburger toggle;
                                        IntersectionObserver wiring for scroll-compact
                                        (mobile menu open/close logic unchanged —
                                        .is-open already toggles correctly; staggered
                                        reveal is pure CSS keyed off that class)
```

No new dependencies, no build step — same vanilla CSS/JS constraint as the rest of the
site (`docs/00-PROJECT.md`: "Bez JS frameworka").

## Components

### Motion tokens (`tokens.css`)

```css
--ease-spring: cubic-bezier(0.32, 0.72, 0, 1);
--dur-fast:  200ms;
--dur-base:  400ms;
--dur-slow:  700ms;
```

All new/touched transitions use these. The existing global `prefers-reduced-motion`
block (`tokens.css`, forces `0.001ms` durations) already covers reduced-motion for
anything using these tokens — no per-component override needed.

### Spacing scale (`tokens.css`)

Fixed steps:

```css
--space-1:  0.25rem;  --space-2:  0.5rem;   --space-3: 0.75rem;
--space-4:  1rem;     --space-5:  1.25rem;  --space-6: 1.5rem;
--space-7:  2rem;     --space-8:  2.5rem;   --space-9: 3rem;
--space-10: 4rem;     --space-11: 5rem;     --space-12: 6rem;
--space-13: 8rem;
```

Fluid tiers (collapsing ~6 near-duplicate one-off `clamp()` values found across the
CSS into 4 canonical ones):

```css
--space-fluid-sm: clamp(1.5rem, 4vw, 2.5rem);
--space-fluid-md: clamp(2rem, 5vw, 4rem);
--space-fluid-lg: clamp(2.5rem, 6vw, 5rem);
--space-fluid-xl: clamp(3.5rem, 9vw, 8rem);  /* .section rhythm, was clamp(3rem,8vw,7rem) */
```

**Explicitly excluded from the scale** (stay as-is): `em`-based padding/margin that's
tied to a component's own local font-size rather than page rhythm — button internal
padding (`.btn`, `.nav-toggle`), heading margins (e.g. `.collection-card h3`), chip/swatch
padding (`.palette-swatch`, `.color-chip`), and micro-label margins (`.spec-row
.spec-label`). Forcing these to rem tokens would break their intentional scaling with
local type size. `--gutter` (viewport-edge container margin) also stays separate — it
governs edge margin, not inter-element rhythm.

**Snapping rule** used throughout: each existing value moves to the nearest scale step
(fixed) or nearest fluid tier by max-value (fluid `clamp()`); ties broken toward the
tighter option to counteract the "too much air" side of the complaint. Full mapping:

| File | Selector | Old | New |
|---|---|---|---|
| layout.css | `.site-header .container` gap | `2rem` | `var(--space-7)` |
| layout.css | `.nav-primary` gap | `clamp(1.25rem,3vw,2.5rem)` | `var(--space-fluid-sm)` |
| layout.css | `.nav-dropdown` padding | `0.5rem 0` | `var(--space-2) 0` |
| layout.css | `.nav-dropdown a` padding | `0.6em 1.25em` | `0.6rem var(--space-5)` |
| layout.css | `.nav-dropdown-sep` margin | `0.5rem 1.25em` | `var(--space-2) var(--space-5)` |
| layout.css | mobile `.nav-primary` padding | `1rem var(--gutter) 2rem` | `var(--space-4) var(--gutter) var(--space-7)` |
| layout.css | `.nav-cta` margin-top | `0.75rem` | `var(--space-3)` (already matched, now tokenized) |
| layout.css | `.site-footer` padding | `3rem 0` | `var(--space-9) 0` |
| layout.css | `.site-footer` margin-top | `4rem` | `var(--space-10)` |
| layout.css | `.site-footer .container` gap | `1rem` | `var(--space-4)` |
| components.css | `.hero` padding-block | `clamp(2.5rem,6vw,5rem)` | `var(--space-fluid-lg)` (already matched) |
| components.css | `.hero-copy` padding | `clamp(1.5rem,4vw,3rem)` | `var(--space-fluid-sm)` |
| components.css | `.hero-copy .eyebrow` margin-bottom | `1.25rem` | `var(--space-5)` |
| components.css | `.hero-copy h1` margin-bottom | `1.5rem` | `var(--space-6)` |
| components.css | `.hero-actions` gap | `1.5rem` | `var(--space-6)` |
| components.css | `.hero-actions` margin-top | `2rem` | `var(--space-7)` |
| components.css | `.manifesto` gap | `clamp(2rem,5vw,4rem)` | `var(--space-fluid-md)` |
| components.css | `.manifesto-list` margin / gap | `1.5rem 0 0` / `2rem` | `var(--space-6) 0 0` / `var(--space-7)` |
| components.css | `.manifesto-list li` padding-top | `0.6rem` | `var(--space-2)` |
| components.css | `.craft-copy` padding | `clamp(1.5rem,4vw,3rem) var(--gutter)` | `var(--space-fluid-sm) var(--gutter)` |
| components.css | `.collections-grid` margin-top | `clamp(2rem,4vw,3rem)` | `var(--space-fluid-sm)` |
| components.css | `.collection-card .card-body` padding | `1.5rem` | `var(--space-6)` |
| components.css | `.collection-card .card-link` padding-top / margin-top | `0.85rem` / `1rem` | `var(--space-3)` / `var(--space-4)` |
| components.css | `.palette-teaser` gap | `clamp(2rem,5vw,5rem)` | `var(--space-fluid-lg)` (max matches exactly) |
| components.css | `.people-teaser` gap | `clamp(2rem,5vw,4rem)` | `var(--space-fluid-md)` (already matched) |
| components.css | `.section-invert .cta-row` gap | `1.5rem` | `var(--space-6)` |
| components.css | `.process-list li` gap / padding | `1.5rem` / `1.5rem 0` | `var(--space-6)` / `var(--space-6) 0` |
| components.css | `.faq-list details` padding | `1.15rem 0` | `var(--space-5) 0` |
| components.css | `.faq-list p` margin | `0.85rem 0 0` | `var(--space-3) 0 0` |
| components.css | `.collection-intro` gap / margin-bottom | `clamp(2rem,5vw,4rem)` / `clamp(2rem,4vw,3rem)` | `var(--space-fluid-md)` / `var(--space-fluid-sm)` |
| components.css | `.spec-row` gap / padding | `1.5rem` / `1.25rem 0` | `var(--space-6)` / `var(--space-5) 0` (already matched) |
| components.css | `.color-grid` margin-top | `1.5rem` | `var(--space-6)` |
| components.css | `.shape-grid` margin-top | `1.5rem` | `var(--space-6)` |
| components.css | `.shape-tile figcaption` padding | `0.85rem 1rem` | `var(--space-3) var(--space-4)` |
| components.css | `.coming-soon-note` margin-top / padding-top | `1.5rem` / `1rem` | `var(--space-6)` / `var(--space-4)` |
| components.css | `.project` padding-block | `clamp(2.5rem,6vw,5rem)` | `var(--space-fluid-lg)` (already matched) |
| components.css | `.project-text` margin-top | `1.25rem` | `var(--space-5)` |
| components.css | `.project-gallery` / `.project-placeholder` margin-top | `2rem` | `var(--space-7)` |
| onama.css | `.about-hero` gap | `clamp(1.5rem,4vw,2.5rem)` | `var(--space-fluid-sm)` (already matched) |
| onama.css | `.about-grid` gap | `clamp(2rem,5vw,4rem)` | `var(--space-fluid-md)` (already matched) |
| onama.css | `.role-list` margin-top | `1.5rem` | `var(--space-6)` |
| onama.css | `.role-list li` padding | `1.1rem 0` | `var(--space-4) 0` |
| onama.css | `.media-row` gap | `0 1.75rem` | `0 var(--space-6)` |
| onama.css | `.media-row` margin-top | `1.25rem` | `var(--space-5)` |

Entries marked "already matched" get their literal value replaced with the `var()`
token for maintainability even though the number doesn't change.

### Hero type-scale correction (`tokens.css` / `components.css`)

```css
--fs-hero: clamp(2.75rem, 6vw, 5.5rem);  /* was clamp(3.5rem, 8vw, 7rem) */
```
`.hero-copy` max-width: `42rem` → `46rem`. Together these stop the H1 wrapping to four
lines at desktop widths (verified via the same headless-Chrome screenshot check used
for the hero background work earlier this session).

### Nested-hairline frame utility (`components.css`, defined only)

```css
.hairline-frame {
  padding: var(--space-2);
  border: 1px solid var(--line);
  background: var(--paper-2);
}
.hairline-frame > .hairline-frame-inner {
  border: 1px solid var(--line);
  background: var(--paper);
}
```
A concentric double-rectangle — outer hairline on `--paper-2`, small gap, inner hairline
on `--paper` — standing in for the skill's glass "double-bezel," reading as a technical
drawing's mat/mount rather than a card. Zero radius (`02-DESIGN.md`: minimal/0 radius on
technical elements). **Not applied to anything in this sub-project** — the hero already
has its own single-hairline treatment from the prior session and isn't revisited here;
this utility is built now so the Catalog sub-project's cards can use it directly.

### Button hover physics (`components.css`, applies to `.btn`/`.btn-cta` site-wide)

Replaces shadow-lift with a "drafting-pen offset outline": on hover/focus the button
lifts `translate(-2px,-2px)` while a duplicate `1px solid var(--ink)` outline (a `::after`
starting hidden at `translate(4px,4px)`) slides to `translate(0,0)` — reading as a
technical pen's double-stroke. `:active` adds `scale(0.98)` for a physical-press feel.
Both transitions use `--ease-spring` / `--dur-fast`.

(Correction — the outline does not slide as shipped. Verified empirically during Task 7's
review and the final whole-branch review via headless-Chrome rendering: animating the
`::after`'s own transform composes with — and cancels against — the parent `.btn`'s hover
transform, so no slide is ever perceived. The shipped implementation keeps the `::after` at
a fixed `translate(6px, 6px)` and animates **only its opacity** 0 → 1. It also draws only
its right and bottom borders, not a full rectangle, and carries no `z-index: -1` /
`isolation: isolate`: a 4-sided negative-z-index outline painted its top/left edges *over*
the button's own fill per CSS2.1 painting order. Hover lift is `translate(-3px, -3px)`,
`:active` is `translate(-3px, -3px) scale(0.98)`. The static-offset, opacity-only version
is the intended final behavior — do not re-add a slide.)

### Nav / header (`layout.css`, `_Layout.cshtml`, `nav.js`)

- **Hamburger → X morph:** `.nav-toggle`'s "Izbornik" text label is replaced with a
  24×24 two-line hairline icon (`::before`/`::after` or nested spans, 1px lines) that
  rotates/translates into an `×` when `.nav-primary.is-open`. `aria-label` toggles
  between "Otvori izbornik" / "Zatvori izbornik" in `nav.js` alongside the existing
  `aria-expanded` toggle — `aria-expanded` logic is unchanged.
- **Mobile menu blur:** `.nav-primary` (mobile, fixed full-screen panel) background
  changes from opaque `var(--paper)` to `rgba(242,240,234,0.82)` with
  `backdrop-filter: blur(20px) saturate(140%)`. Fixed/full-screen element, so this is
  safe under the performance guardrail (blur never applied to scrolling content).
- **Staggered link reveal:** mobile `.nav-primary > li` get `transition-delay` staggered
  by `:nth-child` (e.g. 40ms increments), animating `opacity`/`translateY` from the
  existing closed state to open — pure CSS, no JS change needed since `.is-open` is
  already the trigger class.
- **Magnetic hover:** `.nav-link`, `.nav-dropdown-toggle` add a subtle
  `translateY(-1px)` on hover/focus-visible alongside the existing underline, using
  `--ease-spring`/`--dur-fast` instead of the current `ease`.
- **Scroll-compact header:** a 1px sentinel `<div>` placed just after `<body>`'s opening
  content (before `.site-header` in DOM, or absolutely positioned at top) is watched by
  an `IntersectionObserver` in `nav.js`; when it scrolls out of view, `.site-header`
  gets `.is-compact` (min-height 84px → `var(--space-10)` = 64px, transition via
  `--ease-spring`/`--dur-base`). Chosen over a scroll listener per the performance
  guardrail (no `window.addEventListener('scroll')`).

## Data flow

None — pure CSS/JS, no server-side or data changes. No new dependencies, no build step.

## Error handling / edge cases

- **No `IntersectionObserver` support** (extremely unlikely in target browsers): header
  simply never compacts — acceptable degraded state, guarded with a feature check
  before instantiating the observer.
- **`prefers-reduced-motion: reduce`:** all new transitions inherit the existing global
  override (durations forced to `0.001ms`) — hamburger morph, button offset-outline,
  header compact, and staggered reveal all become instant state changes instead of
  animated ones, consistent with how the hero's motion already degrades.
- **Backdrop-filter unsupported** (very old browsers): mobile menu falls back to the
  semi-transparent background color without blur — still readable, no JS/CSS error.
- **Touch devices:** magnetic hover / offset-outline hover states simply don't trigger
  (no `:hover` on touch) — buttons and links remain fully usable via tap, focus-visible
  styles unaffected.
- **Keyboard navigation:** hamburger toggle keeps its existing `aria-expanded`/
  `aria-controls` wiring; only the visible label mechanism changes (icon + `aria-label`
  instead of visible text), so screen-reader behavior is preserved, not regressed.

## Testing / verification

- Manual, via `dotnet run` + the headless-Chrome screenshot flow already used for the
  hero: desktop hover states (buttons, nav links), hero H1 no longer wrapping to four
  lines, spacing rhythm visually consistent across sections.
- Manual mobile-viewport check: hamburger morph, blur + staggered menu reveal, dropdown
  behavior unchanged.
- Manual scroll check: header compacts past the sentinel, expands back near the top.
- Manual keyboard tab-through: focus-visible rings intact on all interactive nav/button
  elements.
- Manual `prefers-reduced-motion: reduce` check: state changes still function (menu
  opens/closes, header compacts) with no animation.
- No automated test suite exists in this project (consistent with the prior hero spec);
  none added here.
