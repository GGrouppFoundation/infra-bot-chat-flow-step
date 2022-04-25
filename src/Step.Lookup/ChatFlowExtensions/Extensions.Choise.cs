using System;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class LookupStepChatFlowExtensions
{
    public static ChatFlow<T> AwaitChoiceValue<T>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, LookupValueSetOption> choiceSetFactory,
        Func<T, LookupValue, T> mapFlowState)
        =>
        InnerAwaitChoiceValue(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            choiceSetFactory ?? throw new ArgumentNullException(nameof(choiceSetFactory)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<T> InnerAwaitChoiceValue<T>(
        ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, LookupValueSetOption> choiceSetFactory,
        Func<T, LookupValue, T> mapFlowState)
        =>
        chatFlow.ForwardValue(
            (context, token) => context.GetChoosenValueOrRepeatAsync(choiceSetFactory, mapFlowState, token));

    private static ValueTask<ChatFlowJump<T>> GetChoosenValueOrRepeatAsync<T>(
        this IChatFlowContext<T> context,
        Func<IChatFlowContext<T>, LookupValueSetOption> choiceSetFactory,
        Func<T, LookupValue, T> mapFlowState,
        CancellationToken token)
    {
        if (context.StepState is null)
        {
            return RepeatWithCacheOrSkipAsync();
        }

        return context.GetCardActionValueOrAbsent().FlatMap(context.GetFromLookupCacheOrAbsent).FoldValueAsync(NextAsync, RepeatAsync);

        async ValueTask<ChatFlowJump<T>> RepeatWithCacheOrSkipAsync()
        {
            var option = choiceSetFactory.Invoke(context);
            if (option.SkipStep)
            {
                return context.FlowState;
            }

            var setActivity = context.CreateLookupActivity(option);

            var resource = await context.SendActivityAsync(setActivity, token).ConfigureAwait(false);
            return context.ToRepeatWithLookupCacheJump<T>(resource, option);
        }

        ValueTask<ChatFlowJump<T>> RepeatAsync()
            =>
            new(context.RepeatSameStateJump<T>());

        async ValueTask<ChatFlowJump<T>> NextAsync(LookupCacheResult cacheResult)
        {
            await context.SendResultActivityAsync(cacheResult, token).ConfigureAwait(false);
            return mapFlowState.Invoke(context.FlowState, cacheResult.Value);
        }
    }
}