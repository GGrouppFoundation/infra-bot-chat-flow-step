using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public sealed record class LookupValueSetSeachOut
{
    private const string DefaultChoiceText = "Выберите значение";

    public LookupValueSetSeachOut(
        [AllowNull] IReadOnlyCollection<LookupValue> items,
        [AllowNull] string choiceText = DefaultChoiceText)
    {
        Items = items ?? Array.Empty<LookupValue>();
        ChoiceText = string.IsNullOrEmpty(choiceText) ? DefaultChoiceText : choiceText;
    }

    public string ChoiceText { get; }

    public IReadOnlyCollection<LookupValue> Items { get; }
}