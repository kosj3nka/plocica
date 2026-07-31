(function () {
  var container = document.querySelector(".hero-grid");
  if (!container) return;

  var canvas = document.createElement("canvas");
  canvas.setAttribute("aria-hidden", "true");
  container.appendChild(canvas);
  var ctx = canvas.getContext("2d");
  if (!ctx) return;

  var rootStyle = getComputedStyle(document.documentElement);
  var containerStyle = getComputedStyle(container);
  var borderColor = rootStyle.getPropertyValue("--line").trim() || "#C9C4B8";
  var hoverFillColor = containerStyle.getPropertyValue("--hex-hover").trim() || "#D3BD86";
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
    var dpr = window.devicePixelRatio || 1;
    canvas.width = canvas.offsetWidth * dpr;
    canvas.height = canvas.offsetHeight * dpr;
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
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
    var width = canvas.offsetWidth;
    var height = canvas.offsetHeight;
    ctx.clearRect(0, 0, width, height);

    var colShift = Math.floor(gridOffset.x / hexHoriz);
    var offsetX = ((gridOffset.x % hexHoriz) + hexHoriz) % hexHoriz;
    var offsetY = ((gridOffset.y % hexVert) + hexVert) % hexVert;

    var cols = Math.ceil(width / hexHoriz) + 3;
    var rows = Math.ceil(height / hexVert) + 3;

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
