using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GGroupp.Infra.Bot.Builder;

partial class AwaitDateChatFlowExtensions
{
    private const string TelegramButtonDateFormat = "dd.MM";

    private static Result<DateOnly, BotFlowFailure> ParseDateFromText(ITurnContext context, DateStepOption option)
        =>
        context.MayBeTextMessageActivity() ? ParseDateOrFailure(context.Activity.Text, option) : default;

    private static bool MayBeTextMessageActivity(this ITurnContext context)
        =>
        context.IsMessageType() &&
        string.IsNullOrEmpty(context.Activity.Text) is false &&
        context.GetCardActionValueOrAbsent().IsAbsent;

    private static Activity CreateMessageActivity(ITurnContext context, DateStepOption option)
    {
        var replyActivity = MessageFactory.Text(option.Text);

        if (option.DefaultDate is null || context.IsNotTelegramChannel())
        {
            return replyActivity;
        }

        replyActivity.ChannelData = new TelegramChannelData(
            parameters: new()
            {
                ReplyMarkup = new TelegramReplyKeyboardMarkup(
                    keyboard: new[]
                    {
                        option.Suggestions.Select(ToTelegramKeyboardButton).ToArray()
                    })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true,
                    InputFieldPlaceholder = option.Placeholder
                }
            })
        .ToJObject();

        return replyActivity;
    }

    private static TelegramKeyboardButton ToTelegramKeyboardButton(KeyValuePair<string, DateOnly> suggesion)
        =>
        new(suggesion.Key);
}