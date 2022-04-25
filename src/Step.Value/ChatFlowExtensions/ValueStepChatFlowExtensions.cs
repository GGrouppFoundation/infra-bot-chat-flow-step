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
        var nextCache = cache.HasRepetitions ? cache : cache with
        {
            HasRepetitions = true
        };
        return ChatFlowJump.Repeat<T>(nextCache);
    }

    private static ValueTask<ChatFlowJump<ValueResult>> GetTextOrRepeatAsync<T>(
        this IChatFlowContext<T> context, ValueStepOption valueStepOption, CancellationToken cancellationToken)
    {
        if (context.StepState is ValueCacheJson cache)
        {
            return context.GetTextValueOrAbsent(cache.Suggestions).FoldValueAsync(ToNextAsync, ToRepeatAsync);
        }
        
        return context.SendSuggestionsActivityAsync(valueStepOption, cancellationToken);

        ValueTask<ChatFlowJump<ValueResult>> ToNextAsync(ValueResult valueResult)
            =>
            new(valueResult);

        ValueTask<ChatFlowJump<ValueResult>> ToRepeatAsync()
        {
            var nextCache = cache.HasRepetitions ? cache : cache with
            {
                HasRepetitions = true
            };

            return new(ChatFlowJump.Repeat<ValueResult>(nextCache));
        }
    }

    private static async ValueTask<ChatFlowJump<ValueResult>> SendSuggestionsActivityAsync<T>(
        this IChatFlowContext<T> context, ValueStepOption valueStepOption, CancellationToken cancellationToken)
    {
        var suggestions = valueStepOption.Suggestions.Select(CreateCacheSuggestionRow).ToArray();
        var activity = SuggestionsActivity.Create(context, valueStepOption.MessageText, suggestions);

        var resource = await context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
        var cache = new ValueCacheJson
        {
            HasRepetitions = false,
            Resource = resource,
            Suggestions = suggestions
        };

        return ChatFlowJump.Repeat<ValueResult>(cache);

        KeyValuePair<Guid, string>[] CreateCacheSuggestionRow(IReadOnlyCollection<string> row)
            =>
            row.Select(CreateCacheSuggestion).ToArray();

        KeyValuePair<Guid, string> CreateCacheSuggestion(string suggestion)
            =>
            new(Guid.NewGuid(), suggestion);
    }

    private static Task UpdateResourceAsync(this ITurnContext context, ResourceResponse? resource, string messageText, CancellationToken token)
    {
        if (string.IsNullOrEmpty(resource?.Id))
        {
            return Task.CompletedTask;
        }

        var updatedActivity = MessageFactory.Text(messageText);
        updatedActivity.Id = resource.Id;

        return context.UpdateActivityAsync(updatedActivity, token);
    }

    private static Task SendResultTextActivityAsync(this ITurnContext context, string resultText, string? choosenText, CancellationToken token)
    {
        if (string.IsNullOrEmpty(choosenText))
        {
            return Task.CompletedTask;
        }

        var encodedText = context.EncodeTextWithStyle(choosenText, BotTextStyle.Bold);
        var activity = MessageFactory.Text($"{resultText}: {encodedText}");

        return context.SendActivityAsync(activity, token);
    }
}