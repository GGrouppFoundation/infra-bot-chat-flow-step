using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GarageGroup.Infra.Bot.Builder;

partial class AwaitDateChatFlowExtensions
{
    private static Result<DateOnly, BotFlowFailure> ParseDateFromText(ITurnContext context, DateCacheJson cache)
        =>
        context.MayBeTextMessageActivity() ? ParseDateOrFailure(context.Activity.Text, cache) : default;

    private static bool MayBeTextMessageActivity(this ITurnContext context)
        =>
        context.IsMessageType() &&
        string.IsNullOrEmpty(context.Activity.Text) is false &&
        context.GetCardActionValueOrAbsent().IsAbsent;

    private static Activity CreateMessageActivity(ITurnContext context, DateStepOption option)
    {
        if (option.Suggestions.SelectMany(PipeSelf).Any() is false || context.IsNotTelegramChannel())
        {
            return MessageFactory.Text(option.Text);
        }

        var telegramParameters = new TelegramParameters(option.Text)
        {
            ReplyMarkup = new TelegramReplyKeyboardMarkup(
                keyboard: option.Suggestions.Select(ToTelegramKeyboardButtonRow).ToArray())
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            }
        };

        return telegramParameters.BuildActivity();

        static TelegramKeyboardButton[] ToTelegramKeyboardButtonRow(IReadOnlyCollection<KeyValuePair<string, DateOnly>> row)
            =>
            row.Select(ToTelegramKeyboardButton).ToArray();

        static TelegramKeyboardButton ToTelegramKeyboardButton(KeyValuePair<string, DateOnly> suggesion)
            =>
            new(suggesion.Key);
    }
}