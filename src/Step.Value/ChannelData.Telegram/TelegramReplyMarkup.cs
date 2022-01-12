using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class TelegramReplyMarkup
{
    public TelegramReplyMarkup([AllowNull] TelegramInlineKeyboardButton[][] inlineKeyboard)
        =>
        InlineKeyboard = inlineKeyboard ?? Array.Empty<TelegramInlineKeyboardButton[]>();

    [JsonProperty("inline_keyboard")]
    public TelegramInlineKeyboardButton[][] InlineKeyboard { get; }
}