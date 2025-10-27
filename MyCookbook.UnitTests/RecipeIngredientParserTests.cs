using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using MyCookbook.API.Implementations;
using MyCookbook.Common.Enums;
using Xunit;

namespace MyCookbook.UnitTests;

public static class RecipeIngredientParserTests
{
    [Theory]
    [InlineData("", null)]
    [InlineData("Jeep", null)]
    [InlineData("Unit", Measurement.Unit)]
    [InlineData("Units", Measurement.Unit)]
    [InlineData("UNIT", Measurement.Unit)]
    [InlineData("UNITS", Measurement.Unit)]
    [InlineData("TeaSpoon", Measurement.TeaSpoon)]
    [InlineData("TeaSpoons", Measurement.TeaSpoon)]
    [InlineData("Teaspoon", Measurement.TeaSpoon)]
    [InlineData("Teaspoons", Measurement.TeaSpoon)]
    [InlineData("TeAsPoOnS", Measurement.TeaSpoon)]
    [InlineData("Bunches", Measurement.Bunch)]
    [InlineData("ounce", Measurement.Ounce)]
    [InlineData("Ounce", Measurement.Ounce)]
    [InlineData("Ounces", Measurement.Ounce)]
    [InlineData("Fillet", Measurement.Fillet)]
    [InlineData("Fillets", Measurement.Fillet)]
    [InlineData("Inch", Measurement.Inch)]
    [InlineData("inches", Measurement.Inch)]
    [InlineData("can", Measurement.Can)]
    [InlineData("Cans", Measurement.Can)]
    public static void TestGetMeasurement(
        string input,
        Measurement? expected)
    {
        var result = RecipeIngredientParser.GetMeasurement(
            input);
        using (new AssertionScope())
        {
            result?.ParsedValue
                .Should()
                .Be(expected);
        }
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("Jeep", null)]
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
            result?.ParsedValue
                .Should()
                .Be(expected);
        }
    }

    [Theory]
    [InlineData(Measurement.Unit, Measurement.Unit, "1")]
    [InlineData(Measurement.Unit, Measurement.Piece, "1")]
    [InlineData(Measurement.Unit, Measurement.Fillet, null)]
    [InlineData(Measurement.Slice, Measurement.Slice, "1")]
    [InlineData(Measurement.Slice, Measurement.Cup, null)]
    [InlineData(Measurement.Clove, Measurement.Clove, "1")]
    [InlineData(Measurement.Clove, Measurement.Unit, null)]
    [InlineData(Measurement.Bunch, Measurement.Bunch, "1")]
    [InlineData(Measurement.Bunch, Measurement.Unit, null)]
    [InlineData(Measurement.Fillet, Measurement.Fillet, "1")]
    [InlineData(Measurement.Fillet, Measurement.Unit, null)]
    [InlineData(Measurement.Cup, Measurement.Cup, "1")]
    [InlineData(Measurement.Cup, Measurement.TableSpoon, "16")]
    [InlineData(Measurement.Cup, Measurement.TeaSpoon, "48")]
    [InlineData(Measurement.Cup, Measurement.Ounce, "8")]
    [InlineData(Measurement.Cup, Measurement.Unit, null)]
    [InlineData(Measurement.TableSpoon, Measurement.TableSpoon, "1")]
    [InlineData(Measurement.TableSpoon, Measurement.TeaSpoon, "3")]
    [InlineData(Measurement.TableSpoon, Measurement.Unit, null)]
    [InlineData(Measurement.TeaSpoon, Measurement.TeaSpoon, "1")]
    [InlineData(Measurement.TeaSpoon, Measurement.Unit, null)]
    [InlineData(Measurement.Ounce, Measurement.TableSpoon, "2")]
    [InlineData(Measurement.Ounce, Measurement.TeaSpoon, "6")]
    [InlineData(Measurement.Ounce, Measurement.Ounce, "1")]
    [InlineData(Measurement.Ounce, Measurement.Unit, null)]
    public static void TestGetConversionRate(
        Measurement from,
        Measurement to,
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
    [InlineData(Measurement.Unit, Measurement.Unit, Measurement.Unit)]
    [InlineData(Measurement.Unit, Measurement.Piece, Measurement.Piece)]
    [InlineData(Measurement.Fillet, Measurement.Piece, Measurement.Piece)]
    [InlineData(Measurement.Fillet, Measurement.Cup, Measurement.Cup)]
    [InlineData(Measurement.TableSpoon, Measurement.Cup, Measurement.TableSpoon)]
    [InlineData(Measurement.TableSpoon, Measurement.TableSpoon, Measurement.TableSpoon)]
    [InlineData(Measurement.TableSpoon, Measurement.TeaSpoon, Measurement.TeaSpoon)]
    [InlineData(Measurement.TeaSpoon, Measurement.Cup, Measurement.TeaSpoon)]
    [InlineData(Measurement.TeaSpoon, Measurement.TableSpoon, Measurement.TeaSpoon)]
    [InlineData(Measurement.TeaSpoon, Measurement.TeaSpoon, Measurement.TeaSpoon)]
    [InlineData(Measurement.Ounce, Measurement.TeaSpoon, Measurement.Ounce)]
    [InlineData(Measurement.Ounce, Measurement.TableSpoon, Measurement.Ounce)]
    [InlineData(Measurement.Ounce, Measurement.Cup, Measurement.Ounce)]
    public static void TestGetLowestMeasurement(
        Measurement input1,
        Measurement input2,
        Measurement expected)
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

    [Theory]
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
            otherTokens);
        using (new AssertionScope())
        {
            result.ParsedValue
                .Should()
                .Be(expected);
        }
    }

    [Theory]
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
    }

    [Theory]
    [InlineData("1", "1")]
    [InlineData("1/2", "0.5")]
    [InlineData("4 ", "4")]
    [InlineData(" 4", "4")]
    [InlineData("3/4", "0.75")]
    [InlineData("1/4 ", "0.25")]
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
        var result = RecipeIngredientParser.RemoveParentheticalSets(
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
    [MemberData(nameof(GetData))]
    public static void TestParsingIngredient(
        string data)
    {
        var result = RecipeIngredientParser.Parse(data).ToList();
        using var a = new AssertionScope();
        result.Count.Should().Be(result.Count);
        for (var i = 0; i < result.Count; i++)
        {
            var quantity = "q:" + result[i].Quantity + ", m:" + result[i].Measurement.ToString("G") + ", d:" + data + Environment.NewLine;
            File.AppendAllText("../../../../failedV2.txt", quantity);
            //resultV2[i].Ingredient.Name.Should().Be(result[i].Ingredient.Name);
            result[i].Quantity.Should().Be(result[i].Quantity);
            result[i].Measurement.Should().Be(result[i].Measurement);
            //resultV2[i].Notes.Should().Be(result[i].Notes);
        }
    }

    public static IEnumerable<object[]> GetData() =>
        File.ReadAllLines("../../../../tests.txt")
            .OrderBy(x => x)
            .Select(
                x =>
                    new object[]
                    {
                        x
                    });
}