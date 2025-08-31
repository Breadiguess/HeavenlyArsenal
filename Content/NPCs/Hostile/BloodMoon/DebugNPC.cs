using Luminance.Assets;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    internal class DebugNPC : BloodmoonBaseNPC
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override float buffPrio => 5;

        public override bool canBeSacrificed => false;

        public override int bloodBankMax => 5000;

        public override void SetDefaults()
        {
            NPC.friendly = false;
            NPC.lifeMax = 100_000;
            NPC.dontTakeDamageFromHostiles = false;
            NPC.Size = new Microsoft.Xna.Framework.Vector2(30, 60);
        }

    }
}
