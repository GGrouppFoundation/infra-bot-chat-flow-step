using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public sealed record class LookupValue
{
    public LookupValue(Guid id, [AllowNull] string name, [AllowNull] IReadOnlyCollection<KeyValuePair<string, string>> extensions = default)
    {
        Id = id;
        Name = name.OrEmpty();
        Extensions = extensions ?? Array.Empty<KeyValuePair<string, string>>();
    }

    public Guid Id { get; }

    public string Name { get; }

    public IReadOnlyCollection<KeyValuePair<string, string>> Extensions { get; }
}