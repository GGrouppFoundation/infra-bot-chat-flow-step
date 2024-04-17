using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

partial class CardActivity
{
    internal static IActivity CreateCardActivity(this ITurnContext context, EntityCardOption option, CardButtonsOption? buttons = null)
    {
        if (context.IsCardSupported())
        {
            return context.CreateAdaptiveCardActivity(option, buttons);
        }

        if (context.IsTelegramChannel())
        {
            return CreateTelegramChannelData(option, buttons).CreateActivity();
        }

        return CreateHeroCardConfirmationActivity(option, buttons);
    }

    private static IActivity CreateAdaptiveCardActivity(this ITurnContext context, EntityCardOption option, CardButtonsOption? buttons)
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
                        Text = option.HeaderText,
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
                Actions = buttons is null ? null : CreateAdaptiveCardActivityActions(buttons)
            }
        }
        .ToActivity();

    private static List<AdaptiveAction> CreateAdaptiveCardActivityActions(CardButtonsOption option)
        =>
        [
            new AdaptiveSubmitAction
            {
                Title = option.ConfirmButtonText,
                Data = option.ConfirmButtonText
            },
            new AdaptiveSubmitAction
            {
                Title = option.CancelButtonText,
                Data = option.CancelButtonText
            }
        ];

    private static TelegramChannelData CreateTelegramChannelData(EntityCardOption option, CardButtonsOption? buttons)
    {
        return new(
            parameters: new(option.BuildTelegramText())
            {
                ParseMode = TelegramParseMode.Html,
                ReplyMarkup = buttons is null ? null : new TelegramReplyKeyboardMarkup(
                    keyboard: InnerGetButtons(buttons).ToArray())
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true,
                    InputFieldPlaceholder = option.HeaderText
                }
            });

        static IEnumerable<TelegramKeyboardButton[]> InnerGetButtons(CardButtonsOption option)
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

    private static IActivity CreateHeroCardConfirmationActivity(EntityCardOption option, CardButtonsOption? buttons)
        =>
        new HeroCard
        {
            Title = option.HeaderText,
            Buttons = buttons is null ? null : CreateHeroCardConfirmationButtons(buttons)
        }
        .ToCardActivity(
            new StringBuilder().AppendFields(option.FieldValues.AsEnumerable(), false).ToString());

    private static IList<CardAction> CreateHeroCardConfirmationButtons(CardButtonsOption option)
        =>
        [
            new(ActionTypes.PostBack)
            {
                Title = option.ConfirmButtonText,
                Text = option.ConfirmButtonText,
                Value = option.ConfirmButtonText
            },
            new(ActionTypes.PostBack)
            {
                Title = option.CancelButtonText,
                Text = option.CancelButtonText,
                Value = option.CancelButtonText
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