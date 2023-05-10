using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public sealed record class ValueStepOption<TValue>
{
    private const string DefaultMessageText = "Введите значение";

    private readonly string? messageText;

    public ValueStepOption(
        [AllowNull] string messageText = DefaultMessageText,
        [AllowNull] IReadOnlyCollection<IReadOnlyCollection<KeyValuePair<string, TValue>>> suggestions = default)
    {
        this.messageText = messageText.OrNullIfEmpty();
        Suggestions = suggestions ?? Array.Empty<IReadOnlyCollection<KeyValuePair<string, TValue>>>();
    }

    public string MessageText => messageText ?? DefaultMessageText;

    public IReadOnlyCollection<IReadOnlyCollection<KeyValuePair<string, TValue>>> Suggestions { get; }

    public bool SkipStep { get; init; }
}