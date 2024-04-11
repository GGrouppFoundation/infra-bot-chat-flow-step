using System;
using System.Linq;

namespace GarageGroup.Infra.Bot.Builder;

partial class LookupActivity
{
    internal static Optional<LookupCacheResult> GetFromLookupCacheOrAbsent<T>(this IStepStateSupplier<T> flowContext, Guid id)
    {
        return GetLookupCacheOrAbsent().FlatMap(GetLookupValueOrAbsent);

        Optional<LookupCacheJson> GetLookupCacheOrAbsent()
            =>
            flowContext.StepState is LookupCacheJson cache ? Optional.Present(cache) : default;

        Optional<LookupCacheResult> GetLookupValueOrAbsent(LookupCacheJson cache)
            =>
            cache.Values?.GetValueOrAbsent(id).Map(value => CreateItem(cache, value)) ?? default;

        LookupCacheResult CreateItem(LookupCacheJson cache, LookupCacheValueJson cacheValueJson)
            =>
            new(
                resources: cache.Resources,
                value: new(id, cacheValueJson.Name, cacheValueJson.Data));
    }
}