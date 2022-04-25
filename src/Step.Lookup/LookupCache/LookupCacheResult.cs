using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

internal sealed record class LookupCacheResult
{
    public LookupCacheResult(
        [AllowNull] string resultText,
        [AllowNull] IReadOnlyCollection<ResourceResponse> resources,
        LookupValue value)
    {
        ResultText = resultText ?? string.Empty;
        Resources = resources ?? Array.Empty<ResourceResponse>();
        Value = value;
    }

    public string ResultText { get; }

    public IReadOnlyCollection<ResourceResponse> Resources { get; }

    public LookupValue Value { get; }
}