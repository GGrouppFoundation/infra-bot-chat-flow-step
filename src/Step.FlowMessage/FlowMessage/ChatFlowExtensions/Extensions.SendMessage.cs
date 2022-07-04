using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

partial class FlowMessageChatFlowExtensions
{
    public static ChatFlow<T> SendFlowMessage<T, TValue>(
        this ChatFlow<T> chatFlow,
        Func<T, TValue> valueFactory,
        FlowMessageWriteFunc<TValue> flowMessageWriteFunc,
        Func<IActivity>? temporaryActivityFactory = null)
        where TValue : notnull
    {
        _ = chatFlow ?? throw new ArgumentNullException(nameof(chatFlow));
        _ = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
        _ = flowMessageWriteFunc ?? throw new ArgumentNullException(nameof(flowMessageWriteFunc));

        return chatFlow.NextValue(InnerSendFlowMessageAsync);

        ValueTask<T> InnerSendFlowMessageAsync(IChatFlowContext<T> context, CancellationToken cancellationToken)
            =>
            context.SendFlowMessageAsync(valueFactory, flowMessageWriteFunc, temporaryActivityFactory, cancellationToken);
    }

    private static async ValueTask<T> SendFlowMessageAsync<T, TValue>(
        this IChatFlowContext<T> context,
        Func<T, TValue> valueFactory,
        FlowMessageWriteFunc<TValue> flowMessageWriteFunc,
        Func<IActivity>? temporaryActivityFactory,
        CancellationToken cancellationToken)
        where TValue : notnull
    {
        var temporaryActivityId = await context.SendTemporaryActivityAsync(temporaryActivityFactory, cancellationToken).ConfigureAwait(false);

        var message = new FlowMessage<TValue>(
            channel: context.Activity.ChannelId,
            fromId: context.Activity.From.Id,
            temporaryActivityId: temporaryActivityId,
            value: valueFactory.Invoke(context.FlowState));

        _ = await flowMessageWriteFunc.Invoke(message, cancellationToken).ConfigureAwait(false);

        return context.FlowState;
    }
}