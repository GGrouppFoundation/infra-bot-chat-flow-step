using System;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public sealed record class CardButtonsOption
{
    private const string DefaultConfirmButtonText = "Подтвердить";

    private const string DefaultCancelButtonText = "Отменить";

    private const string DefaultCancelText = "Операция отменена";

    public CardButtonsOption(
        [AllowNull] string confirmButtonText = DefaultConfirmButtonText,
        [AllowNull] string cancelButtonText = DefaultCancelButtonText,
        [AllowNull] string cancelText = DefaultCancelText)
    {
        ConfirmButtonText = confirmButtonText.OrNullIfWhiteSpace() ?? DefaultConfirmButtonText;
        CancelButtonText = cancelButtonText.OrNullIfWhiteSpace() ?? DefaultCancelButtonText;
        CancelText = cancelText.OrNullIfWhiteSpace() ?? DefaultCancelText;
    }

    public string ConfirmButtonText { get; }

    public string CancelButtonText { get; }

    public string CancelText { get; }

    public CardTelegramWebAppOption? TelegramWebApp { get; init; }
}