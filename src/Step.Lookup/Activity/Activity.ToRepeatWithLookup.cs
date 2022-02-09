using System;
using System.Collections.Generic;
using System.Linq;

namespace GGroupp.Infra.Bot.Builder;

partial class LookupActivity
{
    internal static ChatFlowJump<T> ToRepeatWithLookupCacheJump<T>(this IStepStateSupplier context, LookupValueSetOption option)
    {
        var cache = context.StepState as Dictionary<Guid, LookupCacheValueJson> ?? new();
        foreach (var lookupValue in option.Items)
        {
            cache[lookupValue.Id] = new()
            {
                Name = lookupValue.Name,
                Data = lookupValue.Data
            };
        }

        return cache.Any() ? ChatFlowJump.Repeat<T>(cache) : ChatFlowJump.Repeat<T>(default);
    }
}