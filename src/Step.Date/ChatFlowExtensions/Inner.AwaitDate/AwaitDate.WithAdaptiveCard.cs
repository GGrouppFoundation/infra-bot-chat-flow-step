using AdaptiveCards;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;

namespace GGroupp.Infra.Bot.Builder;

partial class AwaitDateChatFlowExtensions
{
    private const string DateId = "date";

    private const string AdaptiveCardDateFormat = "yyyy-MM-dd";

    private static Result<DateOnly, Unit> ParseDateFormAdaptiveCard(IChatFlowContext<AwaitDateOption> context)
        =>
        context.Activity.Value is JObject jObject && jObject.HasValues
        ? ParseDateOrFailure(jObject[DateId]?.ToString(), AdaptiveCardDateFormat)
        : ParseDateOrFailure(context.Activity.Text, context.FlowState.DateFormat);

    private static IActivity CreateDateAdaptiveCardActivity(IChatFlowContext<AwaitDateOption> context)
        =>
        new Attachment
        {
            ContentType = AdaptiveCard.ContentType,
            Content = new AdaptiveCard(context.Activity.ChannelId.GetAdaptiveSchemaVersion())
            {
                Actions = new()
                {
                    new AdaptiveSubmitAction()
                    {
                        Title = context.FlowState.ConfirmButtonText
                    }
                },
                Body = new()
                {
                    new AdaptiveTextBlock
                    {
                        Text = context.FlowState.Text,
                        Wrap = true
                    },
                    new AdaptiveDateInput
                    {
                        Placeholder = context.FlowState.Text,
                        Id = DateId,
                        Value = context.FlowState.DefaultDate?.ToText(AdaptiveCardDateFormat)
                    }
                }
            }
        }
        .ToActivity();

    private static AdaptiveSchemaVersion GetAdaptiveSchemaVersion(this string channelId)
        =>
        channelId.Equals(Channels.Msteams, StringComparison.InvariantCultureIgnoreCase) ? AdaptiveCard.KnownSchemaVersion : new(1, 0);
}