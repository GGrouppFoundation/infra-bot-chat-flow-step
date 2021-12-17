using System.Collections.Generic;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class LookupCacheValueJson
{
    public string? Name { get; init; }

    public KeyValuePair<string, string>[]? Extensions { get; init; }
}