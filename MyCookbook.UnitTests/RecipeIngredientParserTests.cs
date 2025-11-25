using FluentAssertions;
using FluentAssertions.Execution;
using MyCookbook.API.Implementations;
using MyCookbook.Common.Database;
using MyCookbook.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MyCookbook.UnitTests;

public static class RecipeIngredientParserTests
{
    [Theory]
    [InlineData("", MeasurementUnit.Unit)]
    [InlineData("Jeep", MeasurementUnit.Unit)]
    [InlineData("Unit", MeasurementUnit.Unit)]
    [InlineData("Units", MeasurementUnit.Unit)]
    [InlineData("UNIT", MeasurementUnit.Unit)]
    [InlineData("UNITS", MeasurementUnit.Unit)]
    [InlineData("TeaSpoon", MeasurementUnit.TeaSpoon)]
    [InlineData("TeaSpoons", MeasurementUnit.TeaSpoon)]
    [InlineData("Teaspoon", MeasurementUnit.TeaSpoon)]
    [InlineData("Teaspoons", MeasurementUnit.TeaSpoon)]
    [InlineData("TeAsPoOnS", MeasurementUnit.TeaSpoon)]
    [InlineData("Bunches", MeasurementUnit.Bunch)]
    [InlineData("ounce", MeasurementUnit.Ounce)]
    [InlineData("Ounce", MeasurementUnit.Ounce)]
    [InlineData("Ounces", MeasurementUnit.Ounce)]
    [InlineData("Fillet", MeasurementUnit.Fillet)]
    [InlineData("Fillets", MeasurementUnit.Fillet)]
    [InlineData("Inch", MeasurementUnit.Inch)]
    [InlineData("inches", MeasurementUnit.Inch)]
    [InlineData("can", MeasurementUnit.Can)]
    [InlineData("Cans", MeasurementUnit.Can)]
    public static void TestGetMeasurement(
        string input,
        MeasurementUnit? expected)
    {
        var result = RecipeIngredientParser.GetMeasurement(
            input);
        using (new AssertionScope())
        {
            result.ParsedValue
                .Should()
                .Be(expected);
            result.MatchedValue
                .Should()
                .Be(input);
        }
    }

    [Theory]
    [InlineData("", "1")]
    [InlineData("Jeep", "1")]
    [InlineData("1", "1")]
    [InlineData("10", "10")]
    [InlineData("100", "100")]
    [InlineData(" 100 ", "100")]
    [InlineData("5 1/2", "5.5")]
    [InlineData("2 3/4", "2.75")]
    [InlineData("1 1-ounce", "1")]
    [InlineData("1 10-ounce", "1")]
    [InlineData("10 1-ounce", "10")]
    [InlineData("1/4 cup pitted dates, chopped", "0.25")]
    [InlineData("One 8-count can homestyle biscuits, such as Pillsbury Grands! Southern Homestyle Buttermilk Biscuits", "1")]
    public static void TestGetQuantity(
        string input,
        string? value)
    {
        var expected = value == null
            ? (decimal?)null
            : decimal.Parse(value);
        var result = RecipeIngredientParser.GetQuantity(
            input);
        using (new AssertionScope())
        {
            result.NumberValue?.ParsedValue
                .Should()
                .Be(expected);
        }
    }

    [Theory]
    [InlineData(MeasurementUnit.Unit, MeasurementUnit.Unit, "1")]
    [InlineData(MeasurementUnit.Unit, MeasurementUnit.Piece, "1")]
    [InlineData(MeasurementUnit.Unit, MeasurementUnit.Fillet, null)]
    [InlineData(MeasurementUnit.Slice, MeasurementUnit.Slice, "1")]
    [InlineData(MeasurementUnit.Slice, MeasurementUnit.Cup, null)]
    [InlineData(MeasurementUnit.Clove, MeasurementUnit.Clove, "1")]
    [InlineData(MeasurementUnit.Clove, MeasurementUnit.Unit, null)]
    [InlineData(MeasurementUnit.Bunch, MeasurementUnit.Bunch, "1")]
    [InlineData(MeasurementUnit.Bunch, MeasurementUnit.Unit, null)]
    [InlineData(MeasurementUnit.Fillet, MeasurementUnit.Fillet, "1")]
    [InlineData(MeasurementUnit.Fillet, MeasurementUnit.Unit, null)]
    [InlineData(MeasurementUnit.Cup, MeasurementUnit.Cup, "1")]
    [InlineData(MeasurementUnit.Cup, MeasurementUnit.TableSpoon, "16")]
    [InlineData(MeasurementUnit.Cup, MeasurementUnit.TeaSpoon, "48")]
    [InlineData(MeasurementUnit.Cup, MeasurementUnit.Ounce, "8")]
    [InlineData(MeasurementUnit.Cup, MeasurementUnit.Unit, null)]
    [InlineData(MeasurementUnit.TableSpoon, MeasurementUnit.TableSpoon, "1")]
    [InlineData(MeasurementUnit.TableSpoon, MeasurementUnit.TeaSpoon, "3")]
    [InlineData(MeasurementUnit.TableSpoon, MeasurementUnit.Unit, null)]
    [InlineData(MeasurementUnit.TeaSpoon, MeasurementUnit.TeaSpoon, "1")]
    [InlineData(MeasurementUnit.TeaSpoon, MeasurementUnit.Unit, null)]
    [InlineData(MeasurementUnit.Ounce, MeasurementUnit.TableSpoon, "2")]
    [InlineData(MeasurementUnit.Ounce, MeasurementUnit.TeaSpoon, "6")]
    [InlineData(MeasurementUnit.Ounce, MeasurementUnit.Ounce, "1")]
    [InlineData(MeasurementUnit.Ounce, MeasurementUnit.Unit, null)]
    public static void TestGetConversionRate(
        MeasurementUnit from,
        MeasurementUnit to,
        string? value)
    {
        var expected = value == null
            ? (decimal?)null
            : decimal.Parse(value);
        var result = RecipeIngredientParser.GetConversionRate(
            from,
            to);
        using (new AssertionScope())
        {
            result
                .Should()
                .Be(expected);
        }
    }

    [Theory]
    [InlineData(MeasurementUnit.Unit, MeasurementUnit.Unit, MeasurementUnit.Unit)]
    [InlineData(MeasurementUnit.Unit, MeasurementUnit.Piece, MeasurementUnit.Piece)]
    [InlineData(MeasurementUnit.Fillet, MeasurementUnit.Piece, MeasurementUnit.Piece)]
    [InlineData(MeasurementUnit.Fillet, MeasurementUnit.Cup, MeasurementUnit.Cup)]
    [InlineData(MeasurementUnit.TableSpoon, MeasurementUnit.Cup, MeasurementUnit.TableSpoon)]
    [InlineData(MeasurementUnit.TableSpoon, MeasurementUnit.TableSpoon, MeasurementUnit.TableSpoon)]
    [InlineData(MeasurementUnit.TableSpoon, MeasurementUnit.TeaSpoon, MeasurementUnit.TeaSpoon)]
    [InlineData(MeasurementUnit.TeaSpoon, MeasurementUnit.Cup, MeasurementUnit.TeaSpoon)]
    [InlineData(MeasurementUnit.TeaSpoon, MeasurementUnit.TableSpoon, MeasurementUnit.TeaSpoon)]
    [InlineData(MeasurementUnit.TeaSpoon, MeasurementUnit.TeaSpoon, MeasurementUnit.TeaSpoon)]
    [InlineData(MeasurementUnit.Ounce, MeasurementUnit.TeaSpoon, MeasurementUnit.Ounce)]
    [InlineData(MeasurementUnit.Ounce, MeasurementUnit.TableSpoon, MeasurementUnit.Ounce)]
    [InlineData(MeasurementUnit.Ounce, MeasurementUnit.Cup, MeasurementUnit.Ounce)]
    public static void TestGetLowestMeasurement(
        MeasurementUnit input1,
        MeasurementUnit input2,
        MeasurementUnit expected)
    {
        var result = RecipeIngredientParser.GetLowestMeasurement(
            input1,
            input2);
        using (new AssertionScope())
        {
            result
                .Should()
                .Be(expected);
        }
    }

    /*[Theory]
    [InlineData("1 1/4 cups raisins", "raisins", new[] { "1 1/4", "cups" })]
    [InlineData("4 ounces gingersnap cookies (about 16)", "gingersnap cookies", new[] { "4", "ounces" })]
    [InlineData("2 tablespoons plus 1 teaspoon kosher salt, divided", "kosher salt", new[] { "2", "1", "tablespoons", "teaspoon" })]
    [InlineData("1 tablespoon kosher salt, plus more for seasoning", "kosher salt", new[] { "1", "tablespoon" })]
    [InlineData("1-inch piece ginger, thinly sliced", "ginger", new[] { "1", "inch" })]
    public static void TestGetName(
        string input,
        string expected,
        string[] otherTokens)
    {
        var result = RecipeIngredientParser.GetName(
            input,
            otherTokens,
            MeasurementUnit.Unit);
        using (new AssertionScope())
        {
            result.ParsedValue
                .Should()
                .Be(expected);
        }
    }*/

    /*[Theory]
    [InlineData("1 1/4 cups raisins", "", new[] { "1 1/4", "cups", "raisins" })]
    [InlineData("4 ounces gingersnap cookies (about 16)", "(about 16)", new[] { "4", "ounces", "gingersnap cookies" })]
    [InlineData("4 ounces gingersnap cookies (about 16) (Some more info)", "(about 16, Some more info)", new[] { "4", "ounces", "gingersnap cookies" })]
    [InlineData("2 tablespoons plus 1 teaspoon kosher salt, divided", "divided", new[] { "2", "1", "tablespoons", "teaspoon", "kosher salt" })]
    [InlineData("1 tablespoon kosher salt, plus more for seasoning", "more for seasoning", new[] { "1", "tablespoon", "kosher salt" })]
    [InlineData("1-inch piece ginger, thinly sliced", "thinly sliced", new[] { "1", "inch", "ginger" })]
    public static void TestGetNotes(
        string input,
        string expected,
        string[] otherTokens)
    {
        var result = RecipeIngredientParser.GetNotes(
            input,
            otherTokens);
        using (new AssertionScope())
        {
            result
                .Should()
                .Be(expected);
        }
    }*/

    [Theory]
    [InlineData("1", "1")]
    [InlineData("1/2", "0.5")]
    [InlineData("4 ", "4")]
    [InlineData(" 4", "4")]
    [InlineData(" 4 ", "4")]
    [InlineData("3/4", "0.75")]
    [InlineData("1/4 ", "0.25")]
    [InlineData(" 4  1/5", "4.2")]
    [InlineData("4  1/5", "4.2")]
    [InlineData("4  1 /5", "4.2")]
    [InlineData("4  1/ 5", "4.2")]
    [InlineData("4  1 / 5", "4.2")]
    public static void TestParseFractionSection(
        string input,
        string? value)
    {
        var expected = value == null
            ? (decimal?)null
            : decimal.Parse(value);
        var result = RecipeIngredientParser.ParseFractionSection(
            input);
        using (new AssertionScope())
        {
            result
                .Should()
                .Be(expected);
        }
    }

    [Theory]
    [InlineData("Text", "Text")]
    [InlineData("Text (stuff)", "Text")]
    [InlineData("Text )haha(", "Text )haha(")]
    [InlineData("Text (stuff) (stuff)", "Text")]
    [InlineData("Text (stuff) )haha(", "Text  )haha(")]
    public static void TestRemoveParentheticalSets(
        string input,
        string expected)
    {
        var result = RecipeIngredientParser.ReplaceParentheticalSets(
            input);
        using (new AssertionScope())
        {
            result
                .Should()
                .Be(expected);
        }
    }

    [Theory]
    [InlineData("Text", new string[0])]
    [InlineData("Text (stuff)", new[] { "stuff" })]
    [InlineData("Text )haha(", new string[0])]
    [InlineData("Text (stuff) (stuff)", new[] { "stuff", "stuff" })]
    [InlineData("Text (stuff) )haha(", new[] { "stuff" })]
    public static void TestGetParentheticalSetsContents(
        string input,
        string[] expected)
    {
        var result = RecipeIngredientParser.GetParentheticalSetsContents(
            input);
        using (new AssertionScope())
        {
            result
                .Should()
                .BeEquivalentTo(expected);
        }
    }

    [Theory]
    [MemberData(nameof(TestGetIngredientsData))]
    public static void TestGetIngredients(TestCaseData data)
    {
        var results = RecipeIngredientParser.GetIngredients(
                RecipeIngredientParser.Sanitize(data.RawIngredient))
            .ToList();
        using var _ = new AssertionScope();
        results.Should().HaveCount(data.Lines.Count);
        for (var i = 0; i < results.Count; i++)
        {
            var result = results[i];
            result.Should().Be(data.Lines.ElementAtOrDefault(i).RawData);
        }
    }

    [Theory]
    [MemberData(nameof(TestGetIngredientsData))]
    public static void TestGetRawQuantities(TestCaseData data)
    {
        using var _ = new AssertionScope();
        foreach (var ingredientLine in data.Lines)
        {
            var quantities = RecipeIngredientParser.GetRawQuantities(
                    ingredientLine.RawData)
                .ToList();
            quantities.Should().HaveCount(ingredientLine.QuantityLines.Count);
            for (var i = 0; i < ingredientLine.QuantityLines.Count; i++)
            {
                var result = quantities[i];
                var testDataForQuantity = ingredientLine.QuantityLines.ElementAtOrDefault(i);
                result.QuantityType.ParsedValue.Should().Be(testDataForQuantity.QuantityType.ParsedValue);
                result.QuantityType.MatchedValue.Should().Be(testDataForQuantity.QuantityType.MatchedValue);
                result.QuantityType.MatchStartIndex.Should().Be(testDataForQuantity.QuantityType.MatchStartIndex);
                result.Measurement.ParsedValue.Should().Be(testDataForQuantity.Measurement.ParsedValue);
                result.Measurement.MatchedValue.Should().Be(testDataForQuantity.Measurement.MatchedValue);
                result.Measurement.MatchStartIndex.Should().Be(testDataForQuantity.Measurement.MatchStartIndex);
                if (testDataForQuantity.NumberValue == null)
                {
                    result.NumberValue.Should().BeNull();
                }
                else
                {
                    result.NumberValue?.ParsedValue.Should().Be(testDataForQuantity.NumberValue.ParsedValue);
                    result.NumberValue?.MatchedValue.Should().Be(testDataForQuantity.NumberValue.MatchedValue);
                    result.NumberValue?.MatchStartIndex.Should().Be(testDataForQuantity.NumberValue.MatchStartIndex);
                }

                if (testDataForQuantity.MinValue == null)
                {
                    result.MinValue.Should().BeNull();
                }
                else
                {
                    result.MinValue?.ParsedValue.Should().Be(testDataForQuantity.MinValue.ParsedValue);
                    result.MinValue?.MatchedValue.Should().Be(testDataForQuantity.MinValue.MatchedValue);
                    result.MinValue?.MatchStartIndex.Should().Be(testDataForQuantity.MinValue.MatchStartIndex);
                }

                if (testDataForQuantity.MaxValue == null)
                {
                    result.MaxValue.Should().BeNull();
                }
                else
                {
                    result.MaxValue?.ParsedValue.Should().Be(testDataForQuantity.MaxValue.ParsedValue);
                    result.MaxValue?.MatchedValue.Should().Be(testDataForQuantity.MaxValue.MatchedValue);
                    result.MaxValue?.MatchStartIndex.Should().Be(testDataForQuantity.MaxValue.MatchStartIndex);
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(TestGetIngredientsData))]
    public static void TestGetParsedRecipeStepIngredients(TestCaseData data)
    {
        var recipeStepGuid = Guid.NewGuid();
        List<RecipeStepIngredient> parsed;
        Exception? exception = null;

        try
        {
            parsed = RecipeIngredientParser.GetParsedRecipeStepIngredients(
                    recipeStepGuid,
                    data.RawIngredient,
                    [])
                .ToList();
        }
        catch (Exception e)
        {
            parsed = [];
            exception = e;
        }

        using var _ = new AssertionScope();
        parsed.Should().HaveCount(data.TotalRecipeStepIngredients);
        if (data.Exception != null)
        {
            exception.Should().NotBeNull();
            exception?.Message.Should().Be(data.Exception.Message);
        }
        else
        {
            exception.Should().BeNull();
        }

        for (var i = 0; i < data.RecipeStepIngredients.Count; i++)
        {
            var expected = data.RecipeStepIngredients[i];
            var actual = parsed[i];

            actual.Should()
                .BeEquivalentTo(
                    expected,
                    options =>
                        options
                            .Excluding(y => y.StepIngredientId)
                            .Excluding(y => y.Ingredient.IngredientId)
                            .Excluding(y => y.RecipeStep)
                            .Excluding(y => y.RecipeStepId));
            actual.RecipeStepId.Should().Be(recipeStepGuid);
        }
    }

    public static readonly TheoryData<TestCaseData> TestGetIngredientsData =
    [
        new(
            "\"Something oniony,\" like 1 scallion or 1/4 small red onion, chopped",
            [
                new IngredientLine(
                    "Something oniony, like 1 scallion or 1/4 small red onion, chopped",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 23),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 23),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Something oniony, like 1 scallion or 1/4 small red onion, chopped", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Onion"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "Something like scallion or 1/4 small red onion, chopped",
                    RawText = "Something oniony, like 1 scallion or 1/4 small red onion, chopped"
                }
            ]),
        new(
            "(about 2 1/4 teaspoons)",
            [
                new IngredientLine(
                    "(about 2 1/4 teaspoons)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Unknown, "                       ", 0),
                            null,
                            null,
                            null,
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "                       ", 0))
                    ])
            ],
            0,
            new Exception("No valid name found. '(about 2 1/4 teaspoons)'"),
            []),
        new(
            "*3 egg yolks",
            [
                new IngredientLine(
                    "3 egg yolks",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "3", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(3M, "3", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "3 egg yolks", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Egg yolks"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 3M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "",
                    RawText = "3 egg yolks"
                }
            ]),
        new(
            "1 ( 28-ounce) can diced tomatoes with juice",
            [
                new IngredientLine(
                    "1 ( 28-ounce) can diced tomatoes with juice",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Can, "can", 14))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Tomatoes"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "(28-ounce) diced with juice",
                    RawText = "1 ( 28-ounce) can diced tomatoes with juice"
                }
            ]),
        new(
            "1 (1 1/2 pound) piece flank steak",
            [
                new IngredientLine(
                    "1 (1 1/2 pound) piece flank steak",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Piece, "piece", 16))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Flank steak"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Piece,
                    Notes = "(1 1/2 pound)",
                    RawText = "1 (1 1/2 pound) piece flank steak"
                }
            ]),
        new(
            "1 (1 1/2 to 2-pound) butternut squash, peeled, seeded, and cut into 1-inch cubes",
            [
                new IngredientLine(
                    "1 (1 1/2 to 2-pound) butternut squash, peeled, seeded, and cut into 1-inch cubes",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1                    butternut squash, peeled, seeded, and cut into 1-inch cubes", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Butternut squash"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "(1 1/2 to 2-pound) peeled, seeded, and cut into 1-inch cubes",
                    RawText = "1 (1 1/2 to 2-pound) butternut squash, peeled, seeded, and cut into 1-inch cubes"
                }
            ]),
        new(
            "1 (1- pound) acorn squash",
            [
                new IngredientLine(
                    "1 (1- pound) acorn squash",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1            acorn squash", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Acorn squash"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "(1- pound)",
                    RawText = "1 (1- pound) acorn squash"
                }
            ]),
        new(
            "1 (1.1 pound; 500 gram) loaf panettone bread, baking paper removed",
            [
                new IngredientLine(
                    "1 (1.1 pound: 500 gram) loaf panettone bread, baking paper removed",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Loaf, "loaf", 24))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Panettone bread"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Loaf,
                    Notes = "(1.1 pound: 500 gram) baking paper removed",
                    RawText = "1 (1.1 pound: 500 gram) loaf panettone bread, baking paper removed"
                }
            ]),
        new(
            "1 (1/4-ounce) package active dry yeast (2 1/4 teaspoons)",
            [
                new IngredientLine(
                    "1 (1/4-ounce) package active dry yeast (2 1/4 teaspoons)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Package, "package", 14))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Active dry yeast"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Package,
                    Notes = "(1/4-ounce 2 1/4 teaspoons)",
                    RawText = "1 (1/4-ounce) package active dry yeast (2 1/4 teaspoons)"
                }
            ]),
        new(
            "1 (10 3/4-ounce) can chicken broth",
            [
                new IngredientLine(
                    "1 (10 3/4-ounce) can chicken broth",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Can, "can", 17))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Chicken broth"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "(10 3/4-ounce)",
                    RawText = "1 (10 3/4-ounce) can chicken broth"
                }
            ]),
        new(
            "1 (10-ounce) package frozen chopped spinach, cooked and squeezed dry, reserving 1 tablespoon of the liquid",
            [
                new IngredientLine(
                    "1 (10-ounce) package frozen chopped spinach, cooked and squeezed dry, reserving 1 tablespoon of the liquid",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Package, "package", 13))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Frozen chopped spinach"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Package,
                    Notes = "(10-ounce) cooked and squeezed dry, reserving 1 tablespoon of the liquid",
                    RawText = "1 (10-ounce) package frozen chopped spinach, cooked and squeezed dry, reserving 1 tablespoon of the liquid"
                }
            ]),
        new(
            "1 (15.5-ounce) can hominy, drained and rinsed",
            [
                new IngredientLine(
                    "1 (15.5-ounce) can hominy, drained and rinsed",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Can, "can", 15))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Hominy"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "(15.5-ounce) drained and rinsed",
                    RawText = "1 (15.5-ounce) can hominy, drained and rinsed"
                }
            ]),
        new(
            "1 (15-ounce/441 ml) can peeled plum tomatoes",
            [
                new IngredientLine(
                    "1 (15-ounce/441 ml) can peeled plum tomatoes",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Can, "can", 20))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Peeled plum tomatoes"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "(15-ounce/441 ml)",
                    RawText = "1 (15-ounce/441 ml) can peeled plum tomatoes"
                }
            ]),
        new(
            "1 (16 oz.) pkg. Pillsbury� Hot Roll Mix",
            [
                new IngredientLine(
                    "1 (16 oz.) pkg. Pillsbury Hot Roll Mix",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Package, "pkg.", 11))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Pillsbury Hot Roll Mix"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Package,
                    Notes = "(16 oz.)",
                    RawText = "1 (16 oz.) pkg. Pillsbury Hot Roll Mix"
                }
            ]),
        new(
            "1 (16-ounce) package (about 5 1/2 cups) dry broccoli cole slaw",
            [
                new IngredientLine(
                    "1 (16-ounce) package (about 5 1/2 cups) dry broccoli cole slaw",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Package, "package", 13))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Dry broccoli cole slaw"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Package,
                    Notes = "(16-ounce about 5 1/2 cups)",
                    RawText = "1 (16-ounce) package (about 5 1/2 cups) dry broccoli cole slaw"
                }
            ]),
        new(
            "1 (18 1/4-ounce) box devil's food cake mix",
            [
                new IngredientLine(
                    "1 (18 1/4-ounce) box devil's food cake mix",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Package, "box", 17))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Devil's food cake mix"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Package,
                    Notes = "(18 1/4-ounce)",
                    RawText = "1 (18 1/4-ounce) box devil's food cake mix"
                }
            ]),
        new(
            "1 (1-inch piece) ginger, peeled and minced",
            [
                new IngredientLine(
                    "1 (1-inch piece) ginger, peeled and minced",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1                ginger, peeled and minced", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Ginger"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "(1-inch piece) peeled and minced",
                    RawText = "1 (1-inch piece) ginger, peeled and minced"
                }
            ]),
        new(
            "1 (1-inch) cinnamon stick",
            [
                new IngredientLine(
                    "1 (1-inch) cinnamon stick",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Stick, "stick", 20))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Cinnamon"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Stick,
                    Notes = "(1-inch)",
                    RawText = "1 (1-inch) cinnamon stick"
                }
            ]),
        new(
            "1 (1-ounce) piece Parmesan, coarsely chopped",
            [
                new IngredientLine(
                    "1 (1-ounce) piece Parmesan, coarsely chopped",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Piece, "piece", 12))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Parmesan"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Piece,
                    Notes = "(1-ounce) coarsely chopped",
                    RawText = "1 (1-ounce) piece Parmesan, coarsely chopped"
                }
            ]),
        new(
            "1 (1-pound) loaf ciabatta bread, halved horizontally (see Cook's Note)",
            [
                new IngredientLine(
                    "1 (1-pound) loaf ciabatta bread, halved horizontally (see Cook's Note)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Loaf, "loaf", 12))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Ciabatta bread"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Loaf,
                    Notes = "(1-pound) halved horizontally (see Cook's Note)",
                    RawText = "1 (1-pound) loaf ciabatta bread, halved horizontally (see Cook's Note)"
                }
            ]),
        new(
            "1 (1-pound) package frozen green, red and yellow peppers and onions*",
            [
                new IngredientLine(
                    "1 (1-pound) package frozen green, red and yellow peppers and onions",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Package, "package", 12))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Pepper and onion"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Package,
                    Notes = "(1-pound) frozen green, red and yellow",
                    RawText = "1 (1-pound) package frozen green, red and yellow peppers and onions"
                }
            ]),
        new(
            "1 (28-ounce) can whole, peeled tomatoes",
            [
                new IngredientLine(
                    "1 (28-ounce) can whole, peeled tomatoes",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Can, "can", 13))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Tomatoes"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "(28-ounce) whole, peeled",
                    RawText = "1 (28-ounce) can whole, peeled tomatoes"
                }
            ]),
        new(
            "1 (3 rib) roast, about 5 pounds, rimmed of excess but not all fat",
            [
                new IngredientLine(
                    "1 (3 rib) roast, about 5 pounds, rimmed of excess but not all fat",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1         roast, about 5 pounds, rimmed of excess but not all fat", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Roast"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "(3 rib) about 5 pounds, rimmed of excess but not all fat",
                    RawText = "1 (3 rib) roast, about 5 pounds, rimmed of excess but not all fat"
                }
            ]),
        new(
            "1 (3 to 3 1/2-pound) piece center-cut salmon, skin on, pin bones removed, halved",
            [
                new IngredientLine(
                    "1 (3 to 3 1/2-pound) piece center-cut salmon, skin on, pin bones removed, halved",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Piece, "piece", 21))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Center-cut salmon"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Piece,
                    Notes = "(3 to 3 1/2-pound) skin on, pin bones removed, halved",
                    RawText = "1 (3 to 3 1/2-pound) piece center-cut salmon, skin on, pin bones removed, halved"
                }
            ]),
        new(
            "1 (3 to 4-pound) chicken, cut in 8ths",
            [
                new IngredientLine(
                    "1 (3 to 4-pound) chicken, cut in 8ths",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1                chicken, cut in 8ths", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Chicken"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "(3 to 4-pound) cut in 8ths",
                    RawText = "1 (3 to 4-pound) chicken, cut in 8ths"
                }
            ]),
        new(
            "1 (3 to 4-pound) chicken, cut into 1/8's",
            [
                new IngredientLine(
                    "1 (3 to 4-pound) chicken, cut into 1/8's",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1                chicken, cut into 1/8's", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Chicken"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "(3 to 4-pound) cut into 1/8's",
                    RawText = "1 (3 to 4-pound) chicken, cut into 1/8's"
                }
            ]),
        new(
            "1 (6.5 ounce) can clams, drained, juice reserved",
            [
                new IngredientLine(
                    "1 (6.5 ounce) can clams, drained, juice reserved",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Can, "can", 14))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Clams"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "(6.5 ounce) drained, juice reserved",
                    RawText = "1 (6.5 ounce) can clams, drained, juice reserved"
                }
            ]),
        new(
            "1 (6-pound) pork shoulder",
            [
                new IngredientLine(
                    "1 (6-pound) pork shoulder",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1           pork shoulder", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Pork shoulder"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "(6-pound)",
                    RawText = "1 (6-pound) pork shoulder"
                }
            ]),
        new(
            "1 (750 ml) bottle Chianti wine",
            [
                new IngredientLine(
                    "1 (750 ml) bottle Chianti wine",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Bottle, "bottle", 11))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Chianti wine"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Bottle,
                    Notes = "(750 ml)",
                    RawText = "1 (750 ml) bottle Chianti wine"
                }
            ]),
        new(
            "1 1/2 cups (360 grams) cold whole milk (dairy or dairy-free)",
            [
                new IngredientLine(
                    "1 1/2 cups (360 grams) cold whole milk (dairy or dairy-free)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1 1/2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1.5M, "1 1/2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cups", 6))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Cold whole milk"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1.5M,
                    Unit = MeasurementUnit.Cup,
                    Notes = "(360 grams dairy or dairy-free)",
                    RawText = "1 1/2 cups (360 grams) cold whole milk (dairy or dairy-free)"
                }
            ]),
        new(
            "1 1/2 cups (375 milliliters) 2% or whole milk",
            [
                new IngredientLine(
                    "1 1/2 cups (375 milliliters) 2% or whole milk",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1 1/2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1.5M, "1 1/2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cups", 6))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "2% or whole milk"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1.5M,
                    Unit = MeasurementUnit.Cup,
                    Notes = "(375 milliliters)",
                    RawText = "1 1/2 cups (375 milliliters) 2% or whole milk"
                }
            ]),
        new(
            "1 1/2 cups (6 ounces) grated American cheddar, such as Goot Essa Mountain Valley, plus more for the top",
            [
                new IngredientLine(
                    "1 1/2 cups (6 ounces) grated American cheddar, such as Goot Essa Mountain Valley, plus more for the top",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1 1/2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1.5M, "1 1/2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cups", 6))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Grated American cheddar"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1.5M,
                    Unit = MeasurementUnit.Cup,
                    Notes = "(6 ounces) such as Goot Essa Mountain Valley, plus more for the top",
                    RawText = "1 1/2 cups (6 ounces) grated American cheddar, such as Goot Essa Mountain Valley, plus more for the top"
                }
            ]),
        new(
            "1 1/2 cups all-purpose flour, plus more for kneading",
            [
                new IngredientLine(
                    "1 1/2 cups all-purpose flour, plus more for kneading",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1 1/2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1.5M, "1 1/2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cups", 6))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "All-purpose flour"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1.5M,
                    Unit = MeasurementUnit.Cup,
                    Notes = "plus more for kneading",
                    RawText = "1 1/2 cups all-purpose flour, plus more for kneading"
                }
            ]),
        new(
            "1 1/2 cups chopped yellow onions (2 onions)",
            [
                new IngredientLine(
                    "1 1/2 cups chopped yellow onions (2 onions)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1 1/2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1.5M, "1 1/2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cups", 6))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Onion"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1.5M,
                    Unit = MeasurementUnit.Cup,
                    Notes = "chopped yellow (2 onions)",
                    RawText = "1 1/2 cups chopped yellow onions (2 onions)"
                }
            ]),
        new(
            "1 1/2 cups gluten-free flour, such as Cup4Cup�",
            [
                new IngredientLine(
                    "1 1/2 cups gluten-free flour, such as Cup4Cup",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1 1/2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1.5M, "1 1/2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cups", 6))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Gluten-free flour"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1.5M,
                    Unit = MeasurementUnit.Cup,
                    Notes = "such as Cup4Cup",
                    RawText = "1 1/2 cups gluten-free flour, such as Cup4Cup"
                }
            ]),
        new(
            "1 1/4 pounds zucchini, crookneck or pattypan squash, roughly chopped",
            [
                new IngredientLine(
                    "1 1/4 pounds zucchini, crookneck or pattypan squash, roughly chopped",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1 1/4", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1.25M, "1 1/4", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Pound, "pounds", 6))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Zucchini"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1.25M,
                    Unit = MeasurementUnit.Pound,
                    Notes = "crookneck or pattypan squash, roughly chopped",
                    RawText = "1 1/4 pounds zucchini, crookneck or pattypan squash, roughly chopped"
                }
            ]),
        new(
            "1 1/4 sticks (10 tablespoons) unsalted butter, at room temperature, plus more for the bowls and pan",
            [
                new IngredientLine(
                    "1 1/4 sticks (10 tablespoons) unsalted butter, at room temperature, plus more for the bowls and pan",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1 1/4", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1.25M, "1 1/4", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Stick, "sticks", 6))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Unsalted butter"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1.25M,
                    Unit = MeasurementUnit.Stick,
                    Notes = "(10 tablespoons) at room temperature, plus more for the bowls and pan",
                    RawText = "1 1/4 sticks (10 tablespoons) unsalted butter, at room temperature, plus more for the bowls and pan"
                }
            ]),
        new(
            "1 14.5-ounce can no-salt-added diced tomatoes",
            [
                new IngredientLine(
                    "1 14.5-ounce can no-salt-added diced tomatoes",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Can, "can", 13))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Tomatoes"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "14.5-ounce no-salt-added diced",
                    RawText = "1 14.5-ounce can no-salt-added diced tomatoes"
                }
            ]),
        new(
            "�1 carrot, chopped",
            [
                new IngredientLine(
                    "1 carrot, chopped",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 carrot, chopped", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Carrot"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "chopped",
                    RawText = "1 carrot, chopped"
                }
            ]),
        /*new(
            "1 cup (100g) fresh or frozen (thawed) cranberries",
            [
                new IngredientLine(
                    "1 cup (100 g) fresh or frozen (thawed) cranberries",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cup", 2))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Fresh or frozen"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Cup,
                    Notes = "(100 g thawed) cranberries",
                    RawText = "1 cup (100 g) fresh or frozen (thawed) cranberries"
                }
            ]),*/
        new(
            "1 cup (115 grams) grated mozzarella",
            [
                new IngredientLine(
                    "1 cup (115 grams) grated mozzarella",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cup", 2))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Grated mozzarella"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Cup,
                    Notes = "(115 grams)",
                    RawText = "1 cup (115 grams) grated mozzarella"
                }
            ]),
        /*new(
            "1 -inch piece fresh ginger, peeled and minced",
            [
                new IngredientLine(
                    "1 -inch piece fresh ginger, peeled and minced",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Piece, "piece", 8))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Fresh ginger"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Piece,
                    Notes = "1 -inch peeled and minced",
                    RawText = "1 -inch piece fresh ginger, peeled and minced"
                }
            ]),*/
        /*new(
            "1 large (6 inches long or more) or 2 small (4 inches long or less) fish heads from cod or haddock, split lengthwise, gills removed and rinsed clean of any blood.",
            [
                new IngredientLine(
                    "1 large (6 inches long or more) or 2 small (4 inches long or less) fish heads from cod or haddock, split lengthwise, gills removed and rinsed clean of any blood.",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 large                         or 2 small                         fish heads from cod or haddock, split lengthwise, gills removed and rinsed clean of any blood.", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),*/
        /*new(
            "1 large or 2 medium Yukon Gold potatoes, peeled and diced",
            [
                new IngredientLine(
                    "1 large or 2 medium Yukon Gold potatoes, peeled and diced",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 large or 2 medium Yukon Gold potatoes, peeled and diced", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),*/
        new(
            "1 medium red onion, thinly sliced, soaked in ice water for 10 minutes and drained",
            [
                new IngredientLine(
                    "1 medium red onion, thinly sliced, soaked in ice water for 10 minutes and drained",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 medium red onion, thinly sliced, soaked in ice water for 10 minutes and drained", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Onion"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "medium red thinly sliced, soaked in ice water for 10 minutes and drained",
                    RawText = "1 medium red onion, thinly sliced, soaked in ice water for 10 minutes and drained"
                }
            ]),
        new(
            "1 to 1 1/2 cups beef stock�",
            [
                new IngredientLine(
                    "1 to 1 1/2 cups beef stock",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Range, "1 to 1 1/2", 0),
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<decimal>(1.5M, "1 1/2", 5),
                            null,
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cups", 11))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Beef stock"
                    },
                    QuantityType = QuantityType.Range,
                    MinValue = 1M,
                    MaxValue = 1.5M,
                    NumberValue = null,
                    Unit = MeasurementUnit.Cup,
                    Notes = "",
                    RawText = "1 to 1 1/2 cups beef stock"
                }
            ]),
        new(
            "1 to 3 kalamata or nicoise olives, pitted and chopped�",
            [
                new IngredientLine(
                    "1 to 3 kalamata or nicoise olives, pitted and chopped",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Range, "1 to 3", 0),
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<decimal>(3M, "3", 5),
                            null,
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 to 3 kalamata or nicoise olives, pitted and chopped", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Kalamata or nicoise olives"
                    },
                    QuantityType = QuantityType.Range,
                    MinValue = 1M,
                    MaxValue = 3M,
                    NumberValue = null,
                    Unit = MeasurementUnit.Unit,
                    Notes = "pitted and chopped",
                    RawText = "1 to 3 kalamata or nicoise olives, pitted and chopped"
                }
            ]),
        /*new(
            "1/2 cup heavy cream, plus 1 cup, for garnish",
            [
                new IngredientLine(
                    "1/2 cup heavy cream, plus 1 cup, for garnish",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1/2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(0.5M, "1/2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cup", 4)),
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cup", 2))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Heavy cream"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1.5M,
                    Unit = MeasurementUnit.Cup,
                    Notes = "for garnish",
                    RawText = "1/2 cup heavy cream, plus 1 cup, for garnish"
                }
            ]),*/
        new(
            "1/2 head green cabbage, shredded (reserve remaining half for Round 2 Recipe, Bean and Cheese Chalupas)",
            [
                new IngredientLine(
                    "1/2 head green cabbage, shredded (reserve remaining half for Round 2 Recipe, Bean and Cheese Chalupas)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1/2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(0.5M, "1/2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Head, "head", 4))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Green cabbage"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 0.5M,
                    Unit = MeasurementUnit.Head,
                    Notes = "shredded (reserve remaining half for Round 2 Recipe, Bean and Cheese Chalupas)",
                    RawText = "1/2 head green cabbage, shredded (reserve remaining half for Round 2 Recipe, Bean and Cheese Chalupas)"
                }
            ]),
        /*new(
            "1/2 to 1 serrano chile pepper, halved lengthwise and seeded",
            [
                new IngredientLine(
                    "1/2 to 1 serrano chile pepper, halved lengthwise and seeded",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Range, "1/2 to 1", 0),
                            new TokenMatch<decimal>(0.5M, "1/2", 0),
                            new TokenMatch<decimal>(1M, "1", 7),
                            null,
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1/2 to 1 serrano chile pepper, halved lengthwise and seeded", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Serrano chile pepper"
                    },
                    QuantityType = QuantityType.Range,
                    MinValue = 0.5M,
                    MaxValue = 1M,
                    NumberValue = null,
                    Unit = MeasurementUnit.Unit,
                    Notes = "halved lengthwise and seeded",
                    RawText = "1/2 to 1 serrano chile pepper, halved lengthwise and seeded"
                }
            ]),*/
        new(
            "1/4 cup dry vermouth (see Cook's Note)�",
            [
                new IngredientLine(
                    "1/4 cup dry vermouth (see Cook's Note)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1/4", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(0.25M, "1/4", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cup", 4))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Dry vermouth"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 0.25M,
                    Unit = MeasurementUnit.Cup,
                    Notes = "(see Cook's Note)",
                    RawText = "1/4 cup dry vermouth (see Cook's Note)"
                }
            ]),
        /*new(
            "12 large or 16 medium to small tomatillos, peeled, rinsed and halved",
            [
                new IngredientLine(
                    "12 large or 16 medium to small tomatillos, peeled, rinsed and halved",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "12", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(12M, "12", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "12 large or 16 medium to small tomatillos, peeled, rinsed and halved", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),*/
        new(
            "�2 cloves garlic, chopped�",
            [
                new IngredientLine(
                    "2 cloves garlic, chopped",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(2M, "2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Clove, "cloves", 2))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Garlic"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 2M,
                    Unit = MeasurementUnit.Clove,
                    Notes = "chopped",
                    RawText = "2 cloves garlic, chopped"
                }
            ]),
        new(
            "�2 tablespoons minced garlic Fresh herbs, optional",
            [
                new IngredientLine(
                    "2 tablespoons minced garlic Fresh herbs, optional",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(2M, "2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.TableSpoon, "tablespoons", 2))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Minced garlic Fresh herbs"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 2M,
                    Unit = MeasurementUnit.TableSpoon,
                    Notes = "optional",
                    RawText = "2 tablespoons minced garlic Fresh herbs, optional"
                }
            ]),
        new(
            "2 teaspoons red food coloring",
            [
                new IngredientLine(
                    "2 teaspoons red food coloring",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(2M, "2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.TeaSpoon, "teaspoons", 2))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Red food coloring"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 2M,
                    Unit = MeasurementUnit.TeaSpoon,
                    Notes = "",
                    RawText = "2 teaspoons red food coloring"
                }
            ]),
        new(
            "�4 large eggs",
            [
                new IngredientLine(
                    "4 large eggs",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "4", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(4M, "4", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "4 large eggs", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Large eggs"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 4M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "",
                    RawText = "4 large eggs"
                }
            ]),
        new(
            "4 medium garlic cloves, thinly sliced",
            [
                new IngredientLine(
                    "4 medium garlic cloves, thinly sliced",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "4", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(4M, "4", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Clove, "cloves", 16))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Medium garlic"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 4M,
                    Unit = MeasurementUnit.Clove,
                    Notes = "thinly sliced",
                    RawText = "4 medium garlic cloves, thinly sliced"
                }
            ]),
        new(
            "�4 tablespoons unsalted butter, optional",
            [
                new IngredientLine(
                    "4 tablespoons unsalted butter, optional",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "4", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(4M, "4", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.TableSpoon, "tablespoons", 2))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Unsalted butter"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 4M,
                    Unit = MeasurementUnit.TableSpoon,
                    Notes = "optional",
                    RawText = "4 tablespoons unsalted butter, optional"
                }
            ]),
        new(
            "�4 to 6 basil leaves",
            [
                new IngredientLine(
                    "4 to 6 basil leaves",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Range, "4 to 6", 0),
                            new TokenMatch<decimal>(4M, "4", 0),
                            new TokenMatch<decimal>(6M, "6", 5),
                            null,
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "4 to 6 basil leaves", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Basil leaves"
                    },
                    QuantityType = QuantityType.Range,
                    MinValue = 4M,
                    MaxValue = 6M,
                    NumberValue = null,
                    Unit = MeasurementUnit.Unit,
                    Notes = "",
                    RawText = "4 to 6 basil leaves"
                }
            ]),
        new(
            "6 corn tortillas",
            [
                new IngredientLine(
                    "6 corn tortillas",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "6", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(6M, "6", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "6 corn tortillas", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Corn tortillas"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 6M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "",
                    RawText = "6 corn tortillas"
                }
            ]),/*******************/
        new(
            "6 cups (1 1/2 liters) vegetable broth",
            [
                new IngredientLine(
                    "6 cups (1 1/2 liters) vegetable broth",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "6", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(6M, "6", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cups", 2))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "8 cups canned peeled tomatoes",
            [
                new IngredientLine(
                    "8 cups canned peeled tomatoes",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cups", 2))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "�Pinch sugar",
            [
                new IngredientLine(
                    "Pinch sugar",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Pinch sugar", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Pinch sugar", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Pinch, "Pinch", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "�plus 1 whole egg*",
            [
                new IngredientLine(
                    "plus 1 whole egg",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 5),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 5),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "plus 1 whole egg", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "�Several dashes hot sauce",
            [
                new IngredientLine(
                    "Several dashes hot sauce",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Several dashes hot sauce", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Several dashes hot sauce", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Drop, "dashes", 08))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Chicken stock, if needed",
            [
                new IngredientLine(
                    "Chicken stock, if needed",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Chicken stock, if needed", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Chicken stock, if needed", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Chicken stock, if needed", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "�Dark rum, to taste�",
            [
                new IngredientLine(
                    "Dark rum, to taste",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Dark rum, to taste", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Dark rum, to taste", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Dark rum, to taste", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Edible glitter, for that magical touch (I like Jewel Dust!)",
            [
                new IngredientLine(
                    "Edible glitter, for that magical touch (I like Jewel Dust!)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Edible glitter, for that magical touch                     ", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Edible glitter, for that magical touch                     ", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Edible glitter, for that magical touch                     ", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Egg substitute for 2 eggs (follow the directions on the carton)�",
            [
                new IngredientLine(
                    "Egg substitute for 2 eggs (follow the directions on the carton)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "2", 19),
                            null,
                            null,
                            new TokenMatch<decimal>(2M, "2", 19),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Egg substitute for 2 eggs                                      ", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Egg wash (1 egg whisked with 2 tablespoons water)",
            [
                new IngredientLine(
                    "Egg wash (1 egg whisked with 2 tablespoons water)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Egg wash (1 egg whisked with 2 tablespoons water)", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Egg wash (1 egg whisked with 2 tablespoons water)", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Egg wash (1 egg whisked with 2 tablespoons water)", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Egg wash, 1 egg and 1 teaspoon water, beaten",
            [
                new IngredientLine(
                    "Egg wash, 1 egg and 1 teaspoon water, beaten",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Egg wash, 1 egg and 1 teaspoon water, beaten", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Egg wash, 1 egg and 1 teaspoon water, beaten", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Egg wash, 1 egg and 1 teaspoon water, beaten", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Egg wash: 1 egg beaten with 2 tablespoons water.",
            [
                new IngredientLine(
                    "Egg wash: 1 egg beaten with 2 tablespoons water.",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Egg wash: 1 egg beaten with 2 tablespoons water.", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Egg wash: 1 egg beaten with 2 tablespoons water.", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Egg wash: 1 egg beaten with 2 tablespoons water.", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Egg wash",
            [
                new IngredientLine(
                    "Egg wash",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Egg wash", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Egg wash", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Egg wash", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Eggs (optional)",
            [
                new IngredientLine(
                    "Eggs (optional)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Eggs           ", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Eggs           ", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Eggs           ", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Eggs from pasture-raised chickens (quantity depends on how hungry you are!)",
            [
                new IngredientLine(
                    "Eggs from pasture-raised chickens (quantity depends on how hungry you are!)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Eggs from pasture-raised chickens                                          ", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Eggs from pasture-raised chickens                                          ", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Eggs from pasture-raised chickens                                          ", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Eight 1/3- to 1/2-inch-thick slices Cuban sandwich bread, sliced on the diagonal",
            [
                new IngredientLine(
                    "8 1/3- to 1/2-inch-thick slices Cuban sandwich bread, sliced on the diagonal",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Slice, "slices", 25))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Eight 1-inch-thick slices peasant-style white bread",
            [
                new IngredientLine(
                    "8 1-inch-thick slices peasant-style white bread",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Slice, "slices", 15))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Eight 2-inch-wide by 1/2-inch-thick sausage patties�",
            [
                new IngredientLine(
                    "8 2-inch-wide by 1/2-inch-thick sausage patties",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "8 2-inch-wide by 1/2-inch-thick sausage patties", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Eight 3/4- to 1-inch-thick slices sourdough bread",
            [
                new IngredientLine(
                    "8 3/4- to 1-inch-thick slices sourdough bread",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Slice, "slices", 23))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Extra-virgin olive oil, for the bowl�",
            [
                new IngredientLine(
                    "Extra-virgin olive oil, for the bowl",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Extra-virgin olive oil, for the bowl", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Extra-virgin olive oil, for the bowl", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Extra-virgin olive oil, for the bowl", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Extra-virgin olive oil, plus high quality extra-virgin olive oil",
            [
                new IngredientLine(
                    "Extra-virgin olive oil, plus high quality extra-virgin olive oil",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Extra-virgin olive oil,", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Extra-virgin olive oil,", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Extra-virgin olive oil,", 0)),
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "high quality extra-virgin olive oil", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "high quality extra-virgin olive oil", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "high quality extra-virgin olive oil", 0))
                    ])
            ],
            2,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Extra-virgin olive oil, to coat, 2 to 3 tablespoons",
            [
                new IngredientLine(
                    "Extra-virgin olive oil, to coat, 2 to 3 tablespoons",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Range, "2 to 3", 33),
                            new TokenMatch<decimal>(2M, "2", 33),
                            new TokenMatch<decimal>(3M, "3", 38),
                            null,
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.TableSpoon, "tablespoons", 40))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Fast Blue Cheese Dressing, recipe follows",
            [
                new IngredientLine(
                    "Fast Blue Cheese Dressing, recipe follows",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Fast Blue Cheese Dressing, recipe follows", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Fast Blue Cheese Dressing, recipe follows", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Fast Blue Cheese Dressing, recipe follows", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Favorite marinara sauce, warmed, for serving",
            [
                new IngredientLine(
                    "Favorite marinara sauce, warmed, for serving",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Favorite marinara sauce, warmed, for serving", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Favorite marinara sauce, warmed, for serving", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Favorite marinara sauce, warmed, for serving", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Few cups water",
            [
                new IngredientLine(
                    "Few cups water",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Few cups water", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Few cups water", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cups", 4))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Few dashes hot sauce�",
            [
                new IngredientLine(
                    "Few dashes hot sauce",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Few dashes hot sauce", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Few dashes hot sauce", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Drop, "dashes", 4))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Few drops hot sauce",
            [
                new IngredientLine(
                    "Few drops hot sauce",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Few drops hot sauce", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Few drops hot sauce", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Drop, "drops", 4))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Few fresh cilantro sprigs, for garnish",
            [
                new IngredientLine(
                    "Few fresh cilantro sprigs, for garnish",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Few fresh cilantro sprigs, for garnish", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Few fresh cilantro sprigs, for garnish", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Few fresh cilantro sprigs, for garnish", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Few grinds black pepper",
            [
                new IngredientLine(
                    "Few grinds black pepper",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Few grinds black pepper", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Few grinds black pepper", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Few grinds black pepper", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Few pinches salt",
            [
                new IngredientLine(
                    "Few pinches salt",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Few pinches salt", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Few pinches salt", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Pinch, "pinches", 4))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Fifteen 1/2-inch cubes whole-milk mozzarella cheese (from a 3.5-ounce piece)�",
            [
                new IngredientLine(
                    "15 1/2-inch cubes whole-milk mozzarella cheese (from a 3.5-ounce piece)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "15", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(15M, "15", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "15 1/2-inch cubes whole-milk mozzarella cheese                         ", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "FILLING:",
            [
                new IngredientLine(
                    "FILLING:",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "FILLING:", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "FILLING:", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "FILLING:", 0))
                    ])
            ],
            0,
            new Exception("No valid name found. 'FILLING:'"),
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Fine salt",
            [
                new IngredientLine(
                    "Fine salt",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Fine salt", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Fine salt", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Fine salt", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Fine sea salt",
            [
                new IngredientLine(
                    "Fine sea salt",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Fine sea salt", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Fine sea salt", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Fine sea salt", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Finely chopped fresh flat-leaf parsley, optional, for garnish",
            [
                new IngredientLine(
                    "Finely chopped fresh flat-leaf parsley, optional, for garnish",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Finely chopped fresh flat-leaf parsley, optional, for garnish", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Finely chopped fresh flat-leaf parsley, optional, for garnish", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Finely chopped fresh flat-leaf parsley, optional, for garnish", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Finely grated zest of 1 lime",
            [
                new IngredientLine(
                    "Finely grated zest of 1 lime",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 22),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 22),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Finely grated zest of 1 lime", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Finely ground black pepper",
            [
                new IngredientLine(
                    "Finely ground black pepper",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Finely ground black pepper", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Finely ground black pepper", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Finely ground black pepper", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Finely ground sea salt, preferably gray salt",
            [
                new IngredientLine(
                    "Finely ground sea salt, preferably gray salt",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Finely ground sea salt, preferably gray salt", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Finely ground sea salt, preferably gray salt", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Finely ground sea salt, preferably gray salt", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Fish sauce",
            [
                new IngredientLine(
                    "Fish sauce",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Fish sauce", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Fish sauce", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Fish sauce", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Fish sticks, as an accompaniment",
            [
                new IngredientLine(
                    "Fish sticks, as an accompaniment",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Fish sticks, as an accompaniment", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Fish sticks, as an accompaniment", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Fish sticks, as an accompaniment", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Five 1-pound lobsters�",
            [
                new IngredientLine(
                    "5 1-pound lobsters",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "5", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(5M, "5", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "5 1-pound lobsters", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Five Vegetable Slaw Salad, recipe follows",
            [
                new IngredientLine(
                    "5 Vegetable Slaw Salad, recipe follows",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "5", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(5M, "5", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "5 Vegetable Slaw Salad, recipe follows", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Flaked sea salt, for garnish",
            [
                new IngredientLine(
                    "Flaked sea salt, for garnish",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Flaked sea salt, for garnish", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Flaked sea salt, for garnish", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Flaked sea salt, for garnish", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "For serving: ketchup, mustard, mayonnaise, lettuce leaves. tomato slices and onion rings",
            [
                new IngredientLine(
                    "For serving: ketchup, mustard, mayonnaise, lettuce leaves. tomato slices and onion rings",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "For serving: ketchup, mustard, mayonnaise, lettuce leaves. tomato slices and onion rings", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "For serving: ketchup, mustard, mayonnaise, lettuce leaves. tomato slices and onion rings", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "For serving: ketchup, mustard, mayonnaise, lettuce leaves. tomato slices and onion rings", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "For serving: lettuce, sliced tomato, pickles,�sliced red onion",
            [
                new IngredientLine(
                    "For serving: lettuce, sliced tomato, pickles, sliced red onion",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "For serving: lettuce, sliced tomato, pickles, sliced red onion", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "For serving: lettuce, sliced tomato, pickles, sliced red onion", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "For serving: lettuce, sliced tomato, pickles, sliced red onion", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Four 12- to 14-inch loaves French bread, halved crosswise, split and lightly toasted�",
            [
                new IngredientLine(
                    "4 12- to 14-inch loaves French bread, halved crosswise, split and lightly toasted",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "4", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(4M, "4", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Loaf, "loaves", 17))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Garnishes, such as lime wedges, sour cream, chopped cooked bacon, chopped red and/or green jalapenos",
            [
                new IngredientLine(
                    "Garnishes, such as lime wedges, sour cream, chopped cooked bacon, chopped red and/or green jalapenos",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Garnishes, such as lime wedges, sour cream, chopped cooked bacon, chopped red and/or green jalapenos", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Garnishes, such as lime wedges, sour cream, chopped cooked bacon, chopped red and/or green jalapenos", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Garnishes, such as lime wedges, sour cream, chopped cooked bacon, chopped red and/or green jalapenos", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "One 18.25-ounce (540-milliliter) can white kidney beans, drained and rinsed",
            [
                new IngredientLine(
                    "1 18.25-ounce (540-milliliter) can white kidney beans, drained and rinsed",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Can, "can", 31))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Pinch of ground cinnamon",
            [
                new IngredientLine(
                    "Pinch of ground cinnamon",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Pinch of ground cinnamon", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Pinch of ground cinnamon", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Pinch, "Pinch", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Zest of 1/2 lemon",
            [
                new IngredientLine(
                    "Zest of 1/2 lemon",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1/2", 8),
                            null,
                            null,
                            new TokenMatch<decimal>(0.5M, "1/2", 8),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Zest of 1/2 lemon", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Zest of 3 lemons plus 1/3 cup lemon juice�",
            [
                new IngredientLine(
                    "Zest of 3 lemons plus 1/3 cup lemon juice",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "3", 8),
                            null,
                            null,
                            new TokenMatch<decimal>(3M, "3", 8),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Zest of 3 lemons", 0)),
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1/3", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(0.3333333333333333333333333333M, "1/3", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Cup, "cup", 4))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Zest of one lemon",
            [
                new IngredientLine(
                    "Zest of one lemon",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Zest of one lemon", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Zest of one lemon", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Zest of one lemon", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "�Kosher salt and freshly ground black pepper",
            [
                new IngredientLine(
                    "Kosher salt",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Kosher salt", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Kosher salt", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Kosher salt", 0))
                    ]),
                new IngredientLine(
                    "freshly ground black pepper",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "freshly ground black pepper", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "freshly ground black pepper", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "freshly ground black pepper", 0))
                    ])
            ],
            2,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "�Salt and freshly ground pepper",
            [
                new IngredientLine(
                    "Salt",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Salt", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Salt", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Salt", 0))
                    ]),
                new IngredientLine(
                    "freshly ground pepper",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "freshly ground pepper", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "freshly ground pepper", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "freshly ground pepper", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "�Sea salt and freshly ground black pepper",
            [
                new IngredientLine(
                    "Sea salt",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Sea salt", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Sea salt", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Sea salt", 0))
                    ]),
                new IngredientLine(
                    "freshly ground black pepper",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "freshly ground black pepper", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "freshly ground black pepper", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "freshly ground black pepper", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "A pinch of salt and pepper",
            [
                new IngredientLine(
                    "A pinch of salt",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "A pinch of salt", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "A pinch of salt", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Pinch, "pinch", 2))
                    ]),
                new IngredientLine(
                    "A pinch of pepper",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "A pinch of pepper", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "A pinch of pepper", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Pinch, "pinch", 2))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "A small bundle of parsley and thyme, tied�",
            [
                new IngredientLine(
                    "A small bundle of parsley and thyme, tied",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "A small bundle of parsley and thyme, tied", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "A small bundle of parsley and thyme, tied", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "A small bundle of parsley and thyme, tied", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Accompaniments such as rice and fried ripe plantains",
            [
                new IngredientLine(
                    "Accompaniments such as rice and fried ripe plantains",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Accompaniments such as rice and fried ripe plantains", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Accompaniments such as rice and fried ripe plantains", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Accompaniments such as rice and fried ripe plantains", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "All-purpose flour, for bench and pizza peel",
            [
                new IngredientLine(
                    "All-purpose flour, for bench and pizza peel",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "All-purpose flour, for bench and pizza peel", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "All-purpose flour, for bench and pizza peel", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "All-purpose flour, for bench and pizza peel", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ]),
        new(
            "Fine sea salt and freshly cracked black pepper",
            [
                new IngredientLine(
                    "Fine sea salt",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Fine sea salt", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Fine sea salt", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Fine sea salt", 0))
                    ]),
                new IngredientLine(
                    "freshly cracked black pepper",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "freshly cracked black pepper", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "freshly cracked black pepper", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "freshly cracked black pepper", 0))
                    ])
            ],
            2,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Name"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "notes",
                    RawText = "Raw"
                }
            ])
    ];

    public readonly record struct TestCaseData(
        string RawIngredient,
        IReadOnlyList<IngredientLine> Lines,
        int TotalRecipeStepIngredients,
        Exception? Exception,
        IReadOnlyList<RecipeStepIngredient> RecipeStepIngredients);

    public readonly record struct IngredientLine(
        string RawData,
        IReadOnlyList<QuantityLine> QuantityLines/*,
        TokenMatch<decimal>? NumberValue,
        TokenMatch<decimal>? MinValue,
        TokenMatch<decimal>? MaxValue,
        TokenMatch<MeasurementUnit> MeasurementUnit*/);
    
    public readonly record struct QuantityLine(
        TokenMatch<QuantityType> QuantityType,
        TokenMatch<decimal>? MinValue,
        TokenMatch<decimal>? MaxValue,
        TokenMatch<decimal>? NumberValue,
        TokenMatch<MeasurementUnit> Measurement);
}