using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

public delegate ValueTask<Unit> FlowMessageWriteFunc<TMessage>(FlowMessage<TMessage> flowMessage, CancellationToken cancellationToken)
    where TMessage : notnull;