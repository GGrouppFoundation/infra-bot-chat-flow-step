using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

partial class CardActivity
{
    internal static IActivity CreateInvalidDataActivity(this ITurnContext context, string text)
    {
        if (context.IsNotTelegramChannel())
        {
            return MessageFactory.Text(text);
        }

        var telegramActivity = MessageFactory.Text(default);
        telegramActivity.ChannelData = BuildTelegramChannelData(text).ToJObject();

        return telegramActivity;

        static TelegramChannelData BuildTelegramChannelData(string text)
            =>
            new(
                parameters: new(text)
                {
                    ParseMode = TelegramParseMode.Html
                });
    }
}