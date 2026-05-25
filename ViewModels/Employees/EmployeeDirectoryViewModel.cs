namespace VacationTracker.Web.ViewModels.Employees;

public class EmployeeDirectoryViewModel
{
    public int SelectedYear { get; set; }

    public List<int> AvailableYears { get; set; } = [];

    public List<EmployeeListItemViewModel> Employees { get; set; } = [];

    public AddExistingEmployeeToYearInputModel AddExisting { get; set; } = new();

    public List<EmployeePickerItemViewModel> AvailableExistingEmployees { get; set; } = [];
}
