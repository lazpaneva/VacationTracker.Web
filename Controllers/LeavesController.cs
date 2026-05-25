using Microsoft.AspNetCore.Mvc;
using VacationTracker.Web.Services;
using VacationTracker.Web.ViewModels.Leaves;

namespace VacationTracker.Web.Controllers;

public class LeavesController : Controller
{
    private readonly ILeaveService _leaveService;

    public LeavesController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    [HttpGet]
    public IActionResult ForEmployee(int employeeId, int year)
    {
        var model = _leaveService.GetEmployeeLeaves(employeeId, year);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(AddLeaveEntryInputModel input)
    {
        if (!ModelState.IsValid)
        {
            var invalidModel = _leaveService.GetEmployeeLeaves(input.EmployeeId, input.Year);
            invalidModel.AddEntry = input;
            return View("ForEmployee", invalidModel);
        }

        var result = _leaveService.AddLeaveEntry(input);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            var model = _leaveService.GetEmployeeLeaves(input.EmployeeId, input.Year);
            model.AddEntry = input;
            return View("ForEmployee", model);
        }

        TempData["StatusMessage"] = "Отпускът е добавен.";
        return RedirectToAction(nameof(ForEmployee), new { employeeId = input.EmployeeId, year = input.Year });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int employeeId, int leaveEntryId, int year)
    {
        _leaveService.DeleteLeaveEntry(employeeId, leaveEntryId);
        TempData["StatusMessage"] = "Отпускът е изтрит.";
        return RedirectToAction(nameof(ForEmployee), new { employeeId, year });
    }
}
