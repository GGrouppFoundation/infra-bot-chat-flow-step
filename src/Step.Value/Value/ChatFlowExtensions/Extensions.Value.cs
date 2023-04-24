using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class ValueStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitValue<T, TValue>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption<TValue>> optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<IChatFlowContext<T>, TValue, string> resultMessageFactory,
        Func<T, TValue, T> mapFlowState)
        =>
        InnerAwaitValue(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            optionFactory ?? throw new ArgumentNullException(nameof(optionFactory)),
            valueParser ?? throw new ArgumentNullException(nameof(valueParser)),
            resultMessageFactory ?? throw new ArgumentNullException(nameof(resultMessageFactory)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    public static ChatFlow<T> AwaitValue<T, TValue>(
        this ChatFlow<T> chatFlow,
        Func<ValueStepOption<TValue>> optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<IChatFlowContext<T>, TValue, string> resultMessageFactory,
        Func<T, TValue, T> mapFlowState)
    {
        _ = chatFlow ?? throw new ArgumentNullException(nameof(chatFlow));
        _ = optionFactory ?? throw new ArgumentNullException(nameof(optionFactory));
        _ = valueParser ?? throw new ArgumentNullException(nameof(valueParser));

        _ = resultMessageFactory ?? throw new ArgumentNullException(nameof(resultMessageFactory));
        _ = mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState));

        return InnerAwaitValue(chatFlow, CreateOption, valueParser, resultMessageFactory, mapFlowState);

        ValueStepOption<TValue> CreateOption(IChatFlowContext<T> _) => optionFactory.Invoke();
    }

    public static ChatFlow<T> AwaitValue<T, TValue>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ValueStepOption<TValue>> optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<T, TValue, T> mapFlowState)
        =>
        InnerAwaitValue(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            optionFactory ?? throw new ArgumentNullException(nameof(optionFactory)),
            valueParser ?? throw new ArgumentNullException(nameof(valueParser)),
            CreateDefaultResultMessage,
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

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

        var textJump = context.GetTextValueOrAbsent(cache.Suggestions).Fold(ChatFlowJump.Next, context.RepeatSameStateJump<string>);
        return textJump.ForwardValueAsync(ParseAsync);

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
    }
}