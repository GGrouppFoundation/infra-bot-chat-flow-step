using System;

namespace GarageGroup.Infra.Bot.Builder;

public sealed record class ConfirmationTelegramWebAppOption
{
    private const string DefaultButtonName = "Редактировать";

    public ConfirmationTelegramWebAppOption(string webAppUrl, string buttonName = DefaultButtonName)
    {
        WebAppUrl = webAppUrl.OrEmpty();
        ButtonName = buttonName.OrNullIfWhiteSpace() ?? DefaultButtonName;
    }

    public string WebAppUrl { get; }

    public string ButtonName { get; }
}