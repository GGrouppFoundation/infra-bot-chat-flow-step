using System;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public readonly record struct AwaitDateOption
{
    private const string DefaultText = "Введите дату";

    private const string DateFormatText = "dd.MM.yyyy";

    private const string DefaultConfirmButtonText = "Выбрать";

    private readonly string? text, dateFormat, confirmButtonText;

    public AwaitDateOption(
        [AllowNull] string text = DefaultText,
        [AllowNull] string dateFormat = DateFormatText,
        [AllowNull] string confirmButtonText = DefaultConfirmButtonText,
        string? invalidDateText = null,
        DateOnly? defaultDate = null)
    {
        this.text = string.IsNullOrEmpty(text) ? default : text;
        this.dateFormat = string.IsNullOrEmpty(dateFormat) ? default : dateFormat;
        this.confirmButtonText = string.IsNullOrEmpty(confirmButtonText) ? default : confirmButtonText;
        InvalidDateText = invalidDateText;
        DefaultDate = defaultDate;
    }

    public string Text => text ?? DefaultText;

    public string DateFormat => dateFormat ?? DateFormatText;

    public string ConfirmButtonText => confirmButtonText ?? DefaultConfirmButtonText;

    public string? InvalidDateText { get; }

    public DateOnly? DefaultDate { get; }
}