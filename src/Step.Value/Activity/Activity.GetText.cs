using System;
using System.Globalization;

namespace GGroupp.Infra.Bot.Builder;

partial class SkipActivity
{
    internal static Result<string?, BotFlowFailure> GetTextOrFailure(this IChatFlowStepContext context, SkipValueStepOption option)
    {
        if (context.IsNotMessageType())
        {
            return default;
        }

        var activityText = context.Activity.Text;

        if (context.IsTelegramChannel())
        {
            if (string.Equals(activityText, option.SkipButtonText, StringComparison.InvariantCulture))
            {
                return null;
            }

            if (context.GetCardActionValueOrAbsent().IsPresent)
            {
                return default;
            }
        }
        else
        {
            var cardActionResult = context.GetCardActionValueOrAbsent();
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

        if (string.IsNullOrEmpty(activityText))
        {
            return default;
        }

        return activityText;
    }
}