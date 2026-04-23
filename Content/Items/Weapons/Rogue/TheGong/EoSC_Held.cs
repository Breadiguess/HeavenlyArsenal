using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.TheGong
{
    internal class EoSC_Held : ModProjectile
    {
        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public ref Player Owner => ref Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 40;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = true;
        }

        public override void AI()
        {
            ManageCharge();
            ManageTrail();
            Time++;
        }

        

        #region Collision and stuff
        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;


        #endregion

        #region DrawCode
        public override bool PreDraw(ref Color lightColor)
        {
            var tex = TextureAssets.Projectile[Type].Value;


            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float Rot = Projectile.rotation + MathHelper.PiOver2;
            Vector2 Origin = new Vector2(tex.Width / 2, tex.Height);


            Vector2 Scale = new Vector2(0.2f, 0.2f* Math.Abs(MathF.Cos(Time*0.2f)));
            Main.EntitySpriteDraw(tex, drawPos, null, lightColor, Rot, Origin, Scale, 0);


            for (int i = 1; i < Projectile.oldPos.Length-1; i++)
            {
                Utils.DrawLine(Main.spriteBatch, Projectile.oldPos[i], Projectile.oldPos[i + 1], Color.White);
            }

            return false;
        }

        #endregion

        #region Helpers
        void ManageCharge()
        {
            Owner.heldProj = Projectile.whoAmI;
            Projectile.Center = Owner.MountedCenter - new Vector2(5 * Owner.direction, 10);



            float AdjustedRot = -MathHelper.PiOver4 * Owner.direction +Owner.MountedCenter.AngleTo(Owner.Calamity().mouseWorld);
            float RadiusX = 10 * Owner.direction;
            float RadiusY = 2;
            float AdjustedTime = Time * 0.2f;
            Vector2 orbitOffset = new(
               MathF.Cos(AdjustedTime) * RadiusX,
               MathF.Sin(AdjustedTime) * RadiusY
           );

            Projectile.velocity = orbitOffset.RotatedBy(AdjustedRot) ;
            Projectile.rotation = Projectile.velocity.ToRotation();

            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Owner.MountedCenter.AngleTo(Projectile.Center + Projectile.velocity) - MathHelper.PiOver2);
        }
        private void ManageTrail()
        {

            Projectile.oldPos[0] = Projectile.Center + Projectile.velocity * 10;

            UpdateTrail();
        }
        public void UpdateTrail()
        {
            var playerPosOffset = Owner.position - Owner.oldPosition;

            if (Projectile.numUpdates == 0)
            {

                for (var i = Projectile.oldPos.Length - 1; i > 0; i--)
                {
                    Projectile.oldPos[i] = Projectile.oldPos[i - 1] +playerPosOffset;
                    Projectile.oldRot[i] = Projectile.rotation.AngleLerp(Projectile.oldRot[i - 1], 0.1f);
                }

                //if (!holdTrailUpdate)
                {
                    Projectile.oldPos[0] = Projectile.Center + Projectile.velocity;
                    Projectile.oldRot[0] = Projectile.rotation;
                }
            }
        }
        #endregion
    }
}
