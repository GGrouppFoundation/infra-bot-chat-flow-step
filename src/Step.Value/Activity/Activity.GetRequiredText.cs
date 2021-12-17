using System;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

partial class SkipActivity
{
    internal static Result<string, ChatFlowStepFailure> GetRequiredTextOrFailure(this Activity activity)
    {
        if (activity.IsMessageType() is false)
        {
            return default;
        }

        var cardActionResult = activity.GetCardActionValueOrAbsent();
        if (cardActionResult.IsPresent)
        {
            return default;
        }

        return activity.Text.OrEmpty();
    }
}