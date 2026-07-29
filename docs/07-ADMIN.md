# 07 — Admin panel (/admin)

Vlasnici se uloguju i uređuju oblike, boje i projekte. Bez diranja koda. Promjene su odmah uživo (čitaju se iz iste Azure SQL baze kao javne stranice).

## Auth

- **ASP.NET Core Identity** (ili minimalni cookie auth ako je Identity previše za jedan račun).
- **Jedan račun** — jedan username + password, dijele ga Lorena i Mihael. Bez registracije, bez dodavanja korisnika.
- Lozinka se **ne drži u kodu**. Seed-a se jednom (hash u bazi) ili se postavi preko environment varijable pri prvom pokretanju. Vidi `05-AZURE.md` za tajne.
- Cookie auth, `[Authorize]` na svim `/admin/*` stranicama osim `/admin/login`.
- Login throttling (par pokušaja pa kratka pauza) — osnovna zaštita od brute-force.
- HTTPS obavezno (već uključeno na Azureu).

## Rute

| Ruta | Funkcija |
|------|----------|
| `/admin/login` | Prijava. Nakon uspjeha → `/admin`. |
| `/admin/logout` | Odjava. |
| `/admin` | Nadzorna ploča — tri kartice: Oblici, Boje, Projekti (broj unosa + „Uredi"). |
| `/admin/oblici` | Lista oblika + „Novi". |
| `/admin/oblici/{id}` | Uredi/obriši oblik. |
| `/admin/boje` | Lista boja + „Nova". |
| `/admin/boje/{id}` | Uredi/obriši boju. |
| `/admin/projekti` | Lista projekata + „Novi". |
| `/admin/projekti/{id}` | Uredi/obriši projekt (+ upravljanje slikama). |

## Forme (polja po entitetu)

**Oblik** (`Shape`): Naziv · Kolekcija (dropdown: Oblici / Ručno oslikane / Reljefne) · Debljina · Dimenzija · Slika (upload) · Shema slaganja · Završna obrada · Dostupne boje (slobodan tekst) · Ostali info (textarea) · Cijena (textarea, višeredno) · Redoslijed.

**Boja** (`ColorItem`): Kod (001…) · Naziv (opcionalno) · **Slika (upload) ILI color picker** — UI toggle: „Slika" / „Boja". Ako je odabran picker, spremi `Hex`; ako slika, spremi `ImageUrl`. Barem jedno obavezno. · Redoslijed.

**Projekt** (`Project`): Naziv · Lokacija (opcionalno) · Tekst (textarea) · **Slike (više, upload + preredanje + brisanje)** · Redoslijed.

## Upload slika → Azure Blob

- Prihvati `IFormFile` (jpg/png/webp), ograniči veličinu (npr. ≤ 5 MB), validiraj tip.
- Generiraj jedinstveno ime (`Guid` + ekstenzija) da se izbjegnu kolizije.
- Upload u Blob container (`Azure.Storage.Blobs`, `BlobContainerClient.UploadBlobAsync`).
- Spremi vraćeni javni URL u odgovarajuće polje (`ImageUrl` / `ProjectImage.Url`).
- Pri brisanju/zamjeni slike — obriši i stari blob (da se ne gomilaju).
- (Opcionalno kasnije) resize/kompresija pri uploadu radi bržeg učitavanja.

Blob connection string iz konfiguracije, ne u kodu (`05-AZURE.md`).

## Validacija i sigurnost

- Server-side validacija svih polja (ne samo u pregledniku).
- Antiforgery token na svim POST formama (Razor Pages default — zadrži).
- Potvrda prije brisanja (oblika/boje/projekta/slike).
- Escapiranje korisničkog teksta pri prikazu na javnim stranicama (Razor to radi po defaultu — ne koristiti `@Html.Raw` na admin unosu).

## UX admin panela

- Isti dizajn tokeni kao javna stranica (`02-DESIGN.md`) — čist, arhitektonski, hairline linije — ali funkcionalno i gusto (tablice, forme). Ne mora biti „efektno", mora biti pregledno.
- Nakon spremanja: kratka potvrda („Spremljeno") i povratak na listu.
- Lista svakog entiteta: thumbnail (ako ima slike), naziv, redoslijed, gumbi Uredi / Obriši.
- Preredanje (SortOrder): jednostavno brojčano polje je dovoljno u v1; drag-and-drop kasnije.
