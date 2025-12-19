# MyCookbook.App.Tests

Unit tests for the MyCookbook mobile application.

## Running Tests

```bash
dotnet test MyCookbook.App.Tests/MyCookbook.App.Tests.csproj
```

## Test Coverage

### Services
- `RecipeServiceTests` - Tests for recipe-related API operations
- More service tests to be added for AccountService, AuthorService, SearchService

### Helpers
- `ErrorMessageHelperTests` - Tests for error message conversion logic

### ViewModels
- To be added: Tests for HomePageViewModel, RecipeViewModel, MyCookbookViewModel, etc.

### Converters
- To be added: Tests for value converters used in XAML bindings

## Test Structure

Tests follow the AAA pattern:
- **Arrange**: Set up test data and mocks
- **Act**: Execute the method being tested
- **Assert**: Verify the results using FluentAssertions

## Dependencies

- **xUnit**: Test framework
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Fluent assertion library for readable tests

