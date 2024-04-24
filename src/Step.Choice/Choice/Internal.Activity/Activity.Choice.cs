using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

partial class ChoiceActivity
{
    internal static IActivity[] CreateChoiceActivitiesAsync<T>(this IChatFlowStepContext<T> context, ChoiceSetOption option)
    {
        if (context.IsTelegramChannel())
        {
            return context.CreateTelegramParameters(option).Select(TelegramTurnContextExtensions.BuildActivity).ToArray();
        }

        if (context.IsCardSupported())
        {
            return [context.CreateAdaptiveCard(option).ToActivity()];
        }

        return context.CreateHeroCards(option).Select(InnerCreateHeroCardActivity).ToArray();

        static IActivity InnerCreateHeroCardActivity(HeroCard card)
            =>
            card.ToAttachment().ToActivity();
    }

    private static Attachment CreateAdaptiveCard(this ITurnContext context, ChoiceSetOption option)
    {
        return new()
        {
            ContentType = AdaptiveCard.ContentType,
            Content = new AdaptiveCard(context.GetAdaptiveSchemaVersion())
            {
                Body =
                [
                    new AdaptiveTextBlock
                    {
                        Text = option.ChoiceText,
                        Weight = AdaptiveTextWeight.Bolder,
                        Wrap = true
                    },
                    new AdaptiveActionSet
                    {
                        Actions = option.Items.AsEnumerable().Select(CreateAdaptiveSubmitAction).ToList<AdaptiveAction>()
                    }
                ],
                Actions = string.IsNullOrEmpty(option.NextButton?.NextToken) ? null : [CreateNextButtonAction(option.NextButton)]
            }
        };

        AdaptiveSubmitAction CreateAdaptiveSubmitAction(ChoiceItem item)
            =>
            new()
            {
                Title = item.Title,
                Data = context.BuildCardActionValue(item.Id)
            };

        static AdaptiveSubmitAction CreateNextButtonAction(ChoiceNextButton nextButton)
            =>
            new()
            {
                Title = nextButton.Title,
                Data = nextButton.Title
            };
    }

    private static IEnumerable<HeroCard> CreateHeroCards(this ITurnContext context, ChoiceSetOption option)
    {
        yield return new()
        {
            Title = option.ChoiceText,
            Buttons = option.Items.AsEnumerable().Select(CreateChoiceItemAction).ToArray()
        };

        if (string.IsNullOrEmpty(option.NextButton?.NextToken))
        {
            yield break;
        }

        yield return new()
        {
            Buttons = [CreateNextButtonAction(option.NextButton.Title)]
        };

        CardAction CreateChoiceItemAction(ChoiceItem item)
            =>
            new(ActionTypes.PostBack)
            {
                Title = item.Title,
                Text = item.Title,
                Value = context.BuildCardActionValue(item.Id)
            };

        static CardAction CreateNextButtonAction(string title)
            =>
            new(ActionTypes.PostBack)
            {
                Title = title,
                Text = title,
                Value = title
            };
    }

    private static IEnumerable<TelegramParameters> CreateTelegramParameters<T>(this IChatFlowStepContext<T> context, ChoiceSetOption option)
    {
        var encodedText = HttpUtility.HtmlEncode(option.ChoiceText);
        var buttons = option.Items.AsEnumerable().Select(CreateTelegramButton);

        yield return new(encodedText)
        {
            ParseMode = TelegramParseMode.Html,
            ReplyMarkup = new TelegramInlineKeyboardMarkup(
                keyboard: buttons.Select(CreateRow).ToArray())
        };

        var stepState = context.StepState as ChoiceStepStateJson;

        var hasNextButton = string.IsNullOrEmpty(option.NextButton?.NextToken) is false;
        var nowNextButton = string.IsNullOrEmpty(stepState?.NextButton?.NextToken) is false;

        if (hasNextButton == nowNextButton)
        {
            yield break;
        }

        yield return new(default)
        {
            ReplyMarkup = hasNextButton ? CreateKeyboardMarkup(option.NextButton?.Title) : new TelegramReplyKeyboardRemove()
        };

        static TelegramReplyKeyboardMarkup CreateKeyboardMarkup(string? nextButtonTitle)
            =>
            new(
                keyboard:
                [
                    [new(HttpUtility.HtmlEncode(nextButtonTitle).OrEmpty())]
                ])
            {
                ResizeKeyboard = true
            };

        TelegramInlineKeyboardButton CreateTelegramButton(ChoiceItem item)
            =>
            new(item.Title)
            {
                CallbackData = context.BuildCardActionValue(item.Id)?.ToString()
            };

        static TelegramInlineKeyboardButton[] CreateRow(TelegramInlineKeyboardButton button)
            =>
            [button];
    }

    private static AdaptiveSchemaVersion GetAdaptiveSchemaVersion(this ITurnContext turnContext)
        =>
        turnContext.IsMsteamsChannel() ? AdaptiveCard.KnownSchemaVersion : new(1, 0);
}