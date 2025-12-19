using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyCookbook.App.Implementations;
using Xunit;

namespace MyCookbook.App.Tests.Implementations;

public class ImageCacheServiceTests
{
    private readonly Mock<ILogger<ImageCacheService>> _mockLogger;
    private readonly ImageCacheService _service;

    public ImageCacheServiceTests()
    {
        _mockLogger = new Mock<ILogger<ImageCacheService>>();
        _service = new ImageCacheService(_mockLogger.Object);
    }

    [Fact]
    public async Task GetCachedImagePathAsync_WithNullUrl_ReturnsNull()
    {
        // Act
        var result = await _service.GetCachedImagePathAsync(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ClearCacheAsync_DoesNotThrowException()
    {
        // Act
        Func<Task> act = async () => await _service.ClearCacheAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetCacheSizeAsync_ReturnsNonNegativeValue()
    {
        // Act
        var size = await _service.GetCacheSizeAsync();

        // Assert
        size.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ClearExpiredImagesAsync_WithValidTimeSpan_DoesNotThrowException()
    {
        // Arrange
        var maxAge = TimeSpan.FromDays(1);

        // Act
        Func<Task> act = async () => await _service.ClearExpiredImagesAsync(maxAge);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnforceCacheSizeLimitAsync_WithValidSize_DoesNotThrowException()
    {
        // Arrange
        var maxSize = 100 * 1024 * 1024; // 100MB

        // Act
        Func<Task> act = async () => await _service.EnforceCacheSizeLimitAsync(maxSize);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Act
        Action act = () =>
        {
            _service.Dispose();
            _service.Dispose();
        };

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("https://example.com/image.jpg")]
    [InlineData("https://example.com/image.png")]
    [InlineData("https://example.com/path/to/image.webp")]
    public async Task GetCachedImagePathAsync_WithInvalidUrl_ReturnsNull(string urlString)
    {
        // Arrange
        var url = new Uri(urlString);

        // Act - This will fail to download from a fake URL
        var result = await _service.GetCachedImagePathAsync(url);

        // Assert - Should return null on download failure
        result.Should().BeNull();
    }

    [Fact]
    public async Task ClearCacheAsync_ReducesCacheSize()
    {
        // Arrange
        await _service.ClearCacheAsync();
        var initialSize = await _service.GetCacheSizeAsync();

        // Act
        await _service.ClearCacheAsync();
        var finalSize = await _service.GetCacheSizeAsync();

        // Assert
        finalSize.Should().BeLessThanOrEqualTo(initialSize);
    }

    [Fact]
    public async Task EnforceCacheSizeLimitAsync_WithZeroLimit_ClearsAllCache()
    {
        // Act
        await _service.EnforceCacheSizeLimitAsync(0);
        var size = await _service.GetCacheSizeAsync();

        // Assert
        size.Should().Be(0);
    }

    [Fact]
    public async Task ClearExpiredImagesAsync_WithZeroTimeSpan_ClearsAllImages()
    {
        // Act
        await _service.ClearExpiredImagesAsync(TimeSpan.Zero);
        var size = await _service.GetCacheSizeAsync();

        // Assert
        size.Should().Be(0);
    }
}

