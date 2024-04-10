using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using AdaptiveCards;

namespace GarageGroup.Infra.Bot.Builder;

internal static partial class CardActivity
{
    private static List<AdaptiveElement> AddElements(this List<AdaptiveElement> body, IEnumerable<AdaptiveElement> elements)
    {
        body.AddRange(elements);
        return body;
    }

    private static string BuildTelegramText(this ConfirmationCardOption option)
        =>
        new StringBuilder(
            $"<b>{HttpUtility.HtmlEncode(option.QuestionText)}</b>")
        .AppendFields(
            option.FieldValues.AsEnumerable(), true)
        .ToString();

    private static StringBuilder AppendFields(this StringBuilder builder, IEnumerable<KeyValuePair<string, string?>> fields, bool isTelegram)
    {
        if (fields.Where(NotEmptyField).Any() is false)
        {
            return builder;
        }

        if (builder.Length > 0)
        {
            builder = builder.AppendLineBreak();
        }

        foreach (var field in fields.Where(NotEmptyField))
        {
            var fieldName = isTelegram ? HttpUtility.HtmlEncode(field.Key) : field.Key;
            var fieldValue = isTelegram ? HttpUtility.HtmlEncode(field.Value) : field.Value;

            if (builder.Length > 0)
            {
                builder = builder.AppendLineBreak();
            }

            if (string.IsNullOrEmpty(fieldName) is false)
            {
                if (isTelegram)
                {
                    builder = builder.Append("<b>").Append(fieldName).Append(':').Append("</b>");
                }
                else
                {
                    builder = builder.Append(fieldName).Append(':');
                }

                if (string.IsNullOrEmpty(fieldValue) is false)
                {
                    builder = builder.Append(' ');
                }
            }

            if (string.IsNullOrEmpty(fieldValue) is false)
            {
                builder = builder.Append(fieldValue);
            }
        }

        return builder;
    }

    private static StringBuilder AppendLineBreak(this StringBuilder stringBuilder)
        =>
        stringBuilder.Append("\n\r");

    private static bool NotEmptyField(KeyValuePair<string, string?> field)
        =>
        string.IsNullOrEmpty(field.Key) is false || string.IsNullOrEmpty(field.Value) is false;

    private static bool NotEmptyFieldName(KeyValuePair<string, string?> field)
        =>
        string.IsNullOrEmpty(field.Key) is false;

    private static bool EmptyFieldName(KeyValuePair<string, string?> field)
        =>
        string.IsNullOrEmpty(field.Key);
}