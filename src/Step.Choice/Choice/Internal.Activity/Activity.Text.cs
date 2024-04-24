using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

partial class ChoiceActivity
{
    internal static Activity CreateTextActivity(this ITurnContext context, string text)
        =>
        context.IsNotTelegramChannel() ? MessageFactory.Text(text) : new TelegramParameters(text).BuildActivity();
}