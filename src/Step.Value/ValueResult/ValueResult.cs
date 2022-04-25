using System;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

internal readonly record struct ValueResult
{
    private readonly string? text;

    public ValueResult([AllowNull] string text, bool fromSuggestion)
    {
        this.text = text.OrNullIfEmpty();
        FromSuggestion = fromSuggestion;
    }

    public string Text => text.OrEmpty();

    public bool FromSuggestion { get; }
}