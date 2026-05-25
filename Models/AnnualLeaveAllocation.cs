namespace VacationTracker.Web.Models;

public class AnnualLeaveAllocation
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int Year { get; set; }

    public int TotalDays { get; set; }

    public Employee? Employee { get; set; }
}
