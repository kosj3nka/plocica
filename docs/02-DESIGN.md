# 02 — Dizajn sustav (tokeni)

Smjer: **tehnički crtež koji se ispunjava glazurom.** Čisto, arhitektonski, hairline linije i indeksiranje (studio doslovno numerira boje 001–028 i tehnički specificira svaki oblik — to je istina o sadržaju, ne dekoracija). Boja dolazi od samih glazura, ne od jednog „brand accenta".

Namjerno izbjegavamo AI-default izgled (topla krem pozadina + serif + terakota accent). Umjesto toga: hladnija papir-bijela, tamna tinta, a keramičke boje (sage, clay, indigo, warm sun) su akcenti koji dolaze iz sadržaja.

## Boje (CSS varijable)

```css
:root{
  --paper:      #F2F0EA;  /* papir-bijela, blago topla ali ne krem */
  --paper-2:    #E9E6DD;  /* sekcije / kartice */
  --ink:        #1A1A17;  /* tekst, linije — skoro crna, topli ton */
  --ink-soft:   #55534C;  /* sekundarni tekst */
  --line:       #C9C4B8;  /* hairline linije (tehnički crtež) */

  /* glazure iz kataloga — akcenti, koristiti STRIDNO, jedna po sekciji */
  --glz-sage:   #7C8A5B;
  --glz-clay:   #C06B4A;
  --glz-indigo: #2E3A56;
  --glz-sun:    #E3B23C;
  --glz-rose:   #C98A94;
}
```

Pravilo: svaka sekcija bira NAJVIŠE jednu glazuru kao accent (npr. numeracija, linija ispod naslova). Tekst i strukture su uvijek `--ink` na `--paper`.

## Tipografija

Dvije uloge. Display = grotesk s karakterom (arhitektonski, ide uz logo). Body/tehnika = čist humanist sans.

- **Display:** `Fraunces` u „Soft"/optičkom velikom stanju NE — previše serif/AI-default. Umjesto: **`Archivo`** ili **`Space Grotesk`** za naslove (širok, tehnički grotesk). Preferiraj **Archivo** (ima jaku expanded varijantu za velike naslove).
- **Body / tehničke informacije:** **`Inter`** ili **`IBM Plex Sans`**. Preferiraj **IBM Plex Sans** — ima inženjerski ton koji paše „tehničkom crtežu", uključujući tabelarne brojke za cijene i dimenzije.
- Učitaj preko Google Fonts (`display=swap`). Utility/caption: isti Plex u manjem stupnju s `letter-spacing`.

Type scale (desktop): hero display clamp(3.5rem, 8vw, 7rem); H2 clamp(2rem,4vw,3.25rem); body 1.0625rem/1.6; caption 0.75rem uppercase +0.08em tracking.

## Layout

- Široki vanjski margini, sadržaj u gridu s **vidljivim hairline linijama** kao na nacrtu (tanke `--line` granice između redaka spec tablica, oko kartica boja).
- Numeracija je stvarna: boje `001–028`, kolekcije `01/02/03`, proces narudžbe `01–05`. Koristi tabelarne brojke.
- Border-radius: minimalan. 0 za tehničke elemente; blagi 2–4px samo na fotkama ako treba.
- Karta boja: grid kvadrata (7 u redu na desktopu, 3–4 na mobitelu), svaki s brojem ispod — točno kao katalog str. 7.

## Signature element

**Hairline „nacrt" okvir oko hero medija** + numerirani indeks kroz cijelu stranicu. Hero je jedini glasan trenutak (sketch→ceramic fill). Sve ostalo tiho i disciplinirano.

## Quality floor (obavezno)

Responsive do mobitela · vidljiv keyboard focus · `prefers-reduced-motion` poštovan (hero prelazi na statičnu sliku) · alt tekstovi na svim fotkama · kontrast AA.
