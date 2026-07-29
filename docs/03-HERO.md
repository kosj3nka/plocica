# 03 — Hero (animirani element: sketch → ceramic)

Koncept: framing pločice/zida koji se ispunjava — od skice do gotove keramike. Cilj je „wow" u prvih 3 sekunde.

## Ključna odluka: swappable media slot

Hero je JEDAN media slot koji prima ILI niz slika (crossfade) ILI video. Kreće se sa slikama; video se doda kasnije BEZ prepravljanja komponente. Razlog: slike su lako producirati (jedno popodne snimanja), nikad se ne „lome" na mobitelu, i daju kontrolu nad tajmingom svake faze.

### Zlatno pravilo snimanja (reci vlasnicima)
**Fotoaparat na stativu, ne pomicati ga između faza.** Ako je kadar identičan od skice do gotove pločice, crossfade izgleda kao da se keramika materijalizira na mjestu — to je efekt. Ako kamera odluta između snimaka, nijedan prijelaz to ne spašava. Ta jedna disciplina je važnija od izbora slike-vs-video.

## Varijanta A — crossfade slika (v1, default)

- 3–5 stillova iste kompozicije: (1) skica/crtež, (2) prva glazura, (3) pola ispunjeno, (4) gotovo.
- Slažu se u istom kontejneru, mijenja se `opacity` s `transition: opacity 1.2s ease`.
- Svaka faza stoji ~2,5 s. Ciklus se vrti (loop) ili se zaustavi na zadnjoj (preferiraj: zaustavi na gotovoj pločici nakon jednog prolaza, pa suptilno lagani loop).
- Preload svih slika. Prva slika je `<img>` odmah u HTML-u (LCP), ostale se dodaju.
- Dimenzije: fiksni `aspect-ratio` kontejner da nema layout shifta. Slike `object-fit: cover`.

### Markup (referenca)
```html
<div class="hero-media" aria-label="Proces izrade pločice od skice do gotove keramike">
  <img class="hero-frame is-active" src="/img/hero/01-skica.jpg" alt="Skica motiva pločice">
  <img class="hero-frame" src="/img/hero/02-glazura.jpg" alt="Nanošenje prve glazure" loading="lazy">
  <img class="hero-frame" src="/img/hero/03-pola.jpg"    alt="Pločica dopola ispunjena" loading="lazy">
  <img class="hero-frame" src="/img/hero/04-gotovo.jpg"  alt="Gotova glazirana pločica" loading="lazy">
</div>
```
```css
.hero-media{position:relative;aspect-ratio:16/10;overflow:hidden}
.hero-frame{position:absolute;inset:0;width:100%;height:100%;object-fit:cover;opacity:0;transition:opacity 1.2s ease}
.hero-frame.is-active{opacity:1}
@media (prefers-reduced-motion: reduce){
  .hero-frame{transition:none} /* JS: samo prikaži zadnji frame, bez ciklusa */
}
```
JS: interval koji rotira `.is-active` klasu. Ako `matchMedia('(prefers-reduced-motion: reduce)')` → prikaži samo zadnji frame, bez intervala.

## Varijanta B — video (kasnije, ako se snimi dobar materijal)

Isti slot, zamijeni `<img>` niz jednim `<video>`:
```html
<video class="hero-video" autoplay muted loop playsinline
       poster="/img/hero/poster.jpg">
  <source src="/img/hero/proces.mp4" type="video/mp4">
  <img src="/img/hero/poster.jpg" alt="Proces izrade pločice"> <!-- fallback -->
</video>
```
Zahtjevi da autoplay radi na mobitelu: `muted` + `playsinline` OBAVEZNO. Kompresiraj na ~1080p, cilj < 3–4 MB. `poster` je prvi frame (nema crnog blica). Uvijek imaj statični fallback.

## Varijanta C — scroll-scrub (NE u v1)

Niz frameova koji napreduju na scroll (korisnik „ispunjava" pločicu skrolanjem). Najefektnije, ali traži lockani kadar i puno ravnomjernih frameova. Spomenuti vlasnicima kao buduću nadogradnju, ne graditi sad.

## Preporuka

Gradi **Varijantu A** sada, slot spreman za B. Ne diraj C dok nema materijala. Placeholder do pravih fotki: 3–4 slike keramike iz kataloga (npr. ista arabesque tekstura u različitim faznim izrezima) da se vidi mehanika — jasno označiti kao privremeno u `UPUTE-ZA-VLASNIKE.md`.
