namespace VacationTracker.Web.Models;

public class LeaveEntry
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int Year { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int UsedDays { get; set; }

    public Employee? Employee { get; set; }
}
