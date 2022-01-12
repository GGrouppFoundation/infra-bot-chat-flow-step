using System;
using System.Globalization;

namespace GGroupp.Infra.Bot.Builder;

public static partial class GetDateChatFlowExtensions
{
    private static Result<DateOnly, Unit> ParseDateOrFailure(string? text)
        =>
        DateOnly.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : default;
}