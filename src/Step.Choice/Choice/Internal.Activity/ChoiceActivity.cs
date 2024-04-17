using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

internal static partial class ChoiceActivity
{
    private const string TelegramEmptyText = "ㅤ";

    private static Activity InnerCreateTelegramActivity(this TelegramChannelData telegramChannelData)
        =>
        new(type: ActivityTypes.Message)
        {
            ChannelData = telegramChannelData.ToJObject()
        };
}