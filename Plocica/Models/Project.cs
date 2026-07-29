namespace Plocica.Models;

public class Project
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty; // "Kuća Heinzel"
    public string? Location { get; set; }                // npr. "Trg Marka Marulića 16, Zagreb"
    public string Text { get; set; } = string.Empty;   // opis projekta
    public int SortOrder { get; set; }
    public List<ProjectImage> Images { get; set; } = new();
}
