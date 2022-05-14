using System;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public readonly record struct DateStepOption
{
    private const string DefaultText = "Введите дату";

    private const string DateFormatText = "dd.MM.yyyy";

    private const string DefaultConfirmButtonText = "Выбрать";

    private const string DefaultResultText = "Выбрано значение";

    private readonly string? text, dateFormat, confirmButtonText, resultText;

    public DateStepOption(
        [AllowNull] string text = DefaultText,
        [AllowNull] string dateFormat = DateFormatText,
        [AllowNull] string confirmButtonText = DefaultConfirmButtonText,
        [AllowNull] string resultText = DefaultResultText,
        string? invalidDateText = null,
        DateOnly? defaultDate = null)
    {
        this.text = text.OrNullIfEmpty();
        this.dateFormat = dateFormat.OrNullIfEmpty();
        this.confirmButtonText = confirmButtonText.OrNullIfEmpty();
        this.resultText = resultText.OrNullIfEmpty();
        InvalidDateText = invalidDateText;
        DefaultDate = defaultDate;
        SkipStep = false;
    }

    public string Text => text ?? DefaultText;

    public string DateFormat => dateFormat ?? DateFormatText;

    public string ConfirmButtonText => confirmButtonText ?? DefaultConfirmButtonText;

    public string ResultText => resultText ?? DefaultResultText;

    public string? InvalidDateText { get; }

    public DateOnly? DefaultDate { get; }

    public bool SkipStep { get; init; }
}