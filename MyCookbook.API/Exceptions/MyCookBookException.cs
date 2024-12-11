using System;

namespace MyCookbook.API.Exceptions;

public class MyCookBookException : Exception
{
    protected MyCookBookException(
        string message,
        Exception? innerException = null)
        : base(
            message,
            innerException)
    {
    }
}