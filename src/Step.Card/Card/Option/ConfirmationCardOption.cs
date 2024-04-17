namespace GarageGroup.Infra.Bot.Builder;

public sealed record class ConfirmationCardOption
{
    public ConfirmationCardOption(EntityCardOption entity, CardButtonsOption buttons)
    {
        Entity = entity;
        Buttons = buttons;
    }

    public EntityCardOption Entity { get; }

    public CardButtonsOption Buttons { get; }
}