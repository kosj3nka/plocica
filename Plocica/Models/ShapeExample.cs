namespace Plocica.Models;

public class ShapeExample
{
    public int Id { get; set; }
    public int ShapeId { get; set; }
    public Shape Shape { get; set; } = null!;
    public string Name { get; set; } = string.Empty;      // npr. "Blue"
    public string ImageUrl { get; set; } = string.Empty;  // Blob URL
    public int SortOrder { get; set; }
}
