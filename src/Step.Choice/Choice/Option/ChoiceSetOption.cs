using System;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public sealed record class ChoiceSetOption
{
    private const string DefaultChoiceText = "Select an item";

    public ChoiceSetOption([AllowNull] string choiceText = DefaultChoiceText)
        =>
        ChoiceText = choiceText.OrNullIfWhiteSpace() ?? DefaultChoiceText;

    public string ChoiceText { get; }

    public FlatArray<ChoiceItem> Items { get; init; }

    public ChoiceNextButton? NextButton { get; init; }
}