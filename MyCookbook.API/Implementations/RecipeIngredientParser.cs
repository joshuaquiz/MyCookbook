using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using MyCookbook.Common.Database;
using MyCookbook.Common.Enums;
using Ingredient = MyCookbook.Common.Database.Ingredient;

namespace MyCookbook.API.Implementations;

public static partial class RecipeIngredientParser
{
    private static readonly ReadOnlyDictionary<MeasurementUnit, string> DefaultValues =
        new(
            new Dictionary<MeasurementUnit, string>
            {
                { MeasurementUnit.Clove, "clove" }
            });

    [GeneratedRegex(@"^[^a-z0-9]*(?:(?<Number>(?:\d{1,3}(?:,?\d{3})*)|(?:(?:\d{1,3}(?:,?\d{3})*\s+(?!/)+)?\d{1,3}\s*/\s*\d{1,3}))|(?<Range>(?:\d{1,3}(?:,?\d{3})*)|(?:(?:\d{1,3}(?:,?\d{3})*\s+(?!/))?\d{1,3}\s*/\s*\d{1,3})\s*-\s*(?:\d{1,3}(?:,?\d{3})*)|(?:(?:\d{1,3}(?:,?\d{3})*\s+(?!/))?\d{1,3}\s*/\s*\d{1,3})))(?<Ending>(?:\s+\d{1,3}(?:,?\d{3})*\s*(?:to\s+\d{1,3}(?:,?\d{3})*)?-\s*[a-z]+)?[a-z !?\(\):;,\-\.]*)$", RegexOptions.IgnoreCase)]
    private static partial Regex QuantityRegex();

    [GeneratedRegex("[a-zA-Z0-9/, ():-]")]
    private static partial Regex SanitizeRegex();

    [GeneratedRegex(@"(?<Number>\d)(?<Letter>[a-zA-Z])")]
    private static partial Regex DigitTextRegex();

    private static readonly Regex MeasurementRegex;

    static RecipeIngredientParser()
    {
        var measurements = Enum.GetNames<MeasurementUnit>();
        MeasurementRegex = new Regex(
            "(?:[^a-zA-Z]|^)("
            + string.Join(
                '|',
                measurements
                    .Select(
                        x =>
                            x.Humanize()
                                .Pluralize()
                                .Replace(
                                    " ",
                                    string.Empty))
                    .Concat(
                        measurements))
            + ")(?:[^a-zA-Z]|$)",
            RegexOptions.IgnoreCase);
    }

    internal static async IAsyncEnumerable<RecipeStepIngredient> ParseRecipeStepIngredients(
        MyCookbookContext db,
        Guid recipeGuid,
        Guid recipeStepGuid,
        string str,
        List<Ingredient> ingredientsAlreadyMatchedAdded,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var sanitizedInput = Sanitize(
            str);
        foreach (var ingredient in GetIngredients(
                     sanitizedInput))
        {
            var rawQuantities = GetRawQuantities(
                    ingredient)
                .Distinct()
                .ToList();
            var firstMeasurement = rawQuantities.First().Measurement;
            var rawQuantitiesAndConversionRates = rawQuantities
                .Where(
                    x =>
                        x is { Measurement: not null, QuantityType.ParsedValue: QuantityType.Number })
                .Select(
                    x =>
                        new
                        {
                            RawQuantity = x,
                            ConversionRate = GetConversionRate(
                                x.Measurement!.ParsedValue,
                                firstMeasurement!.ParsedValue)!
                        })
                .ToList();
            var rawQuantitiesWithConversionRates = rawQuantitiesAndConversionRates
                .Where(
                    x =>
                        x.ConversionRate != null)
                .ToList();
            if (rawQuantitiesWithConversionRates.Count > 0)
            {
                var totalRawQuantity = AddRawQuantities(
                    rawQuantitiesWithConversionRates.Select(x => x.RawQuantity).ToList());
                var matchedTokens = rawQuantitiesWithConversionRates
                    .Select(
                        x =>
                            x.RawQuantity.Measurement!.MatchedValue)
                    .Concat(
                        rawQuantitiesWithConversionRates
                            .Select(
                                x =>
                                    x.RawQuantity.NumberValue!.MatchedValue))
                    .Where(x => x != null)
                    .Select(x => x!)
                    .ToList();
                var name = GetName(
                    ingredient,
                    matchedTokens,
                    totalRawQuantity.MeasurementUnit);
                matchedTokens.Add(
                    name.MatchedValue!);
                var notes = GetNotes(
                    ingredient,
                    matchedTokens);
                var recipeStepIngredientFromDb = await db.StepIngredients.FirstOrDefaultAsync(
                    x =>
                        x.RecipeStep.RecipeId == recipeGuid
                        && x.Ingredient.Name == name.ParsedValue
                        && x.RawText == str
                        && x.Notes == notes
                        && x.NumberValue == totalRawQuantity.NumberValue
                        && x.QuantityType == QuantityType.Number
                        && x.Unit == totalRawQuantity.MeasurementUnit,
                    cancellationToken);
                if (recipeStepIngredientFromDb == null)
                {
                    var ingredientFromDb = ingredientsAlreadyMatchedAdded.FirstOrDefault(x => x.Name == name.ParsedValue)
                                           ?? await db.Ingredients.FirstOrDefaultAsync(
                                               x => x.Name == name.ParsedValue,
                                               cancellationToken);
                    if (ingredientFromDb == null)
                    {
                        var ingFromDb = await db.Ingredients.AddAsync(
                            new Ingredient
                            {
                                Name = name.ParsedValue
                            },
                            cancellationToken);
                        ingredientFromDb = ingFromDb.Entity;
                        ingredientsAlreadyMatchedAdded.Add(ingFromDb.Entity);
                    }

                    var rsiFromDb = await db.StepIngredients.AddAsync(
                        new RecipeStepIngredient
                        {
                            Ingredient = ingredientFromDb,
                            Unit = totalRawQuantity.MeasurementUnit,
                            NumberValue = totalRawQuantity.NumberValue,
                            QuantityType = QuantityType.Number,
                            Notes = notes,
                            RawText = str,
                            RecipeStepId = recipeStepGuid
                        },
                        cancellationToken);
                    recipeStepIngredientFromDb = rsiFromDb.Entity;
                }

                yield return recipeStepIngredientFromDb;
            }

            foreach (var rawQuantity in rawQuantitiesWithConversionRates
                         .Where(
                             x =>
                                 x.ConversionRate == null))
            {
                var matchedTokens = rawQuantities
                    .Select(
                        x =>
                            x.Measurement!.MatchedValue)
                    .Concat(
                        rawQuantities
                            .Select(
                                x =>
                                    x.NumberValue!.MatchedValue))
                    .Where(x => x != null)
                    .Select(x => x!)
                    .ToList();
                var name = GetName(
                    ingredient,
                    matchedTokens,
                    rawQuantity.RawQuantity.Measurement!.ParsedValue);
                matchedTokens.Add(
                    name.MatchedValue!);
                var notes = GetNotes(
                    ingredient,
                    matchedTokens);
                var recipeStepIngredientFromDb = await db.StepIngredients.FirstOrDefaultAsync(
                    x =>
                        x.RecipeStep.RecipeId == recipeGuid
                        && x.Ingredient.Name == name.ParsedValue
                        && x.RawText == str
                        && x.Notes == notes
                        && x.NumberValue == (rawQuantity.RawQuantity.NumberValue != null
                            ? rawQuantity.RawQuantity.NumberValue.ParsedValue
                            : null)
                        && x.MinValue == (rawQuantity.RawQuantity.MinValue != null
                            ? rawQuantity.RawQuantity.MinValue.ParsedValue
                            : null)
                        && x.MaxValue == (rawQuantity.RawQuantity.MaxValue != null
                            ? rawQuantity.RawQuantity.MaxValue.ParsedValue
                            : null)
                        && x.QuantityType == rawQuantity.RawQuantity.QuantityType.ParsedValue
                        && x.Unit == rawQuantity.RawQuantity.Measurement!.ParsedValue,
                    cancellationToken);
                if (recipeStepIngredientFromDb == null)
                {
                    var ingredientFromDb = ingredientsAlreadyMatchedAdded.FirstOrDefault(x => x.Name == name.ParsedValue)
                                           ?? await db.Ingredients.FirstOrDefaultAsync(
                                               x => x.Name == name.ParsedValue,
                                               cancellationToken);
                    if (ingredientFromDb == null)
                    {
                        var ingFromDb = await db.Ingredients.AddAsync(
                            new Ingredient
                            {
                                Name = name.ParsedValue
                            },
                            cancellationToken);
                        ingredientFromDb = ingFromDb.Entity;
                        ingredientsAlreadyMatchedAdded.Add(ingFromDb.Entity);
                    }

                    var rsiFromDb = await db.StepIngredients.AddAsync(
                        new RecipeStepIngredient
                        {
                            Ingredient = ingredientFromDb,
                            Unit = rawQuantity.RawQuantity.Measurement!.ParsedValue,
                            MinValue = rawQuantity.RawQuantity.MinValue?.ParsedValue,
                            MaxValue = rawQuantity.RawQuantity.MaxValue?.ParsedValue,
                            NumberValue = rawQuantity.RawQuantity.NumberValue?.ParsedValue,
                            QuantityType = rawQuantity.RawQuantity.QuantityType.ParsedValue,
                            Notes = notes,
                            RecipeStepId = recipeStepGuid
                        },
                        cancellationToken);
                    recipeStepIngredientFromDb = rsiFromDb.Entity;
                }

                yield return recipeStepIngredientFromDb;
            }
        }
    }

    private static QuantityTotal AddRawQuantities(
        IReadOnlyCollection<RawQuantity> rawQuantities)
    {
        var baseMeasurement = rawQuantities
            .Select(x => x.Measurement!.ParsedValue)
            .Aggregate(GetLowestMeasurement);
        return new QuantityTotal(
            (decimal) rawQuantities.Sum(x => x.NumberValue!.ParsedValue * GetConversionRate(x.Measurement!.ParsedValue, baseMeasurement)!)!,
            baseMeasurement);
    }

    public static decimal? GetConversionRate(
        MeasurementUnit from,
        MeasurementUnit to) =>
        from switch
        {
            MeasurementUnit.Unit or MeasurementUnit.Piece => to switch
            {
                MeasurementUnit.Unit or MeasurementUnit.Piece => 1M,
                _ => null
            },
            MeasurementUnit.Slice => to switch
            {
                MeasurementUnit.Slice => 1M,
                _ => null
            },
            MeasurementUnit.Pound => to switch
            {
                MeasurementUnit.Pound => 1M,
                _ => null
            },
            MeasurementUnit.Stick => to switch
            {
                MeasurementUnit.Stick => 1M,
                _ => null
            },
            MeasurementUnit.Clove => to switch
            {
                MeasurementUnit.Clove => 1M,
                _ => null
            },
            MeasurementUnit.Bunch => to switch
            {
                MeasurementUnit.Bunch => 1M,
                _ => null
            },
            MeasurementUnit.Fillet => to switch
            {
                MeasurementUnit.Fillet => 1M,
                _ => null
            },
            MeasurementUnit.Inch => to switch
            {
                MeasurementUnit.Inch => 1M,
                _ => null
            },
            MeasurementUnit.Can => to switch
            {
                MeasurementUnit.Inch => 1M,
                MeasurementUnit.Can => 1M,
                _ => null
            },
            MeasurementUnit.Cup => to switch
            {
                MeasurementUnit.Cup => 1M,
                MeasurementUnit.TableSpoon => 16M,
                MeasurementUnit.TeaSpoon => 48M,
                MeasurementUnit.Ounce => 8M,
                _ => null
            },
            MeasurementUnit.TableSpoon => to switch
            {
                MeasurementUnit.Cup => 1 / 48,
                MeasurementUnit.TableSpoon => 1M,
                MeasurementUnit.TeaSpoon => 3M,
                MeasurementUnit.Ounce => 1 / 2,
                _ => null
            },
            MeasurementUnit.TeaSpoon => to switch
            {
                MeasurementUnit.Cup => 1 / 48,
                MeasurementUnit.TableSpoon => 1 / 3,
                MeasurementUnit.TeaSpoon => 1M,
                MeasurementUnit.Ounce => 1 / 6,
                _ => null
            },
            MeasurementUnit.Ounce => to switch
            {
                MeasurementUnit.Cup => 1 / 8,
                MeasurementUnit.TableSpoon => 2M,
                MeasurementUnit.TeaSpoon => 6M,
                MeasurementUnit.Ounce => 1M,
                _ => null
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(from),
                from,
                null)
        };

    public static MeasurementUnit GetLowestMeasurement(
        MeasurementUnit m1,
        MeasurementUnit m2) =>
        m1 switch
        {
            MeasurementUnit.Unit
                or MeasurementUnit.Piece
                or MeasurementUnit.Slice
                or MeasurementUnit.Clove
                or MeasurementUnit.Pound
                or MeasurementUnit.Stick
                or MeasurementUnit.Bunch
                or MeasurementUnit.Fillet
                or MeasurementUnit.Inch
                or MeasurementUnit.Can
                or MeasurementUnit.Cup => m2,
            MeasurementUnit.TableSpoon => m2 switch
            {
                MeasurementUnit.Cup => MeasurementUnit.TableSpoon,
                MeasurementUnit.TableSpoon => MeasurementUnit.TableSpoon,
                _ => m2
            },
            MeasurementUnit.TeaSpoon => m2 switch
            {
                MeasurementUnit.Cup => MeasurementUnit.TeaSpoon,
                MeasurementUnit.TableSpoon => MeasurementUnit.TeaSpoon,
                _ => m2
            },
            MeasurementUnit.Ounce => MeasurementUnit.Ounce,
            _ => throw new ArgumentOutOfRangeException(
                nameof(m1),
                m1,
                null)
        };

    public static string Sanitize(
        string str)
    {
        var spacedInput = str.ToList();
        foreach (var match in DigitTextRegex().EnumerateMatches(str))
        {
            spacedInput.Insert(match.Index + 1, ' ');
        }

        return new string(
            spacedInput.Select(c =>
                    SanitizeRegex()
                        .IsMatch(
                            c.ToString())
                        ? c
                        : c switch
                        {
                            '“' or '”' => '"',
                            ';' => ':',
                            '\t' => ' ',
                            '\\' => '/',
                            _ => ' '
                        })
                .ToArray());
    }

    private static IEnumerable<string> GetIngredients(
        string str)
    {
        var potentialSections = str
            .Split(
                [
                    " AND ",
                    " And ",
                    " and "
                ],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(RemoveParentheticalSets)
            .Select(
                x => new
                {
                    Measurement = GetMeasurement(x),
                    Quantity = GetQuantity(x),
                    Section = x
                })
            .ToList();
        if (potentialSections.Count > 1
                 && potentialSections.All(x => x.Measurement.ParsedValue == MeasurementUnit.Unit && x.Quantity.QuantityType.ParsedValue == QuantityType.Unknown))
        {
            for (var i = 0; i < potentialSections.Count; i++)
            {
                var potentialSection = potentialSections[i];
                var remainingFromThisSection = potentialSection.Section
                    .Replace(
                        potentialSection.Quantity.QuantityType.MatchedValue!,
                        string.Empty)
                    .Replace(
                        potentialSection.Measurement.MatchedValue!,
                        string.Empty)
                    .Trim();
                if (string.IsNullOrEmpty(remainingFromThisSection) && potentialSections.Count > i + 1)
                {
                    var nextSection = potentialSections[i + 1];
                    var remainingFromNextSection = nextSection.Section
                        .Replace(
                            nextSection.Quantity.QuantityType.MatchedValue!,
                            string.Empty)
                        .Replace(
                            nextSection.Measurement.MatchedValue!,
                            string.Empty)
                        .Trim();
                    yield return $"{potentialSection.Section} {remainingFromNextSection}";
                }
                else
                {
                    yield return potentialSection.Section;
                }
            }
        }
        else
        {
            yield return str;
        }
    }

    public static TokenMatch<string> GetName(
        string str,
        IReadOnlyCollection<string> otherMatchedTokens,
        MeasurementUnit measurementUnit)
    {
        var cleanedValue = CleanForTextValues(
            str,
            otherMatchedTokens);
        var sections = cleanedValue
            .Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (sections.Length > 0)
        {
            return new TokenMatch<string>(
                sections.First(),
                cleanedValue);
        }

        var hasDefaultValue = DefaultValues.TryGetValue(measurementUnit, out var defaultValue);
        if (hasDefaultValue)
        {
            return new TokenMatch<string>(
                defaultValue!,
                cleanedValue);
        }

        throw new Exception($"No valid name found. '{str}'");
    }

    private static string CleanForTextValues(
        string str,
        IReadOnlyCollection<string> otherMatchedTokens)
    {
        var data = new StringBuilder(
            RemoveParentheticalSets(
                str));
        var cleanedValue = otherMatchedTokens
            .Aggregate(
                data,
                (current, otherMatchedToken) =>
                    string.IsNullOrWhiteSpace(otherMatchedToken)
                        ? current
                        : current
                            .Replace(
                                otherMatchedToken,
                                " "))
            .Replace('-', ' ')
            .Replace("  ", " ")
            .ToString()
            .Trim();
        if (otherMatchedTokens.Count > 2)
        {
            cleanedValue = cleanedValue
                .Replace(
                    "PLUS",
                    string.Empty,
                    StringComparison.InvariantCultureIgnoreCase)
                .Trim();
        }

        var measurementValue = GetMeasurement(
            cleanedValue);
        while (measurementValue.ParsedValue != MeasurementUnit.Unit || (measurementValue.MatchedValue!.Contains("unit", StringComparison.InvariantCultureIgnoreCase) && cleanedValue != measurementValue.MatchedValue))
        {
            cleanedValue = cleanedValue
                .Replace(
                    measurementValue.MatchedValue!,
                    string.Empty)
                .Trim();
            measurementValue = GetMeasurement(
                cleanedValue);
        }

        return cleanedValue;
    }

    public static TokenMatch<MeasurementUnit> GetMeasurement(
        string str)
    {
        var matchedValue = MeasurementRegex
            .Matches(
                str)
            .FirstOrDefault()
            ?.Value;
        var cleanedText = matchedValue
            ?.Trim(',', ' ', '-', '/', '\\')
            .Humanize()
            .Singularize()
            .Replace(
                " ",
                string.Empty)
            .Replace(
                "lb",
                "Pound",
                true,
                CultureInfo.InvariantCulture)
            .Replace(
                "Clofe",
                "Clove",
                true,
                CultureInfo.InvariantCulture);
        return matchedValue != null
            ? new TokenMatch<MeasurementUnit>(
                Enum.TryParse(
                    cleanedText,
                    true,
                    out MeasurementUnit measurementUnit)
                    ? measurementUnit
                    : throw new Exception($"Could not parse '{matchedValue}' (raw '{str}')"),
                matchedValue)
            : new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, str);
    }

    public static RawQuantity GetQuantity(
        string str)
    {
        var match = QuantityRegex()
            .Matches(str)
            .FirstOrDefault();
        if (match == null)
        {
            return new RawQuantity(
                new TokenMatch<QuantityType>(QuantityType.Unknown, str),
                null,
                null,
                new TokenMatch<decimal>(1, str),
                null);
        }

        if (match.Groups["Number"].Success)
        {
            var numberValue = match.Groups["Number"].Value;
            return new RawQuantity(
                new TokenMatch<QuantityType>(QuantityType.Number, numberValue),
                null,
                null,
                new TokenMatch<decimal>(ParseFractionSection(numberValue), numberValue),
                null);
        }

        if (match.Groups["Range"].Success)
        {
            var rangeValue = match.Groups["Range"].Value;
            var r1 = match.Groups["R1"].Value;
            var r2 = match.Groups["R2"].Value;
            var rangeItems = new[]
            {
                (Parsed: ParseFractionSection(r1), Raw: r1),
                (Parsed: ParseFractionSection(r2), Raw: r2)
            }.OrderByDescending(x => x.Parsed)
            .ToArray();
            return new RawQuantity(
                new TokenMatch<QuantityType>(QuantityType.Range, rangeValue),
                new TokenMatch<decimal>(rangeItems[0].Parsed, rangeItems[0].Raw),
                new TokenMatch<decimal>(rangeItems[1].Parsed, rangeItems[1].Raw),
                null,
                null);
        }

        return new RawQuantity(
            new TokenMatch<QuantityType>(QuantityType.Unknown, str),
            null,
            null,
            null,
            null);
    }

    public static decimal ParseFractionSection(
        string section)
    {
        if (!section.Contains('/'))
        {
            return decimal.Parse(
                section);
        }

        var parts = section
            .Replace(' ', '~')
            .Replace("~/", "/")
            .Replace("/~", "/")
            .Replace('~', ' ')
            .Split(
                ' ',
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        decimal number;
        string fractionPart;
        switch (parts.Length)
        {
            case 2:
                number = decimal.Parse(parts[0]);
                fractionPart = parts[1];
                break;
            case 1:
                number = 0;
                fractionPart = section;
                break;
            default:
                throw new Exception($"Invalid fraction parsing: {section}");
        }

        var fractionParts = fractionPart.Split(
                '/',
                StringSplitOptions.TrimEntries)
            .Select(
                decimal.Parse)
            .ToList();
        return number + (fractionParts[0] / fractionParts[1]);
    }

    private static IEnumerable<RawQuantity> GetRawQuantities(
        string str)
    {
        var data = RemoveParentheticalSets(
            str);
        var sections = data
            .Split(
                [
                    " PLUS ",
                    " Plus ",
                    " plus "
                ],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (sections.Length > 1)
        {
            var preParsedData = sections.Select(
                x =>
                    new
                    {
                        Measurement = GetMeasurement(
                            x),
                        Quantity = GetQuantity(
                            x)
                    })
                .ToList();
            foreach (var section in preParsedData)
            {
                yield return section.Quantity
                    with
                    {
                        Measurement = section.Measurement
                    };
            }
        }

        var measurement = GetMeasurement(
                             data);
        var quantityType = GetQuantity(
            data);
        yield return quantityType
                with
                {
                    Measurement = measurement
                };
    }

    public static string GetNotes(
        string str,
        IReadOnlyCollection<string> otherMatchedTokens)
    {
        var cleanedValue = CleanForTextValues(
                str,
                otherMatchedTokens)
            .Trim(' ', ',');
        var results = GetParentheticalSetsContents(
                str)
            .ToList();
        return cleanedValue
               + (results.Count != 0
                   ? " (" + string.Join(
                       ", ",
                       results) + ')'
                   : string.Empty)
               .Trim();
    }

    public static string RemoveParentheticalSets(
        string str)
    {
        var data = str;
        var indexOfOpen = data.IndexOf(
            '(');
        var indexOfClose = indexOfOpen > -1
            ? data.IndexOf(
                ')',
                indexOfOpen)
            : -1;
        while (
            indexOfOpen > -1
            && indexOfClose > -1)
        {
            data = data.Remove(
                    indexOfOpen,
                    indexOfClose - indexOfOpen + 1)
                .Trim();
            indexOfOpen = data.IndexOf(
                '(');
            indexOfClose = indexOfOpen > -1
                ? data.IndexOf(
                    ')',
                    indexOfOpen)
                : -1;
        }

        return data;
    }

    public static IEnumerable<string> GetParentheticalSetsContents(
        string str)
    {
        var startIndex = 0;
        var indexOfOpen = str.IndexOf(
            '(',
            startIndex);
        var indexOfClose = indexOfOpen > -1
            ? str.IndexOf(
                ')',
                indexOfOpen)
            : -1;
        while (
            indexOfOpen > -1
            && indexOfClose > -1)
        {
            yield return str.Substring(
                indexOfOpen + 1,
                indexOfClose - indexOfOpen - 1);
            startIndex = indexOfClose;
            indexOfOpen = str.IndexOf(
                '(',
                startIndex);
            indexOfClose = indexOfOpen > -1
                ? str.IndexOf(
                    ')',
                    indexOfOpen)
                : -1;
        }
    }
}

public sealed record QuantityTotal(
    decimal? NumberValue,
    MeasurementUnit MeasurementUnit);

public sealed record RawQuantity(
    TokenMatch<QuantityType> QuantityType,
    TokenMatch<decimal>? MinValue,
    TokenMatch<decimal>? MaxValue,
    TokenMatch<decimal>? NumberValue,
    TokenMatch<MeasurementUnit>? Measurement);

public sealed record TokenMatch<T>(
    T ParsedValue,
    string? MatchedValue);