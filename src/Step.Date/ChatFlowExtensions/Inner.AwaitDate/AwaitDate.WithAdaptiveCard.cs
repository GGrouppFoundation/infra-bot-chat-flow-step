using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;

namespace GGroupp.Infra.Bot.Builder;

partial class AwaitDateChatFlowExtensions
{
    private const string DateId = "date";

    private const string AdaptiveCardDateFormat = "yyyy-MM-dd";

    private static Result<DateOnly, BotFlowFailure> ParseDateFormAdaptiveCard(IChatFlowContext<AwaitDateOption> context)
    {
        if (context.IsNotMessageType())
        {
            return default;
        }

        if (context.Activity.Value is JObject jObject && jObject.HasValues)
        {
            var dateText = jObject[DateId]?.ToString();

            if (string.IsNullOrEmpty(dateText))
            {
                return default;
            }

            return ParseDateOrFailure(dateText, AdaptiveCardDateFormat, context.FlowState.InvalidDateText);
        }

        if (context.GetCardActionValueOrAbsent().IsPresent)
        {
            return default;
        }

        return ParseDateOrFailure(context.Activity.Text, context.FlowState.DateFormat, context.FlowState.InvalidDateText);
    }

    private static IActivity CreateDateAdaptiveCardActivity(IChatFlowContext<AwaitDateOption> context)
        =>
        new Attachment
        {
            ContentType = AdaptiveCard.ContentType,
            Content = new AdaptiveCard(context.GetAdaptiveSchemaVersion())
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

    private static AdaptiveSchemaVersion GetAdaptiveSchemaVersion(this ITurnContext turnContext)
        =>
        turnContext.IsMsteamsChannel() ? AdaptiveCard.KnownSchemaVersion : new(1, 0);
}