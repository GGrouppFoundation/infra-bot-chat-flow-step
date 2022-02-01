using System;
using Microsoft.Bot.Builder;

namespace GGroupp.Infra.Bot.Builder;

partial class SkipActivity
{
    internal static Result<string, BotFlowFailure> GetRequiredTextOrFailure(this ITurnContext context)
    {
        if (context.IsMessageType() is false)
        {
            return default;
        }

        var cardActionResult = context.GetCardActionValueOrAbsent();
        if (cardActionResult.IsPresent)
        {
            return default;
        }

        return context.Activity.Text ?? string.Empty;
    }
}