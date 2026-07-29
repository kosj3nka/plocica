namespace Plocica.Models;

public class ColorItem
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty; // "001" … "028"
    public string? Name { get; set; }                  // može biti prazno
    public string? Hex { get; set; }                     // iz color pickera, npr. "#E8C321"
    public string? ImageUrl { get; set; }                  // ILI slika glazure (Blob URL)
    public int SortOrder { get; set; }
}
