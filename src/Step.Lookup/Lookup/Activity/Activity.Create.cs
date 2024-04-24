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
            return CreateTelegramParameters(context, option).BuildActivity();
        }

        if (context.IsCardSupported())
        {
            return CreateAdaptiveCard(context, option).ToActivity();
        }

        return CreateHeroCardActivity(context, option).ToAttachment().ToActivity();
    }

    private static Attachment CreateAdaptiveCard(ITurnContext context, LookupValueSetOption option)
    {
        return new()
        {
            ContentType = AdaptiveCard.ContentType,
            Content = new AdaptiveCard(context.IsMsteamsChannel() ? AdaptiveCard.KnownSchemaVersion : new(1, 0))
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
                Actions = option.Items.AsEnumerable().Select(CreateAdaptiveSubmitAction).ToList<AdaptiveAction>()
            }
        };

        AdaptiveSubmitAction CreateAdaptiveSubmitAction(LookupValue item)
            =>
            new()
            {
                Title = item.Name,
                Data = context.BuildCardActionValue(item.Id)
            };
    }

    private static HeroCard CreateHeroCardActivity(ITurnContext context, LookupValueSetOption option)
    {
        return new()
        {
            Title = option.ChoiceText,
            Buttons = option.Items.AsEnumerable().Select(CreateSearchItemAction).ToArray()
        };

        CardAction CreateSearchItemAction(LookupValue item)
            =>
            new(ActionTypes.PostBack)
            {
                Title = item.Name,
                Text = item.Name,
                Value = context.BuildCardActionValue(item.Id)
            };
    }

    private static TelegramParameters CreateTelegramParameters(ITurnContext context, LookupValueSetOption option)
    {
        return new(HttpUtility.HtmlEncode(option.ChoiceText))
        {
            ParseMode = TelegramParseMode.Html,
            ReplyMarkup = new TelegramInlineKeyboardMarkup(
                keyboard: CreateTelegramKeyboard(option))
        };

        TelegramInlineKeyboardButton[][] CreateTelegramKeyboard(LookupValueSetOption option)
        {
            var buttons = option.Items.AsEnumerable().Select(CreateTelegramButton);
            if (option.Direction is LookupValueSetDirection.Horizon)
            {
                return [buttons.ToArray()];
            }

            return buttons.Select(CreateRow).ToArray();

            static TelegramInlineKeyboardButton[] CreateRow(TelegramInlineKeyboardButton button)
                =>
                [button];
        }

        TelegramInlineKeyboardButton CreateTelegramButton(LookupValue item)
            =>
            new(item.Name)
            {
                CallbackData = context.BuildCardActionValue(item.Id)?.ToString()
            };
    }
}