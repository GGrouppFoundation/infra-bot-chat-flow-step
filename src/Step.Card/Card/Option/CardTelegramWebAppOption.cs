using System;

namespace GarageGroup.Infra.Bot.Builder;

public sealed record class CardTelegramWebAppOption
{
    private const string DefaultButtonName = "Редактировать";

    public CardTelegramWebAppOption(string webAppUrl, string buttonName = DefaultButtonName)
    {
        WebAppUrl = webAppUrl.OrEmpty();
        ButtonName = buttonName.OrNullIfWhiteSpace() ?? DefaultButtonName;
    }

    public string WebAppUrl { get; }

    public string ButtonName { get; }
}