namespace DifyApiClient.Exceptions;

/// <summary>
/// Base exception for all Dify API errors
/// </summary>
public class DifyApiException : Exception
{
    public int? StatusCode { get; }
    public string? ResponseBody { get; }

    public DifyApiException(string message) : base(message)
    {
    }

    public DifyApiException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public DifyApiException(string message, int statusCode, string? responseBody = null) 
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
