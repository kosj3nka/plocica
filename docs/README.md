# Pločica — web stranica (paket)

Sve za izradu stranice. Arhitektura: **ASP.NET Core Razor Pages + Azure SQL (EF Core) + Azure Blob Storage**, hostano na Azure App Service. Sadržaj (oblici, boje, projekti) uređuje se kroz **admin panel na `/admin`** iza logina — bez diranja koda.

Redoslijed čitanja / davanja Claude Codeu:

1. **00-PROJECT.md** — arhitektura, tehnologija, mapa stranica (javne + admin), redoslijed izrade.
2. **01-CONTENT.md** — EF Core entiteti (Shape, ColorItem, Project, ProjectImage), DbContext, seed podaci iz kataloga.
3. **02-DESIGN.md** — dizajn tokeni (boje, tipografija, layout).
4. **03-HERO.md** — animacija naslovne (sketch → ceramic), swappable media slot.
5. **04-FORMA.md** — Google Form embed + Gmail sortiranje upita.
6. **07-ADMIN.md** — admin panel: login (jedan račun), CRUD oblika/boja/projekata, upload slika u Blob.
7. **05-AZURE.md** — deploy: App Service + SQL + Blob + tajne/connection stringovi.
8. **06-COPY.md** — svi tekstovi (hrvatski) + navigacija i Kolekcije dropdown.

**plocica-mockup.html** — vizualni mockup naslovne. Otvori u pregledniku. Referenca za izgled javnog dijela.

## Kako krenuti s Claude Codeom
Daj mu `00-PROJECT.md` prvo, pa ga vodi kroz faze 1–7 redoslijedom iz tog dokumenta. Mockup priloži kao vizualnu referencu. Admin panel (faza 5) tek nakon što baza i javne stranice rade.

## Ključne odluke (zabilježeno)
- Baza: **Azure SQL** (~5€/mj) — perzistentno, automatski backup, najmanje admin koda. (Ne JSON, ne SQLite.)
- Slike: **Azure Blob Storage** — u bazi se drži samo URL.
- „Dostupne boje" na obliku: **slobodan tekst** (npr. „vidi Karta boja"), ne povezano s kartom boja.
- Login: **jedan račun** (username + password), dijele ga vlasnici.
