# Pločica — Web stranica (projektni brief)

Studio za ručno rađene keramičke pločice. Duo: Lorena (umjetnički pristup — forma, boja, kompozicija) i Mihael (proizvodni proces, tehnička izvedba). Zagreb.

Tagline: **„Gdje detalji postaju ono što svi primjete."**
Kontakt: plocica.info@gmail.com · @plocica_

---

## Cilj stranice

1. Predstaviti studio i proizvod (ručno rađene pločice) na način koji je **kreativan ali arhitektonski** — čist, uredan, poput tehničkog crteža koji se ispunjava glazurom.
2. Prikazati ponudu koju **vlasnici uređuju kroz admin panel** (`/admin`, iza logina): oblici, boje i projekti — bez diranja koda.
3. **Kontakt forma** (Google Form embed) preko koje upiti stižu ravno na mail i automatski se sortiraju u Gmailu.

## Tehnologija

- **ASP.NET Core Razor Pages** (.NET 8 LTS)
- Hosting: **Azure App Service** (Linux, B1)
- **Baza: Azure SQL** (Basic tier) preko **Entity Framework Core**. Perzistentna, preživljava deploy i restart.
- **Slike: Azure Blob Storage** (upload iz admin panela). NE spremati slike na lokalni disk App Servicea — briše se pri deployu.
- **Auth: ASP.NET Core Identity** (cookie), jedan admin račun (username + password). Vidi `07-ADMIN.md`.
- Bez JS frameworka. Vanilla JS za hero animaciju i sitne interakcije.

## Zašto baza + admin panel (a ne JSON)

Vlasnici uređuju tri stvari: oblike, boje, projekte — uključujući **upload slika**. To traži pravo perzistentno spremište i sučelje za uređivanje bez koda. JSON-u-repou model bi tražio git push za svaku izmjenu i ne rješava upload slika, pa je ovdje neprikladan. Azure SQL + Blob + admin panel daje vlasnicima da se uloguju na `/admin`, urede sadržaj i uploadaju slike; promjene su odmah uživo, bez deploya.

**Napomena o Azure disku:** Azure App Service ima efemeran lokalni disk (briše se pri deployu/restartu). Zato baza mora biti Azure SQL (zasebna), a slike u Blob Storage — nikad SQLite datoteka ili slike na lokalnom disku, jer bi se izgubile.

### Alternativa (jeftinije)
Ako je trošak Azure SQL-a (~5€/mj) problem: **Supabase** (Postgres + Storage, besplatan tier) radi isto, ali baza je izvan Azurea (jedna ovisnost više). Preporuka ostaje Azure SQL + Blob jer drži sve na jednom računu. Odluka zabilježena u `05-AZURE.md`.

## Stranice

### Javne (Razor Pages)
| Ruta | Datoteka | Sadržaj | Izvor |
|------|----------|---------|-------|
| `/` | `Index.cshtml` | Home. Hero, intro, izbor kolekcija, preview karte boja, CTA. | DB |
| `/kolekcije` | `Kolekcije.cshtml` | Oblici + oslikane + reljefne s tehničkim info i cijenama **+ sekcija Karta boja**. | DB |
| `/radovi` | `Radovi.cshtml` | Projekti (Kuća Heinzel, Crkva sv. Mirka, …). | DB |
| `/o-nama` | `ONama.cshtml` | Lorena i Mihael, proces, mediji. | statično (`06-COPY.md`) |
| `/faq` | `Faq.cshtml` | Najčešća pitanja. | statično (`06-COPY.md`) |
| `/kontakt` | `Kontakt.cshtml` | Google Form embed + mail + Instagram + proces narudžbe. | statično + `04-FORMA.md` |

**Navigacija:** Kolekcije (dropdown) · Radovi · O nama · FAQ, + gumb „Stvorimo nešto zajedno" → `/kontakt`. Detalji dropdowna u `06-COPY.md`.

### Admin (iza logina) — vidi `07-ADMIN.md`
| Ruta | Sadržaj |
|------|---------|
| `/admin/login` | Prijava (username + password). |
| `/admin` | Nadzorna ploča — prečaci na Oblike, Boje, Projekte. |
| `/admin/oblici` | CRUD oblika (+ upload slike). |
| `/admin/boje` | CRUD boja (slika ILI color picker). |
| `/admin/projekti` | CRUD projekata (+ više slika). |

Podaci javnih stranica i admina dijele iste EF Core modele (`01-CONTENT.md`).

## Redoslijed izrade (za Claude Code)

1. Scaffold Razor Pages + `_Layout` + navigacija + CSS token sustav (`02-DESIGN.md`).
2. **EF Core modeli + Azure SQL + migracije + seed** početnih podataka iz kataloga (`01-CONTENT.md`).
3. Javne stranice koje čitaju iz baze: Home + `/kolekcije` (oblici, oslikane, reljefne, karta boja).
4. `/radovi`, `/o-nama`, `/faq`.
5. **Auth + admin panel** — login, CRUD za oblike/boje/projekte, upload na Blob Storage (`07-ADMIN.md`).
6. `/kontakt` s Google Form embedom (`04-FORMA.md`).
7. Deploy na Azure — App Service + SQL + Blob + tajne (`05-AZURE.md`).

Detalji svake faze u pratećim .md datotekama.
