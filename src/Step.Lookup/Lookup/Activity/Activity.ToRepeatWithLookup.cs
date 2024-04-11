using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

partial class LookupActivity
{
    internal static ChatFlowJump<T> ToRepeatWithLookupCacheJump<T>(
        this IStepStateSupplier<T> context, ResourceResponse resource, LookupValueSetOption option)
    {
        var cache = context.StepState as LookupCacheJson ?? new();

        if (cache.Resources is null)
        {
            cache.Resources = [resource];
        }
        else
        {
            cache.Resources.Add(resource);
        }

        cache.Values ??= [];

        foreach (var lookupValue in option.Items)
        {
            cache.Values[lookupValue.Id] = new()
            {
                Name = lookupValue.Name,
                Data = lookupValue.Data
            };
        }

        return ChatFlowJump.Repeat<T>(cache);
    }
}