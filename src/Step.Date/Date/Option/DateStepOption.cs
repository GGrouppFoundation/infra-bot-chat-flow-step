using System;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public readonly record struct DateStepOption
{
    private const string DefaultText = "Введите дату";

    private const string DefaultConfirmButtonText = "Выбрать";

    private readonly string? text, confirmButtonText;

    public DateStepOption(
        [AllowNull] string text = DefaultText,
        [AllowNull] string confirmButtonText = DefaultConfirmButtonText,
        string? invalidDateText = null,
        DateOnly? defaultDate = null)
    {
        this.text = text.OrNullIfEmpty();
        this.confirmButtonText = confirmButtonText.OrNullIfEmpty();
        InvalidDateText = invalidDateText;
        DefaultDate = defaultDate;
        SkipStep = false;
    }

    public string Text => text ?? DefaultText;

    public string ConfirmButtonText => confirmButtonText ?? DefaultConfirmButtonText;

    public string? InvalidDateText { get; }

    public DateOnly? DefaultDate { get; }

    public bool SkipStep { get; init; }
}