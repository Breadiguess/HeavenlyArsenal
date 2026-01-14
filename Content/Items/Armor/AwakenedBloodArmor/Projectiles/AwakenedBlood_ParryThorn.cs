using Luminance.Assets;
using Luminance.Common.Easings;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Projectiles
{
    internal class AwakenedBlood_ParryThorn : ModProjectile
    {
        public enum Stage
        {
            preparingToJab,
            jabbing,

            Shattering
        }
        public Stage CurrentStage = Stage.preparingToJab;
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public ref Player Owner => ref Main.player[Projectile.owner];
        public PiecewiseCurve JabCurve;
        public float jabOutput
        {
            get => JabCurve != null ? 1-JabCurve.Evaluate(LumUtils.InverseLerp(0, 60, Projectile.timeLeft)) : 0f;
        }
        public override void SetDefaults()
        {
            JabCurve = new PiecewiseCurve()
                .Add(EasingCurves.Cubic, EasingType.Out, 0.4f, 0.4f)
                .Add(EasingCurves.Exp, EasingType.Out, 1f, 1f);
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 60;
            Projectile.Size = new Vector2(20, 20);
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0;
        }
        public override void PostAI()
        {
            Projectile.Center = Owner.Center + Projectile.rotation.ToRotationVector2()*10;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return base.Colliding(projHitbox, targetHitbox);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D thorn = GennedAssets.Textures.GreyscaleTextures.WhitePixel;


            Main.EntitySpriteDraw(thorn, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, new Vector2(thorn.Width, thorn.Height / 2), new Vector2(7*jabOutput, 1)*10, 0);
            return false;
        }
    }
}
