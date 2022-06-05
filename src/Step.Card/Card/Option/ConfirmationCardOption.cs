using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public sealed record class ConfirmationCardOption
{
    private const string DefaultQuestionText = "Подтвердить операцию?";

    private const string DefaultConfirmButtonText = "Подтвердить";

    private const string DefaultCancelButtonText = "Отменить";

    private const string DefaultCancelText = "Операция отменена";

    public ConfirmationCardOption(
        [AllowNull] string questionText = DefaultQuestionText,
        [AllowNull] string confirmButtonText = DefaultConfirmButtonText,
        [AllowNull] string cancelButtonText = DefaultCancelButtonText,
        [AllowNull] string cancelText = DefaultCancelText,
        [AllowNull] IReadOnlyCollection<KeyValuePair<string, string?>> fieldValues = default)
    {
        QuestionText = string.IsNullOrEmpty(questionText) ? DefaultQuestionText : questionText;
        ConfirmButtonText = string.IsNullOrEmpty(confirmButtonText) ? DefaultConfirmButtonText : confirmButtonText;
        CancelButtonText = string.IsNullOrEmpty(cancelButtonText) ? DefaultCancelButtonText : cancelButtonText;
        CancelText = string.IsNullOrEmpty(cancelText) ? DefaultCancelText : cancelText;
        FieldValues = fieldValues ?? Array.Empty<KeyValuePair<string, string?>>();
    }

    public string QuestionText { get; }

    public string ConfirmButtonText { get; }

    public string CancelButtonText { get; }

    public string CancelText { get; }

    public IReadOnlyCollection<KeyValuePair<string, string?>> FieldValues { get; }

    public bool SkipStep { get; init; }
}