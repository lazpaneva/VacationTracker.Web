using System.Text;
using Microsoft.AspNetCore.Mvc;
using VacationTracker.Web.Services;
using VacationTracker.Web.ViewModels.Employees;

namespace VacationTracker.Web.Controllers;

public class EmployeesController : Controller
{
    private readonly IEmployeeDirectoryService _employeeDirectoryService;

    public EmployeesController(IEmployeeDirectoryService employeeDirectoryService)
    {
        _employeeDirectoryService = employeeDirectoryService;
    }

    [HttpGet]
    public IActionResult Index(int? year)
    {
        var selectedYear = year ?? DateTime.Today.Year;
        var model = _employeeDirectoryService.GetDirectory(selectedYear);
        return View(model);
    }

    [HttpGet]
    public IActionResult Create(int? year)
    {
        var selectedYear = year ?? DateTime.Today.Year;
        var model = BuildCreatePageModel(new CreateEmployeeInputModel { Year = selectedYear });
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateEmployeeInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return View(BuildCreatePageModel(input));
        }

        var result = _employeeDirectoryService.AddEmployee(input);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            return View(BuildCreatePageModel(input));
        }

        TempData["StatusMessage"] = "Служителят е добавен.";
        return RedirectToAction(nameof(Index), new { year = input.Year });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddExistingToYear(AddExistingEmployeeToYearInputModel input)
    {
        if (!ModelState.IsValid)
        {
            TempData["StatusMessage"] = "Моля избери служител и валиден брой дни.";
            return RedirectToAction(nameof(Index), new { year = input.Year });
        }

        _employeeDirectoryService.UpdateAnnualLeave(input.EmployeeId, input.Year, input.TotalDays);
        TempData["StatusMessage"] = "Служителят е добавен към годината.";
        return RedirectToAction(nameof(Index), new { year = input.Year });
    }

    [HttpGet]
    public IActionResult ExportRemaining(int year)
    {
        var model = _employeeDirectoryService.GetDirectory(year);
        var rows = model.Employees
            .Where(x => x.RemainingDaysForYear > 0)
            .OrderBy(x => x.FullName)
            .Select(x => new { x.FullName, x.AnnualLeaveDaysForYear, x.UsedDaysForYear, x.RemainingDaysForYear })
            .ToList();

        var lines = new List<string> { "Employee;TotalDays;UsedDays;RemainingDays" };
        foreach (var row in rows)
        {
            lines.Add($"{Escape(row.FullName)};{row.AnnualLeaveDaysForYear};{row.UsedDaysForYear};{row.RemainingDaysForYear}");
        }

        var csv = string.Join(Environment.NewLine, lines);
        var bytes = Encoding.UTF8.GetBytes("\uFEFF" + csv);
        return File(bytes, "text/csv; charset=utf-8", $"remaining-leave-{year}.csv");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateAnnualLeave(UpdateAnnualLeaveInputModel input)
    {
        if (!ModelState.IsValid)
        {
            TempData["StatusMessage"] = "Моля въведи валиден брой дни отпуск.";
            return RedirectToAction(nameof(Index), new { year = input.Year });
        }

        _employeeDirectoryService.UpdateAnnualLeave(input.EmployeeId, input.Year, input.TotalDays);
        TempData["StatusMessage"] = "Годишният отпуск е обновен.";
        return RedirectToAction(nameof(Index), new { year = input.Year });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int employeeId, int year)
    {
        _employeeDirectoryService.DeleteEmployee(employeeId);
        TempData["StatusMessage"] = "Служителят е изтрит.";
        return RedirectToAction(nameof(Index), new { year });
    }

    private CreateEmployeePageViewModel BuildCreatePageModel(CreateEmployeeInputModel input)
    {
        var availableYears = _employeeDirectoryService.GetDirectory(input.Year).AvailableYears;
        return new CreateEmployeePageViewModel
        {
            AvailableYears = availableYears,
            Input = input
        };
    }

    private static string Escape(string value)
    {
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}

