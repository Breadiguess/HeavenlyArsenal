using System.Collections.Generic;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.Temp;

public class BrazilPlayer : ModPlayer
{
    public List<NPC> TrappedNPCs = new(Main.npc.Length);

    public override void PostUpdateMiscEffects() { }

    private void ManageBrazil()
    {
        foreach (var npc in TrappedNPCs) { }
    }
}

public class BrazilVictim : GlobalNPC
{
    public DeadUniverse_Rift Rift;

    public Player Banisher;

    public bool Active;

    public bool HasDealtSuctionDamage;

    public bool HasBeenYoinkedOut;

    public float StartSize;

    public int BrazilTimer;

    public bool InBrazil;

    public override bool InstancePerEntity => true;

    public float BrazilInterpolant
    {
        get
        {
            float BellAdjustment = 0;

            var Remap = Utils.Remap(BrazilTimer, 0, 180, 0, 1);
            Main.NewText(Remap);
            BellAdjustment = 1 - MathF.Abs(2 * Remap - 1);

            return BellAdjustment;
        }
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        StartSize = npc.scale;
    }

    public override bool PreAI(NPC npc)
    {
        if (Active && Rift != null && Banisher != null)
        {
            if (BrazilTimer > 0)
            {
                if (npc.scale <= StartSize && !HasDealtSuctionDamage)
                {
                    HasDealtSuctionDamage = true;
                    var hitInfo = npc.CalculateHitInfo(DeadUniverse_Rift.CalculateSizeDamage(npc, Rift), 0);
                    Banisher.StrikeNPCDirect(npc, hitInfo);
                }

                npc.scale = StartSize * BrazilInterpolant;
                BrazilTimer--;
            }
            else
            {
                npc.scale = StartSize;
                HasDealtSuctionDamage = false;
                Banisher = null;
                Rift = null;
                Active = false;
            }

            return false;
        }

        StartSize = npc.scale;

        return base.PreAI(npc);
    }

    public override void PostAI(NPC npc)
    {
        if (Active)
        {
            return;
        }
    }
}