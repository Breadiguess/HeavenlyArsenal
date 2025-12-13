using Luminance.Common.Utilities;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Core.World.Subworlds;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Solyn;

public partial class BattleSolynBird : ModNPC
{
    /// <summary>
    /// Makes Solyn fight the Evil fuckass bird.
    /// </summary>
    public void DoBehavior_FightBird()
    {
        bool birdIsAbsent = voidVulture.Myself is null && voidVulture.Myself is null;
        if (birdIsAbsent || EternalGardenUpdateSystem.WasInSubworldLastUpdateFrame)
        {
            // Immediately vanish if this isn't actually Solyn.
            if (FakeGhostForm)
            {
                NPC.active = false;
                return;
            }

            // If this is actually Solyn, turn into her non-battle form again.
            NPC.Transform(ModContent.NPCType<NoxusBoss.Content.NPCs.Friendly.Solyn>());
            return;
        }

        NPC.scale = 1f;
        NPC.target = Player.FindClosest(NPC.Center, 1, 1);
        NPC.immortal = true;
        NPC.noGravity = true;
        NPC.noTileCollide = true;

        if (voidVulture.Myself is not null)
        {
            voidVulture vulture = voidVulture.Myself.As<voidVulture>();
            //if (Avatar_ShouldSwap(vulture))
            //{
            //    SwitchTo(vulture);
            //}

            vulture.SolynAction?.Invoke(this);
        }
      
    }

    private bool AvatarRift_ShouldSwap(AvatarRift rift)
    {
        if (IsMultiplayerClone)
            return false;
        return rift.NPC.target != MultiplayerIndex;
    }

    private bool Avatar_ShouldSwap(voidVulture bird)
    {
        if (IsMultiplayerClone)
            return false;
        // current target is entity, so check if it's a Player
        if (!(bird.currentTarget is Player))
            return false;

        return bird.NPC.target != MultiplayerIndex;
    }
}
