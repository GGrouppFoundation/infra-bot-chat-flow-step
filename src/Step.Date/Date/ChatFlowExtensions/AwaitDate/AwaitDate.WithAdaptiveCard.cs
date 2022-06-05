using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace GGroupp.Infra.Bot.Builder;

partial class AwaitDateChatFlowExtensions
{
    private const string DateId = "date";

    private const string AdaptiveCardDateFormat = "yyyy-MM-dd";

    private static Result<DateOnly, BotFlowFailure> ParseDateFormAdaptiveCard(ITurnContext context, DateStepOption option)
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

            return DateOnly.ParseExact(dateText, AdaptiveCardDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        if (context.GetCardActionValueOrAbsent().IsPresent)
        {
            return default;
        }

        return ParseDateOrFailure(context.Activity.Text, option);
    }

    private static IActivity CreateDateAdaptiveCardActivity(ITurnContext context, DateStepOption option)
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
                        Title = option.ConfirmButtonText
                    }
                },
                Body = new()
                {
                    new AdaptiveTextBlock
                    {
                        Text = option.Text,
                        Weight = AdaptiveTextWeight.Bolder,
                        Wrap = true
                    },
                    new AdaptiveDateInput
                    {
                        Placeholder = option.Placeholder,
                        Id = DateId,
                        Value = option.DefaultDate?.ToText(AdaptiveCardDateFormat)
                    }
                }
            }
        }
        .ToActivity();

    private static AdaptiveSchemaVersion GetAdaptiveSchemaVersion(this ITurnContext turnContext)
        =>
        turnContext.IsMsteamsChannel() ? AdaptiveCard.KnownSchemaVersion : new(1, 0);
}