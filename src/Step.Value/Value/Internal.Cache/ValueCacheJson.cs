using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class ValueCacheJson
{
    [JsonProperty("resource")]
    public ResourceResponse? Resource { get; init; }

    [JsonProperty("suggestions")]
    public KeyValuePair<Guid, string>[][]? Suggestions { get; init; }
}