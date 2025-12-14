namespace HeavenlyArsenal.Content.Items.Armor.ShintoArmor;

[JITWhenModsEnabled("EntropyMod")]
internal class EntropyBarrierCompat : ModPlayer
{
    [JITWhenModsEnabled("EntropyMod")]
    public override void PostUpdateMiscEffects()
    {
        if (Player.GetModPlayer<ShintoArmorPlayer>().Enraged && ModLoader.HasMod("CalamityEntropy"))
        {
            ManageEntropyBarrier();
        }
    }

    [JITWhenModsEnabled("EntropyMod")]
    private void ManageEntropyBarrier()
    {
        if (ModLoader.HasMod("CalamityEntropy"))
        {
            // TODO: Fix missing references:
            // Player.Entropy().MagiShield = 0;
            // Player.Entropy().visualMagiShield = false;
        }
    }
}