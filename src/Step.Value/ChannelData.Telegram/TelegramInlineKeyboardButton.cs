using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class TelegramInlineKeyboardButton
{
    public TelegramInlineKeyboardButton(string text, object callbackData)
    {
        Text = text ?? string.Empty;
        CallbackData = callbackData;
    }

    [JsonProperty("text")]
    public string Text { get; }

    [JsonProperty("callback_data")]
    public object CallbackData { get; }
}