using Humanizer;
using Microsoft.EntityFrameworkCore;
using MyCookbook.Common.Database;
using MyCookbook.Common.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Ingredient = MyCookbook.Common.Database.Ingredient;

namespace MyCookbook.API.Implementations;

public static partial class RecipeIngredientParser
{
    private static readonly IReadOnlyDictionary<string, string> MeasurementSynonyms = new Dictionary<string, string>
    {
        { "clofe", nameof(MeasurementUnit.Clove) },
        { "lb", nameof(MeasurementUnit.Pound) },
        { "oz", nameof(MeasurementUnit.Ounce) },
        { "tsp", nameof(MeasurementUnit.TeaSpoon) },
        { "tbsp", nameof(MeasurementUnit.TableSpoon) },
        { "pkg", nameof(MeasurementUnit.Package) },
        { "box", nameof(MeasurementUnit.Package) },
        { "dash", nameof(MeasurementUnit.Drop) }
    };

    private static readonly IReadOnlyDictionary<string, string> MeasurementOverrides = new Dictionary<string, string>
    {
        { nameof(MeasurementUnit.Head), @"(?<!(?:fish|salmon|cod|tuna|tilapia|halibut|mackerel|flounder|snapper|sea-bass|seabass|sea\s+bass|bass|mahi-mahi|mahimahi|mahi\s+mahi)\s+)(?:head|heads)" },
        { nameof(MeasurementUnit.Stick), @"(?<!(?:fish)\s+)(?:stick|sticks)" },
        { nameof(MeasurementUnit.Package), @"(?:(?:box(?!ed|(?:\s+(?:grater|knife)))|boxes|package|packages|pkg(?:\.)?|pkgs))" }
    };

    private static readonly ReadOnlyDictionary<MeasurementUnit, string> DefaultValues =
        new(
            new Dictionary<MeasurementUnit, string>
            {
                { MeasurementUnit.Clove, "clove" }
            });

    [GeneratedRegex(@"(?:^[^a-z0-9]*|\b)(?:(?:(?<Range>(?<R1>(?:(?:\d+(?!,)(?=\.|\s+))|\d{1,3}(?:,?\d{3})*)(?:\.\d+)?(?:(?:\s+(?:(?:\d+(?!,)(?=\.|\s+))|\d{1,3}(?:,?\d{3})*)(?:\.\d+)?)?\s*/\s*(?:(?:\d+(?!,)(?=\.|\s+))|\d{1,3}(?:,?\d{3})*)(?:\.\d+)?)?)?\s+to\s+(?<R2>(?:(?:\d+(?!,)(?=\.|\s+))|\d{1,3}(?:,?\d{3})*)(?:\.\d+)?(?:(?:\s+(?:(?:\d+(?!,)(?=\.|\s+))|\d{1,3}(?:,?\d{3})*)(?:\.\d+)?)?\s*/\s*(?:(?:\d+(?!,)(?=\.|\s+))|\d{1,3}(?:,?\d{3})*)(?:\.\d+)?)?))|(?<Number>(?:(?:\d+(?!,)(?=\.|\s+))|\d{1,3}(?:,?\d{3})*)(?:\.\d+)?(?:(?:\s+(?:(?:\d+(?!,)(?=\.|\s+))|\d{1,3}(?:,?\d{3})*)(?:\.\d+)?)?\s*/\s*(?:(?:\d+(?!,)(?=\.|\s+))|\d{1,3}(?:,?\d{3})*)(?:\.\d+)?)?(?!\s+to\s+)))(?!-))(?<Ending>.*)$", RegexOptions.IgnoreCase)]
    private static partial Regex QuantityRegex();

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex MultiWhitespaceRegex();

    [GeneratedRegex(@"(?<!\([^\)]*)(and(?>!squeezed)|(?:beaten with)|(?:whisked with)|(?:whipped with))(?= )")]
    private static partial Regex LineSeparatorRegex();

    [GeneratedRegex(@"[a-zA-Z0-9/, \(\):\-\.!'%]")]
    private static partial Regex SanitizeRegex();

    [GeneratedRegex(@"^[^\d]+$")]
    private static partial Regex NoNumbersRegex();

    [GeneratedRegex(@"(?<=^|\s|\()(?<Number>\d+)(?!(?:s|rds|nds|nd|st|erd|rd|erds|rds|th|ths)(?:\s|$))(?=[a-zA-Z\)])")]
    private static partial Regex DigitTextRegex();

    [GeneratedRegex(@"\b(?:(?<!\((?!\)))\s*)(?:(?:fresh|flaked|fine(?:ly)?|coarse|kosher|sea)\s+)*salt(?:(?:\s+\(\s*(?:(?:fresh|flaked|fine(?:ly)?|coarse|kosher|sea)\s*)*\))|\b)", RegexOptions.IgnoreCase)]
    private static partial Regex SaltRegex();

    [GeneratedRegex(@"\b(?:(?<!chile|serrano|(?:\((?!\))))\s*)(?:(?:fresh(?:ly)?|fine(?:ly)?|coarse|ground|black|cracked)\s+)*pepper(?:(?:\s+\(\s*(?:(?:fresh(?:ly)?|fine(?:ly)?|coarse|ground|black|cracked)\s*)*\))|\b)", RegexOptions.IgnoreCase)]
    private static partial Regex PepperRegex();

    [GeneratedRegex(@"\b((?:(?:(?:pepper(?<PepperQuilifier>s|y|ie)?)|(?:onion(?<OnionQuilifier>s|y|ie)?))(?:\s+and\s+)?)+)\b", RegexOptions.IgnoreCase)]
    private static partial Regex PeppersAndOnionsRegex();

    [GeneratedRegex(@"\b((?<!plum\s+)tomatoe(?:s)?)\b", RegexOptions.IgnoreCase)]
    private static partial Regex TomatoesRegex();

    private static readonly Regex MeasurementRegex;

    static RecipeIngredientParser()
    {
        var measurements = Enum.GetNames<MeasurementUnit>()
            .Except(MeasurementOverrides.Keys)
            .ToList();
        var measurementSynonymsKeys = MeasurementSynonyms
            .Where(x => !MeasurementOverrides.ContainsKey(x.Value))
            .Select(x => x.Key)
            .ToList();
        MeasurementRegex = new Regex(
            @"(?<=[^a-z]|^)(?<!(?:-\s*)|(?:into\s+)|(?:into(?:\s|\-)+\d+(?:\s|\-)?)|(?:(?<!\d[^\s]+)\s+about(?:\s|\-)+\d+(?:\s|\-)?))("
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
                        measurements)
                    .Concat(
                        measurementSynonymsKeys)
                    .Concat(
                        measurementSynonymsKeys
                            .Select(
                                x =>
                                    x.Pluralize()))
                    .Concat(
                        MeasurementOverrides.Values))
            + ")(?=[^a-z]|$)",
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
        foreach (var parsedRecipeStepIngredient in GetParsedRecipeStepIngredients(recipeStepGuid, str, ingredientsAlreadyMatchedAdded))
        {
            var recipeStepIngredientFromDb = await db.StepIngredients.FirstOrDefaultAsync(
                x =>
                    x.RecipeStep.RecipeId == recipeGuid
                    && x.Ingredient.Name == parsedRecipeStepIngredient.Ingredient.Name
                    && x.RawText == str
                    && x.Notes == parsedRecipeStepIngredient.Notes
                    && x.NumberValue == parsedRecipeStepIngredient.NumberValue
                    && x.MinValue == parsedRecipeStepIngredient.MinValue
                    && x.MaxValue == parsedRecipeStepIngredient.MaxValue
                    && x.QuantityType == parsedRecipeStepIngredient.QuantityType
                    && x.Unit == parsedRecipeStepIngredient.Unit,
                cancellationToken);
            var ingredientFromDb = ingredientsAlreadyMatchedAdded.FirstOrDefault(x => x.Name == parsedRecipeStepIngredient.Ingredient.Name)
                                   ?? await db.Ingredients.FirstOrDefaultAsync(
                                       x => x.Name == parsedRecipeStepIngredient.Ingredient.Name,
                                       cancellationToken);
            if (ingredientFromDb == null)
            {
                var ingFromDb = await db.Ingredients.AddAsync(
                    new Ingredient
                    {
                        Name = parsedRecipeStepIngredient.Ingredient.Name
                    },
                    cancellationToken);
                ingredientFromDb = ingFromDb.Entity;
                ingredientsAlreadyMatchedAdded.Add(ingFromDb.Entity);
            }

            if (recipeStepIngredientFromDb == null)
            {
                parsedRecipeStepIngredient.Ingredient = ingredientFromDb;
                var rsiFromDb = await db.StepIngredients.AddAsync(
                    parsedRecipeStepIngredient,
                    cancellationToken);
                recipeStepIngredientFromDb = rsiFromDb.Entity;
            }

            yield return recipeStepIngredientFromDb;
        }
    }

    internal static IEnumerable<RecipeStepIngredient> GetParsedRecipeStepIngredients(
        Guid recipeStepGuid,
        string str,
        List<Ingredient> ingredientsAlreadyMatchedAdded)
    {
        var sanitizedInput = Sanitize(
            str);
        foreach (var ingredient in GetIngredients(
                     sanitizedInput))
        {
            if (str.StartsWith("Egg wash", StringComparison.InvariantCultureIgnoreCase)
                || str.StartsWith("For serving", StringComparison.InvariantCultureIgnoreCase))
            {
                yield return new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = "Egg wash"
                    },
                    Unit = MeasurementUnit.Unit,
                    NumberValue = 1,
                    QuantityType = QuantityType.Number,
                    Notes = str.Replace("Egg wash", string.Empty, StringComparison.InvariantCultureIgnoreCase).Replace("For serving", string.Empty, StringComparison.InvariantCultureIgnoreCase).Trim(' ', ':').TrimStart('(').TrimEnd(')'),
                    RawText = str,
                    RecipeStepId = recipeStepGuid
                };
                yield break;
            }

            var rawQuantities = GetRawQuantities(
                    ingredient)
                .Distinct()
                .ToList();
            var firstMeasurement = rawQuantities.First().Measurement;
            foreach (var item in rawQuantities
                         .Select(x =>
                             new
                             {
                                 RawQuantity = x,
                                 ConversionRate = GetConversionRate(
                                     x.Measurement.ParsedValue,
                                     firstMeasurement.ParsedValue)!
                             })
                         .GroupBy(x =>
                             x.ConversionRate.HasValue)
                         .SelectMany(x =>
                         {
                             var items = x.Select(y => y.RawQuantity).ToList();
                             if (x.Key && items.Count > 1)
                             {
                                 var itemsToAdd = items
                                     .Where(
                                         y =>
                                            y is { QuantityType.ParsedValue: QuantityType.Number })
                                     .ToList();
                                 var itemsNotToAdd = items.Except(itemsToAdd).ToList();
                                 items = itemsNotToAdd;
                                 if (itemsNotToAdd.Count > 0)
                                 {
                                     var total = AddRawQuantities(itemsToAdd);
                                     items.Add(
                                         new RawQuantity(
                                             str,
                                             new TokenMatch<QuantityType>(QuantityType.Number, str, 0),
                                             null,
                                             null,
                                             new TokenMatch<decimal>(total.NumberValue, str, 0),
                                             new TokenMatch<MeasurementUnit>(total.MeasurementUnit, str, 0)));
                                 }
                             }

                             return items;
                         }))
            {
                var matchedTokens = new List<MatchLocation>();
                AddMatchLocation(
                    ref matchedTokens,
                    item.RawText.Length,
                    item.QuantityType);
                AddMatchLocation(
                    ref matchedTokens,
                    item.RawText.Length,
                    item.Measurement);
                var name = GetName(
                    ingredient,
                    matchedTokens);
                AddMatchLocation(
                    ref matchedTokens,
                    item.RawText.Length,
                    name);
                var notes = GetNotes(
                    ingredient,
                    matchedTokens);
                yield return new RecipeStepIngredient
                {
                    Ingredient = new Ingredient
                    {
                        Name = name.ParsedValue
                    },
                    QuantityType = item.QuantityType.ParsedValue,
                    Unit = item.Measurement.ParsedValue,
                    MinValue = item.MinValue?.ParsedValue,
                    MaxValue = item.MaxValue?.ParsedValue,
                    NumberValue = item.NumberValue?.ParsedValue,
                    Notes = notes,
                    RawText = ingredient,
                    RecipeStepId = recipeStepGuid
                };
            }
        }
    }

    private static void AddMatchLocation(
        ref List<MatchLocation> matchLocations,
        int originalLength,
        MatchLocation? newLocation)
    {
        if (newLocation == null
            || matchLocations.Any(x => x.MatchStartIndex == newLocation.MatchStartIndex)
            || (newLocation.MatchStartIndex == 0
                && newLocation.MatchedValue?.Length == originalLength))
        {
            return;
        }

        matchLocations.Add(
            newLocation);
        matchLocations = matchLocations
            .OrderByDescending(x => x.MatchStartIndex)
            .ToList();
    }

    private static QuantityTotal AddRawQuantities(
        IReadOnlyCollection<RawQuantity> rawQuantities)
    {
        var baseMeasurement = rawQuantities
            .Select(x => x.Measurement.ParsedValue)
            .Aggregate(GetLowestMeasurement);
        return new QuantityTotal(
            (decimal) rawQuantities.Sum(x => x.NumberValue!.ParsedValue * GetConversionRate(x.Measurement.ParsedValue, baseMeasurement)!)!,
            baseMeasurement);
    }

    internal static decimal? GetConversionRate(
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
            MeasurementUnit.Package => to switch
            {
                MeasurementUnit.Package => 1M,
                _ => null
            },
            MeasurementUnit.Loaf => to switch
            {
                MeasurementUnit.Loaf => 1M,
                _ => null
            },
            MeasurementUnit.Bottle => to switch
            {
                MeasurementUnit.Bottle => 1M,
                _ => null
            },
            MeasurementUnit.Head => to switch
            {
                MeasurementUnit.Head => 1M,
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
            MeasurementUnit.Drop => to switch
            {
                MeasurementUnit.Drop => 1M,
                MeasurementUnit.Pinch => 6M,
                _ => null
            },
            MeasurementUnit.Pinch => to switch
            {
                MeasurementUnit.Pinch => 1M,
                MeasurementUnit.Drop => 1 / 6,
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

    internal static MeasurementUnit GetLowestMeasurement(
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
                or MeasurementUnit.Package
                or MeasurementUnit.Loaf
                or MeasurementUnit.Bottle
                or MeasurementUnit.Head
                or MeasurementUnit.Bunch
                or MeasurementUnit.Fillet
                or MeasurementUnit.Inch
                or MeasurementUnit.Can
                or MeasurementUnit.Cup => m2,
            MeasurementUnit.Drop => m2 switch
            {
                MeasurementUnit.Drop => MeasurementUnit.Drop,
                MeasurementUnit.Pinch => MeasurementUnit.Drop,
                _ => m2
            },
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

    internal static string Sanitize(
        string str)
    {
        if (!int.TryParse(new string(str[0], 1), out _))
        {
            var parts = str.Split([' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var first = parts[0].ToUpperInvariant();
            var remainder = string.Join(" ", parts[1..]);
            str = first switch
            {
                "ONE" => $"1 {remainder}",
                "TWO" => $"2 {remainder}",
                "THREE" => $"3 {remainder}",
                "FOUR" => $"4 {remainder}",
                "FIVE" => $"5 {remainder}",
                "SIX" => $"6 {remainder}",
                "SEVEN" => $"7 {remainder}",
                "EIGHT" => $"8 {remainder}",
                "NINE" => $"9 {remainder}",
                "TEN" => $"10 {remainder}",
                "ELEVEN" => $"11 {remainder}",
                "TWELVE" => $"12 {remainder}",
                "THIRTEEN" => $"13 {remainder}",
                "FOURTEEN" => $"14 {remainder}",
                "FIFTEEN" => $"15 {remainder}",
                "SIXTEEN" => $"16 {remainder}",
                "SEVENTEEN" => $"17 {remainder}",
                "EIGHTEEN" => $"18 {remainder}",
                "NINETEEN" => $"19 {remainder}",
                "TWENTY" => $"20 {remainder}",
                _ => str
            };
        }

        var spacedInput = str.ToList();
        foreach (var match in DigitTextRegex().EnumerateMatches(str))
        {
            spacedInput.Insert(match.Index + match.Length, ' ');
        }

        return MultiWhitespaceRegex()
                .Replace(
                    new string(spacedInput
                        .Select(c =>
                            SanitizeRegex()
                                .IsMatch(
                                    c.ToString())
                                ? c
                                : c switch
                                {
                                    '�' or '�' => '"',
                                    ';' => ':',
                                    '\t' => ' ',
                                    '\\' => '/',
                                    _ => ' '
                                })
                        .ToArray()),
                    " ");
    }

    internal static IEnumerable<string> GetIngredients(
        string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            yield break;
        }

        if (str.StartsWith("Egg wash", StringComparison.InvariantCultureIgnoreCase)
            || str.StartsWith("For serving", StringComparison.InvariantCultureIgnoreCase))
        {
            yield return str;
            yield break;
        }

        var potentialSections = LineSeparatorRegex().Split(str)
            .Select(x =>
                new
                {
                    Quantity = GetQuantity(x),
                    Section = x
                })
            .ToList();
        if (potentialSections.Count > 1
            && potentialSections.All(x => x.Quantity.QuantityType.ParsedValue != QuantityType.Unknown))
        {
            for (var i = 0; i < potentialSections.Count; i++)
            {
                var potentialSection = potentialSections[i];
                var remainingFromThisSection = potentialSection.Section
                    .Replace(
                        potentialSection.Quantity.QuantityType.MatchedValue!,
                        string.Empty)
                    .Replace(
                        potentialSection.Quantity.Measurement.MatchedValue!,
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
                            nextSection.Quantity.Measurement.MatchedValue!,
                            string.Empty)
                        .Trim();
                    yield return $"{potentialSection.Section} {remainingFromNextSection}".Trim();
                }
                else
                {
                    yield return potentialSection.Section.Trim();
                }
            }
        }
        else
        {
            var saltMatch = SaltRegex().Match(str);
            var pepperMatch = PepperRegex().Match(str);
            if (saltMatch.Success && pepperMatch.Success)
            {
                var start = Math.Min(saltMatch.Index, pepperMatch.Index);
                var prefix = start > 0
                    ? str[..start]
                    : null;
                var end = Math.Max(saltMatch.Index + saltMatch.Length, pepperMatch.Index + pepperMatch.Length);
                var suffix = end < str.Length
                    ? str[end..]
                    : null;
                yield return (prefix + saltMatch.Value + suffix).Trim();
                yield return (prefix + pepperMatch.Value + suffix).Trim();
            }
            else
            {
                yield return str.Trim();
            }
        }
    }

    internal static TokenMatch<string> GetName(
        string str,
        IReadOnlyCollection<MatchLocation> otherMatchedTokens)
    {
        var cleanedValue = ReplaceFoundTokens(
            ReplaceParentheticalSets(
                str),
            otherMatchedTokens);
        var peppersAndOnionsMatch = PeppersAndOnionsRegex().Match(cleanedValue);
        if (peppersAndOnionsMatch.Success)
        {
            var peppersAndOnionsValue = peppersAndOnionsMatch.Value;
            if (peppersAndOnionsMatch.Groups["OnionQuilifier"].Success)
            {
                peppersAndOnionsValue = peppersAndOnionsValue.Replace("onion" + peppersAndOnionsMatch.Groups["OnionQuilifier"].Value, "onion", StringComparison.InvariantCultureIgnoreCase);
            }

            if (peppersAndOnionsMatch.Groups["PepperQuilifier"].Success)
            {
                peppersAndOnionsValue = peppersAndOnionsValue.Replace("pepper" + peppersAndOnionsMatch.Groups["PepperQuilifier"].Value, "pepper", StringComparison.InvariantCultureIgnoreCase);
            }

            var hasTrailingComma = cleanedValue[(peppersAndOnionsMatch.Index + peppersAndOnionsMatch.Length)..]
                .StartsWith(',');
            return new TokenMatch<string>(
                FormatName(peppersAndOnionsValue),
                hasTrailingComma
                    ? peppersAndOnionsMatch.Value + ','
                    : peppersAndOnionsMatch.Value,
                peppersAndOnionsMatch.Index);
        }

        var tomatosMatch = TomatoesRegex().Match(cleanedValue);
        if (tomatosMatch.Success)
        {
            var hasTrailingComma = cleanedValue[(tomatosMatch.Index + tomatosMatch.Length)..]
                .StartsWith(',');
            return new TokenMatch<string>(
                FormatName(tomatosMatch.Value),
                hasTrailingComma
                    ? tomatosMatch.Value + ','
                    : tomatosMatch.Value,
                tomatosMatch.Index);
        }

        var pepperMatch = PepperRegex().Match(cleanedValue);
        if (pepperMatch.Success)
        {
            var hasTrailingComma = cleanedValue[(pepperMatch.Index + pepperMatch.Length)..]
                .StartsWith(',');
            return new TokenMatch<string>(
                FormatName(pepperMatch.Value),
                hasTrailingComma
                    ? pepperMatch.Value + ','
                    : pepperMatch.Value,
                pepperMatch.Index);
        }

        var saltMatch = SaltRegex().Match(cleanedValue);
        if (saltMatch.Success)
        {
            var hasTrailingComma = cleanedValue[(saltMatch.Index + saltMatch.Length)..]
                .StartsWith(',');
            return new TokenMatch<string>(
                FormatName(saltMatch.Value),
                hasTrailingComma
                    ? saltMatch.Value + ','
                    : saltMatch.Value,
                saltMatch.Index);
        }

        var splitIndex = cleanedValue.IndexOfAny([',', '.', ':']);
        var indexOffset = cleanedValue.TakeWhile(c => c == ' ').Count();
        var initialValue = splitIndex > -1
            ? cleanedValue[indexOffset..splitIndex]
            : cleanedValue[indexOffset..];
        var withSplitIndex = initialValue.IndexOf("with ", StringComparison.InvariantCultureIgnoreCase);
        var value = withSplitIndex > -1
            ? initialValue[..withSplitIndex]
            : initialValue;
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new Exception($"No valid name found. '{str}'");
        }

        return new TokenMatch<string>(
            FormatName(value),
            value.Trim(),
            indexOffset);

        /*var hasDefaultValue = DefaultValues.TryGetValue(measurementUnit, out var defaultValue);
        if (hasDefaultValue)
        {
            return new TokenMatch<string>(
                defaultValue!,
                cleanedValue,
                0);
        }

        throw new Exception($"No valid name found. '{str}'");*/
    }

    internal static string FormatName(
        string str) =>
        CleaUpWhitespace(str)
            .Transform(To.SentenceCase);

    internal static TokenMatch<MeasurementUnit> GetMeasurement(
        string str)
    {
        var matchedValue = MeasurementRegex
            .Matches(
                str)
            .FirstOrDefault();
        if (matchedValue != null)
        {
            var singularized = matchedValue
                .Value
                .Trim(',', ' ', '-', '/', '\\')
                .Humanize()
                .Singularize();
            return new TokenMatch<MeasurementUnit>(
                Enum.TryParse(
                    MeasurementSynonyms
                        .Aggregate(
                            singularized,
                            (current, measurementSynonym) =>
                                current
                                    .Replace(
                                        measurementSynonym.Key,
                                        measurementSynonym.Value,
                                        StringComparison.InvariantCultureIgnoreCase))
                        .Replace(
                            " ",
                            string.Empty),
                    true,
                    out MeasurementUnit measurementUnit)
                    ? measurementUnit
                    : throw new Exception($"Could not parse '{matchedValue}' (raw '{str}')"),
                matchedValue.Value,
                matchedValue.Index);
        }

        return new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, str, 0);
    }

    internal static RawQuantity GetQuantity(
        string str)
    {
        var measurement = GetMeasurement(str);
        if (string.IsNullOrWhiteSpace(str))
        {
            return new RawQuantity(
                str,
                new TokenMatch<QuantityType>(QuantityType.Unknown, str, 0),
                null,
                null,
                null,
                measurement);
        }

        var match = QuantityRegex()
            .Matches(str)
            .FirstOrDefault();
        if (match == null)
        {
            var quantityType = NoNumbersRegex().IsMatch(str)
                ? QuantityType.Number
                : QuantityType.Unknown;
            return new RawQuantity(
                str,
                new TokenMatch<QuantityType>(quantityType, str, 0),
                null,
                null,
                new TokenMatch<decimal>(1, str, 0),
                measurement);
        }

        if (match.Groups["Number"].Success)
        {
            var numberValue = match.Groups["Number"].Value;
            var matchStartIndex = match.Groups["Number"].Index;
            return new RawQuantity(
                str,
                new TokenMatch<QuantityType>(QuantityType.Number, numberValue, matchStartIndex),
                null,
                null,
                new TokenMatch<decimal>(ParseFractionSection(numberValue), numberValue, matchStartIndex),
                measurement);
        }

        if (match.Groups["Range"].Success)
        {
            var rangeValue = match.Groups["Range"].Value;
            var matchStartIndex = match.Groups["Range"].Index;
            var r1 = match.Groups["R1"];
            var r2 = match.Groups["R2"];
            var rangeItems = new[]
            {
                (Parsed: ParseFractionSection(r1.Value), Raw: r1),
                (Parsed: ParseFractionSection(r2.Value), Raw: r2)
            }.OrderBy(x => x.Parsed)
            .ToArray();
            return new RawQuantity(
                str,
                new TokenMatch<QuantityType>(QuantityType.Range, rangeValue, matchStartIndex),
                new TokenMatch<decimal>(rangeItems[0].Parsed, rangeItems[0].Raw.Value, rangeItems[0].Raw.Index),
                new TokenMatch<decimal>(rangeItems[1].Parsed, rangeItems[1].Raw.Value, rangeItems[1].Raw.Index),
                null,
                measurement);
        }

        return new RawQuantity(
            str,
            new TokenMatch<QuantityType>(QuantityType.Unknown, str, 0),
            null,
            null,
            null,
            measurement);
    }

    internal static decimal ParseFractionSection(
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

    internal static IEnumerable<RawQuantity> GetRawQuantities(
        string str)
    {
        if (str.StartsWith("Egg wash", StringComparison.InvariantCultureIgnoreCase)
            || str.StartsWith("For serving", StringComparison.InvariantCultureIgnoreCase))
        {
            return new List<RawQuantity>(1)
            {
                new(
                    str,
                    new TokenMatch<QuantityType>(QuantityType.Number, str, 0),
                    null,
                    null,
                    new TokenMatch<decimal>(1M, str, 0),
                    new TokenMatch<MeasurementUnit>(MeasurementUnit.Unit, str, 0))
            };
        }

        var data = ReplaceParentheticalSets(
            str);
        var sections = data
            .Split(
                [
                    " PLUS ",
                    " Plus ",
                    " plus "
                ],
                StringSplitOptions.RemoveEmptyEntries);
        var rawQuantities = sections.Length > 1
            ? sections
                .Select(GetQuantity)
                .ToList()
            : [GetQuantity(data)];

        if (rawQuantities.Count == 2
            && rawQuantities[0].QuantityType.ParsedValue != QuantityType.Unknown
            && rawQuantities[1].QuantityType.MatchedValue!.Contains("more for ", StringComparison.InvariantCultureIgnoreCase)
            && (rawQuantities[1].QuantityType.MatchedValue!.Contains("top", StringComparison.InvariantCultureIgnoreCase)
                || rawQuantities[1].QuantityType.MatchedValue!.Contains(" bottom", StringComparison.InvariantCultureIgnoreCase)
                || rawQuantities[1].QuantityType.MatchedValue!.Contains(" pan", StringComparison.InvariantCultureIgnoreCase)
                || rawQuantities[1].QuantityType.MatchedValue!.Contains(" bowl", StringComparison.InvariantCultureIgnoreCase)
                || rawQuantities[1].QuantityType.MatchedValue!.Contains(" kneading", StringComparison.InvariantCultureIgnoreCase)))
        {
            rawQuantities[0] = rawQuantities[0]
                with
                {
                    RawText = str
                };
            rawQuantities.RemoveAt(1);
        }

        return rawQuantities;
    }

    internal static string GetNotes(
        string str,
        IReadOnlyCollection<MatchLocation> otherMatchedTokens)
    {
        var cleanedValue = ReplaceFoundTokens(
            str,
            otherMatchedTokens);
        return CleaUpWhitespace(cleanedValue)
            .Replace("),", ")")
            .Replace(") ,", ")")
            .Replace(")(", " ")
            .Replace(") (", " ")
            .Replace(" )", ")")
            .Replace("( ", "(");
    }

    private static string ReplaceFoundTokens(
        string str,
        IReadOnlyCollection<MatchLocation> otherMatchedTokens) =>
        otherMatchedTokens
            .Aggregate(
                str,
                (current, token) =>
                    current
                        .Remove(
                            token.MatchStartIndex,
                            token.MatchedValue!.Length)
                        .Insert(
                            token.MatchStartIndex,
                            new string(' ', token.MatchedValue!.Length)));

    private static string CleaUpWhitespace(
        string str)
    {
        return MultiWhitespaceRegex()
                .Replace(
                    str,
                    " ")
                .Trim(' ', ',');
    }

    internal static string ReplaceParentheticalSets(
        string str)
    {
        var indexOfOpen = str.IndexOf(
            '(');
        var indexOfClose = indexOfOpen > -1
            ? str.IndexOf(
                ')',
                indexOfOpen)
            : -1;
        if (indexOfClose > -1)
        {
            var newString = new StringBuilder();
            while (
                indexOfOpen > -1
                && indexOfClose > -1)
            {
                newString.Append(str[newString.Length..indexOfOpen]);
                newString.Append(new string(Enumerable.Repeat(' ', indexOfClose - indexOfOpen + 1).ToArray()));
                indexOfOpen = str.IndexOf(
                    '(',
                    indexOfClose);
                indexOfClose = indexOfOpen > -1
                    ? str.IndexOf(
                        ')',
                        indexOfOpen)
                    : -1;
            }

            newString.Append(str[newString.Length..]);
            return newString.ToString();
        }

        return str;
    }

    internal static IEnumerable<string> GetParentheticalSetsContents(
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
    decimal NumberValue,
    MeasurementUnit MeasurementUnit);

public sealed record RawQuantity(
    string RawText,
    TokenMatch<QuantityType> QuantityType,
    TokenMatch<decimal>? MinValue,
    TokenMatch<decimal>? MaxValue,
    TokenMatch<decimal>? NumberValue,
    TokenMatch<MeasurementUnit> Measurement);

public sealed record TokenMatch<T>(
    T ParsedValue,
    string? MatchedValue,
    int MatchStartIndex)
    : MatchLocation(
        MatchedValue,
        MatchStartIndex);

public abstract record MatchLocation(
    string? MatchedValue,
    int MatchStartIndex);