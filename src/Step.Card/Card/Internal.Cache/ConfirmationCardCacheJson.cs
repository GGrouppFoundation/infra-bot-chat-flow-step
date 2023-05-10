using System;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace GarageGroup.Infra.Bot.Builder;

internal sealed record class ConfirmationCardCacheJson
{
    [JsonProperty("confirmButtonGuid")]
    public Guid ConfirmButtonGuid { get; init; }

    [JsonProperty("cancelButtonGuid")]
    public Guid CancelButtonGuid { get; init; }

    [JsonProperty("resource")]
    public ResourceResponse? Resource { get; init; }
}