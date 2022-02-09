using System;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public readonly record struct SkipValueStepOption
{
    private const string DefaultMessageText = "Введите значение";

    private const string DefaultSkipButtonText = "Пропустить";

    private readonly string? messageText, skipButtonText;

    public SkipValueStepOption(
        [AllowNull] string messageText = DefaultMessageText,
        [AllowNull] string skipButtonText = DefaultSkipButtonText)
    {
        this.messageText = messageText.OrNullIfEmpty();
        this.skipButtonText = skipButtonText.OrNullIfEmpty();
        SkipStep = false;
    }

    public string MessageText => messageText ?? DefaultMessageText;

    public string SkipButtonText => skipButtonText ?? DefaultSkipButtonText;

    public bool SkipStep { get; init; }
}