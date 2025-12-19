using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MyCookbook.App.Helpers;

/// <summary>
/// Helper class to convert exceptions into user-friendly error messages
/// </summary>
public static class ErrorMessageHelper
{
    public static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.Unauthorized =>
                "Your session has expired. Please log in again.",
            
            HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.NotFound =>
                "The requested item could not be found.",
            
            HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.BadRequest =>
                "Invalid request. Please check your input and try again.",
            
            HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.InternalServerError =>
                "Server error. Please try again later.",
            
            HttpRequestException httpEx when httpEx.StatusCode == HttpStatusCode.ServiceUnavailable =>
                "Service is temporarily unavailable. Please try again later.",
            
            HttpRequestException =>
                "Network error. Please check your connection and try again.",
            
            TaskCanceledException =>
                "Request timed out. Please check your connection and try again.",
            
            TimeoutException =>
                "Request timed out. Please try again.",
            
            OperationCanceledException =>
                "Operation was cancelled.",
            
            UnauthorizedAccessException =>
                "You don't have permission to access this resource.",
            
            InvalidOperationException invalidEx =>
                invalidEx.Message,
            
            _ =>
                "An unexpected error occurred. Please try again."
        };
    }
    
    public static bool IsNetworkError(Exception exception)
    {
        return exception is HttpRequestException 
            or TaskCanceledException 
            or TimeoutException;
    }
    
    public static bool IsAuthenticationError(Exception exception)
    {
        return exception is HttpRequestException httpEx 
            && httpEx.StatusCode == HttpStatusCode.Unauthorized;
    }
}

