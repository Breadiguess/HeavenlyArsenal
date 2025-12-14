using Luminance.Common.Utilities;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture;

internal class VoidVultureBar : ModBossBar
{
    public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
    {
        if (npc.type == ModContent.NPCType<voidVulture>() && npc.As<voidVulture>().hideBar)
        {
            return false;
        }

        return base.PreDraw(spriteBatch, npc, ref drawParams);
    }
}