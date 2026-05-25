using System.ComponentModel.DataAnnotations;

namespace VacationTracker.Web.ViewModels.Employees;

public class AddExistingEmployeeToYearInputModel
{
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Range(2000, 2100)]
    public int Year { get; set; } = DateTime.Today.Year;

    [Range(1, 365, ErrorMessage = "Броят дни трябва да е между 1 и 365.")]
    [Display(Name = "Дни отпуск")]
    public int TotalDays { get; set; } = 20;
}

