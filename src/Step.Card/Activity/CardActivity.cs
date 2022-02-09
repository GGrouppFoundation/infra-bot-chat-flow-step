using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdaptiveCards;
using Microsoft.Bot.Builder;

namespace GGroupp.Infra.Bot.Builder;

internal static partial class CardActivity
{
    private static List<AdaptiveElement> AddElements(this List<AdaptiveElement> body, IEnumerable<AdaptiveElement> elements)
    {
        body.AddRange(elements);
        return body;
    }

    private static string BuildTelegramText(this ITurnContext context, ConfirmationCardOption option)
        =>
        new StringBuilder(
            $"**{context.EncodeText(option.QuestionText)}**")
        .AppendLineBreak()
        .Append(
            context.BuildFieldsText(option.FieldValues))
        .ToString();

    private static string? BuildFieldsText(this ITurnContext context, IEnumerable<KeyValuePair<string, string?>> fields)
    {
        if (fields.Any() is false)
        {
            return null;
        }

        var isTelegram = context.IsTelegramChannel();
        var builder = new StringBuilder();

        foreach (var field in fields.Where(NotEmptyField).Select(Encode))
        {
            var fieldName = field.Key;
            var fieldValue = field.Value;

            if (builder.Length > 0)
            {
                builder = builder.AppendLineBreak();
            }

            if (string.IsNullOrEmpty(fieldName) is false)
            {
                if (isTelegram)
                {
                    builder = builder.Append("**").Append(fieldName).Append(':').Append("**");
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

        return builder.ToString();

        KeyValuePair<string, string?> Encode(KeyValuePair<string, string?> field)
            =>
            new(
                key: context.EncodeText(field.Key),
                value: context.EncodeText(field.Value));
    }

    private static StringBuilder AppendLineBreak(this StringBuilder stringBuilder)
        =>
        stringBuilder.Append("\n\r\n\r");

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