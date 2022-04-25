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
        LookupCacheResult cacheResult,
        CancellationToken cancellationToken)
    {
        var encodedText = turnContext.EncodeTextWithStyle(cacheResult.Value.Name, BotTextStyle.Bold);
        var activity = MessageFactory.Text($"{cacheResult.ResultText}: {encodedText}");

        var tasks = new List<Task>(cacheResult.Resources.Where(NotEmpty).Select(InnerDeleteAsync))
        {
            turnContext.SendActivityAsync(activity, cancellationToken)
        };
        return Task.WhenAll(tasks);

        Task InnerDeleteAsync(ResourceResponse resource)
            =>
            turnContext.DeleteActivityAsync(resource.Id, cancellationToken);

        static bool NotEmpty(ResourceResponse? resource)
            =>
            string.IsNullOrEmpty(resource?.Id) is false;
    }
}