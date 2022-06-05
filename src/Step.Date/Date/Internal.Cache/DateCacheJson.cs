using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class DateCacheJson
{
    [JsonProperty("resource")]
    public ResourceResponse? Resource { get; init; }
}