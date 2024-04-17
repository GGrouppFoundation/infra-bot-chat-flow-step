using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GarageGroup.Infra.Bot.Builder;

public static partial class LookupStepChatFlowExtensions
{
    private static Task SendResultActivityAsync(
        this ITurnContext turnContext,
        string resultMessage,
        IReadOnlyCollection<ResourceResponse> cacheResources,
        CancellationToken cancellationToken)
    {
        var resultMessageActivity = turnContext.CreateTextActivity(resultMessage);

        if (turnContext.IsNotTelegramChannel() && turnContext.IsNotMsteamsChannel())
        {
            return turnContext.SendActivityAsync(resultMessageActivity, cancellationToken);
        }

        var tasks = new List<Task>(cacheResources.Where(NotEmpty).Select(InnerDeleteAsync))
        {
            turnContext.SendActivityAsync(resultMessageActivity, cancellationToken)
        };

        return Task.WhenAll(tasks);

        Task InnerDeleteAsync(ResourceResponse resource)
            =>
            turnContext.DeleteActivityAsync(resource.Id, cancellationToken);

        static bool NotEmpty(ResourceResponse? resource)
            =>
            string.IsNullOrEmpty(resource?.Id) is false;
    }

    private static Activity CreateTextActivity(this ITurnContext turnContext, string resultMessage)
    {
        if (turnContext.IsNotTelegramChannel() || string.IsNullOrEmpty(resultMessage))
        {
            return MessageFactory.Text(resultMessage);
        }

        var activity = MessageFactory.Text(default);

        var channelData = new TelegramChannelData(
            parameters: new(resultMessage)
            {
                ParseMode = TelegramParseMode.Html
            });

        activity.ChannelData = channelData.ToJObject();
        return activity;
    }

    private static string CreateDefaultResultMessage<T>(IChatFlowContext<T> context, LookupValue lookupValue)
        =>
        context.IsNotTelegramChannel() ? $"Выбрано значение: **{lookupValue.Name}**" : $"Выбрано значение: <b>{lookupValue.Name}</b>";
}