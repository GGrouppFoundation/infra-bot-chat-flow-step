using System;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public sealed record class CardButtonsOption
{
    private const string DefaultConfirmButtonText = "Confirm";

    private const string DefaultCancelButtonText = "Cancel";

    private const string DefaultCancelText = "Operation was canceled";

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