using System;
using System.Globalization;

namespace GGroupp.Infra.Bot.Builder;

partial class SkipActivity
{
    internal static Result<string?, BotFlowFailure> GetTextOrFailure(this IChatFlowStepContext context, SkipActivityOption option)
    {
        var activity = context.Activity;
        if (activity.IsNotMessageType())
        {
            return default;
        }

        if (activity.IsTelegram())
        {
            if (string.Equals(activity.Text, option.SkipButtonText, StringComparison.InvariantCulture))
            {
                return null;
            }
            if (activity.GetCardActionValueOrAbsent().IsPresent)
            {
                return default;
            }
        }
        else
        {
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

                return default;
            }
        }

        if (string.IsNullOrEmpty(activity.Text))
        {
            return default;
        }

        return activity.Text;
    }
}