using HeavenlyArsenal.Common.IK;
using Luminance.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs
{
    public class AnalyticIKAnchorNPC : ModNPC
    {
        private IKSkeletonAnalytic Limb;

        public override void SetDefaults()
        {
            NPC.width = 40;
            NPC.height = 40;
            NPC.dontTakeDamage = true;
            NPC.lifeMax = 999999;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Limb = new IKSkeletonAnalytic
            {
                Root = NPC.Center,
                Joint = NPC.Center + new Vector2(40, 0),
                Tip = NPC.Center + new Vector2(80, 0),
                UpperLength = 48f,
                LowerLength = 48f
            };
        }
        public override void AI()
        {
            NPC.velocity = Vector2.Zero;
            NPC.rotation = 0f;

            Player player = Main.LocalPlayer;

            Vector2 hand =
                player.MountedCenter +
                new Vector2(player.direction * 16f, -6f)
                .RotatedBy(player.fullRotation);

            Limb.Root = NPC.Center;

            Vector2 pole =
                (player.Center - NPC.Center)
                .RotatedBy(MathHelper.PiOver2);

            Limb.Solve(hand, pole);

            Dust.NewDustPerfect(Limb.Root,
                DustID.GoldFlame,
                Vector2.Zero).noGravity = true;

            Dust.NewDustPerfect(Limb.Joint,
                DustID.FireworkFountain_Red,
                Vector2.Zero).noGravity = true;

            Dust.NewDustPerfect(Limb.Tip,
                DustID.FireworkFountain_Blue,
                Vector2.Zero).noGravity = true;
        }
        public override bool PreDraw
    (
        SpriteBatch spriteBatch,
        Vector2 screenPos,
        Color drawColor
    )
        {
            if (NPC.IsABestiaryIconDummy)
                return false;

            if (Limb == null)
                return false;
            NoxusBoss.Core.Utilities.Utilities.DrawLineBetter(spriteBatch,Limb.Root, Limb.Joint, Color.Red, 2f);
            NoxusBoss.Core.Utilities.Utilities.DrawLineBetter(spriteBatch,Limb.Joint, Limb.Tip, Color.Blue, 2f);


            return true;
        }
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
    }
}
