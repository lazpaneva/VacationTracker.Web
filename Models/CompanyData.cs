namespace VacationTracker.Web.Models;

public class CompanyData
{
    public int NextEmployeeId { get; set; } = 1;

    public int NextAllocationId { get; set; } = 1;

    public int NextLeaveEntryId { get; set; } = 1;

    public List<Employee> Employees { get; set; } = [];
}
