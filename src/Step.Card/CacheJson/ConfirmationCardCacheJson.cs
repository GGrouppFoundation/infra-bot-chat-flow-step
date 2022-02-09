using System;
using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class ConfirmationCardCacheJson
{
    [JsonProperty("confirmButtonGuid")]
    public Guid ConfirmButtonGuid { get; init; }

    [JsonProperty("cancelButtonGuid")]
    public Guid CancelButtonGuid { get; init; }
}