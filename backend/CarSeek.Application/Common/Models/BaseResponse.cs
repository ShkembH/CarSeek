namespace CarSeek.Application.Common.Models;

public class BaseResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }

    public static BaseResponse SuccessResult()
        => new() { Success = true };

    public static BaseResponse ErrorResult(string error)
        => new() { Success = false, Error = error };
}

public class BaseResponse<T> : BaseResponse
{
    public T? Data { get; set; }

    public static BaseResponse<T> SuccessResult(T data)
        => new() { Success = true, Data = data };

    public new static BaseResponse<T> ErrorResult(string error)
        => new() { Success = false, Error = error };
}
