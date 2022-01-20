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
            Buttons = searchOut.Items.Select(context.Activity.CreateSearchItemAction).ToArray()
        }
        .ToAttachment()
        .ToActivity();

    private static CardAction CreateSearchItemAction(this Activity activity, LookupValue item)
        =>
        CreateSearchItemAction(
            name: item.Name.ToEncodedActivityText(),
            value: activity.BuildCardActionValue(item.Id));

    private static CardAction CreateSearchItemAction(string name, object value)
        =>
        new(ActionTypes.PostBack)
        {
            Title = name,
            Text = name,
            Value = value
        };
}