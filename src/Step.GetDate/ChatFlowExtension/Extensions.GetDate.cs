using AdaptiveCards;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

public static class GetDateChatFlowExtensions
{
    private const string dateId = "date";
    public static ChatFlow<TNext> GetDate<T, TNext>(
        this ChatFlow<T> chatFlow,
        Func<T, GetDateOption> optionFactory,
        Func<T, DateOnly, TNext> mapFlowState)
        =>
        InnerGetDate(
            chatFlow ?? throw new ArgumentNullException(nameof(chatFlow)),
            optionFactory ?? throw new ArgumentNullException(nameof(optionFactory)),
            mapFlowState ?? throw new ArgumentNullException(nameof(mapFlowState)));

    private static ChatFlow<TNext> InnerGetDate<T, TNext>(
       this ChatFlow<T> chatFlow,
       Func<T, GetDateOption> optionFactory,
       Func<T, DateOnly, TNext> mapFlowState)
        =>
        chatFlow.ForwardValue(
            optionFactory,
            InnerGetDateAsync,
            mapFlowState);

    private static ValueTask<ChatFlowJump<DateOnly>> InnerGetDateAsync(
        IChatFlowContext<GetDateOption> context,
        CancellationToken cancellationToken)
        => 
        context.Activity.IsCardSupported() ? 
        InnerGetDateCardSupportedAsync(context, cancellationToken) : 
        InnerGetDateCardNotSupportedAsync(context, cancellationToken);

    private static async ValueTask<ChatFlowJump<DateOnly>> InnerGetDateCardSupportedAsync(
        IChatFlowContext<GetDateOption> context,
        CancellationToken cancellationToken)
    {
        if (context.StepState is null)
        {
            var dateActivity = CreateGetDateAttachment(context.FlowState, GetAdaptiveSchemaVersion(context.Activity.ChannelId));
            await context.SendActivityAsync(dateActivity, cancellationToken).ConfigureAwait(false);
            return ChatFlowJump.Repeat<DateOnly>(new object());
        }

        return context.Activity
            .GetDateFormActivity()
            .Fold(
                date => date,
                _ => ChatFlowJump.Repeat<DateOnly>(context.StepState));
    }

    private static ValueTask<ChatFlowJump<DateOnly>> InnerGetDateCardNotSupportedAsync(
        IChatFlowContext<GetDateOption> context,
        CancellationToken cancellationToken)
        => 
        throw new NotImplementedException();

    private static Result<DateOnly, ChatFlowStepFailure> GetDateFormActivity(this Activity activity)
        =>
        Pipeline.Pipe(
            activity.Value as JObject)
        .Pipe(
            jObject => jObject is null || jObject.HasValues is false ? default : jObject[dateId]?.ToString())
        .Pipe(
            stringDate =>
            DateOnly.TryParse(stringDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ?
            date : default);

    private static IActivity CreateGetDateAttachment(GetDateOption option, AdaptiveSchemaVersion schemaVersion)
        =>
        new Attachment()
        {
            ContentType = AdaptiveCard.ContentType,
            Content = new AdaptiveCard(schemaVersion)
            {
                Actions = new() { new AdaptiveSubmitAction() { Title = "Ok" } },
                Body = new() 
                { 
                    new AdaptiveDateInput() 
                    { 
                        Placeholder = "Enter date", 
                        Id = dateId, 
                        Value = option.DefaultDate.ToString("MM/dd/yyyy") ,
                        //Min = option.MinDate.ToString(CultureInfo.InvariantCulture),
                        //Max = option.MaxDate.ToString(CultureInfo.InvariantCulture)
                    } 
                }
            }
        }
        .ToActivity();

    private static AdaptiveSchemaVersion GetAdaptiveSchemaVersion(string channelId)
        =>
        channelId.Equals("msteams") ? 
        AdaptiveCard.KnownSchemaVersion : 
        new AdaptiveSchemaVersion(1, 0);
}