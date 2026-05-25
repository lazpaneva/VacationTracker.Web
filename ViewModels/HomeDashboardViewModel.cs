namespace VacationTracker.Web.ViewModels;

public class HomeDashboardViewModel
{
    public string ProductName { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public List<string> Highlights { get; set; } = [];

    public List<string> NextSteps { get; set; } = [];
}
