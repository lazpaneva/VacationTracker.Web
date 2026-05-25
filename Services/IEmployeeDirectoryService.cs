using VacationTracker.Web.ViewModels.Employees;

namespace VacationTracker.Web.Services;

public interface IEmployeeDirectoryService
{
    EmployeeDirectoryViewModel GetDirectory(int year);

    EmployeeOperationResult AddEmployee(CreateEmployeeInputModel input);

    void UpdateAnnualLeave(int employeeId, int year, int totalDays);

    void DeleteEmployee(int employeeId);
}
