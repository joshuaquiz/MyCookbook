using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Humanizer;
using MyCookbook.Common.Enums;
using Ingredient = MyCookbook.Common.Database.Ingredient;
using RecipeStepIngredient = MyCookbook.Common.Database.RecipeStepIngredient;

namespace MyCookbook.API.Implementations;

public static partial class RecipeIngredientParser
{

    [GeneratedRegex(@"(?:[^a-z0-9]|^)(?:(?:(?<Name>(?:\s*\d{1,3}\/\d{1,3})\s*(?:[^a-z0-9\-]|$)))|(?:(?<Name>\d{1,3}\s+)?(?:\d{1,3})\-[a-z])|(?:(?<Name>\d{1,3}(?:\s+\d{1,3}\/\d{1,3})?(?:[^a-z0-9\-]|$))))", RegexOptions.IgnoreCase)]
    private static partial Regex QuantityRegex();

    [GeneratedRegex("[a-zA-Z0-9/, ():-]")]
    private static partial Regex SanitizeRegex();

    /*private static readonly Regex UnicodeRegex = new(
        "\\p{Z}",
        RegexOptions.IgnoreCase);
    private static readonly Regex NumbersRegex = new(
        "\\d",
        RegexOptions.IgnoreCase);
    private static readonly Regex LettersRegex = new(
        "[A-Z]",
        RegexOptions.IgnoreCase);*/
    private static readonly Regex MeasurementRegex;

    static RecipeIngredientParser()
    {
        var measurements = Enum.GetNames<Measurement>();
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

    /*public static IReadOnlyList<RecipeIngredient> Parse(
        string str)
    {
        List<RecipeIngredient> items;
        try
        {
            items = UnicodeRegex.Replace(
                    str,
                    " ")
                .Split(
                    [
                        " AND ",
                        " PLUS ",
                        " And ",
                        " Plus ",
                        " and ",
                        " plus "
                    ],
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ParseSingle)
                .ToList();
        }
        catch
        {
            items =
            [
                ParseSingle(str)
            ];
        }

        var emptyNames = items
            .Where(x =>
                string.IsNullOrWhiteSpace(
                    x.Ingredient.Name))
            .ToList();
        if (str.Contains(
                " PLUS ",
                StringComparison.InvariantCultureIgnoreCase)
            && emptyNames.Any())
        {
            var defaultName = items
                .FirstOrDefault(x =>
                    !string.IsNullOrWhiteSpace(
                        x.Ingredient.Name));
            foreach (var recipeIngredient in emptyNames)
            {
                recipeIngredient.Ingredient.Name = defaultName?.Ingredient.Name
                                                   ?? string.Empty;
            }
        }

        return items;
    }

    private static RecipeIngredient ParseSingle(
        string str)
    {
        var name = str;
        var quantity = "1";
        var measurement = Measurement.Unit;
        string? notes = null;
        try
        {
            name = UnicodeRegex.Replace(
                name,
                " ");
            var mainSections = name.Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries);
            var mainSection = mainSections.FirstOrDefault()
                              ?? string.Empty;
            if (NumbersRegex.IsMatch(mainSection)
                || MeasurementRegex.IsMatch(mainSection))
            {
                var parts = mainSection.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();
                var quantityParts = parts.GetQuantityParts();
                quantityParts.ForEach(x => parts.Remove(x));
                quantity = string.Join(' ', quantityParts);
                var measurementPart = parts.GetMeasurementPart();
                if (measurementPart != null)
                {
                    parts.Remove(measurementPart);
                }

                measurement = measurementPart.ParseMeasurement();
                name = string.Join(' ', parts);
                if (mainSections.Length > 1)
                {
                    notes = string.Join(
                            ' ',
                            mainSections.Skip(
                                1))
                        .Trim()
                        .Replace(
                            "  ",
                            " ");
                }

                var indexOfOpen = name.IndexOf(
                    '(');
                while (indexOfOpen > -1)
                {
                    var indexOfClose = name.IndexOf(
                        ')');
                    var adjustedIndexOfOpen = indexOfOpen == 0
                        ? 0
                        : indexOfOpen - 1;
                    var adjustedIndexOfClose = indexOfClose + 1;
                    var extra = name[adjustedIndexOfOpen..adjustedIndexOfClose];
                    name = name
                        .Replace(
                            extra,
                            string.Empty);
                    extra = extra
                        .Trim()
                        .Trim(
                            '(')
                        .Trim(
                            ')')
                        .Trim()
                        .Replace(
                            "  ",
                            " ");
                    notes = notes?.Length > 1
                        ? notes + "; " + extra
                        : extra;
                    indexOfOpen = name.IndexOf(
                        '(');
                }
            }
        }
        catch (Exception e)
        {
            notes = e.Message;
        }

        return new RecipeIngredient
        {
            Ingredient = new Ingredient
            {
                Name = name.CleanNameForDisplay()
            },
            Measurement = measurement,
            Quantity = quantity,
            Notes = notes
        };
    }

    private static string CleanNameForDisplay(
        this string str) =>
        str
            .Trim()
            .Humanize(
                LetterCasing.Sentence)
            .Replace(
                " s ",
                "'s ");

    private static List<string> GetQuantityParts(
        this IEnumerable<string> parts) =>
        parts
            .Where(
                x =>
                    !LettersRegex.IsMatch(
                        x)
                    && NumbersRegex.IsMatch(
                        x))
            .ToList();

    private static string? GetMeasurementPart(
        this IEnumerable<string> parts) =>
        parts.FirstOrDefault(
            MeasurementRegex.IsMatch);

    private static Measurement ParseMeasurement(
        this string? str)
    {
        var value = str?.TrimEnd('s');
        return string.IsNullOrWhiteSpace(value)
            || !MeasurementRegex.IsMatch(value)
            ? Measurement.Unit
            : Enum.Parse<Measurement>(
                value,
                true);
    }*/

    public static IEnumerable<RecipeStepIngredient> Parse(
        string str)
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
                .Select(
                    x =>
                        new
                        {
                            RawQuantity = x,
                            ConversionRate = GetConversionRate(
                                x.Measurement.ParsedValue,
                                firstMeasurement.ParsedValue)!
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
                            x.RawQuantity.Measurement.MatchedValue)
                    .Concat(
                        rawQuantitiesWithConversionRates
                            .Select(
                                x =>
                                    x.RawQuantity.Quantity.MatchedValue))
                    .Where(x => x != null)
                    .Select(x => x!)
                    .ToList();
                var name = GetName(
                    ingredient,
                    matchedTokens);
                matchedTokens.Add(
                    name.MatchedValue!);
                var notes = GetNotes(
                    ingredient,
                    matchedTokens);
                yield return new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = name.ParsedValue
                    },
                    Measurement = totalRawQuantity.Measurement,
                    Quantity = totalRawQuantity.Quantity
                        .ToString(
                            CultureInfo.InvariantCulture),
                    Notes = notes
                };
            }

            foreach (var rawQuantity in rawQuantitiesWithConversionRates
                         .Where(
                             x =>
                                 x.ConversionRate == null))
            {
                var totalRawQuantity = new QuantityTotal(
                    rawQuantity.RawQuantity.Quantity.ParsedValue,
                    rawQuantity.RawQuantity.Measurement.ParsedValue);
                var matchedTokens = rawQuantities
                    .Select(
                        x =>
                            x.Measurement.MatchedValue)
                    .Concat(
                        rawQuantities
                            .Select(
                                x =>
                                    x.Quantity.MatchedValue))
                    .Where(x => x != null)
                    .Select(x => x!)
                    .ToList();
                var name = GetName(
                    ingredient,
                    matchedTokens);
                matchedTokens.Add(
                    name.MatchedValue!);
                var notes = GetNotes(
                    ingredient,
                    matchedTokens);
                yield return new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = name.ParsedValue
                    },
                    Measurement = totalRawQuantity.Measurement,
                    Quantity = totalRawQuantity.Quantity
                        .ToString(
                            CultureInfo.InvariantCulture),
                    Notes = notes
                };
            }
        }
    }

    private static QuantityTotal AddRawQuantities(
        IReadOnlyCollection<RawQuantity> rawQuantities)
    {
        var baseMeasurement = rawQuantities
            .Select(x => x.Measurement.ParsedValue)
            .Aggregate(GetLowestMeasurement);
        return new QuantityTotal(
            (decimal) rawQuantities.Sum(x => x.Quantity.ParsedValue * GetConversionRate(x.Measurement.ParsedValue, baseMeasurement)!)!,
            baseMeasurement);
    }

    public static decimal? GetConversionRate(
        Measurement from,
        Measurement to) =>
        from switch
        {
            Measurement.Unit or Measurement.Piece => to switch
            {
                Measurement.Unit or Measurement.Piece => 1M,
                _ => null
            },
            Measurement.Slice => to switch
            {
                Measurement.Slice => 1M,
                _ => null
            },
            Measurement.Clove => to switch
            {
                Measurement.Clove => 1M,
                _ => null
            },
            Measurement.Bunch => to switch
            {
                Measurement.Bunch => 1M,
                _ => null
            },
            Measurement.Fillet => to switch
            {
                Measurement.Fillet => 1M,
                _ => null
            },
            Measurement.Inch => to switch
            {
                Measurement.Inch => 1M,
                _ => null
            },
            Measurement.Can => to switch
            {
                Measurement.Inch => 1M,
                Measurement.Can => 1M,
                _ => null
            },
            Measurement.Cup => to switch
            {
                Measurement.Cup => 1M,
                Measurement.TableSpoon => 16M,
                Measurement.TeaSpoon => 48M,
                Measurement.Ounce => 8M,
                _ => null
            },
            Measurement.TableSpoon => to switch
            {
                Measurement.Cup => 1 / 48,
                Measurement.TableSpoon => 1M,
                Measurement.TeaSpoon => 3M,
                Measurement.Ounce => 1 / 2,
                _ => null
            },
            Measurement.TeaSpoon => to switch
            {
                Measurement.Cup => 1 / 48,
                Measurement.TableSpoon => 1 / 3,
                Measurement.TeaSpoon => 1M,
                Measurement.Ounce => 1 / 6,
                _ => null
            },
            Measurement.Ounce => to switch
            {
                Measurement.Cup => 1 / 8,
                Measurement.TableSpoon => 2M,
                Measurement.TeaSpoon => 6M,
                Measurement.Ounce => 1M,
                _ => null
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(from),
                from,
                null)
        };

    public static Measurement GetLowestMeasurement(
        Measurement m1,
        Measurement m2) =>
        m1 switch
        {
            Measurement.Unit
                or Measurement.Piece
                or Measurement.Slice
                or Measurement.Clove
                or Measurement.Bunch
                or Measurement.Fillet
                or Measurement.Inch
                or Measurement.Can
                or Measurement.Cup => m2,
            Measurement.TableSpoon => m2 switch
            {
                Measurement.Cup => Measurement.TableSpoon,
                Measurement.TableSpoon => Measurement.TableSpoon,
                _ => m2
            },
            Measurement.TeaSpoon => m2 switch
            {
                Measurement.Cup => Measurement.TeaSpoon,
                Measurement.TableSpoon => Measurement.TeaSpoon,
                _ => m2
            },
            Measurement.Ounce => Measurement.Ounce,
            _ => throw new ArgumentOutOfRangeException(
                nameof(m1),
                m1,
                null)
        };

    public static string Sanitize(
        string str) =>
        new(
            str.Select(
                    c =>
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
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (potentialSections.Length > 1
                 && potentialSections
                     .Select(RemoveParentheticalSets)
                     .All(
                     x => GetMeasurement(
                              x) != null
                          && GetQuantity(
                              x) != null))
        {
            foreach (var potentialSection in potentialSections)
            {
                yield return potentialSection;
            }
        }
        else
        {
            yield return str;
        }
    }

    public static TokenMatch<string> GetName(
        string str,
        IReadOnlyCollection<string> otherMatchedTokens)
    {
        var cleanedValue = CleanForTextValues(
            str,
            otherMatchedTokens);
        var sections = cleanedValue
            .Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return new TokenMatch<string>(
            sections.First(),
            cleanedValue);
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
        while (measurementValue != null)
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

    public static TokenMatch<Measurement>? GetMeasurement(
        string str)
    {
        var matchedValue = MeasurementRegex
            .Matches(
                str)
            .FirstOrDefault()
            ?.Value;
        var measurementString = matchedValue?.Humanize()
            .Singularize()
            .Replace(
                " ",
                string.Empty);
        return measurementString != null
            ? new TokenMatch<Measurement>(
                ParseMeasurement(
                    measurementString),
                matchedValue!)
            : null;
    }

    private static Measurement ParseMeasurement(
        string measurementString)
    {
        return Enum.Parse<Measurement>(
            measurementString
                .Replace(
                    "Clofe",
                    "Clove",
                    true,
                    CultureInfo.InvariantCulture)
                .Replace(
                    "Ounces",
                    "Ounce",
                    true,
                    CultureInfo.InvariantCulture),
            true);
    }

    public static TokenMatch<decimal>? GetQuantity(
        string str)
    {
        // TODO: Needs to handle a range too.
        var matchedValue = QuantityRegex()
            .Matches(str)
            .FirstOrDefault()
            ?.Groups["Name"]
            .Value;
        var parsedValue = matchedValue?.Trim(
            [
                ' ',
                '-'
            ])
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Sum(
                ParseFractionSection);
        return parsedValue.HasValue
            ? new TokenMatch<decimal>(
                parsedValue.Value,
                matchedValue)
            : null;
    }

    public static decimal ParseFractionSection(
        string section)
    {
        if (!section.Contains('/'))
        {
            return decimal.Parse(
                section);
        }

        var fractionParts = section.Split(
                '/',
                StringSplitOptions.TrimEntries)
            .Select(
                decimal.Parse)
            .ToList();
        return fractionParts[0] / fractionParts[1];
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
            foreach (var section in preParsedData.Where(x => x.Measurement != null && x.Quantity != null))
            {
                yield return new RawQuantity(
                    section.Quantity!,
                    section.Measurement!);
            }
        }

        yield return new RawQuantity(
            GetQuantity(
                data)
            ?? new TokenMatch<decimal>(
                1,
                null),
            GetMeasurement(
                data)
            ?? new TokenMatch<Measurement>(
                Measurement.Unit,
                null));
    }

    public static string GetNotes(
        string str,
        IReadOnlyCollection<string> otherMatchedTokens)
    {
        var cleanedValue = CleanForTextValues(
                str,
                otherMatchedTokens)
            .Trim(
            [
                ' ',
                ','
            ]);
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
        var data = str;
        var startIndex = 0;
        var indexOfOpen = data.IndexOf(
            '(',
            startIndex);
        var indexOfClose = indexOfOpen > -1
            ? data.IndexOf(
                ')',
                indexOfOpen)
            : -1;
        while (
            indexOfOpen > -1
            && indexOfClose > -1)
        {
            yield return data.Substring(
                indexOfOpen + 1,
                indexOfClose - indexOfOpen - 1);
            startIndex = indexOfClose;
            indexOfOpen = data.IndexOf(
                '(',
                startIndex);
            indexOfClose = indexOfOpen > -1
                ? data.IndexOf(
                    ')',
                    indexOfOpen)
                : -1;
        }
    }
}

public sealed record QuantityTotal(
    decimal Quantity,
    Measurement Measurement);

public sealed record RawQuantity(
    TokenMatch<decimal> Quantity,
    TokenMatch<Measurement> Measurement);

public sealed record TokenMatch<T>(
    T ParsedValue,
    string? MatchedValue);