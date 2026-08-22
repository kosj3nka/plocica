namespace Plocica.Pages.Shared;

public class AdminActionButtonsViewModel
{
    public int EditRouteId { get; set; }
    public string DeleteHandler { get; set; } = "Delete";
    public int DeleteRouteId { get; set; }
    public string ConfirmText { get; set; } = "Obrisati?";
}
