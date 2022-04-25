using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public sealed record class ValueStepOption
{
    private const string DefaultMessageText = "Введите значение";

    private const string DefaultResultText = "Выбрано значение";

    private readonly string? messageText;

    public ValueStepOption(
        [AllowNull] string messageText = DefaultMessageText,
        [AllowNull] IReadOnlyCollection<IReadOnlyCollection<string>> suggestions = default,
        [AllowNull] string resultText = DefaultResultText)
    {
        this.messageText = messageText.OrNullIfEmpty();
        Suggestions = suggestions ?? Array.Empty<IReadOnlyCollection<string>>();
        ResultText = string.IsNullOrEmpty(resultText) ? DefaultResultText : resultText;
    }

    public string MessageText => messageText ?? DefaultMessageText;

    public string ResultText { get; }

    public IReadOnlyCollection<IReadOnlyCollection<string>> Suggestions { get; }

    public bool SkipStep { get; init; }
}