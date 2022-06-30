using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

public static partial class FlowMessageChatFlowExtensions
{
    private static readonly IReadOnlyCollection<string> TemporarySupportedChannels;

    static FlowMessageChatFlowExtensions()
        =>
        TemporarySupportedChannels = new[]
        {
            Channels.Telegram, Channels.Msteams
        };

    private static async ValueTask<string?> SendTemporaryActivityAsync(
        this ITurnContext context, Func<IActivity>? temporaryActivityFactory, CancellationToken cancellationToken)
    {
        if (temporaryActivityFactory is null)
        {
            return null;
        }

        var activity = temporaryActivityFactory.Invoke();
        var response = await context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);

        if (TemporarySupportedChannels.Contains(context.Activity.ChannelId, StringComparer.InvariantCultureIgnoreCase) is false)
        {
            return null;
        }

        return response?.Id;
    }
}