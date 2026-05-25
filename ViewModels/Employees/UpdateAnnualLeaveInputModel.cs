using System.ComponentModel.DataAnnotations;

namespace VacationTracker.Web.ViewModels.Employees;

public class UpdateAnnualLeaveInputModel
{
    public int EmployeeId { get; set; }

    public int Year { get; set; }

    [Range(1, 365, ErrorMessage = "Броят дни трябва да е между 1 и 365.")]
    public int TotalDays { get; set; }
}
