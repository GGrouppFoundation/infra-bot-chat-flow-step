using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder;

namespace GGroupp.Infra.Bot.Builder;

partial class SuggestionsActivity
{
    internal static Optional<ValueResult> GetTextValueOrAbsent(this ITurnContext context, KeyValuePair<Guid, string>[][]? suggestions)
    {
        return context.IsMessageType() ? context.GetCardActionValueOrAbsent().Fold(FromAction, FromText) : default;

        Optional<ValueResult> FromAction(Guid actionGuid)
            =>
            suggestions?.SelectMany(Pipeline.Pipe).GetValueOrAbsent(actionGuid).Map(FromSuggestion) ?? default;

        Optional<ValueResult> FromText()
        {
            var valueResult = new ValueResult(context.Activity.Text, false);
            return Optional.Present(valueResult);
        }

        static ValueResult FromSuggestion(string text)
            =>
            new(text, true);
    }
}