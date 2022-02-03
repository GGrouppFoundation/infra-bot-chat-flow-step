using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public sealed record class LookupValueSetOption
{
    private const string DefaultChoiceText = "Выберите значение";

    public LookupValueSetOption(
        [AllowNull] IReadOnlyCollection<LookupValue> items,
        [AllowNull] string choiceText = DefaultChoiceText,
        LookupValueSetDirection direction = default,
        bool skipStep = false)
    {
        Items = items ?? Array.Empty<LookupValue>();
        ChoiceText = string.IsNullOrEmpty(choiceText) ? DefaultChoiceText : choiceText;
        Direction = direction;
        SkipStep = skipStep;
    }

    public string ChoiceText { get; }

    public IReadOnlyCollection<LookupValue> Items { get; }

    public LookupValueSetDirection Direction { get; }

    public bool SkipStep { get; }
}