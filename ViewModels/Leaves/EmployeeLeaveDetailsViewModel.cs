namespace VacationTracker.Web.ViewModels.Leaves;

public class EmployeeLeaveDetailsViewModel
{
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public int Year { get; set; }

    public int TotalDays { get; set; }

    public int UsedDays { get; set; }

    public int RemainingDays { get; set; }

    public List<LeaveEntryListItemViewModel> Entries { get; set; } = [];

    public AddLeaveEntryInputModel AddEntry { get; set; } = new();
}

