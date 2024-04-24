using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

partial class SuggestionsActivity
{
    internal static IActivity Create(ITurnContext context, string messageText, KeyValuePair<Guid, string>[][] suggestions)
    {
        if (context.IsTelegramChannel())
        {
            return InnerCreateTelegramParameters(messageText, suggestions).BuildActivity();
        }

        if (context.IsNotMsteamsChannel() || suggestions.SelectMany(Pipeline.Pipe).Any() is false)
        {
            return MessageFactory.Text(messageText);
        }

        return context.InnerCreateAdaptiveCard(messageText, suggestions).ToActivity();
    }

    private static Attachment InnerCreateAdaptiveCard(
        this ITurnContext context, string messageText, KeyValuePair<Guid, string>[][] suggestions)
    {
        var body = new List<AdaptiveElement>()
        {
            new AdaptiveTextBlock
            {
                Text = messageText,
                Wrap = true
            }
        };

        body.AddRange(suggestions.Select(CreateAdaptiveActionSet));

        return new()
        {
            ContentType = AdaptiveCard.ContentType,
            Content = new AdaptiveCard(context.IsMsteamsChannel() ? AdaptiveCard.KnownSchemaVersion : new(1, 0))
            {
                Body = body
            }
        };

        AdaptiveActionSet CreateAdaptiveActionSet(KeyValuePair<Guid, string>[] suggestions)
            =>
            new()
            {
                Actions = suggestions.Select(CreateAdaptiveAction).ToList()
            };

        AdaptiveAction CreateAdaptiveAction(KeyValuePair<Guid, string> suggestion)
            =>
            new AdaptiveSubmitAction
            {
                Title = suggestion.Value,
                Data = context.BuildCardActionValue(suggestion.Key)
            };
    }

    private static TelegramParameters InnerCreateTelegramParameters(string messageText, KeyValuePair<Guid, string>[][] suggestions)
    {
        return new(messageText)
        {
            ParseMode = TelegramParseMode.Html,
            ReplyMarkup = new TelegramReplyKeyboardMarkup(
                keyboard: suggestions.Select(CreateRow).ToArray())
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            }
        };

        static TelegramKeyboardButton[] CreateRow(KeyValuePair<Guid, string>[] suggestionsRow)
            =>
            suggestionsRow.Select(CreateTelegramKeyboardButton).ToArray();

        static TelegramKeyboardButton CreateTelegramKeyboardButton(KeyValuePair<Guid, string> suggestion)
            =>
            new(suggestion.Value);
    }
}