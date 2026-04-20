using CalamityEntropy.Core;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using Luminance.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ZealotsReward
{
    internal class Zealots_IceSpike : ModProjectile
    {
        public static readonly SoundStyle Impact = new SoundStyle(Zealots_Item.Path + "_Spike");
        public ref Player Owner => ref Main.player[Projectile.owner];
        public int Timer
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.Size = new(30);
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.extraUpdates = 3;
        }
        public override void OnSpawn(IEntitySource source)
        {

            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override bool PreAI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            return base.PreAI();
        }

        public override void AI()
        {
            if (Timer > 60)
            {

                Projectile.velocity.X *= 0.98f;
                Projectile.velocity.Y += 0.2f;
            }


            MediumMistParticle mist = new MediumMistParticle(Projectile.Center, Projectile.velocity.RotatedByRandom(1),
             Main.rand.NextBool(3) ? Color.LightSteelBlue : Color.SteelBlue, Color.CadetBlue, Main.rand.NextFloat(0.4f, 0.685f), 150);
            
                GeneralParticleHandler.SpawnParticle(mist, true);

            Timer++;
        }
        void HitAndShatter()
        {
            int Type = ModContent.ProjectileType<Zealots_StasisBurst>();
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.AbsoluteZeroWave with { PitchVariance = 0.25f}, Projectile.Center).WithVolumeBoost(3);
            Luminance.Core.Graphics.ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, 12, shakeStrengthDissipationIncrement: 0.4f);
            Projectile.NewProjectileDirect(Owner.HeldItem.GetSource_FromThis(), Projectile.Center, Vector2.Zero, Type, Projectile.originalDamage, 0);


            Projectile.Kill();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            HitAndShatter();
          
            return false;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            HitAndShatter();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            HitAndShatter();
        }


        public override bool PreDraw(ref Color lightColor)
        {
            var tex = TextureAssets.Projectile[Type].Value;

            Vector2 drawPos;

            Main.spriteBatch.UseBlendState(BlendState.Additive);

            Vector2 scale;
            for(int i= 0; i< Projectile.oldPos.Length-1; i++)
            {
                drawPos = Projectile.oldPos[i] - Main.screenPosition + Projectile.Hitbox.Size()/2;
                scale = new Vector2(1, 0.9f) * (1 - LumUtils.InverseLerp(0, 1, i / (float)Projectile.oldPos.Length));

                Main.EntitySpriteDraw(tex, drawPos, null, Color.White, Projectile.oldRot[i], tex.Size() / 2, scale, 0);
            }
            Main.spriteBatch.ResetToDefault();

            drawPos = Projectile.Center - Main.screenPosition; 

            Main.EntitySpriteDraw(tex, drawPos, null, Color.White, Projectile.rotation, tex.Size() / 2, 1, 0);





            return false;
        }
    }
}
