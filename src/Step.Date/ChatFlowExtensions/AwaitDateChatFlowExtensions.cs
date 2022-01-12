using System;
using System.Globalization;

namespace GGroupp.Infra.Bot.Builder;

public static partial class AwaitDateChatFlowExtensions
{
    private static Result<DateOnly, Unit> ParseDateOrFailure(string? text, string format)
        =>
        DateOnly.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
        ? Result.Present(date)
        : default;

    private static string ToText(this DateOnly date, string format)
        =>
        date.ToString(format, CultureInfo.InvariantCulture);

    private static ChatFlowJump<DateOnly> NextDateJump(DateOnly date)
        =>
        ChatFlowJump.Next(date);
}