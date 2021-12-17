using System;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public readonly record struct LookupValueSetSeachIn
{
    private readonly string? text;

    public LookupValueSetSeachIn([AllowNull] string text)
        =>
        this.text = text.OrNullIfEmpty();

    public string Text => text.OrEmpty();
}