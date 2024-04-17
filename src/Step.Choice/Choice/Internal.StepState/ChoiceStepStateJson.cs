using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed record class ChoiceStepStateJson
{
    [JsonProperty("resources")]
    public List<ResourceResponse>? Resources { get; set; }

    [JsonProperty("values")]
    public Dictionary<Guid, ChoiceStepItemJson>? Items { get; set; }

    [JsonProperty("nextButton")]
    public NextButtonStateJson? NextButton { get; set; }
}