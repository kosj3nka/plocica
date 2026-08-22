namespace Plocica.Models;

public class ShapeGalleryImage
{
    public const string KindLayout = "layout";
    public const string KindReljefna = "reljefna";

    public int Id { get; set; }
    public int ShapeId { get; set; }
    public Shape Shape { get; set; } = null!;
    public string Kind { get; set; } = string.Empty;      // "layout" | "reljefna"
    public string ImageUrl { get; set; } = string.Empty;  // Blob URL
    public int SortOrder { get; set; }
}
