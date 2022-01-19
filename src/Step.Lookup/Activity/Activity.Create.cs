using System.Linq;
using System.Text;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

partial class LookupActivity
{
    internal static IActivity CreateLookupActivity(this ITurnContext context, LookupValueSetSeachOut searchOut)
        =>
        new HeroCard
        {
            Title = context.Activity.IsTelegram() ? BuildTitleForTelegram(searchOut) : searchOut.ChoiceText,
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

    private static string BuildTitleForTelegram(LookupValueSetSeachOut searchOut)
    {
        if (searchOut.Items.Any() is false)
        {
            return searchOut.ChoiceText;
        }

        var titleBuilder = new StringBuilder($"{searchOut.ChoiceText}:").AppendTelegramLine();
        foreach (var name in searchOut.Items.Select(BuildNameForTelegram))
        {
            titleBuilder = titleBuilder.AppendTelegramLine().Append(name);
        }
        return titleBuilder.ToString();
    }

    private static string BuildNameForTelegram(LookupValue item, int index)
        =>
        $"{index + 1} {item.Name.ToEncodedActivityText()}";

    private static StringBuilder AppendTelegramLine(this StringBuilder stringBuilder)
        =>
        stringBuilder.Append("\n\r");
}