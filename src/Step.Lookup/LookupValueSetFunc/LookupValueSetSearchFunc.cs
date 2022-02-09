using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

public delegate ValueTask<Result<LookupValueSetOption, BotFlowFailure>> LookupValueSetSearchFunc<T>(
    IChatFlowContext<T> context, string searchText, CancellationToken cancellationToken);