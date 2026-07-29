# Hero ShapeGrid Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the homepage hero's photo-crossfade visual with a vanilla-JS-ported hexagon "ShapeGrid" canvas effect (recolored to the site's ceramic-glaze palette), and relocate the real tile photography into its own section further down the page.

**Architecture:** Pure client-side change to one Razor Page. A new `wwwroot/js/hero-grid.js` module (ported by hand from react-bits' `ShapeGrid`, verified against its real source) draws an interactive hexagon grid on a `<canvas>` inside a new `.hero-grid` panel. The existing photo-crossfade markup (`.plate-frame` / `.hero-frame` / `hero.js`) is untouched in behavior — it just moves to a new section, since its selectors are class-based and don't care where in the DOM they live.

**Tech Stack:** ASP.NET Core Razor Pages, vanilla JS (no framework, no build step), plain CSS with custom properties.

## Global Constraints

- No npm, React, or any bundler — this repo has zero JS tooling by deliberate project decision (`docs/02-DESIGN.md`: "Bez JS frameworka"). Every new file is plain `.js`/`.css` referenced directly by Razor `<script>`/`<link>` tags.
- Colors must come from the existing CSS custom properties in `wwwroot/css/tokens.css` (`--line: #C9C4B8`, `--glz-sun: #E3B23C`) — read via `getComputedStyle`, with the literal hex as a fallback only, never hardcoded as the primary source.
- This project was just git-initialized specifically to support this plan's subagent-driven execution (it had no repository before). Commit normally after each task, as usual for this workflow.
- This project has **no automated test suite**. Verification steps are manual: run `dotnet run` from `Plocica/`, open the page in a browser, and check the described behavior. Do not treat the absence of automated tests as a defect to fix — commit the manual verification results as described in each task's steps.
- Preserve the existing accessibility quality floor: keyboard focus visibility, `prefers-reduced-motion` support, alt text on all photos — don't regress any of it.
- `hero.js` and the `.plate-frame`/`.plate-img-wrap`/`.hero-frame`/`.is-active` CSS/JS are reused completely unchanged — only their container's position in the page moves. Do not rename these classes; there is no naming collision with the new `.hero-grid`.

---

### Task 1: Hexagon hero grid + relocated photo section

**Files:**
- Modify: `Plocica/wwwroot/css/components.css:43-108` (add `.hero-grid` styles, remove dead `.plate-caption` rules)
- Create: `Plocica/wwwroot/js/hero-grid.js`
- Modify: `Plocica/Pages/Index.cshtml:11-37` (hero markup) and `Plocica/Pages/Index.cshtml:142-145` (Scripts section)

**Interfaces:**
- Produces: a `.hero-grid` container element that `hero-grid.js` finds via `document.querySelector(".hero-grid")` and fills with a `<canvas>` it creates itself. No other file depends on anything this task exports.
- Consumes: CSS custom properties `--line` and `--glz-sun`, already defined in `Plocica/wwwroot/css/tokens.css:9,15`.

- [ ] **Step 1: Update `components.css` — remove the caption, add the hero-grid panel**

Read the current block first (`components.css:43-108`) — it contains `.plate-frame`, `.plate-img-wrap`, `.hero-frame`, `.plate-caption`, a mobile media query, and a reduced-motion media query. `.plate-caption` is going away (it was tied to a specific DB shape's name/price, which no longer applies once the hero shows an abstract visual instead of one specific tile photo). Replace that whole block (lines 43–108) with:

```css
.plate-frame {
  position: relative;
}

.plate-frame .plate-img-wrap {
  position: relative;
  width: 100%;
  height: min(66vh, 620px);
  opacity: 0;
  transform: scale(1.04);
  transition: opacity 0.9s ease, transform 1.1s ease;
}

.plate-frame.is-revealed .plate-img-wrap {
  opacity: 1;
  transform: scale(1);
}

/* carousel — crossfade između slika iz /img/hero, prirodni omjer, bez izrezivanja */
.hero-frame {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  object-fit: contain;
  object-position: center;
  opacity: 0;
  transition: opacity 1.2s ease;
}

.hero-frame.is-active { opacity: 1; }

/* hero hexagon grid — hover-fill canvas, replaces the old hero photo slot */
.hero-grid {
  position: relative;
  aspect-ratio: 1 / 1;
  border: 1px solid var(--line);
  overflow: hidden;
}

.hero-grid canvas {
  display: block;
  width: 100%;
  height: 100%;
}

@media (max-width: 860px) {
  .hero { grid-template-columns: 1fr; }
  .plate-frame .plate-img-wrap { height: min(50vh, 440px); }
  .hero-grid { max-width: min(80vw, 480px); margin-inline: auto; }
}

@media (prefers-reduced-motion: reduce) {
  .plate-frame .plate-img-wrap,
  .hero-frame {
    transition: none !important;
    opacity: 1 !important;
    transform: none !important;
  }
}
```

This drops `.plate-caption` and the two rules that referenced it, keeps `.plate-frame`/`.hero-frame` byte-for-byte identical (just relocated in the file next to their new neighbor), and adds the new `.hero-grid` panel + its mobile sizing.

- [ ] **Step 2: Create `Plocica/wwwroot/js/hero-grid.js`**

```js
(function () {
  var container = document.querySelector(".hero-grid");
  if (!container) return;

  var canvas = document.createElement("canvas");
  canvas.setAttribute("aria-hidden", "true");
  container.appendChild(canvas);
  var ctx = canvas.getContext("2d");
  if (!ctx) return;

  var rootStyle = getComputedStyle(document.documentElement);
  var borderColor = rootStyle.getPropertyValue("--line").trim() || "#C9C4B8";
  var hoverFillColor = rootStyle.getPropertyValue("--glz-sun").trim() || "#E3B23C";
  var squareSize = 45;
  var hoverTrailAmount = 6;

  var hexHoriz = squareSize * 1.5;
  var hexVert = squareSize * Math.sqrt(3);

  var gridOffset = { x: 0, y: 0 };
  var hoveredCell = null;
  var trailCells = [];
  var cellOpacities = new Map();

  var isTouch = window.matchMedia("(hover: none)").matches;
  var reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  function resizeCanvas() {
    canvas.width = canvas.offsetWidth;
    canvas.height = canvas.offsetHeight;
  }

  function drawHex(cx, cy, size) {
    ctx.beginPath();
    for (var i = 0; i < 6; i++) {
      var angle = (Math.PI / 3) * i;
      var vx = cx + size * Math.cos(angle);
      var vy = cy + size * Math.sin(angle);
      if (i === 0) ctx.moveTo(vx, vy);
      else ctx.lineTo(vx, vy);
    }
    ctx.closePath();
  }

  function drawGrid() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    var colShift = Math.floor(gridOffset.x / hexHoriz);
    var offsetX = ((gridOffset.x % hexHoriz) + hexHoriz) % hexHoriz;
    var offsetY = ((gridOffset.y % hexVert) + hexVert) % hexVert;

    var cols = Math.ceil(canvas.width / hexHoriz) + 3;
    var rows = Math.ceil(canvas.height / hexVert) + 3;

    for (var col = -2; col < cols; col++) {
      for (var row = -2; row < rows; row++) {
        var cx = col * hexHoriz + offsetX;
        var cy = row * hexVert + ((col + colShift) % 2 !== 0 ? hexVert / 2 : 0) + offsetY;

        var cellKey = col + "," + row;
        var alpha = cellOpacities.get(cellKey);
        if (alpha) {
          ctx.globalAlpha = alpha;
          drawHex(cx, cy, squareSize);
          ctx.fillStyle = hoverFillColor;
          ctx.fill();
          ctx.globalAlpha = 1;
        }

        drawHex(cx, cy, squareSize);
        ctx.strokeStyle = borderColor;
        ctx.stroke();
      }
    }
  }

  function seedStaticCells() {
    var seeds = [
      { x: 1, y: 1 }, { x: 3, y: 0 }, { x: 2, y: 3 },
      { x: 5, y: 2 }, { x: 0, y: 4 }, { x: 4, y: 4 }
    ];
    seeds.forEach(function (c) {
      cellOpacities.set(c.x + "," + c.y, 0.85);
    });
  }

  function pointToCell(clientX, clientY) {
    var rect = canvas.getBoundingClientRect();
    var mouseX = clientX - rect.left;
    var mouseY = clientY - rect.top;

    var colShift = Math.floor(gridOffset.x / hexHoriz);
    var offsetX = ((gridOffset.x % hexHoriz) + hexHoriz) % hexHoriz;
    var offsetY = ((gridOffset.y % hexVert) + hexVert) % hexVert;
    var adjustedX = mouseX - offsetX;
    var adjustedY = mouseY - offsetY;

    var col = Math.round(adjustedX / hexHoriz);
    var rowOffset = (col + colShift) % 2 !== 0 ? hexVert / 2 : 0;
    var row = Math.round((adjustedY - rowOffset) / hexVert);
    return { x: col, y: row };
  }

  resizeCanvas();
  window.addEventListener("resize", function () {
    resizeCanvas();
    drawGrid();
  });

  // Touch devices get a static, pre-glazed-looking pattern — there is no
  // hover to drive the interactive fill, and the site's chosen fallback
  // is decorative rather than tap-to-fill.
  if (isTouch) {
    seedStaticCells();
    drawGrid();
    return;
  }

  // Reduced motion: keep hover as direct interaction feedback (snap fill,
  // no trail, no continuous loop) but drop the ambient drift entirely.
  if (reduceMotion) {
    canvas.addEventListener("mousemove", function (event) {
      var cell = pointToCell(event.clientX, event.clientY);
      if (!hoveredCell || hoveredCell.x !== cell.x || hoveredCell.y !== cell.y) {
        if (hoveredCell) cellOpacities.delete(hoveredCell.x + "," + hoveredCell.y);
        hoveredCell = cell;
        cellOpacities.set(cell.x + "," + cell.y, 1);
        drawGrid();
      }
    });
    canvas.addEventListener("mouseleave", function () {
      if (hoveredCell) cellOpacities.delete(hoveredCell.x + "," + hoveredCell.y);
      hoveredCell = null;
      drawGrid();
    });
    drawGrid();
    return;
  }

  function updateCellOpacities() {
    var targets = new Map();

    if (hoveredCell) {
      targets.set(hoveredCell.x + "," + hoveredCell.y, 1);
    }

    for (var i = 0; i < trailCells.length; i++) {
      var t = trailCells[i];
      var key = t.x + "," + t.y;
      if (!targets.has(key)) {
        targets.set(key, (trailCells.length - i) / (trailCells.length + 1));
      }
    }

    targets.forEach(function (_, key) {
      if (!cellOpacities.has(key)) cellOpacities.set(key, 0);
    });

    cellOpacities.forEach(function (opacity, key) {
      var target = targets.get(key) || 0;
      var next = opacity + (target - opacity) * 0.15;
      if (next < 0.005) cellOpacities.delete(key);
      else cellOpacities.set(key, next);
    });
  }

  function updateAnimation() {
    // effective speed floors at 0.1 even though squareGrid speed is
    // nominally 0 — matches react-bits' own ShapeGrid behavior, produces
    // a barely-perceptible drift rather than a fully frozen grid.
    gridOffset.y = (gridOffset.y - 0.1 + hexVert) % hexVert;
    updateCellOpacities();
    drawGrid();
    requestAnimationFrame(updateAnimation);
  }

  canvas.addEventListener("mousemove", function (event) {
    var cell = pointToCell(event.clientX, event.clientY);
    if (!hoveredCell || hoveredCell.x !== cell.x || hoveredCell.y !== cell.y) {
      if (hoveredCell) {
        trailCells.unshift({ x: hoveredCell.x, y: hoveredCell.y });
        if (trailCells.length > hoverTrailAmount) trailCells.length = hoverTrailAmount;
      }
      hoveredCell = cell;
    }
  });

  canvas.addEventListener("mouseleave", function () {
    if (hoveredCell) {
      trailCells.unshift({ x: hoveredCell.x, y: hoveredCell.y });
      if (trailCells.length > hoverTrailAmount) trailCells.length = hoverTrailAmount;
    }
    hoveredCell = null;
  });

  requestAnimationFrame(updateAnimation);
})();
```

- [ ] **Step 3: Update the hero markup in `Index.cshtml`**

Replace lines 21–36 (the `<div>` wrapping `.plate-frame` and `.plate-caption`) with:

```html
    <div class="hero-grid" aria-hidden="true"></div>
```

So the full hero section (lines 11–37) becomes:

```html
<section class="container hero">
    <div class="hero-copy">
        <h1>Gdje detalji postaju ono što svi primjete.</h1>
        <p>Svaka pločica prolazi kroz ručni proces izrade. Male razlike u teksturi i nijansama dio su procesa. To je ono što svaku pločicu čini jedinstvenom.</p>
        <div class="hero-actions">
            <a class="btn btn-cta" href="@SiteLinks.ContactForm" target="_blank" rel="noopener noreferrer">Stvorimo nešto zajedno</a>
            <a class="text-link" asp-page="/Kolekcije">Kolekcije</a>
        </div>
    </div>

    <div class="hero-grid" aria-hidden="true"></div>
</section>
```

- [ ] **Step 4: Add the relocated photo section right after the hero**

Immediately after the hero section's closing `</section>` and before the `<section class="section section-alt hairline-top hairline-bottom accent-clay">` manifesto section, insert:

```html
<section class="section hairline-top">
    <div class="container">
        <p class="eyebrow">Izbliza</p>
        <h2>Iz naše ponude</h2>
        <div class="plate-frame">
            <div class="plate-img-wrap">
                <img class="hero-frame is-active" src="~/img/hero/arabeskHERO.jpg" alt="Arabesque pločice u ružičastoj glazuri.">
                <img class="hero-frame" src="~/img/hero/curveHERO.jpg" alt="Curve pločice u narančastoj glazuri." loading="lazy">
                <img class="hero-frame" src="~/img/hero/curve1HERO.jpg" alt="Curve pločice u tamnozelenoj i bež glazuri." loading="lazy">
                <img class="hero-frame" src="~/img/hero/kombinacijaOblikaHERO.jpg" alt="Kombinacija oblika u bež i tamnozelenoj glazuri." loading="lazy">
                <img class="hero-frame" src="~/img/hero/lineaHERO.jpg" alt="Linea pločice u bež glazuri." loading="lazy">
                <img class="hero-frame" src="~/img/hero/moduleHERO.jpg" alt="Module pločice u tamnoplavoj i bijeloj glazuri." loading="lazy">
            </div>
        </div>
    </div>
</section>
```

This is the same six images, same classes, same crossfade markup that used to live in the hero — `hero.js` needs no changes since it selects `.plate-frame` by class regardless of where it sits in the page.

- [ ] **Step 5: Register the new script**

In the `@section Scripts { ... }` block at the bottom of `Index.cshtml` (currently lines 142–145), add the new script tag alongside the existing ones:

```html
@section Scripts {
    <script src="~/js/hero-grid.js" asp-append-version="true"></script>
    <script src="~/js/hero.js" asp-append-version="true"></script>
    <script src="~/js/craft-video.js" asp-append-version="true"></script>
}
```

- [ ] **Step 6: Manual verification — desktop**

Run `dotnet run` from `Plocica/`, open the homepage in a browser. Confirm:
- A hairline-bordered square hexagon grid appears where the photo carousel used to be, in the hero's right column.
- Hovering over it fills the hexagon under the cursor in the warm gold accent color (`--glz-sun`), with a fading trail of a few hexagons behind the cursor path as it moves.
- Scrolling down past the hero shows a new "Izbliza / Iz naše ponude" section with the same six-photo crossfade that used to be in the hero, cycling every ~3 seconds.

- [ ] **Step 7: Manual verification — touch/mobile**

In the browser's device toolbar (touch emulation on, e.g. a phone preset), reload the page. Confirm:
- The hexagon grid shows a handful of hexagons already filled in the gold accent color, scattered rather than in an obvious row/pattern, with no console errors.
- There is no interactivity expected here (no tap-to-fill) — this is the intended static fallback.
- The layout stacks to one column below 860px width with no overlap or clipping.

- [ ] **Step 8: Manual verification — reduced motion**

Using the browser DevTools rendering panel, emulate `prefers-reduced-motion: reduce`, then reload (with device toolbar/touch emulation OFF, i.e. a normal desktop mouse context). Confirm:
- The grid shows no ambient drift (hexagons don't slowly scroll).
- Hovering still instantly fills the hexagon under the cursor (no trail, no fade-in animation — it should snap on/off).

- [ ] **Step 9: Manual verification — resize**

With reduced-motion and touch emulation both off, resize the browser window across the 860px breakpoint a few times. Confirm the hexagon grid resizes cleanly with the panel (no stretched/blurry canvas, no layout shift, no JS errors in the console).

---

### Task 2: Remove the now-dead `HeroShape` lookup

**Files:**
- Modify: `Plocica/Pages/Index.cshtml.cs:20,27`

**Interfaces:**
- Consumes: nothing new.
- Produces: nothing — this is a cleanup task with no other task depending on it. It must run after Task 1, since Task 1 is what removes the last view reference to `Model.HeroShape` (the deleted `.plate-caption` markup was the only place it was read).

- [ ] **Step 1: Remove the property and its query**

In `Plocica/Pages/Index.cshtml.cs`, delete line 20 (`public Shape? HeroShape { get; set; }`) and line 27 (`HeroShape = _db.Shapes.FirstOrDefault(s => s.Name == "Arabesque");`) so `OnGet` becomes:

```csharp
public void OnGet()
{
    ObliciShapes = _db.Shapes.Where(s => s.Collection == "oblici").OrderBy(s => s.SortOrder).ToList();
    OslikaneShapes = _db.Shapes.Where(s => s.Collection == "oslikane").OrderBy(s => s.SortOrder).ToList();
    ReljefneShapes = _db.Shapes.Where(s => s.Collection == "reljefne").OrderBy(s => s.SortOrder).ToList();
}
```

- [ ] **Step 2: Verify the build and page still work**

Run `dotnet build` from `Plocica/` — expect a clean build with no errors (nothing else in the codebase references `IndexModel.HeroShape`). Run `dotnet run`, reload the homepage, confirm it renders exactly as it did at the end of Task 1 (no visual regression, no server error).
