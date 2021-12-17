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
            Title = searchOut.ChoiceText.ToEncodedActivityText(),
            Buttons = searchOut.Items.Select(context.Activity.CreateSearchItemAction).ToArray()
        }
        .ToAttachment()
        .ToActivity();

    private static CardAction CreateSearchItemAction(this Activity activity, LookupValue item)
        =>
        new(ActionTypes.PostBack)
        {
            Title = item.Name,
            Text = item.Name,
            Value = activity.BuildCardActionValue(item.Id)
        };
}