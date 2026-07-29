# 05 — Deploy na Azure

Razor Pages (.NET 8) + **Azure SQL** (baza) + **Azure Blob Storage** (slike) na **Azure App Service (Linux)**.

## Resursi (svi u istoj Resource Group, regija West Europe)

1. **App Service Plan** — Linux, **B1** (Basic). F1 (Free) samo za probu (limiti, nema stalne dostupnosti).
2. **App Service** — .NET 8, poveže se na Plan.
3. **Azure SQL Database** — **Basic** tier (~5€/mj, 2 GB — višestruko dovoljno). Kreira i logički SQL Server. Zabilježi admin login SQL servera.
4. **Storage Account** → **Blob container(i)** za slike (`images` ili `shapes`/`colors`/`projects`). Public read na container (slike se serviraju direktno) ili privatno + SAS ako se želi kontrola.

West Europe = najbliže Hrvatskoj, niska latencija.

## Connection stringovi i tajne (NE u kodu)

Sve tajne idu u **App Service → Configuration → Connection strings / Application settings**, nikad u repo:

- `ConnectionStrings:Default` — Azure SQL connection string (iz SQL DB → Connection strings → ADO.NET). Tip: `SQLAzure`.
- `Blob:ConnectionString` — iz Storage Account → Access keys.
- Admin lozinka (ako se postavlja preko env varijable pri prvom pokretanju) — npr. `Admin:InitialPassword`. Nakon seeda ukloniti.

Lokalni dev: iste vrijednosti u **User Secrets** (`dotnet user-secrets`), ne u `appsettings.json`.

### SQL firewall
Azure SQL po defaultu blokira sve. Uključi „Allow Azure services and resources to access this server" da App Service može do baze. Za lokalni dev dodaj svoju IP adresu u firewall pravila.

## Migracije na produkciji

- Migracije se primijene na Azure SQL: ili `dotnet ef database update` s produkcijskim connection stringom, ili automatski `db.Database.Migrate()` na startu aplikacije (jednostavno za solo projekt; pazi da migracije budu idempotentne).
- Seed početnih podataka (`01-CONTENT.md`) pokreni samo ako je baza prazna.

## Deploy opcije (od najlakše)

1. **Visual Studio / VS Code → Publish** (desni klik → Publish → Azure App Service). Najbrže solo.
2. **GitHub Actions** (preporuka dalje): push na `main` → build → deploy. App Service → Deployment Center → GitHub generira workflow. CI/CD besplatno.
3. **Azure CLI:** `az webapp up --runtime "DOTNETCORE:8.0" --sku B1 --location westeurope`.

## Custom domena + HTTPS

- Domena (npr. `plocica.hr` ili `plocica.studio`).
- App Service → Custom domains → dodaj + verificiraj (TXT/CNAME kod registrara).
- **App Service Managed Certificate** — besplatan SSL. Uključi „HTTPS Only" u Configuration.

## Sadržaj i slike na Azureu — riješeno bazom + Blobom

- **Podaci** (oblici/boje/projekti) su u Azure SQL → perzistentni, preživljavaju deploy i restart, automatski backup (point-in-time restore).
- **Slike** su u Blob Storage → odvojene od aplikacije, preživljavaju deploy.
- **Ažuriranje sadržaja = admin panel**, ne deploy. Vlasnici se uloguju na `/admin`, urede, spreme — odmah uživo. Deploy je samo za promjene koda.

Ovime otpada raniji JSON-u-repou model i git-push za sadržaj. Vlasnici ne diraju repo.

## Trošak (okvirno, mjesečno)

- App Service B1: ~13€ · Azure SQL Basic: ~5€ · Blob: centi · Managed cert: 0€.
- Ako treba niže: App Service se može spustiti, ali B1 je razumni minimum za stalnu dostupnost.

## Ostalo

- **Application Insights** (basic, besplatan tier) — promet i greške.
- Backup: Azure SQL ima automatski point-in-time restore (Basic: 7 dana). Blob je durably replicated.
