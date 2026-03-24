namespace FurnitureShop.Application.Common.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = null!;
    public T? Data { get; set; }
    public Dictionary<string, List<string>>? Errors { get; set; }
    public PaginationMeta? Pagination { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string TraceId { get; set; } = Guid.NewGuid().ToString("N")[..12];

    public static ApiResponse<T> Ok(T data, string message)
        => new() { Success = true, StatusCode = 200, Message = message, Data = data };

    public static ApiResponse<T> Ok(T data, PaginationMeta pagination, string message)
        => new() { Success = true, StatusCode = 200, Message = message, Data = data, Pagination = pagination };

    public static ApiResponse<T> Created(T data, string message)
        => new() { Success = true, StatusCode = 201, Message = message, Data = data };

    public static ApiResponse<T> NoContent(string message)
        => new() { Success = true, StatusCode = 204, Message = message };

    public static ApiResponse<T> NotFound(string message)
        => new() { Success = false, StatusCode = 404, Message = message };

    public static ApiResponse<T> ValidationError(Dictionary<string, List<string>> errors, string message)
        => new() { Success = false, StatusCode = 422, Message = message, Errors = errors };

    public static ApiResponse<T> Unauthorized(string message)
        => new() { Success = false, StatusCode = 401, Message = message };

    public static ApiResponse<T> Forbidden(string message)
        => new() { Success = false, StatusCode = 403, Message = message };

    public static ApiResponse<T> ServerError(string message, string traceId)
        => new() { Success = false, StatusCode = 500, Message = message, TraceId = traceId };
}