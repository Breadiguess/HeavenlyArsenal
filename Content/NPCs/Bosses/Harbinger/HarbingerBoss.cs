using Luminance.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Harbinger
{
    public partial class HarbingerBoss : ModNPC
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

        public override void SetDefaults()
        {
            NPC.friendly = false;
            NPC.damage = 0;
            NPC.defense = 9999;
            NPC.lifeMax = 1_000_000;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.Size = new Vector2(200, 200);
            NPC.boss = true;
            NPC.npcSlots = 10f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
        }
    }
}
