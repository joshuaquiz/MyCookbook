using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MyCookbook.App.Implementations;
using MyCookbook.App.Services;
using MyCookbook.Common.ApiModels;
using Xunit;

namespace MyCookbook.App.Tests.Services;

public class RecipeServiceTests
{
    private readonly Mock<CookbookHttpClient> _mockHttpClient;
    private readonly RecipeService _recipeService;

    public RecipeServiceTests()
    {
        _mockHttpClient = new Mock<CookbookHttpClient>();
        _recipeService = new RecipeService(_mockHttpClient.Object);
    }

    [Fact]
    public async Task GetRecipeAsync_WithValidGuid_ReturnsRecipe()
    {
        // Arrange
        var recipeGuid = Guid.NewGuid();
        var expectedRecipe = new RecipeModel
        {
            Guid = recipeGuid,
            Name = "Test Recipe",
            Description = "Test Description"
        };

        _mockHttpClient
            .Setup(x => x.Get<RecipeModel>(
                $"/api/Recipe/{recipeGuid}",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRecipe);

        // Act
        var result = await _recipeService.GetRecipeAsync(recipeGuid);

        // Assert
        result.Should().NotBeNull();
        result.Guid.Should().Be(recipeGuid);
        result.Name.Should().Be("Test Recipe");
    }

    [Fact]
    public async Task TrackRecipeViewAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var recipeGuid = Guid.NewGuid();

        _mockHttpClient
            .Setup(x => x.Post<object, object>(
                $"/api/Recipe/{recipeGuid}/view",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _recipeService.TrackRecipeViewAsync(recipeGuid);

        // Assert
        _mockHttpClient.Verify(
            x => x.Post<object, object>(
                $"/api/Recipe/{recipeGuid}/view",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HeartRecipeAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var recipeGuid = Guid.NewGuid();

        _mockHttpClient
            .Setup(x => x.Post<object, object>(
                $"/api/Recipe/{recipeGuid}/heart",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _recipeService.HeartRecipeAsync(recipeGuid);

        // Assert
        _mockHttpClient.Verify(
            x => x.Post<object, object>(
                $"/api/Recipe/{recipeGuid}/heart",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPopularRecipesAsync_ReturnsRecipeList()
    {
        // Arrange
        var expectedRecipes = new System.Collections.Generic.List<RecipeSummaryViewModel>
        {
            new() { Guid = Guid.NewGuid(), Name = "Recipe 1" },
            new() { Guid = Guid.NewGuid(), Name = "Recipe 2" }
        };

        _mockHttpClient
            .Setup(x => x.Get<System.Collections.Generic.List<RecipeSummaryViewModel>>(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRecipes);

        // Act
        var result = await _recipeService.GetPopularRecipesAsync(10, 0);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Recipe 1");
    }
}

