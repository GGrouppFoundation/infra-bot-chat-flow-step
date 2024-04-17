namespace GarageGroup.Infra.Bot.Builder;

public readonly record struct ChoiceSetRequest
{
    public string? Text { get; init; }

    public string? NextToken { get; init; }
}