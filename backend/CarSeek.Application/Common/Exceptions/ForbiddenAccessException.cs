namespace CarSeek.Application.Common.Exceptions;

public class ForbiddenAccessException : AppException
{
    public ForbiddenAccessException(string message)
        : base("Forbidden Access", message)
    {
    }
}
