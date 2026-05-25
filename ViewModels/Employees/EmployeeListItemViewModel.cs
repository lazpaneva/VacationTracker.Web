namespace VacationTracker.Web.ViewModels.Employees;

public class EmployeeListItemViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public int DefaultAnnualLeaveDays { get; set; }

    public int AnnualLeaveDaysForYear { get; set; }

    public int UsedDaysForYear { get; set; }

    public int RemainingDaysForYear { get; set; }

    public bool HasYearSpecificOverride { get; set; }
}
