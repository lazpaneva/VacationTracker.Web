using System.ComponentModel.DataAnnotations;

namespace VacationTracker.Web.ViewModels.Leaves;

public class AddLeaveEntryInputModel
{
    public int EmployeeId { get; set; }

    [Range(2000, 2100)]
    public int Year { get; set; }

    [Display(Name = "От дата")]
    public DateOnly StartDate { get; set; }

    [Display(Name = "До дата")]
    public DateOnly EndDate { get; set; }

    [Range(1, 365, ErrorMessage = "Броят дни трябва да е между 1 и 365.")]
    [Display(Name = "Брой дни")]
    public int UsedDays { get; set; } = 1;
}

