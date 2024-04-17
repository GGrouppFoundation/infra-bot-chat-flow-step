using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

partial class ChoiceActivity
{
    internal static Activity CreateTextActivity(this ITurnContext context, string text)
    {
        if (context.IsNotTelegramChannel())
        {
            return MessageFactory.Text(text);
        }

        var telegramChannelData = new TelegramChannelData(
            parameters: new(text));

        return telegramChannelData.InnerCreateTelegramActivity();
    }
}