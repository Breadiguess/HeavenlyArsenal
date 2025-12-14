namespace HeavenlyArsenal.Common.Players;

public sealed class TransgenderPlayerGlobalItem : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.type == ItemID.GenderChangePotion;
    }

    public override void OnConsumeItem(Item item, Player player)
    {
        base.OnConsumeItem(item, player);

        if (!player.TryGetModPlayer(out TransgenderPlayer transgenderPlayer) || transgenderPlayer.Enabled)
        {
            return;
        }

        transgenderPlayer.Enabled = true;
    }
}