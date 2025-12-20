namespace HeavenlyArsenal.Content.Items.Accessories.ManaTransfusion;

public sealed class ManaTransfusionGlobalItem : GlobalItem
{
    public override void GetHealMana(Item item, Player player, bool quickHeal, ref int healValue)
    {
        base.GetHealMana(item, player, quickHeal, ref healValue);

        var active = player.GetModPlayer<ManaTransfusionPlayer>().Enabled;

        if (!active)
        {
            return;
        }

        healValue = (int)(healValue * 1.5f);
    }

    public override void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue)
    {
        base.GetHealLife(item, player, quickHeal, ref healValue);

        var active = player.GetModPlayer<ManaTransfusionPlayer>().Enabled;

        if (!active)
        {
            return;
        }

        healValue = 0;
    }

    public override bool ConsumeItem(Item item, Player player)
    {
        var active = player.GetModPlayer<ManaTransfusionPlayer>().Enabled;
        var flag = active && item.healLife > 0;

        return !flag;
    }
}