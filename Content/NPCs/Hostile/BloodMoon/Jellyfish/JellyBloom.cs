using Luminance.Common.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish;

internal class JellyBloom : BloodMoonBaseNPC
{
    private readonly int stage1Time = 60 * 10;

    private readonly int stage2Time = 60 * 20;

    private readonly int stage3Time = 60 * 30;

    public int GrowthStage
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public override void SetStaticDefaults()
    {
        NPCID.Sets.NeverDropsResourcePickups[Type] = true;
        this.ExcludeFromBestiary();
    }

    public override void SetDefaults()
    {
        NPC.damage = 0;
        NPC.value = 0;

        NPC.lifeMax = 3;
        NPC.dontTakeDamage = true;
        NPC.ShowNameOnHover = false;
    }

    public override void AI()
    {
        if (Time < stage1Time) { }

        if (Time > stage3Time) { }

        Time++;
    }
}