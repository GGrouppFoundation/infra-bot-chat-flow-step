using Newtonsoft.Json;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed record class LookupCacheValueJson
{
    [JsonProperty("name")]
    public string? Name { get; init; }

    [JsonProperty("data")]
    public string? Data { get; init; }
}