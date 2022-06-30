using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class ValueCacheJson<TValue>
{
    [JsonProperty("resource")]
    public ResourceResponse? Resource { get; init; }

    [JsonProperty("suggestions")]
    public KeyValuePair<Guid, string>[][]? Suggestions { get; init; }

    [JsonProperty("suggestionValues")]
    public KeyValuePair<string, TValue>[]? SuggestionValues { get; init; }
}