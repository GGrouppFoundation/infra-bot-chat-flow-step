using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Schema;

[assembly: InternalsVisibleTo("GarageGroup.Infra.Bot.Builder.ChatFlow.Step.Date.Test")]

namespace GarageGroup.Infra.Bot.Builder;

public static partial class AwaitDateChatFlowExtensions
{
    private const string JsonDateFormat = AdaptiveCardDateFormat;

    private static Result<DateOnly, BotFlowFailure> ParseDateOrFailure(string text, DateCacheJson cache)
    {
        return cache.GetFromSuggestionsOrAbsent(text).Fold(Result.Present, ParseOrFailure).MapFailure(CreateFlowFailure);

        Result<DateOnly, Unit> ParseOrFailure()
            =>
            DateParser.ParseOrFailure(text);

        BotFlowFailure CreateFlowFailure(Unit _)
            =>
            string.IsNullOrEmpty(cache.InvalidDateText) ? default : BotFlowFailure.From(cache.InvalidDateText);
    }

    private static Optional<DateOnly> GetFromSuggestionsOrAbsent(this DateCacheJson cache, string text)
    {
        return cache.Suggestions?.GetValueOrAbsent(text).Map(ParseJsonDate) ?? default;

        static DateOnly ParseJsonDate(string value)
            =>
            DateOnly.ParseExact(value, JsonDateFormat);
    }

    private static DateCacheJson BuildCacheValue(DateStepOption option, ResourceResponse? resource)
    {
        return new DateCacheJson()
        {
            Resource = resource,
            InvalidDateText = option.InvalidDateText,
            Suggestions = option.Suggestions.SelectMany(PipeSelf).Select(ToStringJson).ToArray().OrNullIfEmpty()
        };

        static KeyValuePair<string, string> ToStringJson(KeyValuePair<string, DateOnly> pair)
            =>
            new(pair.Key, pair.Value.ToText(JsonDateFormat));
    }

    private static T PipeSelf<T>(T value)
        =>
        value;

    private static T[]? OrNullIfEmpty<T>(this T[]? source)
        =>
        source?.Length is not > 0 ? null : source;

    private static string ToText(this DateOnly date, string format)
        =>
        date.ToString(format, CultureInfo.InvariantCulture);
}