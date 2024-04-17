using System;
using Newtonsoft.Json;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed record class ChoiceStepItemJson
{
    [JsonProperty("id")]
    public Guid Id { get; init; }

    [JsonProperty("title")]
    public string? Title { get; init; }

    [JsonProperty("data")]
    public object? Data { get; init; }
}