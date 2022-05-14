using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace GGroupp.Infra.Bot.Builder;

public static partial class ValueStepChatFlowExtensions
{
    private static async ValueTask<ChatFlowJump<T>> ToRepeatJumpAsync<T>(
        this IChatFlowStepContext context, BotFlowFailure failure, CancellationToken token)
    {
        var userMessage = failure.UserMessage;
        if (string.IsNullOrEmpty(userMessage) is false)
        {
            var activity = MessageFactory.Text(userMessage);
            _ = await context.SendActivityAsync(activity, token).ConfigureAwait(false);
        }

        var logMessage = failure.LogMessage;
        if (string.IsNullOrEmpty(logMessage) is false)
        {
            context.Logger.LogError("{logMessage}", logMessage);
        }

        var cache = (context.StepState as ValueCacheJson) ?? new();
        return ChatFlowJump.Repeat<T>(cache);
    }

    private static ValueTask<ChatFlowJump<string>> GetTextOrRepeatAsync<T>(
        this IChatFlowContext<T> context, ValueStepOption valueStepOption, CancellationToken cancellationToken)
    {
        if (context.StepState is ValueCacheJson cache)
        {
            return new(context.GetTextValueOrAbsent(cache.Suggestions).Fold(ChatFlowJump.Next, context.RepeatSameStateJump<string>));
        }
        
        return context.SendSuggestionsActivityAsync(valueStepOption, cancellationToken);
    }

    private static async ValueTask<ChatFlowJump<string>> SendSuggestionsActivityAsync<T>(
        this IChatFlowContext<T> context, ValueStepOption valueStepOption, CancellationToken cancellationToken)
    {
        var suggestions = valueStepOption.Suggestions.Select(CreateCacheSuggestionRow).ToArray();
        var activity = SuggestionsActivity.Create(context, valueStepOption.MessageText, suggestions);

        var resource = await context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
        var cache = new ValueCacheJson
        {
            Resource = context.IsMsteamsChannel() ? resource : null,
            Suggestions = suggestions
        };

        return ChatFlowJump.Repeat<string>(cache);

        KeyValuePair<Guid, string>[] CreateCacheSuggestionRow(IReadOnlyCollection<string> row)
            =>
            row.Select(CreateCacheSuggestion).ToArray();

        KeyValuePair<Guid, string> CreateCacheSuggestion(string suggestion)
            =>
            new(Guid.NewGuid(), suggestion);
    }

    private static Task SendSuccessAsync(
        this IChatFlowStepContext context, ValueStepOption option, string? suggestionText, CancellationToken cancellationToken)
    {
        var cache = context.StepState as ValueCacheJson;

        if (context.Activity.Value is not null && string.IsNullOrEmpty(suggestionText) is false)
        {
            var choosenText = context.EncodeTextWithStyle(suggestionText, BotTextStyle.Bold);
            var resultActivity = MessageFactory.Text($"{option.ResultText}: {choosenText}");
            return context.SendInsteadActivityAsync(cache?.Resource?.Id, resultActivity, cancellationToken);
        }

        if (cache?.Resource is null)
        {
            return Task.CompletedTask;
        }

        var activity = MessageFactory.Text(option.MessageText);
        activity.Id = cache.Resource.Id;

        return context.UpdateActivityAsync(activity, cancellationToken);
    }

    private static Task SendInsteadActivityAsync(this ITurnContext context, string? activityId, IActivity activity, CancellationToken token)
    {
        return string.IsNullOrEmpty(activityId)
            ? SendActivityAsync()
            : Task.WhenAll(DeleteActivityAsync(), SendActivityAsync());

        Task SendActivityAsync()
            =>
            context.SendActivityAsync(activity, token);

        Task DeleteActivityAsync()
            =>
            context.DeleteActivityAsync(activityId, token);
    }
}