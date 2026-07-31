# Foundation + Nav — Motion System, Spacing Scale, Header Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Establish shared motion/spacing tokens, normalize inconsistent spacing across every public-page CSS file, fix the hero's oversized type scale, and elevate the header/nav's craft (hamburger→X morph, blurred/staggered mobile menu, magnetic hover, scroll-compact) — all within the site's existing hairline/paper/ink "technical drawing" identity.

**Architecture:** Pure CSS + vanilla JS, no build step. `tokens.css` gains the new custom properties (motion easing/duration, a fixed + fluid spacing scale) that every other file then consumes via `var()`. `layout.css`, `components.css`, and `onama.css` get their existing hardcoded spacing values snapped onto that scale. Two small, targeted feature additions land in `layout.css`/`_Layout.cshtml`/`nav.js` for the header (icon morph, blur, stagger, magnetic hover, scroll-compact via `IntersectionObserver`).

**Tech Stack:** ASP.NET Core Razor Pages, vanilla JS (no framework, no build step), plain CSS with custom properties.

## Global Constraints

- No npm, React, or any bundler — vanilla CSS/JS only, referenced directly via Razor `<link>`/`<script>` tags (`docs/00-PROJECT.md`: "Bez JS frameworka").
- Every new spacing/motion value must come from a `tokens.css` custom property (`--space-*`, `--space-fluid-*`, `--ease-spring`, `--dur-*`) — never a new hardcoded literal — **except** `em`-based padding/margin tied to a component's own local font-size (button internal padding, heading margins, chip/swatch padding, micro-label margins) and `--gutter` (viewport-edge margin), which are deliberately excluded from the scale (see spec).
- This project has **no automated test suite**. Verification steps are manual: run `dotnet run` from `Plocica/`, open the page in a browser, and check the described behavior. Do not treat the absence of automated tests as a defect to fix.
- Preserve the existing accessibility quality floor: keyboard focus visibility, `prefers-reduced-motion` support (the global override in `tokens.css` already forces all transition/animation durations to `0.001ms`, so every new transition inherits reduced-motion support for free — no per-component override needed), alt text, `aria-expanded`/`aria-controls` wiring on the nav toggle — don't regress any of it.
- `Plocica/wwwroot/css/admin.css` is out of scope — do not touch it.
- Keep the header's full-width sticky structure — do not switch to a floating/detached pill nav.
- No shadow-based (`box-shadow`/`filter: drop-shadow`) button hover lift — use the drafting-pen offset-outline pattern defined in Task 7.
- The primary CTA (`.btn-cta`) stays text-only — no nested icon-in-circle.
- `backdrop-filter` may only be applied to the fixed, full-screen mobile menu overlay (Task 9) — never to scrolling content, per the performance guardrail.
- The hero's `.hero-grid` canvas (`Plocica/wwwroot/js/hero-grid.js`, from the prior session) and its hover interaction must keep working unchanged — only the spacing/type-scale values around it change (Task 2).

---

### Task 1: Foundation tokens — motion, spacing scale, section rhythm, hero type fix

**Files:**
- Modify: `Plocica/wwwroot/css/tokens.css:4-32` (add spacing scale + motion tokens, change `--fs-hero`)
- Modify: `Plocica/wwwroot/css/tokens.css:103-105` (`.section` padding)

**Interfaces:**
- Produces: `--space-1` through `--space-13` (fixed rem steps), `--space-fluid-sm`/`-md`/`-lg`/`-xl` (fluid `clamp()` tiers), `--ease-spring`, `--dur-fast`/`-base`/`-slow`. Every later task consumes these by name — do not rename them.
- Consumes: nothing new (existing `--gutter`, `--paper`, `--ink`, etc. are untouched).

- [ ] **Step 1: Add the spacing scale and motion tokens, and correct `--fs-hero`**

In `Plocica/wwwroot/css/tokens.css`, replace the `:root { ... }` block (lines 4–32) with:

```css
:root {
  --paper:      #F2F0EA;
  --paper-2:    #E9E6DD;
  --ink:        #1A1A17;
  --ink-soft:   #55534C;
  --line:       #C9C4B8;

  /* glazure iz kataloga — akcenti, koristiti STRIDNO, jedna po sekciji */
  --glz-sage:   #7C8A5B;
  --glz-clay:   #C06B4A;
  --glz-indigo: #2E3A56;
  --glz-sun:    #E3B23C;
  --glz-rose:   #C98A94;

  --font-display: "Archivo", sans-serif;
  --font-body: "IBM Plex Sans", sans-serif;

  --fs-hero: clamp(2.75rem, 6vw, 5.5rem);
  --fs-h2: clamp(2rem, 4vw, 3.25rem);
  --fs-body: 1.0625rem;
  --lh-body: 1.6;
  --fs-caption: 0.75rem;

  --radius: 0;
  --radius-photo: 3px;

  --container-max: 1440px;
  --gutter: clamp(1.25rem, 5vw, 4rem);

  /* spacing scale — fixed steps (docs/superpowers/specs/2026-07-31-foundation-nav-spacing-design.md) */
  --space-1:  0.25rem;
  --space-2:  0.5rem;
  --space-3:  0.75rem;
  --space-4:  1rem;
  --space-5:  1.25rem;
  --space-6:  1.5rem;
  --space-7:  2rem;
  --space-8:  2.5rem;
  --space-9:  3rem;
  --space-10: 4rem;
  --space-11: 5rem;
  --space-12: 6rem;
  --space-13: 8rem;

  /* spacing scale — fluid tiers, for gaps/padding that should ease with viewport width */
  --space-fluid-sm: clamp(1.5rem, 4vw, 2.5rem);
  --space-fluid-md: clamp(2rem, 5vw, 4rem);
  --space-fluid-lg: clamp(2.5rem, 6vw, 5rem);
  --space-fluid-xl: clamp(3.5rem, 9vw, 8rem);

  /* motion tokens — spring-style easing instead of linear/ease-in-out */
  --ease-spring: cubic-bezier(0.32, 0.72, 0, 1);
  --dur-fast: 200ms;
  --dur-base: 400ms;
  --dur-slow: 700ms;
}
```

- [ ] **Step 2: Point `.section` at the new fluid-xl tier**

In the same file, find:

```css
.section {
  padding: clamp(3rem, 8vw, 7rem) 0;
}
```

Replace with:

```css
.section {
  padding: var(--space-fluid-xl) 0;
}
```

- [ ] **Step 3: Manual verification**

Run `dotnet run` from `Plocica/`, open the homepage. Confirm:
- No console errors, no visual breakage (this step only adds/changes custom properties and two rules that already consumed a token-like value).
- The hero H1 ("Gdje detalji postaju...") is visibly smaller than before (was maxing at 112px, now maxes at 88px) — it may still wrap awkwardly since `.hero-copy`'s width isn't widened yet; that's completed in Task 2.
- Section vertical padding (e.g. above/below "Iz naše ponude") looks similar to before, slightly more generous at wide viewports.

- [ ] **Step 4: Commit**

```bash
git add Plocica/wwwroot/css/tokens.css
git commit -m "feat: add spacing scale and motion tokens, correct hero type scale"
```

---

### Task 2: Hero spacing + width fix (completes the H1 wrap fix)

**Files:**
- Modify: `Plocica/wwwroot/css/components.css:6-48` (`.hero`, `.hero-copy`, `.hero-actions`)

**Interfaces:**
- Consumes: `--space-fluid-lg`, `--space-fluid-sm`, `--space-5`, `--space-6`, `--space-7` from Task 1.
- Produces: nothing new — this is the hero half of the spacing rollout, split from Task 3 (the rest of `components.css`) because it's the concrete case that motivated the type-scale fix and should be verified together with it.

- [ ] **Step 1: Update `.hero` padding**

Find:

```css
.hero {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: clamp(26rem, 65vh, 42rem);
  padding-block: clamp(2.5rem, 6vw, 5rem);
  border: 1px solid var(--line);
  overflow: hidden;
}
```

Replace with:

```css
.hero {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: clamp(26rem, 65vh, 42rem);
  padding-block: var(--space-fluid-lg);
  border: 1px solid var(--line);
  overflow: hidden;
}
```

- [ ] **Step 2: Widen `.hero-copy` and tokenize its spacing**

Find:

```css
.hero-copy {
  position: relative;
  z-index: 1;
  max-width: 42rem;
  margin-inline: auto;
  padding: clamp(1.5rem, 4vw, 3rem);
  text-align: center;
  /* lets hover-fill on the hexagon grid respond even where this panel's
     empty space overlaps it — only the actual links stay clickable */
  pointer-events: none;
}

.hero-copy .eyebrow { margin-bottom: 1.25rem; }

.hero-copy h1 { margin-bottom: 1.5rem; }
```

Replace with:

```css
.hero-copy {
  position: relative;
  z-index: 1;
  max-width: 46rem;
  margin-inline: auto;
  padding: var(--space-fluid-sm);
  text-align: center;
  /* lets hover-fill on the hexagon grid respond even where this panel's
     empty space overlaps it — only the actual links stay clickable */
  pointer-events: none;
}

.hero-copy .eyebrow { margin-bottom: var(--space-5); }

.hero-copy h1 { margin-bottom: var(--space-6); }
```

- [ ] **Step 3: Tokenize `.hero-actions` spacing**

Find:

```css
.hero-actions {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1.5rem;
  margin-top: 2rem;
  flex-wrap: wrap;
  pointer-events: auto;
}
```

Replace with:

```css
.hero-actions {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-6);
  margin-top: var(--space-7);
  flex-wrap: wrap;
  pointer-events: auto;
}
```

- [ ] **Step 4: Manual verification**

Run `dotnet run`, open the homepage. Confirm:
- The H1 now wraps to three lines (not four) at a 1440px-wide viewport. (Verified empirically during Task 2's review via headless-Chrome rendering with the real Archivo font: at 1440px, `--fs-hero` computes to ~85px and the 46rem `.hero-copy` column gives exactly 3 lines, not 2 — the original 2-line target was an untested estimate. 3 lines at this display size reads fine for a hero statement; the target was corrected post-review rather than forcing an awkward further width/font squeeze.)
- Hero content stays centered; hovering the hexagon grid behind/around the text still fills hexagons (pointer-events unaffected — only `.hero-copy`'s own size/padding changed, not its `pointer-events: none`).
- Resize below 860px — hero still stacks/scales correctly (mobile hero rules from the prior session are untouched).

- [ ] **Step 5: Commit**

```bash
git add Plocica/wwwroot/css/components.css
git commit -m "fix: widen hero copy column and tokenize its spacing"
```

---

### Task 3: Spacing rollout — `components.css` (non-hero sections)

**Files:**
- Modify: `Plocica/wwwroot/css/components.css` (manifesto through project-placeholder — see steps for exact blocks)

**Interfaces:**
- Consumes: `--space-2`, `--space-3`, `--space-4`, `--space-5`, `--space-6`, `--space-7`, `--space-fluid-sm`, `--space-fluid-md`, `--space-fluid-lg` from Task 1.
- Produces: nothing new — value normalization only, no selectors added/removed/renamed.

- [ ] **Step 1: Manifesto strip**

Find:

```css
.manifesto {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: clamp(2rem, 5vw, 4rem);
}
```

Replace with:

```css
.manifesto {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-fluid-md);
}
```

Find:

```css
.manifesto-list {
  list-style: none;
  margin: 1.5rem 0 0;
  padding: 0;
  display: flex;
  gap: 2rem;
  flex-wrap: wrap;
}

.manifesto-list li {
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: var(--ink-soft);
  padding-top: 0.6rem;
  border-top: 1px solid var(--line);
}
```

Replace with:

```css
.manifesto-list {
  list-style: none;
  margin: var(--space-6) 0 0;
  padding: 0;
  display: flex;
  gap: var(--space-7);
  flex-wrap: wrap;
}

.manifesto-list li {
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: var(--ink-soft);
  padding-top: var(--space-2);
  border-top: 1px solid var(--line);
}
```

- [ ] **Step 2: Craft/video section**

Find:

```css
.craft-copy {
  position: absolute;
  left: 0;
  right: 0;
  bottom: 0;
  padding: clamp(1.5rem, 4vw, 3rem) var(--gutter);
  z-index: 1;
}
```

Replace with:

```css
.craft-copy {
  position: absolute;
  left: 0;
  right: 0;
  bottom: 0;
  padding: var(--space-fluid-sm) var(--gutter);
  z-index: 1;
}
```

- [ ] **Step 3: Collections cards**

Find:

```css
.collections-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1px;
  background: var(--line);
  border: 1px solid var(--line);
  margin-top: clamp(2rem, 4vw, 3rem);
}
```

Replace with:

```css
.collections-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1px;
  background: var(--line);
  border: 1px solid var(--line);
  margin-top: var(--space-fluid-sm);
}
```

Find:

```css
.collection-card .card-body {
  padding: 1.5rem;
  flex: 1;
  display: flex;
  flex-direction: column;
}
```

Replace with:

```css
.collection-card .card-body {
  padding: var(--space-6);
  flex: 1;
  display: flex;
  flex-direction: column;
}
```

Find:

```css
.collection-card .card-link {
  font-size: 0.8125rem;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  border-top: 1px solid var(--line);
  padding-top: 0.85rem;
  margin-top: 1rem;
  display: block;
}
```

Replace with:

```css
.collection-card .card-link {
  font-size: 0.8125rem;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  border-top: 1px solid var(--line);
  padding-top: var(--space-3);
  margin-top: var(--space-4);
  display: block;
}
```

- [ ] **Step 4: Colour-map teaser and O nama teaser**

Find:

```css
.palette-teaser {
  display: grid;
  grid-template-columns: 0.9fr 1.1fr;
  gap: clamp(2rem, 5vw, 5rem);
  align-items: center;
}
```

Replace with:

```css
.palette-teaser {
  display: grid;
  grid-template-columns: 0.9fr 1.1fr;
  gap: var(--space-fluid-lg);
  align-items: center;
}
```

Find:

```css
.people-teaser {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: clamp(2rem, 5vw, 4rem);
  align-items: center;
}
```

Replace with:

```css
.people-teaser {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-fluid-md);
  align-items: center;
}
```

- [ ] **Step 5: Inverted CTA strip and process list**

Find:

```css
.section-invert .cta-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 1.5rem;
}
```

Replace with:

```css
.section-invert .cta-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: var(--space-6);
}
```

Find:

```css
.process-list li {
  display: grid;
  grid-template-columns: 3.5rem 1fr;
  gap: 1.5rem;
  padding: 1.5rem 0;
  border-bottom: 1px solid var(--line);
}
```

Replace with:

```css
.process-list li {
  display: grid;
  grid-template-columns: 3.5rem 1fr;
  gap: var(--space-6);
  padding: var(--space-6) 0;
  border-bottom: 1px solid var(--line);
}
```

- [ ] **Step 6: FAQ list**

Find:

```css
.faq-list details {
  border-bottom: 1px solid var(--line);
  padding: 1.15rem 0;
}
```

Replace with:

```css
.faq-list details {
  border-bottom: 1px solid var(--line);
  padding: var(--space-5) 0;
}
```

Find:

```css
.faq-list p {
  margin: 0.85rem 0 0;
  color: var(--ink-soft);
  max-width: 58ch;
}
```

Replace with:

```css
.faq-list p {
  margin: var(--space-3) 0 0;
  color: var(--ink-soft);
  max-width: 58ch;
}
```

- [ ] **Step 7: Kolekcije — intro, spec list, colour/shape grids**

Find:

```css
.collection-intro {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: clamp(2rem, 5vw, 4rem);
  align-items: end;
  margin-bottom: clamp(2rem, 4vw, 3rem);
}
```

Replace with:

```css
.collection-intro {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-fluid-md);
  align-items: end;
  margin-bottom: var(--space-fluid-sm);
}
```

Find:

```css
.spec-row {
  display: grid;
  grid-template-columns: 8ch 1fr 1fr;
  gap: 1.5rem;
  padding: 1.25rem 0;
  border-bottom: 1px solid var(--line);
  align-items: baseline;
}
```

Replace with:

```css
.spec-row {
  display: grid;
  grid-template-columns: 8ch 1fr 1fr;
  gap: var(--space-6);
  padding: var(--space-5) 0;
  border-bottom: 1px solid var(--line);
  align-items: baseline;
}
```

Find:

```css
.color-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1px;
  background: var(--line);
  border: 1px solid var(--line);
  margin-top: 1.5rem;
}
```

Replace with:

```css
.color-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1px;
  background: var(--line);
  border: 1px solid var(--line);
  margin-top: var(--space-6);
}
```

Find:

```css
.shape-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1px;
  background: var(--line);
  border: 1px solid var(--line);
  margin-top: 1.5rem;
}
```

Replace with:

```css
.shape-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1px;
  background: var(--line);
  border: 1px solid var(--line);
  margin-top: var(--space-6);
}
```

Find:

```css
.shape-tile figcaption {
  padding: 0.85rem 1rem;
  font-size: 0.875rem;
}
```

Replace with:

```css
.shape-tile figcaption {
  padding: var(--space-3) var(--space-4);
  font-size: 0.875rem;
}
```

Find:

```css
.coming-soon-note {
  font-size: 0.8125rem;
  color: var(--ink-soft);
  margin-top: 1.5rem;
  padding-top: 1rem;
  border-top: 1px solid var(--line);
}
```

Replace with:

```css
.coming-soon-note {
  font-size: 0.8125rem;
  color: var(--ink-soft);
  margin-top: var(--space-6);
  padding-top: var(--space-4);
  border-top: 1px solid var(--line);
}
```

- [ ] **Step 8: Radovi — projects**

Find:

```css
.project {
  padding: clamp(2.5rem, 6vw, 5rem) 0;
  border-top: 1px solid var(--line);
}
```

Replace with:

```css
.project {
  padding: var(--space-fluid-lg) 0;
  border-top: 1px solid var(--line);
}
```

Find:

```css
.project-text {
  margin-top: 1.25rem;
  color: var(--ink-soft);
  font-size: 1.0625rem;
  max-width: 65ch;
}
```

Replace with:

```css
.project-text {
  margin-top: var(--space-5);
  color: var(--ink-soft);
  font-size: 1.0625rem;
  max-width: 65ch;
}
```

Find:

```css
.project-gallery {
  margin-top: 2rem;
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 1px;
  background: var(--line);
  border: 1px solid var(--line);
}
```

Replace with:

```css
.project-gallery {
  margin-top: var(--space-7);
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 1px;
  background: var(--line);
  border: 1px solid var(--line);
}
```

Find:

```css
.project-placeholder {
  margin-top: 2rem;
  aspect-ratio: 21 / 9;
  border: 1px dashed var(--line);
  background: var(--paper-2);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--ink-soft);
  font-size: 0.875rem;
}
```

Replace with:

```css
.project-placeholder {
  margin-top: var(--space-7);
  aspect-ratio: 21 / 9;
  border: 1px dashed var(--line);
  background: var(--paper-2);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--ink-soft);
  font-size: 0.875rem;
}
```

- [ ] **Step 9: Manual verification across every page that uses these classes**

Run `dotnet run`. Confirm no visual regression (spacing should look the same or very slightly tightened/loosened, never broken/overlapping) on:
- `/` — manifesto strip, craft/video section, collections cards, colour-map teaser, O nama teaser, inverted CTA strip.
- `/kontakt` — process list ("Proces narudžbe").
- `/faq` — FAQ accordion spacing.
- `/kolekcije` — collection intro, spec rows, colour grid, shape grid.
- `/radovi` — project entries, galleries, placeholder.

- [ ] **Step 10: Commit**

```bash
git add Plocica/wwwroot/css/components.css
git commit -m "refactor: normalize components.css spacing onto the shared scale"
```

---

### Task 4: Spacing rollout — `layout.css` (header/nav/footer values only)

**Files:**
- Modify: `Plocica/wwwroot/css/layout.css` (header container gap, nav gap, dropdown padding/margin, mobile nav padding, footer padding/margin/gap)

**Interfaces:**
- Consumes: `--space-2`, `--space-3`, `--space-4`, `--space-5`, `--space-7`, `--space-9`, `--space-10`, `--space-fluid-sm` from Task 1.
- Produces: the exact resulting text of `.nav-primary` (mobile block), `.nav-toggle`, and `.site-header .container` that Tasks 8–11 build on top of — those tasks' "find" snippets assume this task has already run.

- [ ] **Step 1: Header container gap**

Find:

```css
.site-header .container {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 2rem;
  min-height: 84px;
}
```

Replace with:

```css
.site-header .container {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-7);
  min-height: 84px;
}
```

- [ ] **Step 2: Nav gap**

Find:

```css
.nav-primary {
  display: flex;
  align-items: center;
  gap: clamp(1.25rem, 3vw, 2.5rem);
  list-style: none;
  margin: 0;
  padding: 0;
}
```

Replace with:

```css
.nav-primary {
  display: flex;
  align-items: center;
  gap: var(--space-fluid-sm);
  list-style: none;
  margin: 0;
  padding: 0;
}
```

- [ ] **Step 3: Dropdown padding/margin**

Find:

```css
.nav-dropdown {
  position: absolute;
  top: 100%;
  left: 0;
  min-width: 260px;
  background: var(--paper);
  border: 1px solid var(--line);
  padding: 0.5rem 0;
  margin-top: 1px;
  opacity: 0;
  visibility: hidden;
  transform: translateY(4px);
  transition: opacity 0.15s ease, transform 0.15s ease, visibility 0.15s;
}
```

Replace with:

```css
.nav-dropdown {
  position: absolute;
  top: 100%;
  left: 0;
  min-width: 260px;
  background: var(--paper);
  border: 1px solid var(--line);
  padding: var(--space-2) 0;
  margin-top: 1px;
  opacity: 0;
  visibility: hidden;
  transform: translateY(4px);
  transition: opacity 0.15s ease, transform 0.15s ease, visibility 0.15s;
}
```

Find:

```css
.nav-dropdown a {
  display: block;
  padding: 0.6em 1.25em;
  font-size: 0.9375rem;
}
```

Replace with:

```css
.nav-dropdown a {
  display: block;
  padding: 0.6rem var(--space-5);
  font-size: 0.9375rem;
}
```

Find:

```css
.nav-dropdown-sep {
  height: 1px;
  background: var(--line);
  margin: 0.5rem 1.25em;
}
```

Replace with:

```css
.nav-dropdown-sep {
  height: 1px;
  background: var(--line);
  margin: var(--space-2) var(--space-5);
}
```

- [ ] **Step 4: Mobile nav padding and CTA margin**

Find:

```css
  .nav-primary {
    position: fixed;
    inset: 84px 0 0 0;
    background: var(--paper);
    flex-direction: column;
    align-items: flex-start;
    gap: 0;
    padding: 1rem var(--gutter) 2rem;
    overflow-y: auto;
    transform: translateY(-8px);
    opacity: 0;
    visibility: hidden;
    transition: opacity 0.2s ease, transform 0.2s ease, visibility 0.2s;
  }
```

Replace with:

```css
  .nav-primary {
    position: fixed;
    inset: 84px 0 0 0;
    background: var(--paper);
    flex-direction: column;
    align-items: flex-start;
    gap: 0;
    padding: var(--space-4) var(--gutter) var(--space-7);
    overflow-y: auto;
    transform: translateY(-8px);
    opacity: 0;
    visibility: hidden;
    transition: opacity 0.2s ease, transform 0.2s ease, visibility 0.2s;
  }
```

Find:

```css
  .nav-cta { border-bottom: none !important; margin-top: 0.75rem; }
```

Replace with:

```css
  .nav-cta { border-bottom: none !important; margin-top: var(--space-3); }
```

- [ ] **Step 5: Footer**

Find:

```css
.site-footer {
  border-top: 1px solid var(--line);
  padding: 3rem 0;
  margin-top: 4rem;
}

.site-footer .container {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  gap: 1rem;
  font-size: 0.9375rem;
  color: var(--ink-soft);
}
```

Replace with:

```css
.site-footer {
  border-top: 1px solid var(--line);
  padding: var(--space-9) 0;
  margin-top: var(--space-10);
}

.site-footer .container {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  gap: var(--space-4);
  font-size: 0.9375rem;
  color: var(--ink-soft);
}
```

- [ ] **Step 6: Manual verification**

Run `dotnet run`. Confirm the header, "Kolekcije" dropdown, mobile menu (resize <860px), and footer all look visually the same as before (values matched exactly or moved by ≤0.25rem) on any page (`_Layout.cshtml` is shared).

- [ ] **Step 7: Commit**

```bash
git add Plocica/wwwroot/css/layout.css
git commit -m "refactor: normalize layout.css spacing onto the shared scale"
```

---

### Task 5: Spacing rollout — `onama.css`

**Files:**
- Modify: `Plocica/wwwroot/css/onama.css`

**Interfaces:**
- Consumes: `--space-4`, `--space-5`, `--space-6`, `--space-fluid-sm`, `--space-fluid-md` from Task 1.
- Produces: nothing new.

- [ ] **Step 1: About-hero and about-grid gaps**

Find:

```css
.about-hero {
  display: grid;
  gap: clamp(1.5rem, 4vw, 2.5rem);
}
```

Replace with:

```css
.about-hero {
  display: grid;
  gap: var(--space-fluid-sm);
}
```

Find:

```css
.about-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: clamp(2rem, 5vw, 4rem);
  align-items: center;
}
```

Replace with:

```css
.about-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-fluid-md);
  align-items: center;
}
```

- [ ] **Step 2: Role list and media row**

Find:

```css
.role-list {
  list-style: none;
  margin: 1.5rem 0 0;
  padding: 0;
}

.role-list li {
  padding: 1.1rem 0;
  border-top: 1px solid var(--line);
}
```

Replace with:

```css
.role-list {
  list-style: none;
  margin: var(--space-6) 0 0;
  padding: 0;
}

.role-list li {
  padding: var(--space-4) 0;
  border-top: 1px solid var(--line);
}
```

Find:

```css
.media-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0 1.75rem;
  margin-top: 1.25rem;
}
```

Replace with:

```css
.media-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0 var(--space-6);
  margin-top: var(--space-5);
}
```

- [ ] **Step 3: Manual verification**

Run `dotnet run`, open `/o-nama`. Confirm the hero figure, two-column grid, role list, and media-mentions row all look visually unchanged (values matched or moved by ≤0.25rem).

- [ ] **Step 4: Commit**

```bash
git add Plocica/wwwroot/css/onama.css
git commit -m "refactor: normalize onama.css spacing onto the shared scale"
```

---

### Task 6: Nested-hairline frame utility (foundation only, not yet applied)

**Files:**
- Modify: `Plocica/wwwroot/css/components.css` (insert new rule block after the `.hero-grid`/`.hero-grid canvas` rules, before the "Manifesto strip" comment)

**Interfaces:**
- Produces: `.hairline-frame` (outer) and `.hairline-frame-inner` (inner) classes. No file in this plan applies them yet — the Catalog sub-project (a later spec/plan) will use them for card treatments.
- Consumes: `--space-2`, `--line`, `--paper`, `--paper-2` (all pre-existing or from Task 1).

- [ ] **Step 1: Add the utility**

In `Plocica/wwwroot/css/components.css`, find:

```css
.hero-grid canvas {
  display: block;
  width: 100%;
  height: 100%;
}
```

Replace with:

```css
.hero-grid canvas {
  display: block;
  width: 100%;
  height: 100%;
}

/* ---------- Nested hairline frame — foundation utility, not yet used on any page ---------- */

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

- [ ] **Step 2: Verify no regression**

Run `dotnet run`. This step only adds unused CSS rules — confirm the site builds and every page still renders identically to before this task (no selector collisions, since `.hairline-frame`/`.hairline-frame-inner` don't exist anywhere in the markup yet).

- [ ] **Step 3: Commit**

```bash
git add Plocica/wwwroot/css/components.css
git commit -m "feat: add nested-hairline-frame utility for future card work"
```

---

### Task 7: Button hover physics — drafting-pen offset outline

**Files:**
- Modify: `Plocica/wwwroot/css/tokens.css:112-142` (`.btn`, `.btn-cta`)
- Modify: `Plocica/wwwroot/css/components.css` (`.section-invert .btn-cta` block, around line 376)

**Interfaces:**
- Consumes: `--ease-spring`, `--dur-fast`, `--dur-base` from Task 1.
- Produces: a `.btn::after` pseudo-element pattern other button-like components can copy in later sub-projects (no other file in this plan reads it directly).

- [ ] **Step 1: Rewrite `.btn`/`.btn-cta` in `tokens.css`**

Find:

```css
.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5em;
  padding: 0.85em 1.5em;
  border: 1px solid var(--ink);
  font-family: var(--font-body);
  font-size: 0.9375rem;
  letter-spacing: 0.02em;
  background: transparent;
  cursor: pointer;
  transition: background 0.2s ease, color 0.2s ease;
}

.btn:hover,
.btn:focus-visible {
  background: var(--ink);
  color: var(--paper);
}

.btn-cta {
  background: var(--ink);
  color: var(--paper);
}

.btn-cta:hover,
.btn-cta:focus-visible {
  background: var(--accent, var(--glz-clay));
  border-color: var(--accent, var(--glz-clay));
  color: var(--ink);
}
```

Replace with:

```css
.btn {
  position: relative;
  display: inline-flex;
  align-items: center;
  gap: 0.5em;
  padding: 0.85em 1.5em;
  border: 1px solid var(--ink);
  font-family: var(--font-body);
  font-size: 0.9375rem;
  letter-spacing: 0.02em;
  background: transparent;
  cursor: pointer;
  transition: background var(--dur-fast) var(--ease-spring),
    color var(--dur-fast) var(--ease-spring),
    transform var(--dur-fast) var(--ease-spring);
}

.btn::after {
  content: "";
  position: absolute;
  inset: 0;
  border: 1px solid var(--ink);
  transform: translate(4px, 4px);
  z-index: -1;
  opacity: 0;
  transition: opacity var(--dur-fast) var(--ease-spring),
    transform var(--dur-base) var(--ease-spring);
}

.btn:hover,
.btn:focus-visible {
  background: var(--ink);
  color: var(--paper);
  transform: translate(-2px, -2px);
}

.btn:hover::after,
.btn:focus-visible::after {
  opacity: 1;
  transform: translate(0, 0);
}

.btn:active {
  transform: translate(0, 0) scale(0.98);
}

.btn-cta {
  background: var(--ink);
  color: var(--paper);
}

.btn-cta:hover,
.btn-cta:focus-visible {
  background: var(--accent, var(--glz-clay));
  border-color: var(--accent, var(--glz-clay));
  color: var(--ink);
}
```

- [ ] **Step 2: Fix outline visibility on the inverted CTA strip**

`.section-invert` sits on a near-black background, so the offset outline (which defaults to `var(--ink)`, near-black) would be invisible against it. In `Plocica/wwwroot/css/components.css`, find:

```css
.section-invert .btn-cta {
  background: var(--paper);
  color: var(--ink);
  border-color: var(--paper);
}

.section-invert .btn-cta:hover,
.section-invert .btn-cta:focus-visible {
  background: var(--glz-sun);
  border-color: var(--glz-sun);
  color: var(--ink);
}
```

Replace with:

```css
.section-invert .btn-cta {
  background: var(--paper);
  color: var(--ink);
  border-color: var(--paper);
}

.section-invert .btn-cta::after {
  border-color: var(--paper);
}

.section-invert .btn-cta:hover,
.section-invert .btn-cta:focus-visible {
  background: var(--glz-sun);
  border-color: var(--glz-sun);
  color: var(--ink);
}
```

- [ ] **Step 3: Manual verification**

Run `dotnet run`. On the homepage:
- Hover the primary CTA ("Stvorimo nešto zajedno") in the header and hero — confirm it lifts slightly and a thin outline slides in from behind/below-right, using an eased (not linear) motion.
- Hover the "Kolekcije" text-link-style `.btn` variants if any exist — confirm same behavior.
- Click-and-hold a button — confirm it scales down slightly while pressed.
- Tab to a button with the keyboard — confirm the same hover treatment appears on `:focus-visible`.
- Scroll to the inverted CTA strip section — hover its button — confirm the offset outline is a visible paper-colored line against the dark background, not invisible.

- [ ] **Step 4: Commit**

```bash
git add Plocica/wwwroot/css/tokens.css Plocica/wwwroot/css/components.css
git commit -m "feat: replace button shadow-lift with drafting-pen offset outline"
```

---

### Task 8: Hamburger → X morph

**Files:**
- Modify: `Plocica/Pages/Shared/_Layout.cshtml:27-29` (nav-toggle markup)
- Modify: `Plocica/wwwroot/css/layout.css` (`.nav-toggle` block, around line 143 — post-Task-4 state)
- Modify: `Plocica/wwwroot/js/nav.js:1-10` (aria-label toggle)

**Interfaces:**
- Consumes: `--dur-base`, `--dur-fast`, `--ease-spring` from Task 1; the existing `nav.classList.toggle("is-open")`/`toggle.setAttribute("aria-expanded", ...)` logic in `nav.js`, unchanged.
- Produces: the `.nav-toggle[aria-expanded="true"]` CSS hook — driven entirely by the `aria-expanded` attribute `nav.js` already sets, so the morph needs no new JS state.

- [ ] **Step 1: Replace the visible text label with an icon**

In `Plocica/Pages/Shared/_Layout.cshtml`, find:

```html
            <button class="nav-toggle" type="button" aria-expanded="false" aria-controls="nav-primary">
                Izbornik
            </button>
```

Replace with:

```html
            <button class="nav-toggle" type="button" aria-expanded="false" aria-controls="nav-primary" aria-label="Otvori izbornik">
                <span class="nav-toggle-icon" aria-hidden="true"></span>
            </button>
```

- [ ] **Step 2: Restyle `.nav-toggle` and add the icon/morph CSS**

In `Plocica/wwwroot/css/layout.css`, find (this is the state left by Task 4 — unchanged by that task, since `.nav-toggle` wasn't in its mapping table):

```css
.nav-toggle {
  display: none;
  background: none;
  border: 1px solid var(--ink);
  padding: 0.5em 0.75em;
  font: inherit;
  cursor: pointer;
}
```

Replace with:

```css
.nav-toggle {
  display: none;
  align-items: center;
  justify-content: center;
  width: 44px;
  height: 44px;
  background: none;
  border: 1px solid var(--ink);
  padding: 0;
  font: inherit;
  cursor: pointer;
}

.nav-toggle-icon {
  position: relative;
  display: block;
  width: 18px;
  height: 1px;
  background: var(--ink);
  transition: background var(--dur-fast) var(--ease-spring);
}

.nav-toggle-icon::before,
.nav-toggle-icon::after {
  content: "";
  position: absolute;
  left: 0;
  width: 18px;
  height: 1px;
  background: var(--ink);
  transition: transform var(--dur-base) var(--ease-spring), top var(--dur-base) var(--ease-spring);
}

.nav-toggle-icon::before { top: -5px; }
.nav-toggle-icon::after { top: 5px; }

.nav-toggle[aria-expanded="true"] .nav-toggle-icon {
  background: transparent;
}

.nav-toggle[aria-expanded="true"] .nav-toggle-icon::before {
  top: 0;
  transform: rotate(45deg);
}

.nav-toggle[aria-expanded="true"] .nav-toggle-icon::after {
  top: 0;
  transform: rotate(-45deg);
}
```

Leave the existing `@media (max-width: 860px) { .nav-toggle { display: inline-flex; } ... }` rule untouched — it still correctly makes the (now icon-based) button visible on mobile.

- [ ] **Step 3: Swap the `aria-label` on toggle**

In `Plocica/wwwroot/js/nav.js`, find:

```js
  if (toggle && nav) {
    toggle.addEventListener("click", function () {
      var isOpen = nav.classList.toggle("is-open");
      toggle.setAttribute("aria-expanded", isOpen ? "true" : "false");
    });
  }
```

Replace with:

```js
  if (toggle && nav) {
    toggle.addEventListener("click", function () {
      var isOpen = nav.classList.toggle("is-open");
      toggle.setAttribute("aria-expanded", isOpen ? "true" : "false");
      toggle.setAttribute("aria-label", isOpen ? "Zatvori izbornik" : "Otvori izbornik");
    });
  }
```

- [ ] **Step 4: Manual verification**

Run `dotnet run`, resize the browser below 860px. Confirm:
- A 2-line hairline hamburger icon appears in place of the old "Izbornik" text.
- Clicking it morphs the two lines into an `×` (rotate/translate, not an instant swap).
- Inspect the button in DevTools — `aria-expanded` flips `false`→`true` and `aria-label` flips "Otvori izbornik"→"Zatvori izbornik" on click, and both revert on the second click.
- The menu itself still opens/closes as before (this task didn't touch `.nav-primary`'s open/close logic).

- [ ] **Step 5: Commit**

```bash
git add Plocica/Pages/Shared/_Layout.cshtml Plocica/wwwroot/css/layout.css Plocica/wwwroot/js/nav.js
git commit -m "feat: morph hamburger icon into X and swap its aria-label"
```

---

### Task 9: Mobile menu blur + staggered link reveal

**Files:**
- Modify: `Plocica/wwwroot/css/layout.css` (mobile `.nav-primary` and `.nav-primary > li` rules, inside `@media (max-width: 860px)`)

**Interfaces:**
- Consumes: `--dur-base`, `--ease-spring` from Task 1; the existing `.nav-primary.is-open` class toggled by `nav.js` (unchanged — no JS edits in this task).
- Produces: nothing new for other files.

- [ ] **Step 1: Translucent blurred background**

In `Plocica/wwwroot/css/layout.css`, inside the `@media (max-width: 860px)` block, find (this is the state left by Task 4):

```css
  .nav-primary {
    position: fixed;
    inset: 84px 0 0 0;
    background: var(--paper);
    flex-direction: column;
    align-items: flex-start;
    gap: 0;
    padding: var(--space-4) var(--gutter) var(--space-7);
    overflow-y: auto;
    transform: translateY(-8px);
    opacity: 0;
    visibility: hidden;
    transition: opacity 0.2s ease, transform 0.2s ease, visibility 0.2s;
  }
```

Replace with:

```css
  .nav-primary {
    position: fixed;
    inset: 84px 0 0 0;
    background: rgba(242, 240, 234, 0.82);
    backdrop-filter: blur(20px) saturate(140%);
    -webkit-backdrop-filter: blur(20px) saturate(140%);
    flex-direction: column;
    align-items: flex-start;
    gap: 0;
    padding: var(--space-4) var(--gutter) var(--space-7);
    overflow-y: auto;
    transform: translateY(-8px);
    opacity: 0;
    visibility: hidden;
    transition: opacity var(--dur-base) var(--ease-spring),
      transform var(--dur-base) var(--ease-spring), visibility var(--dur-base);
  }
```

- [ ] **Step 2: Staggered link reveal**

In the same media query, find:

```css
  .nav-primary > li {
    width: 100%;
    border-bottom: 1px solid var(--line);
  }
```

Replace with:

```css
  .nav-primary > li {
    width: 100%;
    border-bottom: 1px solid var(--line);
    opacity: 0;
    transform: translateY(12px);
    transition: opacity var(--dur-base) var(--ease-spring), transform var(--dur-base) var(--ease-spring);
  }

  .nav-primary.is-open > li {
    opacity: 1;
    transform: translateY(0);
  }

  .nav-primary > li:nth-child(1) { transition-delay: 40ms; }
  .nav-primary > li:nth-child(2) { transition-delay: 80ms; }
  .nav-primary > li:nth-child(3) { transition-delay: 120ms; }
  .nav-primary > li:nth-child(4) { transition-delay: 160ms; }
  .nav-primary > li:nth-child(5) { transition-delay: 200ms; }
```

- [ ] **Step 3: Manual verification**

Run `dotnet run`, resize below 860px, open the hamburger menu. Confirm:
- The menu background is translucent and blurs the page content behind it (not solid paper-white).
- The nav links fade + slide up into place with a visible stagger (top item first, each subsequent item slightly later) rather than all appearing at once.
- Close and reopen — the effect repeats correctly each time.
- Confirm the "Kolekcije" dropdown still opens/closes correctly inside the blurred panel (this task didn't touch dropdown logic).

- [ ] **Step 4: Commit**

```bash
git add Plocica/wwwroot/css/layout.css
git commit -m "feat: add blurred background and staggered reveal to mobile menu"
```

---

### Task 10: Magnetic hover on nav links

**Files:**
- Modify: `Plocica/wwwroot/css/layout.css` (`.nav-link`, `.nav-dropdown-toggle` — desktop-scope rules, outside the mobile media query)

**Interfaces:**
- Consumes: `--dur-fast`, `--ease-spring` from Task 1.
- Produces: nothing new for other files.

- [ ] **Step 1: `.nav-link` hover lift**

In `Plocica/wwwroot/css/layout.css`, find:

```css
.nav-link {
  font-size: 0.9375rem;
  letter-spacing: 0.01em;
  padding: 0.5em 0;
  border-bottom: 1px solid transparent;
}

.nav-link:hover,
.nav-link:focus-visible {
  border-bottom-color: var(--ink);
}
```

Replace with:

```css
.nav-link {
  display: inline-block;
  font-size: 0.9375rem;
  letter-spacing: 0.01em;
  padding: 0.5em 0;
  border-bottom: 1px solid transparent;
  transition: border-color var(--dur-fast) var(--ease-spring), transform var(--dur-fast) var(--ease-spring);
}

.nav-link:hover,
.nav-link:focus-visible {
  border-bottom-color: var(--ink);
  transform: translateY(-1px);
}
```

- [ ] **Step 2: `.nav-dropdown-toggle` hover lift**

Find:

```css
.nav-dropdown-toggle {
  display: inline-flex;
  align-items: center;
  gap: 0.35em;
  background: none;
  border: none;
  font: inherit;
  font-size: 0.9375rem;
  letter-spacing: 0.01em;
  color: inherit;
  cursor: pointer;
  padding: 0.5em 0;
  border-bottom: 1px solid transparent;
}

.nav-dropdown-toggle:hover,
.nav-dropdown-toggle:focus-visible {
  border-bottom-color: var(--ink);
}
```

Replace with:

```css
.nav-dropdown-toggle {
  display: inline-flex;
  align-items: center;
  gap: 0.35em;
  background: none;
  border: none;
  font: inherit;
  font-size: 0.9375rem;
  letter-spacing: 0.01em;
  color: inherit;
  cursor: pointer;
  padding: 0.5em 0;
  border-bottom: 1px solid transparent;
  transition: border-color var(--dur-fast) var(--ease-spring), transform var(--dur-fast) var(--ease-spring);
}

.nav-dropdown-toggle:hover,
.nav-dropdown-toggle:focus-visible {
  border-bottom-color: var(--ink);
  transform: translateY(-1px);
}
```

- [ ] **Step 3: Manual verification**

Run `dotnet run` at a desktop viewport (≥861px). Hover each top-level nav link ("Radovi", "O nama", "FAQ") and the "Kolekcije" dropdown toggle — confirm each lifts by ~1px with a smooth (not linear) motion as the underline appears, and the dropdown's chevron rotation (untouched by this task) still works alongside it. Tab through with the keyboard — confirm the same lift on `:focus-visible`.

- [ ] **Step 4: Commit**

```bash
git add Plocica/wwwroot/css/layout.css
git commit -m "feat: add magnetic hover lift to nav links"
```

---

### Task 11: Scroll-compact header

**Files:**
- Modify: `Plocica/Pages/Shared/_Layout.cshtml:18-21` (add sentinel element)
- Modify: `Plocica/wwwroot/css/layout.css` (`.site-header .container` min-height transition + `.is-compact` state; mobile `.nav-primary` inset fix)
- Modify: `Plocica/wwwroot/js/nav.js` (append `IntersectionObserver` wiring)

**Interfaces:**
- Consumes: `--space-10`, `--dur-base`, `--ease-spring` from Task 1.
- Produces: `.site-header.is-compact` class toggled by `nav.js` — no other file in this plan reads it besides the mobile `.nav-primary` inset fix in Step 3 below.

- [ ] **Step 1: Add the sentinel element**

In `Plocica/Pages/Shared/_Layout.cshtml`, find:

```html
<body>
    <a class="skip-link" href="#main">Preskoči na sadržaj</a>

    <header class="site-header">
```

Replace with:

```html
<body>
    <a class="skip-link" href="#main">Preskoči na sadržaj</a>

    <div id="header-sentinel" style="position: absolute; top: 0; width: 1px; height: 1px;" aria-hidden="true"></div>

    <header class="site-header">
```

- [ ] **Step 2: Add the compact state and transition**

In `Plocica/wwwroot/css/layout.css`, find (this is the state left by Task 4):

```css
.site-header .container {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-7);
  min-height: 84px;
}
```

Replace with:

```css
.site-header .container {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-7);
  min-height: 84px;
  transition: min-height var(--dur-base) var(--ease-spring);
}

.site-header.is-compact .container {
  min-height: var(--space-10);
}
```

- [ ] **Step 3: Keep the mobile menu's top offset in sync with the compact header**

Still inside the `@media (max-width: 860px)` block, immediately after the `.nav-primary` rule (as left by Task 9), add:

```css
  .site-header.is-compact .nav-primary {
    inset: var(--space-10) 0 0 0;
  }
```

Without this, opening the mobile menu after the header has compacted would leave a 20px gap (84px vs. the header's actual 64px height) between the header and the menu panel.

- [ ] **Step 4: Wire up the `IntersectionObserver`**

In `Plocica/wwwroot/js/nav.js`, find the closing of the IIFE:

```js
    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape") {
        dropdownItem.setAttribute("aria-expanded", "false");
      }
    });
  }
})();
```

Replace with:

```js
    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape") {
        dropdownItem.setAttribute("aria-expanded", "false");
      }
    });
  }

  var header = document.querySelector(".site-header");
  var sentinel = document.getElementById("header-sentinel");

  if (header && sentinel && "IntersectionObserver" in window) {
    var headerObserver = new IntersectionObserver(function (entries) {
      header.classList.toggle("is-compact", !entries[0].isIntersecting);
    });
    headerObserver.observe(sentinel);
  }
})();
```

- [ ] **Step 5: Manual verification**

Run `dotnet run`. Confirm:
- At the top of any page, the header is at its full 84px height.
- Scrolling down even slightly shrinks the header smoothly to 64px (not instantly).
- Scrolling back to the very top restores the full height.
- On a mobile-width viewport, scroll down (header compacts), then open the hamburger menu — confirm the menu panel sits flush below the header with no gap or overlap.
- In DevTools, temporarily rename `IntersectionObserver` to something else in the console (or check the guard reads correctly) to confirm the `"IntersectionObserver" in window` guard is present — the header should simply never compact if unsupported, with no console error.
- In DevTools' rendering panel, emulate `prefers-reduced-motion: reduce` and reload. Confirm the header compact/expand, hamburger morph (Task 8), and mobile menu open/close + stagger (Task 9) all still function but as instant state changes with no visible animation — this should happen automatically via the global override in `tokens.css` with no extra code, since every transition added in Tasks 7–11 uses `var(--dur-*)`.

- [ ] **Step 6: Commit**

```bash
git add Plocica/Pages/Shared/_Layout.cshtml Plocica/wwwroot/css/layout.css Plocica/wwwroot/js/nav.js
git commit -m "feat: add scroll-compact header via IntersectionObserver"
```
