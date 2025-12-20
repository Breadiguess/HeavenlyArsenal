namespace HeavenlyArsenal.Content.Items.Accessories.ManaTransfusion;

public sealed class ManaTransfusionPlayer : ModPlayer
{
    public bool Enabled { get; set; }

    public override void ResetEffects()
    {
        base.ResetEffects();

        Enabled = false;
    }

    public override void PostUpdate()
    {
        base.PostUpdate();

        //TODO: IF THIS ACCESSORY IS EQUIPPED, PLAYER'S LIFE IS SET TO THEIR MANA.
        // IN ESSENCE, THEIR MAX HEALTH IS THEIR MAX MANA, AND THEY CANNOT HEAL LIFE.
        if (!Enabled)
        {
            return;
        }

        var text = "\n";

        text += $"Player.manaRegen: {Player.manaRegen}\n";
        text += $"Player.manaRegenBonus: {Player.manaRegenBonus}\n";
        text += $"Player.manaRegenCount: {Player.manaRegenCount}\n";
        text += $"Player.manaRegenDelay: {Player.manaRegenDelay}\n";
        text += $"Player.manaRegenDelayBonus: {Player.manaRegenDelayBonus}\n";

        Main.NewText(text);

        Player.statLifeMax2 = Player.statManaMax2;
        Player.statLife = Player.statMana;

        Player.lifeRegen = 0;
        Player.manaRegenDelayBonus = 40;

        Player.manaRegen = (int)(Player.manaRegen * 0.5f);
    }

    public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
    {
        base.ModifyManaCost(item, ref reduce, ref mult);

        if (!Enabled)
        {
            return;
        }

        mult = 0.56f;
    }

    public override void OnHurt(Player.HurtInfo info)
    {
        base.OnHurt(info);

        if (!Enabled)
        {
            return;
        }

        Player.statMana -= info.Damage;
        Player.manaRegenDelay = Math.Clamp(Player.manaRegenDelay + 160, 0, 300);
    }

    public override void UpdateLifeRegen()
    {
        base.UpdateLifeRegen();

        if (!Enabled)
        {
            return;
        }

        Player.lifeRegen *= 0;
    }

    public override void NaturalLifeRegen(ref float regen)
    {
        base.NaturalLifeRegen(ref regen);

        if (!Enabled)
        {
            return;
        }

        regen *= 0;
    }

    public override bool ModifyNurseHeal(NPC nurse, ref int health, ref bool removeDebuffs, ref string chatText)
    {
        if (Enabled)
        {
            chatText = "You need a Mage, not a surgeon.";

            return false;
        }

        return true;
    }
}