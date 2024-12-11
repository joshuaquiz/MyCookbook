using System;

namespace MyCookbook.App.Exceptions;

public sealed class NoInternetException : Exception
{
    public NoInternetException()
        : base("Check your network connection and try again.")
    {
    }
}