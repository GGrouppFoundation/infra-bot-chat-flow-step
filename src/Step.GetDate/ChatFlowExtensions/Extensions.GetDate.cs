using AdaptiveCards;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace GGroupp.Infra.Bot.Builder;

partial class GetDateChatFlowExtensions
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
        chatFlow.ForwardValue(optionFactory, InnerGetDateAsync, mapFlowState);

    private static ValueTask<ChatFlowJump<DateOnly>> InnerGetDateAsync(
        IChatFlowContext<GetDateOption> context,
        CancellationToken cancellationToken)
        => 
        context.Activity.IsCardSupported()
        ? InnerGetDateCardSupportedAsync(context, cancellationToken)
        : InnerGetDateCardNotSupportedAsync(context, cancellationToken);

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
                context.RepeatSameStateJump<DateOnly>);
    }

    private static ValueTask<ChatFlowJump<DateOnly>> InnerGetDateCardNotSupportedAsync(
        IChatFlowContext<GetDateOption> context,
        CancellationToken cancellationToken)
        => 
        throw new NotImplementedException();

    private static Result<DateOnly, Unit> GetDateFormActivity(this Activity activity)
    {
        if (activity.Value is not JObject jObject || jObject.HasValues is false)
        {
            return default;
        }

        return ParseDateOrFailure(jObject[dateId]?.ToString());
    }

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
        channelId.Equals(Channels.Msteams) ?  AdaptiveCard.KnownSchemaVersion : new(1, 0);
}