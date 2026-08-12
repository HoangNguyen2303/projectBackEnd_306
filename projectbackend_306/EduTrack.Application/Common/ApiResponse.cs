namespace EduTrack.Application.Common;

/// <summary>
/// Unified response envelope for every endpoint (team standard):
/// { success, data, message, statusCode }.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "OK", int statusCode = 200)
        => new() { Success = true, Data = data, Message = message, StatusCode = statusCode };

    public static ApiResponse<T> Fail(string message, int statusCode = 400)
        => new() { Success = false, Data = default, Message = message, StatusCode = statusCode };
}
