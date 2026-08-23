(function () {
  var tiles = document.querySelector(".hero-tiles");
  var canvas = tiles && tiles.querySelector(".hero-tiles-canvas");
  if (!tiles || !canvas) return;

  var COLS = 19; // matches the real tile grid photographed in tilesHero.jpg (verified via pixel-edge autocorrelation)
  var ROWS = 6;
  var RADIUS_FACTOR = 2.8; // splash radius = this * sqrt(cellWidth * cellHeight)
  var JITTER_STRENGTH = 0.32; // fraction of radius, breaks a clean circle into an uneven splash
  var LERP_RATE = 0.12; // per-frame ease toward target opacity (drives both rise and the trail's fade)
  var OPACITY_EPSILON = 0.004;

  var ctx = canvas.getContext("2d");
  var img = new Image();
  img.src = tiles.dataset.heroTilesSrc || "";

  var reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  var ready = false;
  var rectWidth = 0, rectHeight = 0;
  var cellWidth = 0, cellHeight = 0;
  var dpr = 1;
  var splashRadius = 0;

  var pointer = null; // { x, y } in local coords, or null when not hovering
  var cellOpacities = new Map(); // "col_row" -> 0..1
  var rafId = null;

  // Mirrors the .hero-tiles::before breakpoint in components.css: below
  // 860px the dull background switches from a height-locked scale (full
  // image width shown, repeated) to a scale showing 8 of the 19 columns
  // across the viewport width, with height still locked to 100% (all 6
  // rows, no vertical crop). The canvas grid has to use the same column
  // count or the hover-reveal drifts off the dull photo underneath it.
  var MOBILE_QUERY = window.matchMedia("(max-width: 860px)");

  // Deterministic per-cell pseudo-random offset in [-1, 1], stable across
  // frames so the splash's uneven edge stays put rather than shimmering.
  function jitter(col, row) {
    var n = Math.sin(col * 12.9898 + row * 78.233) * 43758.5453;
    return (n - Math.floor(n)) * 2 - 1;
  }

  function clamp01(v) {
    return v < 0 ? 0 : v > 1 ? 1 : v;
  }

  function measure() {
    var rect = tiles.getBoundingClientRect();
    rectWidth = rect.width;
    rectHeight = rect.height;
    dpr = window.devicePixelRatio || 1;

    canvas.width = Math.max(1, Math.round(rectWidth * dpr));
    canvas.height = Math.max(1, Math.round(rectHeight * dpr));
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);

    var renderedImgWidth = MOBILE_QUERY.matches
      ? rectWidth * (COLS / 8)
      : img.naturalWidth * (rectHeight / img.naturalHeight);
    cellWidth = renderedImgWidth / COLS;
    cellHeight = rectHeight / ROWS;
    splashRadius = RADIUS_FACTOR * Math.sqrt(cellWidth * cellHeight);

    pointer = null;
    cellOpacities.clear();
    render();
  }

  function render() {
    if (!ready) return;
    ctx.clearRect(0, 0, rectWidth, rectHeight);
    if (cellOpacities.size === 0) return;

    var srcCellW = img.naturalWidth / COLS;
    var srcCellH = img.naturalHeight / ROWS;

    cellOpacities.forEach(function (opacity, key) {
      var parts = key.split("_");
      var col = parseInt(parts[0], 10);
      var row = parseInt(parts[1], 10);
      var sourceCol = ((col % COLS) + COLS) % COLS;

      ctx.globalAlpha = opacity;
      ctx.drawImage(
        img,
        sourceCol * srcCellW, row * srcCellH, srcCellW, srcCellH,
        col * cellWidth, row * cellHeight, cellWidth, cellHeight
      );
    });
    ctx.globalAlpha = 1;
  }

  // Every cell within splashRadius (in real screen pixels, not grid-cell
  // units) of the pointer gets a target opacity that falls off linearly
  // with distance from its center — a soft, multi-tile splash rather than
  // a single hard-edged cell, sized so it doesn't blow out across all 6
  // rows the way a grid-cell-unit radius did with these tall, narrow tiles.
  function computeTargets() {
    var targets = new Map();
    if (!pointer) return targets;

    var centerCol = Math.floor(pointer.x / cellWidth);
    var centerRow = Math.floor(pointer.y / cellHeight);
    var colSpan = Math.ceil(splashRadius / cellWidth) + 1;
    var rowSpan = Math.ceil(splashRadius / cellHeight) + 1;

    // The grid only has ROWS rows of real image content — there's nothing
    // above row 0 or below the last row. A full-size splash hovered near
    // the top/bottom edge would otherwise get sliced flat by that boundary
    // (like a circle cut by a wall). Shrink the splash's own radius as the
    // pointer nears an edge so it tapers away gracefully instead.
    var edgeDist = Math.min(pointer.y, rectHeight - pointer.y);
    var edgeScale = Math.max(clamp01(edgeDist / splashRadius), 0.55);
    var radius = splashRadius * edgeScale;

    for (var row = centerRow - rowSpan; row <= centerRow + rowSpan; row++) {
      if (row < 0 || row >= ROWS) continue;
      for (var col = centerCol - colSpan; col <= centerCol + colSpan; col++) {
        var cellCx = col * cellWidth + cellWidth / 2;
        var cellCy = row * cellHeight + cellHeight / 2;
        var dx = cellCx - pointer.x;
        var dy = cellCy - pointer.y;
        var dist = Math.sqrt(dx * dx + dy * dy) + jitter(col, row) * radius * JITTER_STRENGTH;
        var target = clamp01(1 - dist / radius);
        if (target > 0) targets.set(col + "_" + row, target);
      }
    }
    return targets;
  }

  // Lerping every cell toward a target computed fresh from the *current*
  // pointer position (rather than tracking hover history) gives the trail
  // for free: cells the cursor has left simply lerp their target down to 0
  // and fade out over a few frames instead of snapping off.
  function tick() {
    var targets = computeTargets();

    targets.forEach(function (_, key) {
      if (!cellOpacities.has(key)) cellOpacities.set(key, 0);
    });

    cellOpacities.forEach(function (opacity, key) {
      var target = targets.get(key) || 0;
      var next = opacity + (target - opacity) * LERP_RATE;
      if (next < OPACITY_EPSILON && target === 0) {
        cellOpacities.delete(key);
      } else {
        cellOpacities.set(key, next);
      }
    });

    render();
    rafId = (cellOpacities.size > 0 || pointer) ? requestAnimationFrame(tick) : null;
  }

  function ensureLoop() {
    if (rafId === null) rafId = requestAnimationFrame(tick);
  }

  function localPoint(clientX, clientY) {
    var rect = tiles.getBoundingClientRect();
    return { x: clientX - rect.left, y: clientY - rect.top };
  }

  function moveAnimated(e) {
    pointer = localPoint(e.clientX, e.clientY);
    ensureLoop();
  }

  function leaveAnimated() {
    pointer = null;
    ensureLoop();
  }

  function moveReduced(e) {
    pointer = localPoint(e.clientX, e.clientY);
    cellOpacities = computeTargets();
    render();
  }

  function leaveReduced() {
    pointer = null;
    cellOpacities.clear();
    render();
  }

  var onMove = reduceMotion ? moveReduced : moveAnimated;
  var onLeave = reduceMotion ? leaveReduced : leaveAnimated;

  tiles.addEventListener("pointerdown", onMove);
  tiles.addEventListener("pointermove", onMove);
  tiles.addEventListener("pointerleave", onLeave);
  tiles.addEventListener("pointercancel", onLeave);
  tiles.addEventListener("pointerup", function (e) {
    if (e.pointerType === "touch") onLeave();
  });

  function start() {
    ready = true;
    measure();
    if (window.ResizeObserver) {
      new ResizeObserver(measure).observe(tiles);
    } else {
      window.addEventListener("resize", measure);
    }
  }

  if (img.complete && img.naturalWidth) {
    start();
  } else {
    img.addEventListener("load", start);
  }
})();
