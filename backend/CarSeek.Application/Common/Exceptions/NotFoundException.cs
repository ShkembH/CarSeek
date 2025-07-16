namespace CarSeek.Application.Common.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string name, object key)
        : base("Not Found", $"Entity \"{name}\" ({key}) was not found.")
    {
    }
}
