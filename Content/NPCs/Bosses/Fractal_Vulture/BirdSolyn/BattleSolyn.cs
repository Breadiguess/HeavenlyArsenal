using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Items.MiscOPTools;
using NoxusBoss.Content.NPCs.Bosses.Avatar;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers.ParadiseReclaimed;
using NoxusBoss.Content.NPCs.Bosses.Draedon.SpecificEffectManagers;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.Utilities;
using NoxusBoss.Core.World.Subworlds;
using NoxusBoss.Core.World.WorldSaving;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Solyn;

public class SolynBirdOverride : GlobalNPC
{
    public override bool InstancePerEntity => true;
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == ModContent.NPCType<NoxusBoss.Content.NPCs.Friendly.BattleSolyn>();
    }


    public override void AI(NPC npc)
    {
        if (npc.ai[0] == (float)voidVulture.Funny)
        {
            DoBehavior_FightBird(npc);
        }
    }




    /// <summary>
    ///     Makes Solyn fight the Evil fuckass bird.
    /// </summary>
    public void DoBehavior_FightBird(NPC solyn)
    {

        BattleSolyn battleSolyn = solyn.As<BattleSolyn>();
        var birdIsAbsent = voidVulture.Myself is null && voidVulture.Myself is null;

        if (birdIsAbsent || EternalGardenUpdateSystem.WasInSubworldLastUpdateFrame)
        {
            // Immediately vanish if this isn't actually Solyn.
            if (battleSolyn.FakeGhostForm)
            {
                solyn.active = false;

                return;
            }

            // If this is actually Solyn, turn into her non-battle form again.
            solyn.Transform(ModContent.NPCType<NoxusBoss.Content.NPCs.Friendly.Solyn>());

            return;
        }

        solyn.scale = 1f;
        solyn.target = Player.FindClosest(solyn.Center, 1, 1);
        solyn.immortal = true;
        solyn.noGravity = true;
        solyn.noTileCollide = true;

        if (voidVulture.Myself is not null)
        {
            var vulture = voidVulture.Myself.As<voidVulture>();
            //if (Avatar_ShouldSwap(vulture))
            //{
            //    SwitchTo(vulture);
            //}

            vulture.SolynAction?.Invoke(battleSolyn);
        }
    }
}