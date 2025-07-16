namespace CarSeek.Application.Common.Exceptions;

public class BadRequestException : AppException
{
    public BadRequestException(string message)
        : base("Bad Request", message)
    {
    }
}
