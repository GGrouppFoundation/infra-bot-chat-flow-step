using System;
using System.Threading;
using System.Threading.Tasks;

namespace GarageGroup.Infra.Bot.Builder;

partial class CardChatFlowExtensions
{
    public static ChatFlow<T> ShowEntityCard<T>(this ChatFlow<T> chatFlow, Func<IChatFlowContext<T>, EntityCardOption> optionFactory)
    {
        ArgumentNullException.ThrowIfNull(chatFlow);
        ArgumentNullException.ThrowIfNull(optionFactory);

        return chatFlow.On(InnerShowEntityCardAsync);

        Task InnerShowEntityCardAsync(IChatFlowContext<T> context, CancellationToken cancellationToken)
        {
            var option = optionFactory.Invoke(context);
            if (option.SkipStep)
            {
                return Task.CompletedTask;
            }

            var activity = context.CreateCardActivity(option, null);
            return context.SendActivityAsync(activity, cancellationToken);
        }
    }
}