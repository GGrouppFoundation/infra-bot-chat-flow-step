using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

public delegate ValueTask<Unit> FlowMessageWriteFunc<T>(FlowMessage<T> flowMessage, CancellationToken cancellationToken)
    where T : notnull;