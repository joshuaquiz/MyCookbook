using System;

namespace MyCookbook.App.Exceptions;

/// <summary>
/// Exception thrown when an HTTP request returns a 401 Unauthorized status code.
/// This indicates that the user's authentication token is invalid or expired.
/// </summary>
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException() 
        : base("Authentication failed. Please log in again.")
    {
    }

    public UnauthorizedException(string message) 
        : base(message)
    {
    }

    public UnauthorizedException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

