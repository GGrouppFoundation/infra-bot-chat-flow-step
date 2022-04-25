using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace GGroupp.Infra.Bot.Builder;

partial class CardActivity
{
    internal static IActivity CreateConfirmationActivity(
        this ITurnContext context, ConfirmationCardOption option, ConfirmationCardCacheJson cache)
    {
        if (context.IsCardSupported())
        {
            return context.CreateAdaptiveCardActivity(option, cache);
        }

        if (context.IsTelegramChannel())
        {
            var telegramActivity = MessageFactory.Text(context.BuildTelegramText(option));
            telegramActivity.ChannelData = context.CreateTelegramChannelData(option, cache);
            return telegramActivity;
        }

        return context.CreateHeroCardConfirmationActivity(option, cache);
    }

    private static IActivity CreateAdaptiveCardActivity(
        this ITurnContext context, ConfirmationCardOption option, ConfirmationCardCacheJson cache)
        =>
        new Attachment
        {
            ContentType = AdaptiveCard.ContentType,
            Content = new AdaptiveCard(context.GetAdaptiveSchemaVersion())
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveTextBlock
                    {
                        Text = option.QuestionText,
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Medium
                    },
                    new AdaptiveFactSet
                    {
                        Facts = option.FieldValues.Where(NotEmptyField).Where(NotEmptyFieldName).Select(CreateFact).ToList()
                    }
                }
                .AddElements(
                    option.FieldValues.Where(NotEmptyField).Where(EmptyFieldName).Select(CreateTextBlock)),
                Actions = new List<AdaptiveAction>
                {

                    new AdaptiveSubmitAction
                    {
                        Title = option.ConfirmButtonText,
                        Data = context.BuildCardActionValue(cache.ConfirmButtonGuid)
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = option.CancelButtonText,
                        Data = context.BuildCardActionValue(cache.CancelButtonGuid)
                    }
                }
            }
        }
        .ToActivity();

    private static JObject CreateTelegramChannelData(
        this ITurnContext context, ConfirmationCardOption option, ConfirmationCardCacheJson cache)
        =>
        new TelegramChannelData(
            parameters: new()
            {
                ReplyMarkup = new TelegramReplyKeyboardMarkup(
                    keyboard: new[]
                    {
                        new TelegramKeyboardButton[]
                        {
                            new(context.EncodeText(option.CancelButtonText)),
                            new(context.EncodeText(option.ConfirmButtonText))
                        }
                    })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true,
                    InputFieldPlaceholder = option.QuestionText
                }
            })
        .ToJObject();

    private static IActivity CreateHeroCardConfirmationActivity(
        this ITurnContext context, ConfirmationCardOption option, ConfirmationCardCacheJson cache)
        =>
        new HeroCard
        {
            Title = option.QuestionText,
            Buttons = new CardAction[]
            {
                new(ActionTypes.PostBack)
                {
                    Title = option.ConfirmButtonText,
                    Text = option.ConfirmButtonText,
                    Value = context.BuildCardActionValue(cache.ConfirmButtonGuid)
                },
                new(ActionTypes.PostBack)
                {
                    Title = option.CancelButtonText,
                    Text = option.CancelButtonText,
                    Value = context.BuildCardActionValue(cache.CancelButtonGuid)
                }
            }
        }
        .ToCardActivity(
            context.BuildFieldsText(option.FieldValues));

    private static IActivity ToCardActivity(this HeroCard card, string? fieldsText)
        =>
        MessageFactory.Attachment(card.ToAttachment(), fieldsText);

    private static AdaptiveFact CreateFact(KeyValuePair<string, string?> field)
        =>
        new()
        {
            Title = field.Key,
            Value = field.Value
        };

    private static AdaptiveTextBlock CreateTextBlock(KeyValuePair<string, string?> field)
        =>
        new()
        {
            Text = field.Value,
            Wrap = true
        };

    private static AdaptiveSchemaVersion GetAdaptiveSchemaVersion(this ITurnContext turnContext)
        =>
        turnContext.IsMsteamsChannel() ? AdaptiveCard.KnownSchemaVersion : new(1, 0);
}