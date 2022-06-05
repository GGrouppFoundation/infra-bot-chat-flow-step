using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class LookupCacheJson
{
    [JsonProperty("resultText")]
    public string? ResultText { get; set; }

    [JsonProperty("resources")]
    public List<ResourceResponse>? Resources { get; set; }

    [JsonProperty("values")]
    public Dictionary<Guid, LookupCacheValueJson>? Values { get; set; }
}