using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

public delegate ValueTask<LookupValueSetSeachOut> LookupValueSetDefaultFunc<T>(
    T flowState, CancellationToken cancellationToken);