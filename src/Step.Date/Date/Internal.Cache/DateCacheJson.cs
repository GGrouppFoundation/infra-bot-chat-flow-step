using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class DateCacheJson
{
    [JsonProperty("resource")]
    public ResourceResponse? Resource { get; init; }

    [JsonProperty("suggestions")]
    public KeyValuePair<string, string>[]? Suggestions { get; init; }

    [JsonProperty("invalidDateText")]
    public string? InvalidDateText { get; init; }
}