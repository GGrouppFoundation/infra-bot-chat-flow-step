using System.Linq;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace GGroupp.Infra.Bot.Builder;

partial class LookupActivity
{
    internal static IActivity CreateLookupActivity(this ITurnContext context, LookupValueSetOption option)
    {
        if (context.IsTelegramChannel())
        {
            var telegramReply = MessageFactory.Text(default);
            telegramReply.ChannelData = CreateTelegramChannelData(context, option);

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
                Body = new()
                {
                    new AdaptiveTextBlock
                    {
                        Text = option.ChoiceText,
                        Weight = AdaptiveTextWeight.Bolder,
                        Wrap = true
                    }
                },
                Actions = option.Items.Select(context.CreateAdaptiveSubmitAction).ToList<AdaptiveAction>()
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
            Buttons = option.Items.Select(context.CreateSearchItemAction).ToArray()
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

    private static JObject CreateTelegramChannelData(ITurnContext context, LookupValueSetOption option)
        =>
        new TelegramChannelData(
            parameters: new(context.EncodeText(option.ChoiceText))
            {
                ReplyMarkup = new TelegramInlineKeyboardMarkup(
                    keyboard: CreateTelegramKeyboard(context, option))
            })
        .ToJObject();

    private static TelegramInlineKeyboardButton[][] CreateTelegramKeyboard(ITurnContext context, LookupValueSetOption option)
    {
        var buttons = option.Items.Select(context.CreateTelegramButton);
        if (option.Direction is LookupValueSetDirection.Horizon)
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

    private static AdaptiveSchemaVersion GetAdaptiveSchemaVersion(this ITurnContext turnContext)
        =>
        turnContext.IsMsteamsChannel() ? AdaptiveCard.KnownSchemaVersion : new(1, 0);
}