using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;

namespace GGroupp.Infra.Bot.Builder;

partial class AwaitDateChatFlowExtensions
{
    private static Result<DateOnly, BotFlowFailure> ParseDateFromText(ITurnContext context, DateStepOption option)
        =>
        context.MayBeTextMessageActivity()
        ? ParseDateOrFailure(context.Activity.Text, option.DateFormat, option.InvalidDateText)
        : default;

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

        replyActivity.ChannelData = CreateTelegramChannelData(option.DefaultDate.Value, option.DateFormat, option.Text);
        return replyActivity;
    }

    private static JObject CreateTelegramChannelData(DateOnly defaultDate, string dateFormat, string? placeholder)
        =>
        new TelegramChannelData(
            parameters: new()
            {
                ReplyMarkup = new TelegramReplyKeyboardMarkup(
                    keyboard: new[]
                    {
                        new TelegramKeyboardButton[]
                        {
                            new(defaultDate.AddDays(-1).ToText(dateFormat)),
                            new(defaultDate.ToText(dateFormat))
                        }
                    })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true,
                    InputFieldPlaceholder = placeholder
                }
            })
        .ToJObject();
}