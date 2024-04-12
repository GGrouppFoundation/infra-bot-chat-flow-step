using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

partial class CardActivity
{
    internal static IActivity CreateConfirmationActivity(
        this ITurnContext context, ConfirmationCardOption option, ConfirmationCardCacheJson cache, bool useButtons = true)
    {
        if (context.IsCardSupported())
        {
            return context.CreateAdaptiveCardActivity(option, cache, useButtons);
        }

        if (context.IsTelegramChannel())
        {
            var telegramActivity = MessageFactory.Text(default);
            telegramActivity.ChannelData = CreateTelegramChannelData(option, useButtons).ToJObject();

            return telegramActivity;
        }

        return context.CreateHeroCardConfirmationActivity(option, cache, useButtons);
    }

    private static IActivity CreateAdaptiveCardActivity(
        this ITurnContext context, ConfirmationCardOption option, ConfirmationCardCacheJson cache, bool useButtons)
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
                        Facts = option.FieldValues.AsEnumerable().Where(NotEmptyField).Where(NotEmptyFieldName).Select(CreateFact).ToList()
                    }
                }
                .AddElements(
                    option.FieldValues.AsEnumerable().Where(NotEmptyField).Where(EmptyFieldName).Select(CreateTextBlock)),
                Actions = useButtons ? context.CreateAdaptiveCardActivityActions(option, cache) : null
            }
        }
        .ToActivity();

    private static List<AdaptiveAction> CreateAdaptiveCardActivityActions(
        this ITurnContext context, ConfirmationCardOption option, ConfirmationCardCacheJson cache)
        =>
        [
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
        ];

    private static TelegramChannelData CreateTelegramChannelData(ConfirmationCardOption option, bool useButtons)
    {
        return new(
            parameters: new(option.BuildTelegramText())
            {
                ParseMode = TelegramParseMode.Html,
                ReplyMarkup = useButtons is false ? null : new TelegramReplyKeyboardMarkup(
                    keyboard: InnerGetButtons(option).ToArray())
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true,
                    InputFieldPlaceholder = option.QuestionText
                }
            });

        static IEnumerable<TelegramKeyboardButton[]> InnerGetButtons(ConfirmationCardOption option)
        {
            yield return
            [
                new(option.CancelButtonText),
                new(option.ConfirmButtonText)
            ];

            if (string.IsNullOrWhiteSpace(option.TelegramWebApp?.WebAppUrl))
            {
                yield break;
            }

            yield return
            [
                new(option.TelegramWebApp.ButtonName)
                {
                    WebApp = new(option.TelegramWebApp.WebAppUrl)
                }
            ];
        }
    }

    private static IActivity CreateHeroCardConfirmationActivity(
        this ITurnContext context, ConfirmationCardOption option, ConfirmationCardCacheJson cache, bool useButtons)
        =>
        new HeroCard
        {
            Title = option.QuestionText,
            Buttons = useButtons ? context.CreateHeroCardConfirmationButtons(option, cache) : null
        }
        .ToCardActivity(
            new StringBuilder().AppendFields(option.FieldValues.AsEnumerable(), false).ToString());

    private static IList<CardAction> CreateHeroCardConfirmationButtons(
        this ITurnContext context, ConfirmationCardOption option, ConfirmationCardCacheJson cache)
        =>
        [
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
        ];

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