namespace CarSeek.Application.Common.Exceptions;

public class DuplicateEmailException : AppException
{
    public DuplicateEmailException(string email)
        : base("Registration Error", $"User with email '{email}' already exists")
    {
    }
}
