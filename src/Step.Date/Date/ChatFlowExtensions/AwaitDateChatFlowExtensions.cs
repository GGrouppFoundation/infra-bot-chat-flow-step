using System;
using System.Globalization;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("GGroupp.Infra.Bot.Builder.ChatFlow.Step.Date.Tests")]

namespace GGroupp.Infra.Bot.Builder;

public static partial class AwaitDateChatFlowExtensions
{
    private static Result<DateOnly, BotFlowFailure> ParseDateOrFailure(string text, string? invalidTextMessage)
        =>
        DateParser.ParseOrFailure(text).MapFailure(
            _ => string.IsNullOrEmpty(invalidTextMessage) ? default : BotFlowFailure.From(invalidTextMessage));

    private static string ToText(this DateOnly date, string format)
        =>
        date.ToString(format, CultureInfo.InvariantCulture);
}