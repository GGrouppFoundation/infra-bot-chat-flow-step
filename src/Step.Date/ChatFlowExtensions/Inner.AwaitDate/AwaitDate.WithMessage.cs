using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;

namespace GGroupp.Infra.Bot.Builder;

partial class AwaitDateChatFlowExtensions
{
    private static Result<DateOnly, BotFlowFailure> ParseDateFromText(IChatFlowContext<AwaitDateOption> context)
        =>
        context.Activity.MayBeTextMessageActivity()
        ? ParseDateOrFailure(context.Activity.Text, context.FlowState.DateFormat, context.FlowState.InvalidDateText)
        : default;

    private static bool MayBeTextMessageActivity(this Activity activity)
        =>
        activity.IsMessageType() && string.IsNullOrEmpty(activity.Text) is false && activity.GetCardActionValueOrAbsent().IsAbsent;

    private static Activity CreateMessageActivity(IChatFlowContext<AwaitDateOption> context)
    {
        var option = context.FlowState;
        var activity = MessageFactory.Text(option.Text);

        if (option.DefaultDate is null || context.Activity.IsTelegram() is false)
        {
            return activity;
        }

        var channelData = CreateTelegramChannelData(option.DefaultDate.Value, option.DateFormat, option.Text);
        return activity.SetTelegramChannelData(channelData);
    }

    private static TelegramChannelData CreateTelegramChannelData(DateOnly defaultDate, string dateFormat, string? placeholder)
        =>
        new()
        {
            Method = TelegramMethod.SendMessage,
            Parameters = new()
            {
                ReplyMarkup = new TelegramReplyKeyboardMarkup
                {
                    Keyboard = new[]
                    {
                        new[]
                        {
                            defaultDate.AddDays(-1).ToTelegramButton(dateFormat),
                            defaultDate.ToTelegramButton(dateFormat)
                        }
                    },
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true,
                    InputFieldPlaceholder = placeholder
                }
            }
        };

    private static TelegramKeyboardButton ToTelegramButton(this DateOnly date, string dateFormat)
        =>
        new()
        {
            Text = date.ToText(dateFormat)
        };
}