namespace PetCare.Application.Common;

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ServiceResult<T> SuccessResult(T data, string message = "Success")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ServiceResult<T> FailureResult(string message, List<string>? errors = null)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}
