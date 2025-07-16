namespace CarSeek.Application.Common.Exceptions;

public class AuthenticationException : AppException
{
    public AuthenticationException(string message)
        : base("Authentication Error", message)
    {
    }
}
