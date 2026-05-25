namespace VacationTracker.Web.ViewModels.Leaves;

public class LeaveEntryListItemViewModel
{
    public int Id { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int UsedDays { get; set; }
}

