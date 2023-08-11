namespace GarageGroup.Infra.Bot.Builder;

public sealed record class ChoiceStepOption
{
    public static ChoiceStepOption Skip()
        =>
        new(true);

    private ChoiceStepOption(bool skipStep)
        =>
        SkipStep = skipStep;

    public bool SkipStep { get; init; }
}