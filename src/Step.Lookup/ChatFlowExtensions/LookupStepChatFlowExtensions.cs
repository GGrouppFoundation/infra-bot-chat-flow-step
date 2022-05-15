using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace GGroupp.Infra.Bot.Builder;

public static partial class LookupStepChatFlowExtensions
{
    private static Task SendResultActivityAsync(
        this ITurnContext turnContext,
        string resultMessage,
        IReadOnlyCollection<ResourceResponse> cacheResources,
        CancellationToken cancellationToken)
    {
        var resultMessageActivity = MessageFactory.Text(resultMessage);

        if (turnContext.IsWebchatChannel() || turnContext.IsEmulatorChannel())
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

    private static string CreateDefaultResultMessage<T>(IChatFlowContext<T> context, LookupValue lookupValue)
    {
        var text = context.EncodeTextWithStyle(lookupValue.Name, BotTextStyle.Bold);
        return $"Выбрано значение: {text}";
    }
}