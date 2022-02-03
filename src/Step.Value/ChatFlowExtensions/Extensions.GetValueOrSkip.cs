using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class ValueStepChatFlowExtensions
{
    public static ChatFlow<T> GetValueOrSkip<T, TValue>(
        this ChatFlow<T> chatFlow,
        Func<T, SkipValueStepOption> optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<T, TValue?, T> mapFlowState)
        where TValue : struct
        =>
        InnerGetValueOrSkip(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            optionFactory ?? throw new ArgumentNullException(nameof(optionFactory)),
            valueParser ?? throw new ArgumentNullException(nameof(valueParser)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<T> InnerGetValueOrSkip<T, TValue>(
        ChatFlow<T> chatFlow,
        Func<T, SkipValueStepOption> optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<T, TValue?, T> mapFlowState)
        where TValue : struct
        =>
        chatFlow.ForwardValue(
            (context, token) => context.GetValueOrSkipOrRepeatAsync(optionFactory, valueParser, mapFlowState, token));

    private static async ValueTask<ChatFlowJump<T>> GetValueOrSkipOrRepeatAsync<T, TValue>(
        this IChatFlowContext<T> context,
        Func<T, SkipValueStepOption> optionFactory,
        Func<string, Result<TValue, BotFlowFailure>> valueParser,
        Func<T, TValue?, T> mapFlowState,
        CancellationToken cancellationToken)
        where TValue : struct
    {
        var option = optionFactory.Invoke(context.FlowState);
        if (option.SkipStep)
        {
            return context.FlowState;
        }

        var textResult = await context.GetTextOrRepeatJumpAsync<T>(option, cancellationToken).ConfigureAwait(false);
        var valueResult = await textResult.ForwardValueAsync(ParseNotNullAsync).ConfigureAwait(false);

        return valueResult.MapSuccess(MapValue).Fold(ChatFlowJump.Next, Pipeline.Pipe);

        ValueTask<Result<TValue?, ChatFlowJump<T>>> ParseNotNullAsync(string? text)
            =>
            string.IsNullOrEmpty(text) switch
            {
                true => Result.Success<TValue?>(default).With<ChatFlowJump<T>>().Pipe(ValueTask.FromResult),
                _ => text.Pipe(valueParser).MapValueAsync(ToNullableAsync, ToRepeatJumpAsync)
            };

        static ValueTask<TValue?> ToNullableAsync(TValue value)
            =>
            new(value);

        ValueTask<ChatFlowJump<T>> ToRepeatJumpAsync(BotFlowFailure failure)
            =>
            context.ToRepeatJumpAsync<T>(failure, cancellationToken);

        T MapValue(TValue? value)
            =>
            mapFlowState.Invoke(context.FlowState, value);
    }
}