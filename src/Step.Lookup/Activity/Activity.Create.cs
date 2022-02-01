using System.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

partial class LookupActivity
{
    internal static IActivity CreateLookupActivity(this ITurnContext context, LookupValueSetSeachOut searchOut)
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
            name: context.EncodeText(item.Name),
            value: context.BuildCardActionValue(item.Id));

    private static CardAction CreateSearchItemAction(string name, object value)
        =>
        new(ActionTypes.PostBack)
        {
            Title = name,
            Text = name,
            Value = value
        };
}