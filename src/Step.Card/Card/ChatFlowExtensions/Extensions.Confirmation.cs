using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

partial class CardChatFlowExtensions
{
    public static ChatFlow<T> AwaitConfirmation<T>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ConfirmationCardOption> optionFactory)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(optionFactory);

        return chatFlow.ForwardValue(GetResultOrRepeatAsync);

        ValueTask<ChatFlowJump<T>> GetResultOrRepeatAsync(IChatFlowContext<T> context, CancellationToken token)
            =>
            context.GetConfirmationResultOrRepeatAsync<T, object>(optionFactory, null, token);
    }

    public static ChatFlow<T> AwaitConfirmation<T, TWebAppDataJson>(
        this ChatFlow<T> chatFlow,
        Func<IChatFlowContext<T>, ConfirmationCardOption> optionFactory,
        Func<IChatFlowContext<T>, TWebAppDataJson, Result<T, BotFlowFailure>> forwardFlowState)
        where TWebAppDataJson : notnull
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(optionFactory);
        ArgumentNullException.ThrowIfNull(forwardFlowState);

        return chatFlow.ForwardValue(GetResultOrRepeatAsync);

        ValueTask<ChatFlowJump<T>> GetResultOrRepeatAsync(IChatFlowContext<T> context, CancellationToken token)
            =>
            context.GetConfirmationResultOrRepeatAsync(optionFactory, forwardFlowState, token);
    }

    private static async ValueTask<ChatFlowJump<T>> GetConfirmationResultOrRepeatAsync<T, TWebAppDataJson>(
        this IChatFlowContext<T> context,
        Func<IChatFlowContext<T>, ConfirmationCardOption> optionFactory,
        Func<IChatFlowContext<T>, TWebAppDataJson, Result<T, BotFlowFailure>>? forwardFlowState,
        CancellationToken cancellationToken)
        where TWebAppDataJson : notnull
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
            return await ToNextAsync(context.FlowState).ConfigureAwait(false);
        }

        if (IsTextEqualTo(option.CancelButtonText))
        {
            return await ToBreakAsync().ConfigureAwait(false);
        }

        if (forwardFlowState is not null && string.IsNullOrWhiteSpace(option.TelegramWebApp?.WebAppUrl) is false)
        {
            return await context.GetWebAppConfirmationResultOrRepeatAsync(forwardFlowState, ToNextAsync, cancellationToken).ConfigureAwait(false);
        }

        return await context.GetCardActionValueOrAbsent().FoldValueAsync(CheckButtonIdAsync, context.RepeatSameStateValueTask).ConfigureAwait(false);

        ValueTask<ChatFlowJump<T>> CheckButtonIdAsync(Guid buttonId)
        {
            if (buttonId == cacheJson.ConfirmButtonGuid)
            {
                return ToNextAsync(context.FlowState);
            }

            if (buttonId == cacheJson.CancelButtonGuid)
            {
                return ToBreakAsync();
            }

            return context.RepeatSameStateValueTask();
        }

        async ValueTask<ChatFlowJump<T>> ToNextAsync(T value)
        {
            if (cacheJson.Resource is not null)
            {
                await UpdateResourceAsync(cacheJson.Resource).ConfigureAwait(false);
            }

            return value;
        }

        async ValueTask<ChatFlowJump<T>> ToBreakAsync()
        {
            if (cacheJson.Resource is null)
            {
                await SendCancellationTextAsync().ConfigureAwait(false);
            }
            else
            {
                await Task.WhenAll(UpdateResourceAsync(cacheJson.Resource), SendCancellationTextAsync()).ConfigureAwait(false);
            }

            return ChatFlowBreakState.From(null);
        }

        Task SendCancellationTextAsync()
        {
            var cancellationActivity = context.CreateCancellationActivity(option.CancelText);
            return context.SendActivityAsync(cancellationActivity, cancellationToken);
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