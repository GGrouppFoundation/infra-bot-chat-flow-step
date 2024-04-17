using System;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public sealed record class ChoiceItem
{
    public ChoiceItem(Guid id, [AllowNull] string title, object? data = null)
    {
        Id = id;
        Title = title.OrEmpty();
        Data = data;
    }

    public Guid Id { get; }

    public string Title { get; }

    public object? Data { get; }
}