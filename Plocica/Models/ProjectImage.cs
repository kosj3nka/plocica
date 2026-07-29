namespace Plocica.Models;

public class ProjectImage
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Url { get; set; } = string.Empty; // Blob URL
    public int SortOrder { get; set; }
}
