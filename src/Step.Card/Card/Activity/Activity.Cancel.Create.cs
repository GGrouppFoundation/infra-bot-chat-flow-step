using System.Web;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

partial class CardActivity
{
    internal static IActivity CreateCancelActivity(this ITurnContext context, string text)
    {
        if (context.IsNotTelegramChannel())
        {
            return MessageFactory.Text(text);
        }

        var channelData = new TelegramChannelData(
            parameters: new(HttpUtility.HtmlEncode(text))
            {
                ParseMode = TelegramParseMode.Html,
                ReplyMarkup = new TelegramReplyKeyboardRemove()
            });

        var activity = MessageFactory.Text(default);
        activity.ChannelData = channelData.ToJObject();

        return activity;
    }
}