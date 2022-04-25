using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

partial class ValueStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitValue<T, TValue>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption> optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<T, TValue, T> mapFlowState)
    {
        _ = chatFlow ?? throw new ArgumentNullException(nameof(chatFlow));
        _ = optionFactory ?? throw new ArgumentNullException(nameof(optionFactory));

        _ = valueParser ?? throw new ArgumentNullException(nameof(valueParser));
        _ = mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState));

        return chatFlow.ForwardValue(InnerInvokeStepAsync);

        ValueTask<ChatFlowJump<T>> InnerInvokeStepAsync(IChatFlowContext<T> context, CancellationToken token)
            =>
            context.InvokeAwaitValueStepAsync(optionFactory, valueParser, mapFlowState, token);
    }

    private static async ValueTask<ChatFlowJump<T>> InvokeAwaitValueStepAsync<T, TValue>(
        this IChatFlowContext<T> context,
        Func<IChatFlowContext<T>, ValueStepOption> optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<T, TValue, T> mapFlowState,
        CancellationToken cancellationToken)
    {
        var option = optionFactory.Invoke(context);
        if (option.SkipStep)
        {
            return context.FlowState;
        }
        
        var valueResultJump = await context.GetTextOrRepeatAsync(option, cancellationToken).ConfigureAwait(false);
        var textJump = await valueResultJump.MapValueAsync(SendResultTextAsync, ValueTask.FromResult, ValueTask.FromResult).ConfigureAwait(false);

        return await textJump.ForwardValueAsync(ParseAsync).ConfigureAwait(false);

        async ValueTask<string> SendResultTextAsync(ValueResult valueResult)
        {
            await UpdateAndSendAsync(valueResult).ConfigureAwait(false);
            return valueResult.Text;
        }

        Task UpdateAndSendAsync(ValueResult valueResult)
        {
            var updateTask = context.UpdateResourceAsync(GetResource(), option.MessageText, cancellationToken);
            if (valueResult.FromSuggestion is false)
            {
                return updateTask;
            }

            var sendTask = context.SendResultTextActivityAsync(option.ResultText, valueResult.Text, cancellationToken);
            return Task.WhenAll(updateTask, sendTask);
        }

        ResourceResponse? GetResource()
        {
            if (context.StepState is ValueCacheJson cache)
            {
                return cache.Resource;
            }

            return default;
        }

        ValueTask<ChatFlowJump<T>> ParseAsync(string text)
            =>
            valueParser.Invoke(text).FoldValueAsync(ToNextAsync, ToRepeatJumpAsync);

        ValueTask<ChatFlowJump<T>> ToRepeatJumpAsync(BotFlowFailure failure)
            =>
            context.ToRepeatJumpAsync<T>(failure, cancellationToken);

        ValueTask<ChatFlowJump<T>> ToNextAsync(TValue value)
            =>
            new(mapFlowState.Invoke(context.FlowState, value));
    }
}