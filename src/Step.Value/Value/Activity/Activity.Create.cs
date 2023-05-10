using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace GarageGroup.Infra.Bot.Builder;

partial class SuggestionsActivity
{
    internal static IActivity Create(ITurnContext context, string messageText, KeyValuePair<Guid, string>[][] suggestions)
    {
        if (context.IsTelegramChannel())
        {
            var telegramActivity = MessageFactory.Text(default);
            telegramActivity.ChannelData = InnerCreateTelegramChannelData(messageText, suggestions);

            return telegramActivity;
        }

        if (context.IsNotMsteamsChannel() || suggestions.SelectMany(Pipeline.Pipe).Any() is false)
        {
            return MessageFactory.Text(messageText);
        }

        return context.InnerCreateAdaptiveCardActivity(messageText, suggestions);
    }

    private static IActivity InnerCreateAdaptiveCardActivity(
        this ITurnContext context, string messageText, KeyValuePair<Guid, string>[][] suggestions)
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
                        Text = messageText,
                        Wrap = true
                    }
                }
                .AddActionSets(
                    suggestions.Select(context.CreateAdaptiveActionSet))
            }
        }
        .ToActivity();

    private static JObject InnerCreateTelegramChannelData(string messageText, KeyValuePair<Guid, string>[][] suggestions)
        =>
        new TelegramChannelData(
            parameters: new(HttpUtility.HtmlEncode(messageText))
            {
                ParseMode = TelegramParseMode.Html,
                ReplyMarkup = new TelegramReplyKeyboardMarkup(
                    keyboard: suggestions.Select(CreateRow).ToArray())
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true,
                    InputFieldPlaceholder = messageText
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

    private static AdaptiveAction CreateAdaptiveAction(this ITurnContext turnContext, KeyValuePair<Guid, string> suggestion)
        =>
        new AdaptiveSubmitAction
        {
            Title = suggestion.Value,
            Data = turnContext.BuildCardActionValue(suggestion.Key)
        };

    private static TelegramKeyboardButton[] CreateRow(KeyValuePair<Guid, string>[] suggestionsRow)
        =>
        suggestionsRow.Select(CreateTelegramKeyboardButton).ToArray();

    private static TelegramKeyboardButton CreateTelegramKeyboardButton(KeyValuePair<Guid, string> suggestion)
        =>
        new(suggestion.Value);

    private static AdaptiveSchemaVersion GetAdaptiveSchemaVersion(this ITurnContext turnContext)
        =>
        turnContext.IsMsteamsChannel() ? AdaptiveCard.KnownSchemaVersion : new(1, 0);
}