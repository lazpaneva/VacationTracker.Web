using System.Text.Json;
using VacationTracker.Web.Models;
using VacationTracker.Web.ViewModels.Leaves;

namespace VacationTracker.Web.Services;

public class LeaveService : ILeaveService
{
    private static readonly Lock SyncRoot = new();
    private readonly string _dataFilePath;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public LeaveService(IWebHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);
        _dataFilePath = Path.Combine(dataDirectory, "company-data.json");
    }

    public EmployeeLeaveDetailsViewModel GetEmployeeLeaves(int employeeId, int year)
    {
        lock (SyncRoot)
        {
            var data = LoadData();
            var employee = data.Employees.FirstOrDefault(x => x.Id == employeeId && x.IsActive);

            if (employee is null)
            {
                return new EmployeeLeaveDetailsViewModel
                {
                    EmployeeId = employeeId,
                    Year = year,
                    EmployeeName = "Няма такъв служител"
                };
            }

            var totalDays = employee.AnnualLeaveAllocations.FirstOrDefault(x => x.Year == year)?.TotalDays
                            ?? employee.DefaultAnnualLeaveDays;

            var entries = employee.LeaveEntries
                .Where(x => MatchesYear(x, year))
                .OrderByDescending(x => x.StartDate)
                .Select(x => new LeaveEntryListItemViewModel
                {
                    Id = x.Id,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    UsedDays = x.UsedDays
                })
                .ToList();

            var usedDays = entries.Sum(x => x.UsedDays);

            return new EmployeeLeaveDetailsViewModel
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                Year = year,
                TotalDays = totalDays,
                UsedDays = usedDays,
                RemainingDays = Math.Max(0, totalDays - usedDays),
                Entries = entries,
                AddEntry = new AddLeaveEntryInputModel
                {
                    EmployeeId = employee.Id,
                    Year = year,
                    StartDate = DateOnly.FromDateTime(DateTime.Today),
                    EndDate = DateOnly.FromDateTime(DateTime.Today),
                    UsedDays = 1
                }
            };
        }
    }

    public EmployeeOperationResult AddLeaveEntry(AddLeaveEntryInputModel input)
    {
        lock (SyncRoot)
        {
            var data = LoadData();
            var employee = data.Employees.FirstOrDefault(x => x.Id == input.EmployeeId && x.IsActive);

            if (employee is null)
            {
                return EmployeeOperationResult.Failure("Няма такъв служител.");
            }

            if (input.EndDate < input.StartDate)
            {
                return EmployeeOperationResult.Failure("Крайната дата трябва да е след началната.");
            }

            var minAllowedDate = new DateOnly(input.Year, 1, 1);
            var maxAllowedDate = new DateOnly(input.Year + 1, 12, 31);

            if (input.StartDate < minAllowedDate || input.StartDate > maxAllowedDate ||
                input.EndDate < minAllowedDate || input.EndDate > maxAllowedDate)
            {
                return EmployeeOperationResult.Failure("Датите трябва да са в рамките на годината на отпуска или следващата година.");
            }

            var totalDays = employee.AnnualLeaveAllocations.FirstOrDefault(x => x.Year == input.Year)?.TotalDays
                            ?? employee.DefaultAnnualLeaveDays;

            var usedDaysSoFar = employee.LeaveEntries.Where(x => MatchesYear(x, input.Year)).Sum(x => x.UsedDays);
            var remaining = totalDays - usedDaysSoFar;

            if (input.UsedDays > remaining)
            {
                return EmployeeOperationResult.Failure($"Няма достатъчно оставащи дни. Оставащи: {Math.Max(0, remaining)}.");
            }

            employee.LeaveEntries.Add(new LeaveEntry
            {
                Id = data.NextLeaveEntryId++,
                EmployeeId = employee.Id,
                Year = input.Year,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                UsedDays = input.UsedDays
            });

            SaveData(data);
            return EmployeeOperationResult.Success();
        }
    }

    public void DeleteLeaveEntry(int employeeId, int leaveEntryId)
    {
        lock (SyncRoot)
        {
            var data = LoadData();
            var employee = data.Employees.FirstOrDefault(x => x.Id == employeeId && x.IsActive);
            if (employee is null)
            {
                return;
            }

            var entry = employee.LeaveEntries.FirstOrDefault(x => x.Id == leaveEntryId);
            if (entry is null)
            {
                return;
            }

            employee.LeaveEntries.Remove(entry);
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
}
