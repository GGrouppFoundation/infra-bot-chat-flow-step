using System;
using Microsoft.Bot.Builder;

namespace GGroupp.Infra.Bot.Builder;

partial class LookupActivity
{
    public static Optional<LookupValue> GetChoosenValueOrAbsent<TContext>(this TContext context)
        where TContext : IStepStateSupplier, ITurnContext
        =>
        context.Activity.GetCardActionValueOrAbsent().FlatMap(
            context.GetFromLookupCacheOrAbsent);
}