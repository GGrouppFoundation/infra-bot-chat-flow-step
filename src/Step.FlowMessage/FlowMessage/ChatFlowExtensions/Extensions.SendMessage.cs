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
        IQueueWriter<BotFlowMessage<TMessage>> messageWriter,
        Func<IActivity>? temporaryActivityFactory = null,
        Func<T, MessageSendOut, T>? mapFlowState = null)
    {
        _ = chatFlow ?? throw new ArgumentNullException(nameof(chatFlow));
        _ = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
        _ = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));

        return chatFlow.NextValue(InnerSendFlowMessageAsync);

        ValueTask<T> InnerSendFlowMessageAsync(IChatFlowContext<T> context, CancellationToken cancellationToken)
            =>
            context.SendFlowMessageAsync(messageFactory, messageWriter, temporaryActivityFactory, mapFlowState, cancellationToken);
    }

    private static async ValueTask<T> SendFlowMessageAsync<T, TMessage>(
        this IChatFlowContext<T> context,
        Func<T, TMessage> messageFactory,
        IQueueWriter<BotFlowMessage<TMessage>> messageWriter,
        Func<IActivity>? temporaryActivityFactory,
        Func<T, MessageSendOut, T>? mapFlowState,
        CancellationToken cancellationToken)
    {
        var temporaryActivityId = await context.SendTemporaryActivityAsync(temporaryActivityFactory, cancellationToken).ConfigureAwait(false);

        var message = new BotFlowMessage<TMessage>(
            channelId: context.Activity.ChannelId,
            recipientId: context.Activity.Recipient.Id,
            temporaryActivityId: temporaryActivityId,
            message: messageFactory.Invoke(context.FlowState));

        var result = await messageWriter.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);

        return mapFlowState is null ? context.FlowState : mapFlowState.Invoke(context.FlowState, result);
    }
}