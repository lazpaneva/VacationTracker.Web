namespace VacationTracker.Web.ViewModels.Employees;

public class CreateEmployeePageViewModel
{
    public List<int> AvailableYears { get; set; } = [];

    public CreateEmployeeInputModel Input { get; set; } = new();
}
