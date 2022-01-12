using System.Collections.Generic;
using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class LookupCacheValueJson
{
    [JsonProperty("name")]
    public string? Name { get; init; }

    [JsonProperty("extensions")]
    public KeyValuePair<string, string>[]? Extensions { get; init; }
}