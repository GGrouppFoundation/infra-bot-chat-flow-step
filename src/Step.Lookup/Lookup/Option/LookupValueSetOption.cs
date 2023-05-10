using System;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public sealed record class LookupValueSetOption
{
    private const string DefaultChoiceText = "Выберите значение";

    public LookupValueSetOption(
        FlatArray<LookupValue> items,
        [AllowNull] string choiceText = DefaultChoiceText,
        LookupValueSetDirection direction = default)
    {
        Items = items;
        ChoiceText = string.IsNullOrEmpty(choiceText) ? DefaultChoiceText : choiceText;
        Direction = direction;
    }

    public string ChoiceText { get; }

    public FlatArray<LookupValue> Items { get; }

    public LookupValueSetDirection Direction { get; }

    public bool SkipStep { get; init; }
}