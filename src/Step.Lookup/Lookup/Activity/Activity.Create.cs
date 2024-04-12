using System.Linq;
using System.Web;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

partial class LookupActivity
{
    internal static IActivity CreateLookupActivity(this ITurnContext context, LookupValueSetOption option)
    {
        if (context.IsTelegramChannel())
        {
            var telegramReply = MessageFactory.Text(default);
            telegramReply.ChannelData = CreateTelegramChannelData(context, option).ToJObject();

            return telegramReply;
        }

        if (context.IsCardSupported())
        {
            return CreateAdaptiveCardActivity(context, option);
        }

        return CreateHeroCardActivity(context, option);
    }

    private static IActivity CreateAdaptiveCardActivity(ITurnContext context, LookupValueSetOption option)
        =>
        new Attachment
        {
            ContentType = AdaptiveCard.ContentType,
            Content = new AdaptiveCard(context.GetAdaptiveSchemaVersion())
            {
                Body =
                [
                    new AdaptiveTextBlock
                    {
                        Text = option.ChoiceText,
                        Weight = AdaptiveTextWeight.Bolder,
                        Wrap = true
                    }
                ],
                Actions = option.Items.AsEnumerable().Select(context.CreateAdaptiveSubmitAction).ToList<AdaptiveAction>()
            }
        }
        .ToActivity();

    private static AdaptiveSubmitAction CreateAdaptiveSubmitAction(this ITurnContext context, LookupValue item)
        =>
        new()
        {
            Title = item.Name,
            Data = context.BuildCardActionValue(item.Id)
        };

    private static IActivity CreateHeroCardActivity(ITurnContext context, LookupValueSetOption option)
        =>
        new HeroCard
        {
            Title = option.ChoiceText,
            Buttons = option.Items.AsEnumerable().Select(context.CreateSearchItemAction).ToArray()
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

    private static TelegramChannelData CreateTelegramChannelData(ITurnContext context, LookupValueSetOption option)
        =>
        new(
            parameters: new(HttpUtility.HtmlEncode(option.ChoiceText))
            {
                ParseMode = TelegramParseMode.Html,
                ReplyMarkup = new TelegramInlineKeyboardMarkup(
                    keyboard: CreateTelegramKeyboard(context, option))
            });

    private static TelegramInlineKeyboardButton[][] CreateTelegramKeyboard(ITurnContext context, LookupValueSetOption option)
    {
        var buttons = option.Items.AsEnumerable().Select(context.CreateTelegramButton);
        if (option.Direction is LookupValueSetDirection.Horizon)
        {
            return [buttons.ToArray()];
        }

        return buttons.Select(CreateRow).ToArray();

        static TelegramInlineKeyboardButton[] CreateRow(TelegramInlineKeyboardButton button)
            =>
            [button];
    }

    private static TelegramInlineKeyboardButton CreateTelegramButton(this ITurnContext context, LookupValue item)
        =>
        new(item.Name)
        {
            CallbackData = context.BuildCardActionValue(item.Id)?.ToString()
        };

    private static AdaptiveSchemaVersion GetAdaptiveSchemaVersion(this ITurnContext turnContext)
        =>
        turnContext.IsMsteamsChannel() ? AdaptiveCard.KnownSchemaVersion : new(1, 0);
}