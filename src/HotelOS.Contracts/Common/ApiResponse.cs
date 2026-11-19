namespace HotelOS.Contracts.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public int StatusCode { get; set; } = 200;

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Message = message, Data = data, StatusCode = 200 };

    public static ApiResponse<T> Fail(string message, int statusCode = 400, List<string>? errors = null) =>
        new() { Success = false, Message = message, StatusCode = statusCode, Errors = errors ?? new List<string>() };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse OkMessage(string message = "Success") =>
        new() { Success = true, Message = message, StatusCode = 200 };

    public static ApiResponse FailMessage(string message, int statusCode = 400, List<string>? errors = null) =>
        new() { Success = false, Message = message, StatusCode = statusCode, Errors = errors ?? new List<string>() };
}
