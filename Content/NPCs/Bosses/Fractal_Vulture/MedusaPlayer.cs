namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture;

internal class MedusaPlayer : ModPlayer
{
    /// <summary>
    ///     the amount of medusa stacks accumulated by looking at the vulture.
    /// </summary>
    public int MedusaStacks;

    /// <summary>
    ///     Amount of time this player has spent looking at the vulture.
    /// </summary>
    public int MedusaTimer;

    /// <summary>
    ///     timer to tick down to start causing medusa stacks to tick down;
    /// </summary>
    public int PurgeTimer;

    public int PurgeTimeMax = 70;

    /// <summary>
    ///     amount of time the vulture can be safely looked at.
    /// </summary>
    public int SafeThreshold = (int)(60 * 1.4f);

    public override void PostUpdateMiscEffects()
    {
        if (MedusaTimer > SafeThreshold && MedusaTimer % 30 == 0)
        {
            MedusaStacks++;
        }

        if (PurgeTimer > PurgeTimeMax)
        {
            MedusaTimer =
                MedusaStacks = Math.Clamp(MedusaStacks - 1, 0, 7);

            PurgeTimer = -1;
        }
    }

    public override void UpdateBadLifeRegen()
    {
        PurgeTimer++;
    }
}