using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyCookbook.App.Interfaces;
using MyCookbook.App.Services;
using MyCookbook.App.ViewModels;
using MyCookbook.Common.ApiModels;
using Xunit;

namespace MyCookbook.App.Tests.ViewModels;

public class RecipeViewModelTests
{
    private readonly Mock<IRecipeService> _mockRecipeService;
    private readonly Mock<ICookbookStorage> _mockCookbookStorage;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ILogger<RecipeViewModel>> _mockLogger;
    private readonly RecipeViewModel _viewModel;

    public RecipeViewModelTests()
    {
        _mockRecipeService = new Mock<IRecipeService>();
        _mockCookbookStorage = new Mock<ICookbookStorage>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<RecipeViewModel>>();

        _viewModel = new RecipeViewModel(
            _mockRecipeService.Object,
            _mockCookbookStorage.Object,
            _mockNotificationService.Object,
            _mockLogger.Object);
    }

    [Theory]
    [InlineData(4, 2, 0.5)]  // Halving the recipe
    [InlineData(4, 8, 2.0)]  // Doubling the recipe
    [InlineData(4, 4, 1.0)]  // Same servings
    [InlineData(4, 6, 1.5)]  // 1.5x the recipe
    [InlineData(2, 3, 1.5)]  // 1.5x from 2 servings
    public void UpdateServingsMultiplier_WithValidServings_CalculatesCorrectMultiplier(
        int originalServings, 
        int newServings, 
        decimal expectedMultiplier)
    {
        // Arrange
        var recipe = new RecipeModel
        {
            Guid = Guid.NewGuid(),
            Name = "Test Recipe",
            Servings = originalServings,
            Ingredients = new List<RecipeIngredientModel>
            {
                new RecipeIngredientModel
                {
                    Name = "Flour",
                    Quantity = 2.0m,
                    Unit = "cups"
                }
            }
        };
        _viewModel.Recipe = recipe;

        // Act
        _viewModel.UpdateServingsMultiplier(newServings);

        // Assert
        _viewModel.ServingsMultiplier.Should().Be(expectedMultiplier);
    }

    [Fact]
    public void UpdateServingsMultiplier_WithZeroOriginalServings_DoesNotUpdateMultiplier()
    {
        // Arrange
        var recipe = new RecipeModel
        {
            Guid = Guid.NewGuid(),
            Name = "Test Recipe",
            Servings = 0,
            Ingredients = new List<RecipeIngredientModel>()
        };
        _viewModel.Recipe = recipe;
        var originalMultiplier = _viewModel.ServingsMultiplier;

        // Act
        _viewModel.UpdateServingsMultiplier(4);

        // Assert
        _viewModel.ServingsMultiplier.Should().Be(originalMultiplier);
    }

    [Fact]
    public void UpdateServingsMultiplier_WithNullOriginalServings_DoesNotUpdateMultiplier()
    {
        // Arrange
        var recipe = new RecipeModel
        {
            Guid = Guid.NewGuid(),
            Name = "Test Recipe",
            Servings = null,
            Ingredients = new List<RecipeIngredientModel>()
        };
        _viewModel.Recipe = recipe;
        var originalMultiplier = _viewModel.ServingsMultiplier;

        // Act
        _viewModel.UpdateServingsMultiplier(4);

        // Assert
        _viewModel.ServingsMultiplier.Should().Be(originalMultiplier);
    }

    [Theory]
    [InlineData(4, 2, 1.0, 0.5)]   // 1 cup becomes 0.5 cups
    [InlineData(4, 8, 2.5, 5.0)]   // 2.5 cups becomes 5 cups
    [InlineData(2, 6, 0.5, 1.5)]   // 0.5 cups becomes 1.5 cups
    public void UpdateServingsMultiplier_UpdatesIngredientQuantities(
        int originalServings,
        int newServings,
        decimal originalQuantity,
        decimal expectedQuantity)
    {
        // Arrange
        var recipe = new RecipeModel
        {
            Guid = Guid.NewGuid(),
            Name = "Test Recipe",
            Servings = originalServings,
            Ingredients = new List<RecipeIngredientModel>
            {
                new RecipeIngredientModel
                {
                    Name = "Flour",
                    Quantity = originalQuantity,
                    Unit = "cups"
                }
            }
        };
        _viewModel.Recipe = recipe;

        // Act
        _viewModel.UpdateServingsMultiplier(newServings);

        // Assert
        var ingredient = _viewModel.Ingredients.First();
        var scaledQuantity = ingredient.Quantity * _viewModel.ServingsMultiplier;
        scaledQuantity.Should().Be(expectedQuantity);
    }
}

