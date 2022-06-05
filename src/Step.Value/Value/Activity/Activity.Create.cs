using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace GGroupp.Infra.Bot.Builder;

partial class SuggestionsActivity
{
    internal static IActivity Create(ITurnContext context, string messageText, KeyValuePair<Guid, string>[][] suggestions)
    {
        var encodedText = context.EncodeText(messageText);
        if (suggestions.Sum(row => row.Length) is not > 0)
        {
            return MessageFactory.Text(encodedText);
        }

        if (context.IsCardSupported())
        {
            return context.InnerCreateAdaptiveCardActivity(encodedText, suggestions);
        }

        if (context.IsTelegramChannel())
        {
            var telegramActivity = MessageFactory.Text(encodedText);
            telegramActivity.ChannelData = context.InnerCreateTelegramChannelData(encodedText, suggestions);

            return telegramActivity;
        }

        return MessageFactory.Text(encodedText);
    }

    private static IActivity InnerCreateAdaptiveCardActivity(
        this ITurnContext context, string encodedMessageText, KeyValuePair<Guid, string>[][] suggestions)
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
                        Text = encodedMessageText,
                        Wrap = true
                    }
                }
                .AddActionSets(
                    suggestions.Select(context.CreateAdaptiveActionSet))
            }
        }
        .ToActivity();

    private static JObject InnerCreateTelegramChannelData(
        this ITurnContext turnContext, string encodedMessageText, KeyValuePair<Guid, string>[][] suggestions)
        =>
        new TelegramChannelData(
            parameters: new()
            {
                ReplyMarkup = new TelegramReplyKeyboardMarkup(
                    keyboard: suggestions.Select(turnContext.CreateRow).ToArray())
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true,
                    InputFieldPlaceholder = encodedMessageText
                }
            })
        .ToJObject();

    private static List<AdaptiveElement> AddActionSets(this List<AdaptiveElement> list, IEnumerable<AdaptiveActionSet> sets)
    {
        list.AddRange(sets);
        return list;
    }

    private static AdaptiveActionSet CreateAdaptiveActionSet(
        this ITurnContext turnContext, KeyValuePair<Guid, string>[] suggestions)
        =>
        new()
        {
            Actions = suggestions.Select(turnContext.CreateAdaptiveAction).ToList()
        };

    private static AdaptiveAction CreateAdaptiveAction(
        this ITurnContext turnContext, KeyValuePair<Guid, string> suggestion)
        =>
        new AdaptiveSubmitAction
        {
            Title = suggestion.Value,
            Data = turnContext.BuildCardActionValue(suggestion.Key)
        };

    private static TelegramKeyboardButton[] CreateRow(
        this ITurnContext turnContext, KeyValuePair<Guid, string>[] suggestionsRow)
        =>
        suggestionsRow.Select(turnContext.CreateTelegramKeyboardButton).ToArray();

    private static TelegramKeyboardButton CreateTelegramKeyboardButton(
        this ITurnContext turnContext, KeyValuePair<Guid, string> suggestion)
        =>
        new(
            turnContext.EncodeText(suggestion.Value));

    private static AdaptiveSchemaVersion GetAdaptiveSchemaVersion(this ITurnContext turnContext)
        =>
        turnContext.IsMsteamsChannel() ? AdaptiveCard.KnownSchemaVersion : new(1, 0);
}