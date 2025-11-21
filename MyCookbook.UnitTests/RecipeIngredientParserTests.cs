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
            "1 (1 1/2 to 2-pound) pork tenderloin",
            [
                new IngredientLine(
                    "1 (1 1/2 to 2-pound) pork tenderloin",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1                    pork tenderloin", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Pork tenderloin"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "(1 1/2 to 2-pound)",
                    RawText = "1 (1 1/2 to 2-pound) pork tenderloin"
                }
            ]),
        new(
            "1 (1 1/2-pound) flank steak",
            [
                new IngredientLine(
                    "1 (1 1/2-pound) flank steak",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1               flank steak", 0))
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
                    Unit = MeasurementUnit.Unit,
                    Notes = "(1 1/2-pound)",
                    RawText = "1 (1 1/2-pound) flank steak"
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
            "1 (1 pound) loaf ciabatta or rustic bread, ends trimmed, cut in 1/2 horizontally",
            [
                new IngredientLine(
                    "1 (1 pound) loaf ciabatta or rustic bread, ends trimmed, cut in 1/2 horizontally",
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
                        Name = "Ciabatta or rustic bread"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Loaf,
                    Notes = "(1 pound) ends trimmed, cut in 1/2 horizontally",
                    RawText = "1 (1 pound) loaf ciabatta or rustic bread, ends trimmed, cut in 1/2 horizontally"
                }
            ]),
        new(
            "1 (1 pound) skirt steak, cut in half",
            [
                new IngredientLine(
                    "1 (1 pound) skirt steak, cut in half",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1           skirt steak, cut in half", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Skirt steak"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "(1 pound) cut in half",
                    RawText = "1 (1 pound) skirt steak, cut in half"
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
            "1 (1/2 to 1-pound) rack spareribs",
            [
                new IngredientLine(
                    "1 (1/2 to 1-pound) rack spareribs",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1                  rack spareribs", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Rack spareribs"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "(1/2 to 1-pound)",
                    RawText = "1 (1/2 to 1-pound) rack spareribs"
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
            "1 (1/4-ounce) package active dry yeast�",
            [
                new IngredientLine(
                    "1 (1/4-ounce) package active dry yeast",
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
                    Notes = "(1/4-ounce)",
                    RawText = "1 (1/4-ounce) package active dry yeast"
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
            "1 (10-ounce) can clam base",
            [
                new IngredientLine(
                    "1 (10-ounce) can clam base",
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
                        Name = "Clam base"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "(10-ounce)",
                    RawText = "1 (10-ounce) can clam base"
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
            "1 (12-ounce) can tomato sauce",
            [
                new IngredientLine(
                    "1 (12-ounce) can tomato sauce",
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
                        Name = "Tomato sauce"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "(12-ounce)",
                    RawText = "1 (12-ounce) can tomato sauce"
                }
            ]),
        new(
            "1 (15 - ounce) can pumpkin puree (not pumpkin pie filling)",
            [
                new IngredientLine(
                    "1 (15 - ounce) can pumpkin puree (not pumpkin pie filling)",
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
                        Name = "Pumpkin puree"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "(15 - ounce not pumpkin pie filling)",
                    RawText = "1 (15 - ounce) can pumpkin puree (not pumpkin pie filling)"
                }
            ]),
        new(
            "1 (15.5-ounce) can black beans, preferably low-sodium, drained and rinsed",
            [
                new IngredientLine(
                    "1 (15.5-ounce) can black beans, preferably low-sodium, drained and rinsed",
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
                        Name = "Black beans"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "(15.5-ounce) preferably low-sodium, drained and rinsed",
                    RawText = "1 (15.5-ounce) can black beans, preferably low-sodium, drained and rinsed"
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
            "1 (15.5-ounce) can pink beans, rinsed and drained",
            [
                new IngredientLine(
                    "1 (15.5-ounce) can pink beans, rinsed and drained",
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
                        Name = "Pink beans"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Can,
                    Notes = "(15.5-ounce) rinsed and drained",
                    RawText = "1 (15.5-ounce) can pink beans, rinsed and drained"
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
            "1 (1-inch) stick true canela (soft Ceylon cinnamon), coarsely chopped",
            [
                new IngredientLine(
                    "1 (1-inch) stick true canela (soft Ceylon cinnamon), coarsely chopped",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Stick, "stick", 11))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "True canela"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Stick,
                    Notes = "(1-inch soft Ceylon cinnamon) coarsely chopped",
                    RawText = "1 (1-inch) stick true canela (soft Ceylon cinnamon), coarsely chopped"
                }
            ]),
        new(
            "1 (1-inch-thick) sliced pancetta, cut into small dice",
            [
                new IngredientLine(
                    "1 (1-inch-thick) sliced pancetta, cut into small dice",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1                sliced pancetta, cut into small dice", 0))
                    ])
            ],
            1,
            null,
            [
                new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Sliced pancetta"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Unit,
                    Notes = "(1-inch-thick) cut into small dice",
                    RawText = "1 (1-inch-thick) sliced pancetta, cut into small dice"
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
            "1 (1-pound) loaf ciabatta bread (or 8 slices country bread)",
            [
                new IngredientLine(
                    "1 (1-pound) loaf ciabatta bread (or 8 slices country bread)",
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
                    Notes = "(1-pound or 8 slices country bread)",
                    RawText = "1 (1-pound) loaf ciabatta bread (or 8 slices country bread)"
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
            "1 (1-pound) loaf purchased frozen white bread dough, thawed (recommended: Bridgeford)",
            [
                new IngredientLine(
                    "1 (1-pound) loaf purchased frozen white bread dough, thawed (recommended: Bridgeford)",
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
                        Name = "Purchased frozen white bread dough"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Loaf,
                    Notes = "(1-pound) thawed (recommended: Bridgeford)",
                    RawText = "1 (1-pound) loaf purchased frozen white bread dough, thawed (recommended: Bridgeford)"
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
            "1 (2-inch) piece carrot, peeled",
            [
                new IngredientLine(
                    "1 (2-inch) piece carrot, peeled",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Piece, "piece", 11))
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
                    Unit = MeasurementUnit.Piece,
                    Notes = "(2-inch) peeled",
                    RawText = "1 (2-inch) piece carrot, peeled"
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
            ]),/*******************/
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
            "1 (3 to 4-pound) chicken, cut into 1/8's",
            [
                new IngredientLine(
                    "1 (3 to 4-pound) chicken, cut into 1/8 s",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1                chicken, cut into 1/8 s", 0))
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
            "1 (4-pound) whole chicken, cut into pieces (giblets, neck and backbone reserved for another use)",
            [
                new IngredientLine(
                    "1 (4-pound) whole chicken, cut into pieces (giblets, neck and backbone reserved for another use)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1           whole chicken, cut into pieces                                                      ", 0))
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
                        Name = "fresh ginger"
                    },
                    QuantityType = QuantityType.Number,
                    MinValue = null,
                    MaxValue = null,
                    NumberValue = 1M,
                    Unit = MeasurementUnit.Piece,
                    Notes = "1 -inch peeled and minced",
                    RawText = "1 -inch piece fresh ginger, peeled and minced"
                }
            ]),
        new(
            "1 -ounce red pepper, diced",
            [
                new IngredientLine(
                    "1 -ounce red pepper, diced",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 -ounce red pepper, diced", 0))
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
            "1 (8 to 10-pound) smoked ham, bone-in, skin on",
            [
                new IngredientLine(
                    "1 (8 to 10-pound) smoked ham, bone-in, skin on",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1                 smoked ham, bone-in, skin on", 0))
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
            "1 1/2 cups (369 grams) whole milk",
            [
                new IngredientLine(
                    "1 1/2 cups (369 grams) whole milk",
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
            "1 1/2 cups (375 milliliters) 2% or whole milk",
            [
                new IngredientLine(
                    "1 1/2 cups (375 milliliters) 2 or whole milk",
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
            "1 1/2 cups (375 milliliters) apple cider",
            [
                new IngredientLine(
                    "1 1/2 cups (375 milliliters) apple cider",
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
            "1 1/2 cups (4 ounces) grated Italian Fontina cheese (6 ounces with rind)",
            [
                new IngredientLine(
                    "1 1/2 cups (4 ounces) grated Italian Fontina cheese (6 ounces with rind)",
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
            "1 1/2 cups (500 grams) assorted preserves or chocolate spreads (see Cook's Note)�",
            [
                new IngredientLine(
                    "1 1/2 cups (500 grams) assorted preserves or chocolate spreads (see Cook s Note)",
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
            "1 1/2 cups Arborio rice (10 ounces)",
            [
                new IngredientLine(
                    "1 1/2 cups Arborio rice (10 ounces)",
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
            "1 1/2 cups Arborio rice or medium-grain white rice",
            [
                new IngredientLine(
                    "1 1/2 cups Arborio rice or medium-grain white rice",
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
            "1 1/2 cups Arborio rice or short-grain white rice",
            [
                new IngredientLine(
                    "1 1/2 cups Arborio rice or short-grain white rice",
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
            "1 1/2 cups fresh corn, cut from 1 or 2 cobs. (frozen may be used, but when in season, you cannot beat fresh corn!)",
            [
                new IngredientLine(
                    "1 1/2 cups fresh corn, cut from 1 or 2 cobs. (frozen may be used, but when in season, you cannot beat fresh corn!)",
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
            "1 1/2 cups frozen peas (not \"baby\" peas)",
            [
                new IngredientLine(
                    "1 1/2 cups frozen peas (not baby peas)",
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
            "1 1/4 sticks (10 tablespoons) unsalted butter, cubed and at room temperature, plus more for the loaf pan and plastic wrap",
            [
                new IngredientLine(
                    "1 1/4 sticks (10 tablespoons) unsalted butter, cubed and at room temperature, plus more for the loaf pan and plastic wrap",
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
            "1 1/4 sticks unsalted butter, melted",
            [
                new IngredientLine(
                    "1 1/4 sticks unsalted butter, melted",
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
            "1 1/4 sticks unsalted butter",
            [
                new IngredientLine(
                    "1 1/4 sticks unsalted butter",
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
            "1 1/4 teaspoon baking soda",
            [
                new IngredientLine(
                    "1 1/4 teaspoon baking soda",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1 1/4", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1.25M, "1 1/4", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.TeaSpoon, "teaspoon", 6))
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
            "1 14-ounce can chickpeas, drained and rinsed",
            [
                new IngredientLine(
                    "1 14-ounce can chickpeas, drained and rinsed",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Can, "can", 11))
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
            "1 cucumber, peeled and sliced",
            [
                new IngredientLine(
                    "1 cucumber, peeled and sliced",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 cucumber, peeled and sliced", 0))
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
            "1 cup (1/4-inch) diced onion",
            [
                new IngredientLine(
                    "1 cup (1/4-inch) diced onion",
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
            "1 cup (100 grams) pecan halves, toasted and chopped",
            [
                new IngredientLine(
                    "1 cup (100 grams) pecan halves, toasted and chopped",
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
            "1 cup (105 grams) blue cornmeal",
            [
                new IngredientLine(
                    "1 cup (105 grams) blue cornmeal",
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
            "1 medium zucchini, grated on the large holes of a box grater (about 1 cup)",
            [
                new IngredientLine(
                    "1 medium zucchini, grated on the large holes of a box grater (about 1 cup)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 medium zucchini, grated on the large holes of a box grater              ", 0))
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
            "1 small avocado, sliced",
            [
                new IngredientLine(
                    "1 small avocado, sliced",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 small avocado, sliced", 0))
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
            "1 to 2 fresh jalapeno peppers, seeded and finely diced",
            [
                new IngredientLine(
                    "1 to 2 fresh jalapeno peppers, seeded and finely diced",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Range, "1 to 2", 0),
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<decimal>(2M, "2", 5),
                            null,
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 to 2 fresh jalapeno peppers, seeded and finely diced", 0))
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
            "1 to 2 leeks, washed and thinly sliced",
            [
                new IngredientLine(
                    "1 to 2 leeks, washed and thinly sliced",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Range, "1 to 2", 0),
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<decimal>(2M, "2", 5),
                            null,
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 to 2 leeks, washed and thinly sliced", 0))
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
            "1 to 2 medium white potatoes, peeled and roughly cubed�",
            [
                new IngredientLine(
                    "1 to 2 medium white potatoes, peeled and roughly cubed",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Range, "1 to 2", 0),
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<decimal>(2M, "2", 5),
                            null,
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 to 2 medium white potatoes, peeled and roughly cubed", 0))
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
            "1/4 cup dry shiitake mushrooms, rehydrated in warm water for 30 minutes and thinly�sliced�",
            [
                new IngredientLine(
                    "1/4 cup dry shiitake mushrooms, rehydrated in warm water for 30 minutes and thinly sliced",
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
            "1/4 cup dry vermouth (see Cook's Note)�",
            [
                new IngredientLine(
                    "1/4 cup dry vermouth (see Cook s Note)",
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
            "12 frozen home-style waffles",
            [
                new IngredientLine(
                    "12 frozen home-style waffles",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "12", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(12M, "12", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "12 frozen home-style waffles", 0))
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
            "6 cornichon, finely diced",
            [
                new IngredientLine(
                    "6 cornichon, finely diced",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "6", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(6M, "6", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "6 cornichon, finely diced", 0))
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
            "6 cornichons, sliced",
            [
                new IngredientLine(
                    "6 cornichons, sliced",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "6", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(6M, "6", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "6 cornichons, sliced", 0))
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
            "6 crimini mushrooms (baby portobellos) stems removed and finely chopped",
            [
                new IngredientLine(
                    "6 crimini mushrooms (baby portobellos) stems removed and finely chopped",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "6", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(6M, "6", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "6 crimini mushrooms                    stems removed and finely chopped", 0))
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
            "6 croissants, sliced in half lengthwise",
            [
                new IngredientLine(
                    "6 croissants, sliced in half lengthwise",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "6", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(6M, "6", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "6 croissants, sliced in half lengthwise", 0))
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
            "6 crostini",
            [
                new IngredientLine(
                    "6 crostini",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "6", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(6M, "6", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "6 crostini", 0))
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
            "8 cups canola oil",
            [
                new IngredientLine(
                    "8 cups canola oil",
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
            "8 cups cauliflower florets (from 1 head cauliflower; about 2 1/2 pounds)",
            [
                new IngredientLine(
                    "8 cups cauliflower florets (from 1 head cauliflower: about 2 1/2 pounds)",
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
            "�1 green bell pepper, cut into 2-inch strips",
            [
                new IngredientLine(
                    "1 green bell pepper, cut into 2-inch strips",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 green bell pepper, cut into 2-inch strips", 0))
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
            "�1/2 cup olive oil",
            [
                new IngredientLine(
                    "1/2 cup olive oil",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1/2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(0.5M, "1/2", 0),
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
            "�1/2 pound confectioners' sugar",
            [
                new IngredientLine(
                    "1/2 pound confectioners sugar",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1/2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(0.5M, "1/2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Pound, "pound", 4))
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
            "�1/2 eggplant, cut into rounds",
            [
                new IngredientLine(
                    "1/2 eggplant, cut into rounds",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1/2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(0.5M, "1/2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1/2 eggplant, cut into rounds", 0))
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
            "1 bouquet garni (1 sprig each of basil, parsley and bay leaf)",
            [
                new IngredientLine(
                    "1 bouquet garni (1 sprig each of basil, parsley and bay leaf)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 bouquet garni                                              ", 0))
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
            "1 bouquet garni (1 sprig each of bay leaf, thyme and parsley)",
            [
                new IngredientLine(
                    "1 bouquet garni (1 sprig each of bay leaf, thyme and parsley)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 bouquet garni                                              ", 0))
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
            "1 head green cabbage, shredded (reserve half for Online Round 2 Recipe Cabbage and Pear Slaw)",
            [
                new IngredientLine(
                    "1 head green cabbage, shredded (reserve half for Online Round 2 Recipe Cabbage and Pear Slaw)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Head, "head", 2))
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
            ]),
        new(
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
            ]),
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
            "1 zucchini, cut in 1/2 lengthwise and cut on the bias",
            [
                new IngredientLine(
                    "1 zucchini, cut in 1/2 lengthwise and cut on the bias",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "1", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1 zucchini, cut in 1/2 lengthwise and cut on the bias", 0))
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
            "1/2 red onion, cut in 1/2 and thinly sliced",
            [
                new IngredientLine(
                    "1/2 red onion, cut in 1/2 and thinly sliced",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "1/2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(0.5M, "1/2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "1/2 red onion, cut in 1/2 and thinly sliced", 0))
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
            ]),
        new(
            "12 pickled pepperoncini (4 stemmed and thinly sliced, 8 left whole)",
            [
                new IngredientLine(
                    "12 pickled pepperoncini (4 stemmed and thinly sliced, 8 left whole)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "12", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(12M, "12", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "12 pickled pepperoncini                                            ", 0))
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
            "�2 (32-ounce) cans crushed tomatoes",
            [
                new IngredientLine(
                    "2 (32-ounce) cans crushed tomatoes",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(2M, "2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Can, "cans", 13))
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
            "�2 carrots, grated",
            [
                new IngredientLine(
                    "2 carrots, grated",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(2M, "2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "2 carrots, grated", 0))
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
            "�2 dried bay leaves",
            [
                new IngredientLine(
                    "2 dried bay leaves",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(2M, "2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "2 dried bay leaves", 0))
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
            "2 large leeks, washed and chopped (you can use the green tops, but wash them well since they are quite sandy)",
            [
                new IngredientLine(
                    "2 large leeks, washed and chopped (you can use the green tops, but wash them well since they are quite sandy)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(2M, "2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "2 large leeks, washed and chopped                                                                            ", 0))
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
            "2 large leeks, white and light green parts chopped�",
            [
                new IngredientLine(
                    "2 large leeks, white and light green parts chopped",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(2M, "2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "2 large leeks, white and light green parts chopped", 0))
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
            "2 large leeks, white and light green parts only, coarsely chopped and thoroughly washed",
            [
                new IngredientLine(
                    "2 large leeks, white and light green parts only, coarsely chopped and thoroughly washed",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(2M, "2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "2 large leeks, white and light green parts only, coarsely chopped and thoroughly washed", 0))
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
            "2 pounds russet potatoes, peeled and cut into 1/8-inch-thick slices",
            [
                new IngredientLine(
                    "2 pounds russet potatoes, peeled and cut into 1/8-inch-thick slices",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(2M, "2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Pound, "pounds", 2))
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
            "2 pounds russet potatoes, peeled and grated",
            [
                new IngredientLine(
                    "2 pounds russet potatoes, peeled and grated",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(2M, "2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Pound, "pounds", 2))
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
            "2 pounds sauerkraut, rinsed and drained",
            [
                new IngredientLine(
                    "2 pounds sauerkraut, rinsed and drained",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(2M, "2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Pound, "pounds", 2))
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
            "2 pounds shaved honey ham�",
            [
                new IngredientLine(
                    "2 pounds shaved honey ham",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "2", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(2M, "2", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Pound, "pounds", 2))
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
            "�2 tablespoons honey",
            [
                new IngredientLine(
                    "2 tablespoons honey",
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
            "2 tablespoons masa harina�",
            [
                new IngredientLine(
                    "2 tablespoons masa harina",
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
            "3/4 cup crushed tortilla chips",
            [
                new IngredientLine(
                    "3/4 cup crushed tortilla chips",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "3/4", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(0.75M, "3/4", 0),
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
            "Eight 1/2-inch slices day-old challah or brioche�",
            [
                new IngredientLine(
                    "8 1/2-inch slices day-old challah or brioche",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Slice, "slices", 11))
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
            "Eight 1/2-inch slices sourdough or pain de mie�bread",
            [
                new IngredientLine(
                    "8 1/2-inch slices sourdough or pain de mie bread",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Slice, "slices", 11))
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
            "Eight 1/2-inch thick slices country-style white bread",
            [
                new IngredientLine(
                    "8 1/2-inch thick slices country-style white bread",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Slice, "slices", 17))
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
            "Eight 1/3- to 1/2-inch-thick slices brioche bread",
            [
                new IngredientLine(
                    "8 1/3- to 1/2-inch-thick slices brioche bread",
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
            "Eight 1/3- to 1/2-inch-thick slices Pullman bread",
            [
                new IngredientLine(
                    "8 1/3- to 1/2-inch-thick slices Pullman bread",
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
            "Eight 1/4-inch-thick slices provolone",
            [
                new IngredientLine(
                    "8 1/4-inch-thick slices provolone",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Slice, "slices", 17))
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
            "Eight 10-inch flour tortillas�",
            [
                new IngredientLine(
                    "8 10-inch flour tortillas",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "8 10-inch flour tortillas", 0))
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
            "Eight 1-ounce slices Smoked Pork Belly, recipe follows",
            [
                new IngredientLine(
                    "8 1-ounce slices Smoked Pork Belly, recipe follows",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Slice, "slices", 10))
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
            "Eight 3-ounce chicken cutlets",
            [
                new IngredientLine(
                    "8 3-ounce chicken cutlets",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "8 3-ounce chicken cutlets", 0))
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
            "Eight 4-inch flour tortillas�",
            [
                new IngredientLine(
                    "8 4-inch flour tortillas",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "8 4-inch flour tortillas", 0))
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
            "Eight 6-inch all-beef hot dogs, ends trimmed, cut into fifths�",
            [
                new IngredientLine(
                    "8 6-inch all-beef hot dogs, ends trimmed, cut into fifths",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "8 6-inch all-beef hot dogs, ends trimmed, cut into fifths", 0))
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
            "Eight 6-inch corn tortillas",
            [
                new IngredientLine(
                    "8 6-inch corn tortillas",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "8 6-inch corn tortillas", 0))
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
            "Eight 8-inch flour tortillas",
            [
                new IngredientLine(
                    "8 8-inch flour tortillas",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "8", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(8M, "8", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "8 8-inch flour tortillas", 0))
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
            "Extra-virgin olive oil, for the casserole dish",
            [
                new IngredientLine(
                    "Extra-virgin olive oil, for the casserole dish",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Extra-virgin olive oil, for the casserole dish", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Extra-virgin olive oil, for the casserole dish", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Extra-virgin olive oil, for the casserole dish", 0))
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
            "Extra-virgin olive oil, for the pan",
            [
                new IngredientLine(
                    "Extra-virgin olive oil, for the pan",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Extra-virgin olive oil, for the pan", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Extra-virgin olive oil, for the pan", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Extra-virgin olive oil, for the pan", 0))
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
            "Extra-virgin olive oil, optional",
            [
                new IngredientLine(
                    "Extra-virgin olive oil, optional",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Extra-virgin olive oil, optional", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Extra-virgin olive oil, optional", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Extra-virgin olive oil, optional", 0))
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
            "Extra-virgin olive oil, to coat",
            [
                new IngredientLine(
                    "Extra-virgin olive oil, to coat",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Extra-virgin olive oil, to coat", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Extra-virgin olive oil, to coat", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Extra-virgin olive oil, to coat", 0))
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
            "Extra-virgin olive oil, to drizzle",
            [
                new IngredientLine(
                    "Extra-virgin olive oil, to drizzle",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Extra-virgin olive oil, to drizzle", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Extra-virgin olive oil, to drizzle", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Extra-virgin olive oil, to drizzle", 0))
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
            "Extra-virgin olive oil",
            [
                new IngredientLine(
                    "Extra-virgin olive oil",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Extra-virgin olive oil", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Extra-virgin olive oil", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Extra-virgin olive oil", 0))
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
            "Favorite pizza toppings, as desired",
            [
                new IngredientLine(
                    "Favorite pizza toppings, as desired",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Favorite pizza toppings, as desired", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Favorite pizza toppings, as desired", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Favorite pizza toppings, as desired", 0))
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
            "Few dashes hot sauce, optional�",
            [
                new IngredientLine(
                    "Few dashes hot sauce, optional",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Few dashes hot sauce, optional", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Few dashes hot sauce, optional", 0),
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
            "Few dashes hot sauce, such as Cholula",
            [
                new IngredientLine(
                    "Few dashes hot sauce, such as Cholula",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Few dashes hot sauce, such as Cholula", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Few dashes hot sauce, such as Cholula", 0),
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
            "Few dashes hot sauce",
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
            "Few drops lemon juice, optional",
            [
                new IngredientLine(
                    "Few drops lemon juice, optional",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Few drops lemon juice, optional", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Few drops lemon juice, optional", 0),
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
            "Few drops white balsamic vinegar, optional",
            [
                new IngredientLine(
                    "Few drops white balsamic vinegar, optional",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Few drops white balsamic vinegar, optional", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Few drops white balsamic vinegar, optional", 0),
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
            "Few leaves fresh basil, torn",
            [
                new IngredientLine(
                    "Few leaves fresh basil, torn",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Few leaves fresh basil, torn", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Few leaves fresh basil, torn", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Few leaves fresh basil, torn", 0))
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
            "Finely chopped chives, for serving",
            [
                new IngredientLine(
                    "Finely chopped chives, for serving",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Finely chopped chives, for serving", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Finely chopped chives, for serving", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Finely chopped chives, for serving", 0))
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
            "Finely chopped chives",
            [
                new IngredientLine(
                    "Finely chopped chives",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Finely chopped chives", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Finely chopped chives", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Finely chopped chives", 0))
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
            "Finely chopped fresh chives, for garnish�",
            [
                new IngredientLine(
                    "Finely chopped fresh chives, for garnish",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Finely chopped fresh chives, for garnish", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Finely chopped fresh chives, for garnish", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Finely chopped fresh chives, for garnish", 0))
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
            "Finely chopped fresh chives, for garnish",
            [
                new IngredientLine(
                    "Finely chopped fresh chives, for garnish",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Finely chopped fresh chives, for garnish", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Finely chopped fresh chives, for garnish", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Finely chopped fresh chives, for garnish", 0))
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
            "Finely chopped fresh chives",
            [
                new IngredientLine(
                    "Finely chopped fresh chives",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Finely chopped fresh chives", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Finely chopped fresh chives", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Finely chopped fresh chives", 0))
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
            "Finely chopped fresh dill or other fresh herbs, as needed",
            [
                new IngredientLine(
                    "Finely chopped fresh dill or other fresh herbs, as needed",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Finely chopped fresh dill or other fresh herbs, as needed", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Finely chopped fresh dill or other fresh herbs, as needed", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Finely chopped fresh dill or other fresh herbs, as needed", 0))
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
            "Finely chopped fresh thyme",
            [
                new IngredientLine(
                    "Finely chopped fresh thyme",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Finely chopped fresh thyme", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Finely chopped fresh thyme", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Finely chopped fresh thyme", 0))
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
            "Finely chopped lettuce, for garnish",
            [
                new IngredientLine(
                    "Finely chopped lettuce, for garnish",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Finely chopped lettuce, for garnish", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Finely chopped lettuce, for garnish", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Finely chopped lettuce, for garnish", 0))
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
            "Finely chopped parsley",
            [
                new IngredientLine(
                    "Finely chopped parsley",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Finely chopped parsley", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Finely chopped parsley", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Finely chopped parsley", 0))
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
            "Finely shaved onion, for serving (optional)",
            [
                new IngredientLine(
                    "Finely shaved onion, for serving (optional)",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Finely shaved onion, for serving           ", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Finely shaved onion, for serving           ", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Finely shaved onion, for serving           ", 0))
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
            "Five 8-ounce packages fresh mozzarella, sliced",
            [
                new IngredientLine(
                    "5 8-ounce packages fresh mozzarella, sliced",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "5", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(5M, "5", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Package, "packages", 10))
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
            "Flaked coconut, for garnish",
            [
                new IngredientLine(
                    "Flaked coconut, for garnish",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Flaked coconut, for garnish", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Flaked coconut, for garnish", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Flaked coconut, for garnish", 0))
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
            "Flaked sea salt, such as Maldon, for serving",
            [
                new IngredientLine(
                    "Flaked sea salt, such as Maldon, for serving",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Flaked sea salt, such as Maldon, for serving", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Flaked sea salt, such as Maldon, for serving", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Flaked sea salt, such as Maldon, for serving", 0))
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
            "Flaked sea salt, such as Maldon, for sprinkling",
            [
                new IngredientLine(
                    "Flaked sea salt, such as Maldon, for sprinkling",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Flaked sea salt, such as Maldon, for sprinkling", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Flaked sea salt, such as Maldon, for sprinkling", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Flaked sea salt, such as Maldon, for sprinkling", 0))
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
            "For serving: Slider buns, coleslaw, baked beans, dill pickle slices, BBQ sauce",
            [
                new IngredientLine(
                    "For serving: Slider buns, coleslaw, baked beans, dill pickle slices, BBQ sauce",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "For serving: Slider buns, coleslaw, baked beans, dill pickle slices, BBQ sauce", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "For serving: Slider buns, coleslaw, baked beans, dill pickle slices, BBQ sauce", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "For serving: Slider buns, coleslaw, baked beans, dill pickle slices, BBQ sauce", 0))
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
            "Zesty Dipping Sauce, recipe follows",
            [
                new IngredientLine(
                    "Zesty Dipping Sauce, recipe follows",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Zesty Dipping Sauce, recipe follows", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Zesty Dipping Sauce, recipe follows", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Zesty Dipping Sauce, recipe follows", 0))
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
            "Zucchini Pesto, recipe follows",
            [
                new IngredientLine(
                    "Zucchini Pesto, recipe follows",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Zucchini Pesto, recipe follows", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Zucchini Pesto, recipe follows", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Zucchini Pesto, recipe follows", 0))
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
            "All-purpose flour, for bench and pizza peel�",
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
            "Applesauce and steamed vegetables, for serving",
            [
                new IngredientLine(
                    "Applesauce and steamed vegetables, for serving",
                    [
                        new QuantityLine(
                            new TokenMatch<QuantityType>(QuantityType.Number, "Applesauce and steamed vegetables, for serving", 0),
                            null,
                            null,
                            new TokenMatch<decimal>(1M, "Applesauce and steamed vegetables, for serving", 0),
                            new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, "Applesauce and steamed vegetables, for serving", 0))
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