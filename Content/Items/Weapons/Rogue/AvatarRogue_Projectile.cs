using HeavenlyArsenal.Common.utils;
using Luminance.Assets;
using Luminance.Common.Easings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue
{
    public class AvatarRogue_Projectile : ModProjectile
    {
        public ref Player Owner => ref Main.player[Projectile.owner];
        public Vector2 ChainEnd { get; set; }
        public Rope Chain;
        public PiecewiseCurve ChainCurve;
        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            writer.WriteVector2(ChainEnd);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            ChainEnd = reader.ReadVector2();
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;

        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.Center += Main.rand.NextFloat(0,60) * Vector2.UnitY;
            if(Chain is null)
            {
                Chain = new Rope(Projectile.Center-Vector2.One, Projectile.Center, 20, 2, Vector2.Zero);

            }
            ChainCurve = new PiecewiseCurve().Add(EasingCurves.Sextic, EasingType.In, 1, 1f);
            ChainEnd = Projectile.Center;
        }
        public override void AI()
        {
            Chain.segments[0].position = ChainEnd;
            Chain.segments[^1].position = Projectile.Center;
            Chain.Update();

            float interp = ChainCurve.Evaluate(LumUtils.InverseLerp(0, 60, Time));
            Projectile.Center = Vector2.Lerp(ChainEnd, Main.MouseWorld, interp);
            Time++;
        }


        public override bool PreDraw(ref Color lightColor)
        {

            DrawChain();
            return base.PreDraw(ref lightColor);
        }
        private void DrawChain()
        {
            if (Chain is null)
                return;

            for(int i = 0; i< Chain.segments.Length-1; i++)
            {
                NoxusBoss.Core.Utilities.Utilities.DrawLineBetter(Main.spriteBatch, Chain.segments[i].position, Chain.segments[i + 1].position, Color.Wheat, 2);
            }
        }
    }
}
