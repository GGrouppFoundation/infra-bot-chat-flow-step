using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed record class ConfirmationCardCacheJson
{
    [JsonProperty("resource")]
    public ResourceResponse? Resource { get; init; }
}