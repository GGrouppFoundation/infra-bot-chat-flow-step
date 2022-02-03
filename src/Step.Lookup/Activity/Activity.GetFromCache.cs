using System;
using System.Collections.Generic;
using System.Linq;

namespace GGroupp.Infra.Bot.Builder;

partial class LookupActivity
{
    internal static Optional<LookupValue> GetFromLookupCacheOrAbsent(this IStepStateSupplier flowContext, Guid id)
    {
        return GetLookupCacheOrAbsent().FlatMap(GetLookupValueOrAbsent);

        Optional<Dictionary<Guid, LookupCacheValueJson>> GetLookupCacheOrAbsent()
            =>
            flowContext.StepState is Dictionary<Guid, LookupCacheValueJson> cache ? Optional.Present(cache) : default;

        Optional<LookupValue> GetLookupValueOrAbsent(Dictionary<Guid, LookupCacheValueJson> cacheItems)
            =>
            cacheItems.GetValueOrAbsent(id).Map(CreateItem);

        LookupValue CreateItem(LookupCacheValueJson cacheValueJson)
            =>
            new(id, cacheValueJson.Name, cacheValueJson.Data);
    }
}