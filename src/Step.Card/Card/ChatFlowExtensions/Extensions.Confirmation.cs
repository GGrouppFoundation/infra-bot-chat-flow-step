using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

partial class CardChatFlowExtensions
{
    public static ChatFlow<T> AwaitConfirmation<T>(this ChatFlow<T> chatFlow, Func<IChatFlowContext<T>, ConfirmationCardOption> optionFactory)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(optionFactory);

        return chatFlow.ForwardValue(GetResultOrRepeatAsync);

        ValueTask<ChatFlowJump<T>> GetResultOrRepeatAsync(IChatFlowContext<T> context, CancellationToken token)
            =>
            context.GetConfirmationResultOrRepeatAsync(optionFactory, token);
    }

    private static async ValueTask<ChatFlowJump<T>> GetConfirmationResultOrRepeatAsync<T>(
        this IChatFlowContext<T> context,
        Func<IChatFlowContext<T>, ConfirmationCardOption> optionFactory,
        CancellationToken cancellationToken)
    {
        var option = optionFactory.Invoke(context);
        if (option.SkipStep)
        {
            return context.FlowState;
        }

        if (context.StepState is not ConfirmationCardCacheJson cacheJson)
        {
            cacheJson = new()
            {
                ConfirmButtonGuid = Guid.NewGuid(),
                CancelButtonGuid = Guid.NewGuid()
            };

            var activity = context.CreateConfirmationActivity(option, cacheJson);
            var resource = await context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);

            if (context.IsMsteamsChannel())
            {
                cacheJson = cacheJson with
                {
                    Resource = resource
                };
            }

            return ChatFlowJump.Repeat<T>(cacheJson);
        }

        if (IsTextEqualTo(option.ConfirmButtonText))
        {
            return await ToNextAsync().ConfigureAwait(false);
        }

        if (IsTextEqualTo(option.CancelButtonText))
        {
            return await ToBreakAsync().ConfigureAwait(false);
        }

        return await context.GetCardActionValueOrAbsent().FoldValueAsync(CheckButtonIdAsync, ToRepeatAsync).ConfigureAwait(false);

        ValueTask<ChatFlowJump<T>> CheckButtonIdAsync(Guid buttonId)
        {
            if (buttonId == cacheJson.ConfirmButtonGuid)
            {
                return ToNextAsync();
            }

            if (buttonId == cacheJson.CancelButtonGuid)
            {
                return ToBreakAsync();
            }

            return ToRepeatAsync();
        }

        async ValueTask<ChatFlowJump<T>> ToNextAsync()
        {
            if (cacheJson.Resource is not null)
            {
                await UpdateResourceAsync(cacheJson.Resource).ConfigureAwait(false);
            }
            return context.FlowState;
        }

        ValueTask<ChatFlowJump<T>> ToRepeatAsync()
            =>
            new(context.RepeatSameStateJump<T>());

        async ValueTask<ChatFlowJump<T>> ToBreakAsync()
        {
            if (cacheJson.Resource is null)
            {
                await SendCancelTextAsync().ConfigureAwait(false);
            }
            else
            {
                await Task.WhenAll(UpdateResourceAsync(cacheJson.Resource), SendCancelTextAsync()).ConfigureAwait(false);
            }

            return ChatFlowBreakState.From(null);
        }

        Task SendCancelTextAsync()
        {
            var cancelActivity = context.CreateCancelActivity(option.CancelText);
            return context.SendActivityAsync(cancelActivity, cancellationToken);
        }

        Task UpdateResourceAsync(ResourceResponse resource)
        {
            var activity = context.CreateConfirmationActivity(option, cacheJson, false);
            activity.Id = resource.Id;
            return context.UpdateActivityAsync(activity, cancellationToken);
        }

        bool IsTextEqualTo(string buttonText)
            =>
            string.Equals(context?.Activity?.Text, buttonText, StringComparison.InvariantCultureIgnoreCase);
    }
}