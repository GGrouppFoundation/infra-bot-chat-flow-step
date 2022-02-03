using System;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace GGroupp.Infra.Bot.Builder;

partial class SkipActivity
{
    internal static IActivity CreateSkipActivity(this ITurnContext context, SkipValueStepOption option, Guid skipButtonId)
    {
        if (context.IsCardSupported())
        {
            return context.InnerCreateAdaptiveCardSkipActivity(option, skipButtonId);
        }

        if (context.IsTelegramChannel())
        {
            var telegramActivity = MessageFactory.Text(context.EncodeText(option.MessageText));
            telegramActivity.ChannelData = InnerCreateTelegramChannelData(option);

            return telegramActivity;
        }

        return context.InnerCreateHeroCardSkipActivity(option, skipButtonId);
    }

    private static IActivity InnerCreateAdaptiveCardSkipActivity(this ITurnContext context, SkipValueStepOption option, Guid skipButtonId)
        =>
        new Attachment
        {
            ContentType = AdaptiveCard.ContentType,
            Content = new AdaptiveCard(context.GetAdaptiveSchemaVersion())
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
                        Data = context.BuildCardActionValue(skipButtonId)
                    }
                }
            }
        }
        .ToActivity();

    private static AdaptiveSchemaVersion GetAdaptiveSchemaVersion(this ITurnContext turnContext)
        =>
        turnContext.IsMsteamsChannel() ? AdaptiveCard.KnownSchemaVersion : new(1, 0);

    private static IActivity InnerCreateHeroCardSkipActivity(this ITurnContext context, SkipValueStepOption option, Guid skipButtonId)
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
                    Value = context.BuildCardActionValue(skipButtonId)
                }
            }
        }
        .ToAttachment()
        .ToActivity(
            inputHint: InputHints.AcceptingInput);

    private static JObject InnerCreateTelegramChannelData(SkipValueStepOption option)
        =>
        new TelegramChannelData(
            parameters: new()
            {
                ReplyMarkup = new TelegramReplyKeyboardMarkup(
                    keyboard: new[]
                    {
                        new TelegramKeyboardButton[]
                        {
                            new(option.SkipButtonText)
                        }
                    })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true,
                    InputFieldPlaceholder = option.MessageText
                }
            })
        .ToJObject();
}