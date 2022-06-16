using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("GGroupp.Infra.Bot.Builder.ChatFlow.Step.Date.Tests")]

namespace GGroupp.Infra.Bot.Builder;

public static partial class AwaitDateChatFlowExtensions
{
    private static Result<DateOnly, BotFlowFailure> ParseDateOrFailure(string text, DateStepOption option)
    {
        return option.Suggestions.SelectMany(PipeSelf).GetValueOrAbsent(text).Fold(Result.Present, ParseOrFailure).MapFailure(CreateFlowFailure);

        static T PipeSelf<T>(T value)
            =>
            value;

        Result<DateOnly, Unit> ParseOrFailure()
            =>
            DateParser.ParseOrFailure(text);

        BotFlowFailure CreateFlowFailure(Unit _)
            =>
            string.IsNullOrEmpty(option.InvalidDateText) ? default : BotFlowFailure.From(option.InvalidDateText);
    }

    private static string ToText(this DateOnly date, string format)
        =>
        date.ToString(format, CultureInfo.InvariantCulture);
}