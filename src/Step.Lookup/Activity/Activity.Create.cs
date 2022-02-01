using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace GGroupp.Infra.Bot.Builder;

partial class LookupActivity
{
    internal static IActivity CreateLookupActivity(this ITurnContext context, LookupValueSetSeachOut searchOut)
    {
        if (context.IsTelegramChannel())
        {
            var telegramReply = MessageFactory.Text(default);
            telegramReply.ChannelData = CreateTelegramChannelData(context, searchOut);

            return telegramReply;
        }

        return CreateHeroCardActivity(context, searchOut);
    }

    private static IActivity CreateHeroCardActivity(ITurnContext context, LookupValueSetSeachOut searchOut)
        =>
        new HeroCard
        {
            Title = searchOut.ChoiceText,
            Buttons = searchOut.Items.Select(context.CreateSearchItemAction).ToArray()
        }
        .ToAttachment()
        .ToActivity();

    private static CardAction CreateSearchItemAction(this ITurnContext context, LookupValue item)
        =>
        CreateSearchItemAction(
            name: item.Name,
            value: context.BuildCardActionValue(item.Id));

    private static CardAction CreateSearchItemAction(string name, object value)
        =>
        new(ActionTypes.PostBack)
        {
            Title = name,
            Text = name,
            Value = value
        };

    private static JObject CreateTelegramChannelData(ITurnContext context, LookupValueSetSeachOut searchOut)
        =>
        new TelegramChannelData(
            parameters: new(context.EncodeText(searchOut.ChoiceText))
            {
                ReplyMarkup = new TelegramInlineKeyboardMarkup(
                    keyboard: CreateTelegramKeyboard(context, searchOut))
            })
        .ToJObject();

    private static TelegramInlineKeyboardButton[][] CreateTelegramKeyboard(ITurnContext context, LookupValueSetSeachOut searchOut)
    {
        var buttons = searchOut.Items.Select(context.CreateTelegramButton);
        if (searchOut.Direction is LookupValueSetDirection.Horizon)
        {
            return new[] { buttons.ToArray() };
        }
        
        return buttons.Select(CreateRow).ToArray();

        static TelegramInlineKeyboardButton[] CreateRow(TelegramInlineKeyboardButton button)
            =>
            new[] { button };
    }

    private static TelegramInlineKeyboardButton CreateTelegramButton(this ITurnContext context, LookupValue item)
        =>
        new(context.EncodeText(item.Name))
        {
            CallbackData = context.BuildCardActionValue(item.Id)?.ToString()
        };
}