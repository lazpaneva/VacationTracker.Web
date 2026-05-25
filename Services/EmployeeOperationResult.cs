namespace VacationTracker.Web.Services;

public class EmployeeOperationResult
{
    public bool IsSuccess { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public static EmployeeOperationResult Success() => new() { IsSuccess = true };

    public static EmployeeOperationResult Failure(string errorMessage) =>
        new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
}
