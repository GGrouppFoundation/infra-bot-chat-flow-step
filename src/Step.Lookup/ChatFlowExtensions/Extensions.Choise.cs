using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class LookupStepChatFlowExtensions
{
    public static ChatFlow<TNext> AwaitChoiceValue<T, TNext>(
        this ChatFlow<T> chatFlow,
        Func<T, LookupValueSetSeachOut> choiceSetFactory,
        Func<T, LookupValue, TNext> mapFlowState)
        =>
        InnerAwaitChoiceValue(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            choiceSetFactory ?? throw new ArgumentNullException(nameof(choiceSetFactory)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<TNext> InnerAwaitChoiceValue<T, TNext>(
        ChatFlow<T> chatFlow,
        Func<T, LookupValueSetSeachOut> choiceSetFactory,
        Func<T, LookupValue, TNext> mapFlowState)
        =>
        chatFlow.ForwardValue(
            (context, token) => context.GetChoosenValueOrRepeatAsync(choiceSetFactory, token),
            mapFlowState);

    private static async ValueTask<ChatFlowJump<LookupValue>> GetChoosenValueOrRepeatAsync<T>(
        this IChatFlowContext<T> context,
        Func<T, LookupValueSetSeachOut> choiceSetFactory,
        CancellationToken token)
    {
        if (context.StepState is null)
        {
            var choiceSet = choiceSetFactory.Invoke(context.FlowState);
            var setActivity = context.CreateLookupActivity(choiceSet);

            _ = await context.SendActivityAsync(setActivity, token).ConfigureAwait(false);
            return context.ToRepeatWithLookupCacheJump(choiceSet);
        }

        return context.GetCardActionValueOrAbsent().FlatMap(context.GetFromLookupCacheOrAbsent).Fold(
            ChatFlowJump.Next,
            context.RepeatSameStateJump<LookupValue>);
    }
}