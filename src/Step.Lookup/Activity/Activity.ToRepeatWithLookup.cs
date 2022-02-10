using System;
using System.Collections.Generic;

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

        return ChatFlowJump.Repeat<T>(cache);
    }
}