using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class TelegramMessage
{
    public TelegramMessage(TelegramReplyMarkup replyMarkup)
        =>
        ReplyMarkup = replyMarkup;

    [JsonProperty("reply_markup")]
    public TelegramReplyMarkup ReplyMarkup { get; }
}