using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class ValueStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitValue<T, TValue>(
        this ChatFlow<T> chatFlow,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<T, TValue, T> mapFlowState)
        =>
        InnerAwaitRequiredValue(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            default,
            valueParser ?? throw new ArgumentNullException(nameof(valueParser)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    public static ChatFlow<T> AwaitValue<T, TValue>(
        this ChatFlow<T> chatFlow,
        Func<T, ValueStepOption>? optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<T, TValue, T> mapFlowState)
        =>
        InnerAwaitRequiredValue(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            optionFactory ?? throw new ArgumentNullException(nameof(optionFactory)),
            valueParser ?? throw new ArgumentNullException(nameof(valueParser)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<T> InnerAwaitRequiredValue<T, TValue>(
        ChatFlow<T> chatFlow,
        Func<T, ValueStepOption>? optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<T, TValue, T> mapFlowState)
        =>
        chatFlow.Await().ForwardValue(
            (context, token) => context.GetRequiredValueOrRepeatAsync(optionFactory, valueParser, mapFlowState, token));

    private static async ValueTask<ChatFlowJump<T>> GetRequiredValueOrRepeatAsync<T, TValue>(
        this IChatFlowContext<T> context,
        Func<T, ValueStepOption>? optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<T, TValue, T> mapFlowState,
        CancellationToken cancellationToken)
    {
        if (optionFactory is not null && optionFactory.Invoke(context.FlowState).SkipStep)
        {
            return context.FlowState;
        }

        var textResult = context.GetRequiredTextOrFailure();
        var valueResult = await textResult.Forward(valueParser).MapFailureValueAsync(ToRepeatJumpAsync).ConfigureAwait(false);

        return valueResult.MapSuccess(MapValue).Fold(ChatFlowJump.Next, Pipeline.Pipe);

        ValueTask<ChatFlowJump<T>> ToRepeatJumpAsync(BotFlowFailure failure)
            =>
            context.ToRepeatJumpAsync<T>(failure, cancellationToken);

        T MapValue(TValue value)
            =>
            mapFlowState.Invoke(context.FlowState, value);
    }
}