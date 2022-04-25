using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public sealed record class LookupValueSetOption
{
    private const string DefaultChoiceText = "Выберите значение";

    private const string DefaultResultText = "Выбрано значение";

    public LookupValueSetOption(
        [AllowNull] IReadOnlyCollection<LookupValue> items,
        [AllowNull] string choiceText = DefaultChoiceText,
        LookupValueSetDirection direction = default,
        [AllowNull] string resultText = DefaultResultText)
    {
        Items = items ?? Array.Empty<LookupValue>();
        ChoiceText = string.IsNullOrEmpty(choiceText) ? DefaultChoiceText : choiceText;
        Direction = direction;
        ResultText = string.IsNullOrEmpty(resultText) ? DefaultResultText : resultText;
    }

    public string ChoiceText { get; }

    public IReadOnlyCollection<LookupValue> Items { get; }

    public LookupValueSetDirection Direction { get; }

    public string ResultText { get; }

    public bool SkipStep { get; init; }
}