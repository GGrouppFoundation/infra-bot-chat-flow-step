using System;
using System.Collections.Generic;
using System.Linq;

namespace GGroupp.Infra.Bot.Builder;

partial class LookupActivity
{
    internal static ChatFlowJump<LookupValue> ToRepeatWithLookupCacheJump(this IStepStateSupplier context, LookupValueSetSeachOut searchOut)
    {
        var cache = context.StepState as Dictionary<Guid, LookupCacheValueJson> ?? new();
        foreach (var lookupValue in searchOut.Items)
        {
            cache[lookupValue.Id] = new()
            {
                Name = lookupValue.Name,
                Extensions = lookupValue.Extensions.ToArray()
            };
        }

        return cache.Any() ? ChatFlowJump.Repeat<LookupValue>(cache) : ChatFlowJump.Repeat<LookupValue>(default);
    }
}