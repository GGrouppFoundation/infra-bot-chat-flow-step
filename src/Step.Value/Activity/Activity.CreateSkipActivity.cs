using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace GGroupp.Infra.Bot.Builder;

partial class SkipActivity
{
    internal static IActivity CreateSkipActivity(this ITurnContext context, SkipActivityOption option, Guid skipButtonId)
        =>
        context.Activity.ChannelId is Channels.Telegram
        ? context.Activity.InnerCreateTelegramSkipActivity(option, skipButtonId)
        : context.Activity.InnerCreateHeroCardSkipActivity(option, skipButtonId);

    private static IActivity InnerCreateHeroCardSkipActivity(this Activity activity, SkipActivityOption option, Guid skipButtonId)
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
                    Value = activity.BuildCardActionValue(skipButtonId)
                }
            }
        }
        .ToAttachment()
        .ToActivity(
            inputHint: InputHints.AcceptingInput);

    private static IActivity InnerCreateTelegramSkipActivity(this Activity activity, SkipActivityOption option, Guid skipButtonId)
    {
        var skipActivity = MessageFactory.Text(option.MessageText.ToEncodedActivityText());

        var channelData = new TelegramChannelData(
            method: "sendMessage",
            message: new(
                replyMarkup: new(
                    inlineKeyboard: new[]
                    {
                        new[]
                        {
                            new TelegramInlineKeyboardButton(
                                text: option.SkipButtonText,
                                callbackData: activity.BuildCardActionValue(skipButtonId))
                        }
                    })));

        skipActivity.ChannelData = JObject.FromObject(channelData);
        return skipActivity;
    }
}