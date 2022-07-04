using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

partial class FlowMessageChatFlowExtensions
{
    public static ChatFlow<T> SendFlowMessage<T, TMessage>(
        this ChatFlow<T> chatFlow,
        Func<T, TMessage> messageFactory,
        FlowMessageWriteFunc<TMessage> flowMessageWriteFunc,
        Func<IActivity>? temporaryActivityFactory = null)
        where TMessage : notnull
    {
        _ = chatFlow ?? throw new ArgumentNullException(nameof(chatFlow));
        _ = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
        _ = flowMessageWriteFunc ?? throw new ArgumentNullException(nameof(flowMessageWriteFunc));

        return chatFlow.NextValue(InnerSendFlowMessageAsync);

        ValueTask<T> InnerSendFlowMessageAsync(IChatFlowContext<T> context, CancellationToken cancellationToken)
            =>
            context.SendFlowMessageAsync(messageFactory, flowMessageWriteFunc, temporaryActivityFactory, cancellationToken);
    }

    private static async ValueTask<T> SendFlowMessageAsync<T, TMessage>(
        this IChatFlowContext<T> context,
        Func<T, TMessage> messageFactory,
        FlowMessageWriteFunc<TMessage> flowMessageWriteFunc,
        Func<IActivity>? temporaryActivityFactory,
        CancellationToken cancellationToken)
        where TMessage : notnull
    {
        var temporaryActivityId = await context.SendTemporaryActivityAsync(temporaryActivityFactory, cancellationToken).ConfigureAwait(false);

        var message = new FlowMessage<TMessage>(
            channel: context.Activity.ChannelId,
            fromId: context.Activity.From.Id,
            temporaryActivityId: temporaryActivityId,
            message: messageFactory.Invoke(context.FlowState));

        _ = await flowMessageWriteFunc.Invoke(message, cancellationToken).ConfigureAwait(false);

        return context.FlowState;
    }
}