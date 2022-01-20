using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GGroupp.Infra.Bot.Builder;

public static partial class AwaitDateChatFlowExtensions
{
    private static Result<DateOnly, BotFlowFailure> ParseDateOrFailure(string? text, string format, [AllowNull] string invalidDateText)
        =>
        DateOnly.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
        ? date
        : BotFlowFailure.From(invalidDateText.OrEmpty());

    private static string ToText(this DateOnly date, string format)
        =>
        date.ToString(format, CultureInfo.InvariantCulture);

    private static ChatFlowJump<DateOnly> NextDateJump(DateOnly date)
        =>
        ChatFlowJump.Next(date);
}