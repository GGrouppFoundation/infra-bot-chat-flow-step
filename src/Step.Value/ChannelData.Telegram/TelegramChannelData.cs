using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class TelegramChannelData
{
    public TelegramChannelData(string method, TelegramMessage message)
    {
        Method = method ?? string.Empty;
        Message = message;
    }

    [JsonProperty("method")]
    public string Method { get; }

    [JsonProperty("parameters")]
    public TelegramMessage Message { get; }
}