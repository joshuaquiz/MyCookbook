using System;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using MyCookbook.App.Helpers;
using Xunit;

namespace MyCookbook.App.Tests.Helpers;

public class ErrorMessageHelperTests
{
    [Fact]
    public void GetUserFriendlyMessage_WithUnauthorizedException_ReturnsSessionExpiredMessage()
    {
        // Arrange
        var exception = new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);

        // Act
        var result = ErrorMessageHelper.GetUserFriendlyMessage(exception);

        // Assert
        result.Should().Be("Your session has expired. Please log in again.");
    }

    [Fact]
    public void GetUserFriendlyMessage_WithNotFoundException_ReturnsNotFoundMessage()
    {
        // Arrange
        var exception = new HttpRequestException("Not Found", null, HttpStatusCode.NotFound);

        // Act
        var result = ErrorMessageHelper.GetUserFriendlyMessage(exception);

        // Assert
        result.Should().Be("The requested item could not be found.");
    }

    [Fact]
    public void GetUserFriendlyMessage_WithTaskCanceledException_ReturnsTimeoutMessage()
    {
        // Arrange
        var exception = new TaskCanceledException("Request timed out");

        // Act
        var result = ErrorMessageHelper.GetUserFriendlyMessage(exception);

        // Assert
        result.Should().Be("Request timed out. Please check your connection and try again.");
    }

    [Fact]
    public void GetUserFriendlyMessage_WithGenericHttpException_ReturnsNetworkErrorMessage()
    {
        // Arrange
        var exception = new HttpRequestException("Network error");

        // Act
        var result = ErrorMessageHelper.GetUserFriendlyMessage(exception);

        // Assert
        result.Should().Be("Network error. Please check your connection and try again.");
    }

    [Fact]
    public void IsNetworkError_WithHttpRequestException_ReturnsTrue()
    {
        // Arrange
        var exception = new HttpRequestException("Network error");

        // Act
        var result = ErrorMessageHelper.IsNetworkError(exception);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNetworkError_WithTaskCanceledException_ReturnsTrue()
    {
        // Arrange
        var exception = new TaskCanceledException();

        // Act
        var result = ErrorMessageHelper.IsNetworkError(exception);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsNetworkError_WithOtherException_ReturnsFalse()
    {
        // Arrange
        var exception = new InvalidOperationException();

        // Act
        var result = ErrorMessageHelper.IsNetworkError(exception);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticationError_WithUnauthorizedException_ReturnsTrue()
    {
        // Arrange
        var exception = new HttpRequestException("Unauthorized", null, HttpStatusCode.Unauthorized);

        // Act
        var result = ErrorMessageHelper.IsAuthenticationError(exception);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticationError_WithOtherException_ReturnsFalse()
    {
        // Arrange
        var exception = new HttpRequestException("Not Found", null, HttpStatusCode.NotFound);

        // Act
        var result = ErrorMessageHelper.IsAuthenticationError(exception);

        // Assert
        result.Should().BeFalse();
    }
}

