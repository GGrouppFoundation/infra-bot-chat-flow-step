using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder;

namespace GGroupp.Infra.Bot.Builder;

partial class SuggestionsActivity
{
    internal static Optional<string> GetTextValueOrAbsent(this ITurnContext context, KeyValuePair<Guid, string>[][]? suggestions)
    {
        return context.IsMessageType() ? context.GetCardActionValueOrAbsent().Fold(FromAction, FromText) : default;

        Optional<string> FromAction(Guid actionGuid)
            =>
            suggestions?.SelectMany(Pipeline.Pipe).GetValueOrAbsent(actionGuid) ?? default;

        Optional<string> FromText()
            =>
            new(context.Activity.Text.OrEmpty());
    }
}