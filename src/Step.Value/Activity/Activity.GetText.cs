using System;
using System.Globalization;

namespace GGroupp.Infra.Bot.Builder;

partial class SkipActivity
{
    internal static Result<string?, ChatFlowStepFailure> GetTextOrFailure(this IChatFlowStepContext context)
    {
        var activity = context.Activity;
        if (activity.IsMessageType() is false)
        {
            return default;
        }

        var cardActionResult = activity.GetCardActionValueOrAbsent();
        if (cardActionResult.IsPresent)
        {
            var cardId = cardActionResult.OrThrow();
            if (context.StepState is Guid cachedId && cardId == cachedId)
            {
                return null;
            }

            var cardIdString = cardId.ToString("D", CultureInfo.InvariantCulture);
            if (string.Equals(cardIdString, context.StepState?.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
        }

        if (string.IsNullOrEmpty(activity.Text))
        {
            return default;
        }

        return activity.Text;
    }
}