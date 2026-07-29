namespace Plocica.Models;

public class Shape
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;      // "Arabesque"
    public string Collection { get; set; } = string.Empty; // "oblici" | "oslikane" | "reljefne"
    public string? Thickness { get; set; }                 // debljina, npr. "0,8 cm"
    public string? Dimensions { get; set; }                 // dimenzija, npr. "14,5 × 14,5 cm"
    public string? ImageUrl { get; set; }                    // individualna slika (Blob URL)
    public string? LayoutScheme { get; set; }                // shema slaganja (tekst ili slika URL)
    public string? Finish { get; set; }                       // završna obrada, npr. "mat i sjajno"
    public string? AvailableColors { get; set; }               // dostupne boje — slobodan tekst
    public string? OtherInfo { get; set; }                       // ostali info
    public string? Price { get; set; }                            // cijena — tekst, može biti višeredna
    public int SortOrder { get; set; }
}
