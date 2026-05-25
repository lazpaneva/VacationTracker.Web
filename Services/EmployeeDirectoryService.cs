using System.Text.Json;
using VacationTracker.Web.Models;
using VacationTracker.Web.ViewModels.Employees;

namespace VacationTracker.Web.Services;

public class EmployeeDirectoryService : IEmployeeDirectoryService
{
    private static readonly Lock SyncRoot = new();
    private readonly string _dataFilePath;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public EmployeeDirectoryService(IWebHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);
        _dataFilePath = Path.Combine(dataDirectory, "company-data.json");
    }

    public EmployeeDirectoryViewModel GetDirectory(int year)
    {
        lock (SyncRoot)
        {
            var data = LoadData();
            var years = BuildYearOptions(year, data);
            var existingEmployeesForPicker = data.Employees
                .Where(employee => employee.IsActive && employee.AnnualLeaveAllocations.All(x => x.Year != year))
                .OrderBy(employee => employee.FirstName)
                .ThenBy(employee => employee.MiddleName)
                .Select(employee => new EmployeePickerItemViewModel
                {
                    Id = employee.Id,
                    FullName = employee.FullName
                })
                .ToList();

            return new EmployeeDirectoryViewModel
            {
                SelectedYear = year,
                AvailableYears = years,
                AvailableExistingEmployees = existingEmployeesForPicker,
                AddExisting = new AddExistingEmployeeToYearInputModel
                {
                    Year = year,
                    TotalDays = 20
                },
                Employees = data.Employees
                    .Where(employee => employee.IsActive &&
                        employee.AnnualLeaveAllocations.Any(allocation => allocation.Year == year))
                    .OrderBy(employee => employee.FirstName)
                    .ThenBy(employee => employee.MiddleName)
                    .Select(employee => new EmployeeListItemViewModel
                    {
                        Id = employee.Id,
                        FullName = employee.FullName,
                        DefaultAnnualLeaveDays = employee.DefaultAnnualLeaveDays,
                        AnnualLeaveDaysForYear = GetAnnualLeaveDaysForYear(employee, year),
                        UsedDaysForYear = employee.LeaveEntries.Where(x => MatchesYear(x, year)).Sum(x => x.UsedDays),
                        RemainingDaysForYear = Math.Max(0, GetAnnualLeaveDaysForYear(employee, year) - employee.LeaveEntries.Where(x => MatchesYear(x, year)).Sum(x => x.UsedDays)),
                        HasYearSpecificOverride = employee.AnnualLeaveAllocations.Any(x => x.Year == year)
                    })
                    .ToList()
            };
        }
    }

    public EmployeeOperationResult AddEmployee(CreateEmployeeInputModel input)
    {
        lock (SyncRoot)
        {
            var data = LoadData();
            var cleanFirstName = NormalizeName(input.FirstName);
            var cleanMiddleName = NormalizeName(input.MiddleName);

            var duplicateExists = data.Employees.Any(employee =>
                employee.IsActive &&
                string.Equals(NormalizeName(employee.FirstName), cleanFirstName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(NormalizeName(employee.MiddleName), cleanMiddleName, StringComparison.OrdinalIgnoreCase));

            if (duplicateExists)
            {
                return EmployeeOperationResult.Failure("Вече има служител със същото име и презиме. Ако искаш да му добавиш отпуск за нова година, използвай секцията „Добави съществуващ служител за годината“ в екрана „Служители“.");
            }

            var employee = new Employee
            {
                Id = data.NextEmployeeId++,
                FirstName = cleanFirstName,
                MiddleName = cleanMiddleName,
                DefaultAnnualLeaveDays = input.DefaultAnnualLeaveDays,
                AnnualLeaveAllocations =
                [
                    new AnnualLeaveAllocation
                    {
                        Id = data.NextAllocationId++,
                        Year = input.Year,
                        TotalDays = input.DefaultAnnualLeaveDays
                    }
                ]
            };

            data.Employees.Add(employee);
            SaveData(data);
            return EmployeeOperationResult.Success();
        }
    }

    public void UpdateAnnualLeave(int employeeId, int year, int totalDays)
    {
        lock (SyncRoot)
        {
            var data = LoadData();
            var employee = data.Employees.FirstOrDefault(x => x.Id == employeeId && x.IsActive);

            if (employee is null)
            {
                return;
            }

            var allocation = employee.AnnualLeaveAllocations.FirstOrDefault(x => x.Year == year);

            if (allocation is null)
            {
                employee.AnnualLeaveAllocations.Add(new AnnualLeaveAllocation
                {
                    Id = data.NextAllocationId++,
                    EmployeeId = employee.Id,
                    Year = year,
                    TotalDays = totalDays
                });
            }
            else
            {
                allocation.TotalDays = totalDays;
            }

            SaveData(data);
        }
    }

    public void DeleteEmployee(int employeeId)
    {
        lock (SyncRoot)
        {
            var data = LoadData();
            var employee = data.Employees.FirstOrDefault(x => x.Id == employeeId);

            if (employee is null)
            {
                return;
            }

            data.Employees.Remove(employee);
            SaveData(data);
        }
    }

    private CompanyData LoadData()
    {
        if (!File.Exists(_dataFilePath))
        {
            return new CompanyData();
        }

        var json = File.ReadAllText(_dataFilePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new CompanyData();
        }

        return JsonSerializer.Deserialize<CompanyData>(json, _jsonOptions) ?? new CompanyData();
    }

    private void SaveData(CompanyData data)
    {
        foreach (var employee in data.Employees)
        {
            foreach (var allocation in employee.AnnualLeaveAllocations)
            {
                allocation.EmployeeId = employee.Id;
            }

            foreach (var leaveEntry in employee.LeaveEntries)
            {
                leaveEntry.EmployeeId = employee.Id;
                if (leaveEntry.Year == 0)
                {
                    leaveEntry.Year = leaveEntry.StartDate.Year;
                }
            }
        }

        var json = JsonSerializer.Serialize(data, _jsonOptions);
        File.WriteAllText(_dataFilePath, json);
    }

    private static int GetAnnualLeaveDaysForYear(Employee employee, int year)
    {
        var allocation = employee.AnnualLeaveAllocations
            .Where(x => x.Year == year)
            .Select(x => (int?)x.TotalDays)
            .FirstOrDefault();

        if (allocation.HasValue)
        {
            return allocation.Value;
        }

        return employee.DefaultAnnualLeaveDays;
    }

    private static List<int> BuildYearOptions(int selectedYear, CompanyData data)
    {
        var years = data.Employees
            .SelectMany(x => x.AnnualLeaveAllocations.Select(a => a.Year))
            .Append(selectedYear)
            .Append(selectedYear - 1)
            .Append(selectedYear + 1)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        return years;
    }

    private static bool MatchesYear(LeaveEntry entry, int year)
    {
        if (entry.Year == year)
        {
            return true;
        }

        // Backward-compatibility: older data may not have Year serialized.
        if (entry.Year == 0 && entry.StartDate.Year == year && entry.EndDate.Year == year)
        {
            return true;
        }

        return false;
    }

    private static string NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join(' ', value.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
