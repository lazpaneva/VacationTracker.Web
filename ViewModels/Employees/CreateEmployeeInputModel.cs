using System.ComponentModel.DataAnnotations;

namespace VacationTracker.Web.ViewModels.Employees;

public class CreateEmployeeInputModel
{
    [Required(ErrorMessage = "Името е задължително.")]
    [Display(Name = "Име")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Презимето е задължително.")]
    [Display(Name = "Презиме")]
    [StringLength(50)]
    public string MiddleName { get; set; } = string.Empty;

    [Range(2000, 2100)]
    public int Year { get; set; } = DateTime.Today.Year;

    [Range(1, 365, ErrorMessage = "Броят дни трябва да е между 1 и 365.")]
    [Display(Name = "Годишен отпуск")]
    public int DefaultAnnualLeaveDays { get; set; } = 20;
}
