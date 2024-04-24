using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

partial class ChoiceActivity
{
    internal static Activity CreateDefaultResultActivity(this ITurnContext context, string name, string value)
    {
        if (context.IsNotTelegramChannel())
        {
            return MessageFactory.Text($"{name}: {context.EncodeTextWithStyle(value, BotTextStyle.Bold)}");
        }

        var telegramParameters = new TelegramParameters($"{name}: {context.EncodeHtmlTextWithStyle(value, BotTextStyle.Bold)}")
        {
            ParseMode = TelegramParseMode.Html,
            ReplyMarkup = new TelegramReplyKeyboardRemove()
        };

        return telegramParameters.BuildActivity();
    }
}