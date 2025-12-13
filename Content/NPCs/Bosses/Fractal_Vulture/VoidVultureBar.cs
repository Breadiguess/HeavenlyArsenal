using Luminance.Common.Utilities;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture
{
    internal class VoidVultureBar : ModBossBar
    {
        public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
        {
            if (npc.type == ModContent.NPCType<voidVulture>() && npc.As<voidVulture>().hideBar)
                return false;
            return base.PreDraw(spriteBatch, npc, ref drawParams);
        }
    }
}
