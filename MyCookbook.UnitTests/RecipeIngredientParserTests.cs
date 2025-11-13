using FluentAssertions;
using FluentAssertions.Execution;
using MyCookbook.API.Implementations;
using MyCookbook.Common.Enums;
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
            otherTokens,
            MeasurementUnit.Unit);
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

    /*[Theory]
    [MemberData(nameof(GetData))]
    public static void TestParsingIngredient(
        string data)
    {
        var result = RecipeIngredientParser.Parse(data).ToList();
        using var a = new AssertionScope();
        result.Count.Should().Be(result.Count);
        for (var i = 0; i < result.Count; i++)
        {
            var quantity = "q:" + result[i].Quantity + ", m:" + result[i].Unit.ToString("G") + ", d:" + data + Environment.NewLine;
            File.AppendAllText("../../../../failedV2.txt", quantity);
            //resultV2[i].Ingredient.Name.Should().Be(result[i].Ingredient.Name);
            result[i].Quantity.Should().Be(result[i].Quantity);
            result[i].Unit.Should().Be(result[i].Unit);
            //resultV2[i].Notes.Should().Be(result[i].Notes);
        }
    }

    public static IEnumerable<object[]> GetData() => []
        File.ReadAllLines("../../../../tests.txt")
            .OrderBy(x => x)
            .Select(
                x =>
                    new object[]
                    {
                        x
                    });*/
}