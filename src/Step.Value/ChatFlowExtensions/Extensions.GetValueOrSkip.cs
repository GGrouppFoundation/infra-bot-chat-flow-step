using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class ValueStepChatFlowExtensions
{
    public static ChatFlow<TNext> GetValueOrSkip<T, TValue, TNext>(
        this ChatFlow<T> chatFlow,
        Func<T, SkipActivityOption> optionFactory,
        Func<string, Result<TValue, ChatFlowStepFailure>> valueParser,
        Func<T, TValue?, TNext> mapFlowState)
        where TValue : struct
        =>
        InnerGetValueOrSkip(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            optionFactory ?? throw new ArgumentNullException(nameof(optionFactory)),
            valueParser ?? throw new ArgumentNullException(nameof(valueParser)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<TNext> InnerGetValueOrSkip<T, TValue, TNext>(
        ChatFlow<T> chatFlow,
        Func<T, SkipActivityOption> optionFactory,
        Func<string, Result<TValue, ChatFlowStepFailure>> valueParser,
        Func<T, TValue?, TNext> mapFlowState)
        where TValue : struct
        =>
        chatFlow.ForwardValue(
            optionFactory,
            (context, token) => context.GetValueOrRepeatAsync(valueParser, token),
            mapFlowState);

    private static async ValueTask<ChatFlowJump<TValue?>> GetValueOrRepeatAsync<TValue>(
        this IChatFlowContext<SkipActivityOption> context,
        Func<string, Result<TValue, ChatFlowStepFailure>> valueParser,
        CancellationToken cancellationToken)
        where TValue : struct
    {
        var textResult = await context.GetTextOrRepeatJumpAsync<TValue?>(cancellationToken).ConfigureAwait(false);
        var valueResult = await textResult.ForwardValueAsync(ParseNotNullAsync).ConfigureAwait(false);

        return valueResult.Fold(ChatFlowJump.Next, Pipeline.Pipe);

        ValueTask<Result<TValue?, ChatFlowJump<TValue?>>> ParseNotNullAsync(string? text)
            =>
            string.IsNullOrEmpty(text) ? new(default(TValue?)) : ParseAsync(text);

        ValueTask<Result<TValue?, ChatFlowJump<TValue?>>> ParseAsync(string text)
            =>
            valueParser.Invoke(text).MapSuccess<TValue?>(v => v).MapFailureValueAsync(
                ui => context.ToRepeatJumpAsync<TValue?>(ui, cancellationToken));
    }
}