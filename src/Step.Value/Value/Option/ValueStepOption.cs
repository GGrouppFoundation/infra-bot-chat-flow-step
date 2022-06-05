using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public sealed record class ValueStepOption
{
    private const string DefaultMessageText = "Введите значение";

    private readonly string? messageText;

    public ValueStepOption(
        [AllowNull] string messageText = DefaultMessageText,
        [AllowNull] IReadOnlyCollection<IReadOnlyCollection<string>> suggestions = default)
    {
        this.messageText = messageText.OrNullIfEmpty();
        Suggestions = suggestions ?? Array.Empty<IReadOnlyCollection<string>>();
    }

    public string MessageText => messageText ?? DefaultMessageText;

    public IReadOnlyCollection<IReadOnlyCollection<string>> Suggestions { get; }

    public bool SkipStep { get; init; }
}