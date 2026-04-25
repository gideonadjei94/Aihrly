namespace Aihrly.Api.Common;

public enum ServiceError { None, NotFound, Conflict, BadRequest }

// Wraps a service response so controllers can handle errors without try/catch
public class ServiceResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public ServiceError ErrorType { get; }

    private ServiceResult(bool isSuccess, T? value, string? errorMessage, ServiceError errorType)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
    }

    public static ServiceResult<T> Ok(T value) =>
        new(true, value, null, ServiceError.None);

    public static ServiceResult<T> NotFound(string message) =>
        new(false, default, message, ServiceError.NotFound);

    public static ServiceResult<T> Conflict(string message) =>
        new(false, default, message, ServiceError.Conflict);

    public static ServiceResult<T> BadRequest(string message) =>
        new(false, default, message, ServiceError.BadRequest);
}
