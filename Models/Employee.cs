namespace VacationTracker.Web.Models;

public class Employee
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;

    public int DefaultAnnualLeaveDays { get; set; } = 20;

    public string FullName => $"{FirstName} {MiddleName}".Trim();

    public bool IsActive { get; set; } = true;

    public List<AnnualLeaveAllocation> AnnualLeaveAllocations { get; set; } = [];

    public List<LeaveEntry> LeaveEntries { get; set; } = [];
}
