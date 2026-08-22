namespace Plocica.Pages.Shared;

public class AdminImageRowViewModel
{
    public string ImageUrl { get; set; } = default!;
    public int Id { get; set; }
    public string IdInputName { get; set; } = default!;
    public string ImageUrlInputName { get; set; } = default!;
    public int SortOrder { get; set; }
    public string SortOrderInputName { get; set; } = default!;
    public string DeleteInputName { get; set; } = default!;
    public string? NameInputName { get; set; }
    public string? Name { get; set; }
    public bool ShowSortOrder { get; set; } = true;
}
