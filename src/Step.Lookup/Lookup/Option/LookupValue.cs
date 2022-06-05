using System;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public sealed record class LookupValue
{
    public LookupValue(Guid id, [AllowNull] string name)
    {
        Id = id;
        Name = name.OrEmpty();
        Data = default;
    }

    public LookupValue(Guid id, [AllowNull] string name, string? data)
    {
        Id = id;
        Name = name.OrEmpty();
        Data = data;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string? Data { get; }
}