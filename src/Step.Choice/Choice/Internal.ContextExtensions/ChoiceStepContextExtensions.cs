using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Infra.Bot.Builder;

internal static partial class ChoiceStepContextExtensions
{
    private static ChoiceItem? GetSelectedItem(this ITurnContext context, ChoiceStepStateJson state)
    {
        if (state.Items?.Count is not > 0)
        {
            return default;
        }

        return context.GetCardActionValueOrAbsent().FlatMap(state.Items.GetValueOrAbsent).Map(InnerMapItem).OrDefault();

        static ChoiceItem InnerMapItem(ChoiceStepItemJson item)
            =>
            new(item.Id, item.Title, item.Data);
    }

    private static ChoiceSetRequest? GetChoiceSetRequest(this ITurnContext context, ChoiceStepStateJson state)
    {
        var text = context.Activity.Value?.ToString() ?? context.Activity.Text;
        if (string.IsNullOrEmpty(text))
        {
            return default;
        }

        if ((state.NextButton is null) || string.Equals(text, state.NextButton.Title, StringComparison.InvariantCultureIgnoreCase) is false)
        {
            return new()
            {
                Text = text
            };
        }

        return new()
        {
            Text = state.NextButton.Text,
            NextToken = state.NextButton.NextToken
        };
    }

    private static ChatFlowJump<T> RepeatWithNewStepState<T>(
        this IStepStateSupplier<T> context, ResourceResponse[] responses, ChoiceSetRequest request, ChoiceSetOption option)
    {
        var state = context.StepState as ChoiceStepStateJson ?? new();
        var resourceResponses = responses ?? [];

        if (state.Resources is null)
        {
            state.Resources = resourceResponses.ToList();
        }
        else
        {
            state.Resources.AddRange(resourceResponses);
        }

        state.Items ??= [];

        foreach (var item in option.Items)
        {
            state.Items[item.Id] = new()
            {
                Id = item.Id,
                Title = item.Title,
                Data = item.Data
            };
        }

        state.NextButton = option.NextButton is null ? null : new()
        {
            Text = request.Text,
            Title = option.NextButton.Title,
            NextToken = option.NextButton.NextToken
        };

        return ChatFlowJump.Repeat<T>(state);
    }

    private static IActivity CreateResultActivity(this ITurnContext context, ChoiceStepOption option, ChoiceItem item)
    {
        if (option.ResultActivityFactory is not null)
        {
            return option.ResultActivityFactory.Invoke(item);
        }

        return context.CreateDefaultResultActivity("Выбрано значение", item.Title);
    }

    private static Task InstallResultActivityAsync(
        this ITurnContext turnContext, IActivity resultActivity, IReadOnlyList<ResourceResponse>? responses, CancellationToken cancellationToken)
    {
        if ((responses?.Count is not > 0) || (turnContext.IsNotTelegramChannel() && turnContext.IsNotMsteamsChannel()))
        {
            return turnContext.SendActivityAsync(resultActivity, cancellationToken);
        }

        var tasks = new List<Task>(responses.Where(NotEmpty).Select(InnerDeleteAsync))
        {
            turnContext.SendActivityAsync(resultActivity, cancellationToken)
        };

        return Task.WhenAll(tasks);

        Task InnerDeleteAsync(ResourceResponse resource)
            =>
            turnContext.DeleteActivityAsync(resource.Id, cancellationToken);

        static bool NotEmpty(ResourceResponse? resource)
            =>
            string.IsNullOrEmpty(resource?.Id) is false;
    }

    private static Task SendFlowFailureAsync(this ITurnContext context, BotFlowFailure searchFailure, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchFailure.UserMessage))
        {
            return Task.CompletedTask;
        }

        var failureActivity = context.CreateTextActivity(searchFailure.UserMessage);
        return context.SendActivityAsync(failureActivity, cancellationToken);
    }

    private static void LogFlowFailure<T>(this IChatFlowContext<T> context, BotFlowFailure failure)
    {
        if (string.IsNullOrWhiteSpace(failure.LogMessage) && failure.SourceException is null)
        {
            return;
        }

        context.Logger.LogError(failure.SourceException, "{logMessage}", failure.LogMessage);

        var properties = new Dictionary<string, string>
        {
            ["flowId"] = context.ChatFlowId,
            ["message"] = failure.LogMessage
        };

        if (failure.SourceException is not null)
        {
            properties["errorMessage"] = failure.SourceException.Message ?? string.Empty;
            properties["errorType"] = failure.SourceException.GetType().FullName ?? string.Empty;
            properties["stackTrace"] = failure.SourceException.StackTrace ?? string.Empty;
        }

        context.BotTelemetryClient.TrackEvent($"{context.ChatFlowId}StepChoiceFailure", properties);
    }
}