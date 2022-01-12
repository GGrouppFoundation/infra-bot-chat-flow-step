using System;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

partial class SkipActivity
{
    internal static IActivity CreateSkipActivity(this ITurnContext context, SkipActivityOption option, Guid skipButtonId)
    {
        var activity = context.Activity;
        if (activity.IsCardSupported())
        {
            return activity.InnerCreateAdaptiveCardSkipActivity(option, skipButtonId);
        }

        if (activity.IsTelegram())
        {
            return InnerCreateTelegramSkipActivity(option);
        }

        return activity.InnerCreateHeroCardSkipActivity(option, skipButtonId);
    }

    private static IActivity InnerCreateAdaptiveCardSkipActivity(this Activity activity, SkipActivityOption option, Guid skipButtonId)
        =>
        new Attachment
        {
            ContentType = AdaptiveCard.ContentType,
            Content = new AdaptiveCard(activity.ChannelId.GetAdaptiveSchemaVersion())
            {
                Body = new()
                {
                    new AdaptiveTextBlock
                    {
                        Text = option.MessageText,
                        Wrap = true
                    }
                },
                Actions = new()
                {
                    new AdaptiveSubmitAction()
                    {
                        Title = option.SkipButtonText,
                        Data = activity.BuildCardActionValue(skipButtonId)
                    }
                }
            }
        }
        .ToActivity();

    private static AdaptiveSchemaVersion GetAdaptiveSchemaVersion(this string channelId)
        =>
        channelId.Equals(Channels.Msteams, StringComparison.InvariantCultureIgnoreCase) ? AdaptiveCard.KnownSchemaVersion : new(1, 0);

    private static IActivity InnerCreateHeroCardSkipActivity(this Activity activity, SkipActivityOption option, Guid skipButtonId)
        =>
        new HeroCard
        {
            Title = option.MessageText,
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

    private static IActivity InnerCreateTelegramSkipActivity(SkipActivityOption option)
        =>
        new TelegramChannelData
        {
            Method = TelegramMethod.SendMessage,
            Parameters = new()
            {
                ReplyMarkup = new TelegramReplyKeyboardMarkup
                {
                    Keyboard = new[]
                    {
                        new TelegramKeyboardButton[]
                        {
                            new()
                            {
                                Text = option.SkipButtonText
                            }
                        }
                    },
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true,
                    InputFieldPlaceholder = option.MessageText
                }
            }
        }
        .ToActivity(
            text: option.MessageText);
}