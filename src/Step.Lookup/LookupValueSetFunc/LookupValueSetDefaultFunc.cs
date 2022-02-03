using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

public delegate ValueTask<LookupValueSetOption> LookupValueSetDefaultFunc<T>(
    IChatFlowContext<T> context, CancellationToken cancellationToken);