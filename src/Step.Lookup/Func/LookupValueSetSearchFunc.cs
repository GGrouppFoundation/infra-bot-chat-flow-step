using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

public delegate ValueTask<Result<LookupValueSetSeachOut, ChatFlowStepFailure>> LookupValueSetSearchFunc<T>(
    T flowState, LookupValueSetSeachIn searchInput, CancellationToken cancellationToken);