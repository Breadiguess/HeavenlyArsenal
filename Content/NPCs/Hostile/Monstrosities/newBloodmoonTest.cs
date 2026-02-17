using Luminance.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.NPCs.Hostile.Monstrosities
{
    internal class newBloodmoonTest : BaseBloodMoonNPC
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override int MaxBlood => 40;

        public override BloodMoonBalanceStrength Strength => new(1f, 0.5f, 1f);

        protected override void SetDefaults2()
        {
            NPC.Size = new Vector2(40, 40);
            NPC.lifeMax = 400;

        }

        public override void AI()
        {
            Main.NewText(NPC.ToString());
            Main.NewText(NPC.lifeMax);
        }
    }
}
