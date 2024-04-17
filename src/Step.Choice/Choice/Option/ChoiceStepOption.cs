using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

public sealed record class ChoiceStepOption
{
    public static ChoiceStepOption Skip()
        =>
        new(true);

    private ChoiceStepOption(bool skipStep)
    {
        ChoiceSetFactory = GetDefaultChoiceSetValueTask;
        SkipStep = skipStep;
    }

    public ChoiceStepOption(ChoiceSetOption choiceSet)
    {
        ChoiceSetFactory = InnerGetChoiceSeyValueTask;

        ValueTask<Result<ChoiceSetOption, BotFlowFailure>> InnerGetChoiceSeyValueTask(
            ChoiceSetRequest _, CancellationToken token)
            =>
            new(choiceSet);
    }

    public ChoiceStepOption(Func<ChoiceSetRequest, CancellationToken, ValueTask<Result<ChoiceSetOption, BotFlowFailure>>> choiceSetFactory)
        =>
        ChoiceSetFactory = choiceSetFactory;

    public Func<ChoiceSetRequest, CancellationToken, ValueTask<Result<ChoiceSetOption, BotFlowFailure>>> ChoiceSetFactory { get; }

    public Func<ChoiceItem, IActivity>? ResultActivityFactory { get; init; }

    public bool SkipStep { get; init; }

    private static ValueTask<Result<ChoiceSetOption, BotFlowFailure>> GetDefaultChoiceSetValueTask(ChoiceSetRequest _, CancellationToken token)
        =>
        ValueTask.FromResult<Result<ChoiceSetOption, BotFlowFailure>>(new ChoiceSetOption());
}