using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public readonly record struct DateStepOption
{
    private const string DefaultText = "Enter the date";

    private const string DefaultConfirmButtonText = "Choose";

    private readonly string? text, confirmButtonText;

    private readonly IReadOnlyCollection<IReadOnlyCollection<KeyValuePair<string, DateOnly>>>? suggestions;

    public DateStepOption(
        [AllowNull] string text = DefaultText,
        [AllowNull] string confirmButtonText = DefaultConfirmButtonText,
        [AllowNull] string invalidDateText = null,
        DateOnly? defaultDate = null,
        [AllowNull] string placeholder = null,
        [AllowNull] IReadOnlyCollection<IReadOnlyCollection<KeyValuePair<string, DateOnly>>> suggestions = null)
    {
        this.text = text.OrNullIfEmpty();
        this.confirmButtonText = confirmButtonText.OrNullIfEmpty();
        InvalidDateText = invalidDateText.OrNullIfEmpty();
        DefaultDate = defaultDate;
        Placeholder = placeholder.OrNullIfEmpty();
        this.suggestions = suggestions?.Count is not > 0 ? null : suggestions;
        SkipStep = false;
    }

    public string Text
        =>
        text ?? DefaultText;

    public string ConfirmButtonText
        =>
        confirmButtonText ?? DefaultConfirmButtonText;

    public string? InvalidDateText { get; }

    public DateOnly? DefaultDate { get; }

    public string? Placeholder { get; }

    public IReadOnlyCollection<IReadOnlyCollection<KeyValuePair<string, DateOnly>>> Suggestions
        =>
        suggestions ?? [];

    public bool SkipStep { get; init; }
}