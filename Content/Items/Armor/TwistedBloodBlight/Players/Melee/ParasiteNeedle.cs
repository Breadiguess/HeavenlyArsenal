using CalamityMod;
using Luminance.Assets;
using Luminance.Common.Easings;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Melee
{
    internal class ParasiteNeedle : ModProjectile
    {
        public PiecewiseCurve attackCurve;
        private float t = 0.0f;
        public float LerpStrength
        {
            get
            {
                if (attackCurve == null)
                    return 0.0f;
                return attackCurve.Evaluate(t);
            }
        }
        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public int Index
        {
            get
            {
                MeleeBloodController melee = parasite.ConstructController as MeleeBloodController;
                if(melee == null)
                    return 0;


                return melee.Needles.IndexOf(Projectile.whoAmI);
            }
        }
        public ref Player Owner => ref Main.player[Projectile.owner];
        public BloodBlightParasite_Player parasite => Owner.GetModPlayer<BloodBlightParasite_Player>();
        public override string Texture =>  MiscTexturesRegistry.InvisiblePixelPath;

        public override void OnSpawn(IEntitySource source)
        {
            int offset = Index % 2 == 0 ? 1 : -1;
            HomePosition = Owner.Center + new Vector2(70 * offset, 0 + MathHelper.SmoothStep(0, 10 * MathF.Cos(Main.GameUpdateCount / 60f), 4));
        }
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.Size = new Vector2(10, 10);
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = ModContent.GetInstance<TrueMeleeNoSpeedDamageClass>();
        }

        public Vector2 HomePosition;

        public override void PostAI()
        {
            int offset = Index %2 == 0 ? 1 : -1;
            HomePosition = Owner.Center + new Vector2(70 *offset, 0 + MathHelper.SmoothStep(0, 10*MathF.Cos(Main.GameUpdateCount/60f), 4));   
        }
        public override void AI()
        {
            Projectile.Center = Vector2.Lerp(Projectile.Center, HomePosition, 1f-LerpStrength);
            if (Owner.controlUseItem)
                attack();
           
        }

        void attack()
        {
            attackCurve = new PiecewiseCurve()
                .Add(EasingCurves.Sine, EasingType.In, 0.5f, 1f);

           
                t = LumUtils.InverseLerp(0, 60, Time);
            if (Time > 60)
            {
                t = 0f;
                Time = -1;

            }
            Projectile.velocity = Projectile.Center.DirectionTo(Main.MouseWorld)* 50 * LerpStrength;
            Time++;
        }
        


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(tex, DrawPos, null, Color.White, 0, tex.Size() / 2, 1, 0);

            return false;
        }
    }
}
