namespace GGroupp.Infra.Bot.Builder;

public readonly record struct ValueStepOption
{
    public ValueStepOption(bool skipStep = false)
        =>
        SkipStep = skipStep;

    public bool SkipStep { get; }
}