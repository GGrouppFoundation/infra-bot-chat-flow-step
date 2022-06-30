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
    private static async ValueTask<ChatFlowJump<T>> SendSuggestionsActivityAsync<T, TValue>(
        this IChatFlowContext<T> context, ValueStepOption<TValue> valueStepOption, CancellationToken cancellationToken)
    {
        var suggestions = valueStepOption.Suggestions.Select(CreateCacheSuggestionRow).ToArray();
        var activity = SuggestionsActivity.Create(context, valueStepOption.MessageText, suggestions);

        var resource = await context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
        var cache = new ValueCacheJson<TValue>
        {
            Resource = context.IsMsteamsChannel() ? resource : null,
            Suggestions = suggestions.OrNullIfEmpty(),
            SuggestionValues = valueStepOption.Suggestions.SelectMany(Pipeline.Pipe).ToArray().OrNullIfEmpty()
        };

        return ChatFlowJump.Repeat<T>(cache);

        static KeyValuePair<Guid, string>[] CreateCacheSuggestionRow(IReadOnlyCollection<KeyValuePair<string, TValue>> row)
            =>
            row.Select(CreateCacheSuggestion).ToArray();

        static KeyValuePair<Guid, string> CreateCacheSuggestion(KeyValuePair<string, TValue> suggestion)
            =>
            new(Guid.NewGuid(), suggestion.Key);
    }

    private static async ValueTask<ChatFlowJump<T>> ToRepeatJumpAsync<T, TValue>(
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

        var cache = (context.StepState as ValueCacheJson<TValue>) ?? new();
        return ChatFlowJump.Repeat<T>(cache);
    }

    private static Task SendSuccessAsync<T, TValue>(
        this IChatFlowContext<T> context,
        ValueStepOption<TValue> option,
        TValue suggestionValue,
        Func<IChatFlowContext<T>, TValue, string> resultMessageFactory,
        CancellationToken cancellationToken)
    {
        var cache = context.StepState as ValueCacheJson<TValue>;

        if (context.Activity.Value is not null)
        {
            var resultMessage = resultMessageFactory.Invoke(context, suggestionValue);
            var resultMessageActivity = MessageFactory.Text(resultMessage);

            return context.SendInsteadActivityAsync(cache?.Resource?.Id, resultMessageActivity, cancellationToken);
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

    private static string CreateDefaultResultMessage<T, TValue>(IChatFlowContext<T> context, TValue value)
    {
        var text = context.EncodeTextWithStyle(value?.ToString(), BotTextStyle.Bold);
        return $"Выбрано значение: {text}";
    }

    private static T[]? OrNullIfEmpty<T>(this T[]? source)
        =>
        source?.Length is not > 0 ? null : source;
}