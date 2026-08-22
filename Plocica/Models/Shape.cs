using System.ComponentModel.DataAnnotations.Schema;

namespace Plocica.Models;

public class Shape
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;      // "Arabesque"
    public string Collection { get; set; } = string.Empty; // "oblici" | "oslikane" | "reljefne"
    [Column(TypeName = "decimal(6,2)")]
    public decimal? Thickness { get; set; }                 // debljina, u cm
    [Column(TypeName = "decimal(6,2)")]
    public decimal? DimensionHeight { get; set; }            // visina, u cm
    [Column(TypeName = "decimal(6,2)")]
    public decimal? DimensionWidth { get; set; }              // širina, u cm
    public string? ImageUrl { get; set; }                    // individualna slika (Blob URL) — ikona u mreži oblika
    public string? PhotoUrl { get; set; }                     // opća fotografija (Blob URL) — prikaz u tehničkim informacijama
    public string? AvailableColors { get; set; }               // dostupne boje — koristi samo kategorija "reljefne"
    public string? OtherInfo { get; set; }                       // ostali info
    public string? Price { get; set; }                            // cijena — tekst, može biti višeredna
    public int SortOrder { get; set; }
    public List<ShapeExample> Examples { get; set; } = new();   // primjeri: naziv + fotografija (npr. "Blue")
    public List<ShapeGalleryImage> GalleryImages { get; set; } = new(); // shema slaganja (Kind=Layout) i reljefne fotografije (Kind=Reljefna)
}
