using Newtonsoft.Json;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed record class NextButtonStateJson
{
    [JsonProperty("text")]
    public string? Text { get; init; }

    [JsonProperty("title")]
    public string? Title { get; init; }

    [JsonProperty("nextToken")]
    public string? NextToken { get; init; }
}