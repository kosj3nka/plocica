# 01 — Sadržajni model (EF Core entiteti)

Sadržaj koji vlasnici uređuju kroz admin panel živi u **Azure SQL** bazi, preko **EF Core**. Tri entiteta: oblici, boje, projekti. Slike su u **Azure Blob Storage** — u bazi se sprema samo URL slike.

## Struktura

```
/Models
  Shape.cs        # oblik (Arabesque, Oval, Wave, Reljefne…)
  ColorItem.cs    # boja iz karte boja
  Project.cs      # custom projekt / realizirani rad
  ProjectImage.cs # slika projekta (projekt ima više slika)
/Data
  AppDbContext.cs # DbContext + DbSet-ovi + seed
/Migrations       # EF Core migracije
```

Slike: Blob container `images` (ili tri: `shapes`, `colors`, `projects`). Upload logika u `07-ADMIN.md`.

## Entiteti

### Shape (oblik)
Polja točno kako ih admin panel uređuje:

```csharp
public class Shape {
    public int Id { get; set; }
    public string Name { get; set; }            // "Arabesque"
    public string Collection { get; set; }      // "oblici" | "oslikane" | "reljefne"
    public string? Thickness { get; set; }      // debljina, npr. "0,8 cm"
    public string? Dimensions { get; set; }     // dimenzija, npr. "14,5 × 14,5 cm"
    public string? ImageUrl { get; set; }       // individualna slika (Blob URL)
    public string? LayoutScheme { get; set; }   // shema slaganja (tekst ili slika URL)
    public string? Finish { get; set; }         // završna obrada, npr. "mat i sjajno"
    public string? AvailableColors { get; set; }// dostupne boje — SLOBODAN TEKST ("vidi Karta boja")
    public string? OtherInfo { get; set; }      // ostali info (napomena o ručnoj izradi)
    public string? Price { get; set; }          // cijena — tekst (višeredni, npr. "300 € / m²")
    public int SortOrder { get; set; }          // redoslijed prikaza
}
```

Napomena: `Price` je tekst, ne broj — cijene u katalogu imaju više redaka/uvjeta ("315 €/m² jednobojne / 325 €/m² dvije boje / +30 €/m² mrežice"). `AvailableColors` je slobodan tekst po odluci (ne povezano s kartom boja).

### ColorItem (boja)
```csharp
public class ColorItem {
    public int Id { get; set; }
    public string Code { get; set; }     // "001" … "028"
    public string? Name { get; set; }    // "Warm Sun" — može biti prazno
    public string? Hex { get; set; }     // iz color pickera, npr. "#E8C321"
    public string? ImageUrl { get; set; }// ILI slika glazure (Blob URL)
    public int SortOrder { get; set; }
}
```
Admin bira: **slika ILI color picker**. Ako je `ImageUrl` postavljen, prikaži sliku; inače prikaži `Hex` kvadrat. Barem jedno mora biti postavljeno.

### Project (projekt) + ProjectImage
```csharp
public class Project {
    public int Id { get; set; }
    public string Title { get; set; }               // "Kuća Heinzel"
    public string? Location { get; set; }           // OPTIONAL, npr. "Trg Marka Marulića 16, Zagreb"
    public string Text { get; set; }                // opis projekta
    public int SortOrder { get; set; }
    public List<ProjectImage> Images { get; set; } = new();
}

public class ProjectImage {
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; }
    public string Url { get; set; }                 // Blob URL
    public int SortOrder { get; set; }
}
```

## DbContext

```csharp
public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) {}
    public DbSet<Shape> Shapes => Set<Shape>();
    public DbSet<ColorItem> Colors => Set<ColorItem>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
}
```

Registracija u `Program.cs`:
```csharp
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
```
Connection string dolazi iz Azure App Service konfiguracije (ne u kodu) — vidi `05-AZURE.md`.

## Migracije + seed

- `dotnet ef migrations add Init` → `dotnet ef database update`.
- Seed početnih podataka iz kataloga u `OnModelCreating` ili poseban seeder koji se pokrene na startu ako je baza prazna.

### Seed podaci (iz kataloga)

**Oblici** (collection: `oblici`) — debljina svima 0,8 cm, finiš "mat i sjajno", dostupne boje "vidi Karta boja", ostali info "Zbog procesa ručne izrade moguće su manje varijacije u dimenzijama i nijansi.":
- Arabesque — 14,5 × 14,5 cm — 300 € / m²
- Oval — 17,5 × 5,3 cm — 315 €/m² jednobojne / 325 €/m² dvije boje / +30 €/m² ljepljenje na mrežice
- Fish scale — 13 × 14,5 cm — 310 €/m² jednobojne / 325 €/m² dvije boje / 340 €/m² tri boje / +30 €/m² mrežice
- Linea — 15,5 × 0,8–1 cm — 340 €/m² jednobojne (ljepljenje na mrežice uključeno)

**Ručno oslikane** (collection: `oslikane`):
- Module — 15,5 × 15,5 cm — 430 €/m² kombinacija dvije boje
- Curve — 20 × 10 cm — motivi + pozitiv/negativ — 470 €/m² dvije boje + dva motiva
- Wave — 15,5 × 15,5 cm — 430 €/m² dvije boje / 460 €/m² tri-četiri boje / 500 €/m² pet-šest boja

**Reljefne** (collection: `reljefne`, cijena "kreće od 500 €/m²"): Reljefne iz kalupa · Ručno rezbarene. (Custom kombinacije — cijena na upit.)

**Boje:** 28 unosa, `Code` "001"–"028". Samo 002 = "Warm Sun" je siguran naziv; ostalima `Name` prazno (imena u katalogu stoje uz oblike, ne uz brojeve karte — ne pogađati mapiranje). `Hex` približno iz mockupa ili prazno dok se ne uploada slika.

**Projekti:**
- Kuća Heinzel — "Zgrada HINA-e, Trg Marka Marulića 16, Zagreb" — Rekonstrukcija originalnih pločica iz 1910. Šest tipova (reljefne, jednobojne, oslikane, tri vrste bordura). Fotografije su kombinacija sačuvanih originala i ručno rekonstruiranih replika.
- Crkva sv. Mirka — "Šestinski vijenac 1, Zagreb" — Rekonstrukcija originalnih keramičkih pločica uz precizno usklađivanje oblika, reljefa i boje. Uključivala izradu novih matrica i kalupa te testiranja glazura.

## Čitanje na javnim stranicama

- `PageModel.OnGet()` → `_db.Shapes.OrderBy(s => s.SortOrder)` itd. Filtriraj oblike po `Collection` za tri sekcije na `/kolekcije`.
- Ako slika (`ImageUrl`) fali → fallback (za boje: `Hex` kvadrat; za oblike/projekte: placeholder).
- Projekti: `_db.Projects.Include(p => p.Images)`.
