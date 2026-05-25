using VacationTracker.Web.ViewModels.Leaves;

namespace VacationTracker.Web.Services;

public interface ILeaveService
{
    EmployeeLeaveDetailsViewModel GetEmployeeLeaves(int employeeId, int year);

    EmployeeOperationResult AddLeaveEntry(AddLeaveEntryInputModel input);

    void DeleteLeaveEntry(int employeeId, int leaveEntryId);
}

