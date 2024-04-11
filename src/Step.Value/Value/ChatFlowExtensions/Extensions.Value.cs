using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class ValueStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitValue<T, TValue>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption<TValue>> optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<IChatFlowContext<T>, TValue, string> resultMessageFactory,
        Func<T, TValue, T> mapFlowState)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(optionFactory);

        ArgumentNullException.ThrowIfNull(valueParser);
        ArgumentNullException.ThrowIfNull(resultMessageFactory);
        ArgumentNullException.ThrowIfNull(mapFlowState);

        return InnerAwaitValue(chatFlow, optionFactory, valueParser, resultMessageFactory, mapFlowState);
    }

    public static ChatFlow<T> AwaitValue<T, TValue>(
        this ChatFlow<T> chatFlow,
        Func<ValueStepOption<TValue>> optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<IChatFlowContext<T>, TValue, string> resultMessageFactory,
        Func<T, TValue, T> mapFlowState)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(optionFactory);

        ArgumentNullException.ThrowIfNull(valueParser);
        ArgumentNullException.ThrowIfNull(resultMessageFactory);
        ArgumentNullException.ThrowIfNull(mapFlowState);

        return InnerAwaitValue(chatFlow, CreateOption, valueParser, resultMessageFactory, mapFlowState);

        ValueStepOption<TValue> CreateOption(IChatFlowContext<T> _) => optionFactory.Invoke();
    }

    public static ChatFlow<T> AwaitValue<T, TValue>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption<TValue>> optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<T, TValue, T> mapFlowState)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(optionFactory);

        ArgumentNullException.ThrowIfNull(valueParser);
        ArgumentNullException.ThrowIfNull(mapFlowState);

        return InnerAwaitValue(chatFlow, optionFactory, valueParser, CreateDefaultResultMessage, mapFlowState);
    }

    private static ChatFlow<T> InnerAwaitValue<T, TValue>(
        ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption<TValue>> optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<IChatFlowContext<T>, TValue, string> resultMessageFactory,
        Func<T, TValue, T> mapFlowState)
    {
        return chatFlow.ForwardValue(InnerInvokeStepAsync);

        ValueTask<ChatFlowJump<T>> InnerInvokeStepAsync(IChatFlowContext<T> context, CancellationToken token)
            =>
            context.InvokeAwaitValueStepAsync(optionFactory, valueParser, resultMessageFactory, mapFlowState, token);
    }

    private static ValueTask<ChatFlowJump<T>> InvokeAwaitValueStepAsync<T, TValue>(
        this IChatFlowContext<T> context,
        Func<IChatFlowContext<T>, ValueStepOption<TValue>> optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<IChatFlowContext<T>, TValue, string> resultMessageFactory,
        Func<T, TValue, T> mapFlowState,
        CancellationToken cancellationToken)
    {
        var option = optionFactory.Invoke(context);
        if (option.SkipStep)
        {
            return new(context.FlowState);
        }

        if (context.StepState is not ValueCacheJson<TValue> cache)
        {
            return context.SendSuggestionsActivityAsync(option, cancellationToken);
        }

        return context.GetTextValueOrAbsent(cache.Suggestions).FoldValueAsync(ParseAsync, RepeatSameStateJumpAsync);

        ValueTask<ChatFlowJump<T>> ParseAsync(string text)
        {
            var suggestionResult = cache.SuggestionValues?.GetValueOrAbsent(text) ?? default;
            if (suggestionResult.IsPresent)
            {
                return suggestionResult.Map(ToNextAsync).OrDefault();
            }

            return valueParser.Invoke(text).FoldValueAsync(ToNextAsync, ToRepeatJumpAsync);
        }

        async ValueTask<ChatFlowJump<T>> ToNextAsync(TValue value)
        {
            await context.SendSuccessAsync(option, value, resultMessageFactory, cancellationToken);
            return mapFlowState.Invoke(context.FlowState, value);
        }

        ValueTask<ChatFlowJump<T>> ToRepeatJumpAsync(BotFlowFailure failure)
            =>
            context.ToRepeatJumpAsync<T, TValue>(context.ChatFlowId, failure, cancellationToken);

        ValueTask<ChatFlowJump<T>> RepeatSameStateJumpAsync()
            =>
            new(context.RepeatSameStateJump());
    }
}