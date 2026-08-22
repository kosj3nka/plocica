namespace Plocica.Pages.Shared;

public class AdminImageUploadViewModel
{
    public string InputName { get; set; } = default!;
    public string? ExistingImageUrl { get; set; }
    public string Accept { get; set; } = ".jpg,.jpeg,.png,.webp";
    public bool Multiple { get; set; }
}
