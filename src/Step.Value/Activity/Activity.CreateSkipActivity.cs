using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

partial class SkipActivity
{
    internal static IActivity CreateSkipActivity(this ITurnContext context, SkipActivityOption option, Guid skipButtonId)
        =>
        new HeroCard
        {
            Title = option.MessageText.ToEncodedActivityText(),
            Buttons = new CardAction[]
            {
                new(ActionTypes.PostBack)
                {
                    Title = option.SkipButtonText,
                    Text = option.SkipButtonText,
                    Value = context.Activity.BuildCardActionValue(skipButtonId)
                }
            }
        }
        .ToAttachment()
        .ToActivity();
}