using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

partial class CardActivity
{
    internal static Activity CreateInvalidDataActivity(this ITurnContext context, string text)
    {
        if (context.IsNotTelegramChannel())
        {
            return MessageFactory.Text(text);
        }

        var telegramParameters = new TelegramParameters(text)
        {
            ParseMode = TelegramParseMode.Html
        };

        return telegramParameters.BuildActivity();
    }
}