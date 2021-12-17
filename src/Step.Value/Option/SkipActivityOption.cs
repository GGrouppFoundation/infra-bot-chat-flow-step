using System;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public readonly record struct SkipActivityOption
{
    private const string DefaultMessageText = "Введите значение";

    private const string DefaultSkipButtonText = "Пропустить";

    private readonly string? messageText, skipButtonText;

    public SkipActivityOption([AllowNull] string messageText = DefaultMessageText, [AllowNull] string skipButtonText = DefaultSkipButtonText)
    {
        this.messageText = messageText.OrNullIfEmpty();
        this.skipButtonText = skipButtonText.OrNullIfEmpty();
    }

    public string MessageText => messageText ?? DefaultMessageText;

    public string SkipButtonText => skipButtonText ?? DefaultSkipButtonText;
}