using System;

namespace GarageGroup.Infra.Bot.Builder;

public sealed record class ChoiceNextButton
{
    public ChoiceNextButton(string title, string nextToken)
    {
        Title = title.OrEmpty();
        NextToken = nextToken.OrEmpty();
    }

    public string Title { get; }

    public string NextToken { get; }
}