using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GarageGroup.Infra.Bot.Builder;

public sealed record class EntityCardOption
{
    private const string DefaultHeaderText = "Operation data";

    public EntityCardOption(
        [AllowNull] string headerText = DefaultHeaderText,
        FlatArray<KeyValuePair<string, string?>> fieldValues = default)
    {
        HeaderText = headerText.OrNullIfWhiteSpace() ?? DefaultHeaderText;
        FieldValues = fieldValues;
    }

    public string HeaderText { get; }

    public FlatArray<KeyValuePair<string, string?>> FieldValues { get; }

    public bool SkipStep { get; init; }
}