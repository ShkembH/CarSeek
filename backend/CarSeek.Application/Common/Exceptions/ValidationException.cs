namespace CarSeek.Application.Common.Exceptions;

public class ValidationException : AppException
{
    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation Error", "One or more validation errors occurred")
    {
        Errors = errors;
    }

    // Add this constructor
    public ValidationException(string message)
        : base("Validation Error", message)
    {
        Errors = new Dictionary<string, string[]>
        {
            { "Error", new[] { message } }
        };
    }

    public IDictionary<string, string[]> Errors { get; }
}
