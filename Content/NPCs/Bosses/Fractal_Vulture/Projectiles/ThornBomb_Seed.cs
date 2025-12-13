using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles
{
    public class ThornBomb_Seed : ModProjectile
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        
        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private const int FuseTime = 120;          
        private const int ThornCount = 5;         
        private const float DriftDamp = 0.985f;   
        private const float DetonateBurstSpeed = 6.5f;

        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;

            Projectile.hostile = true;
            Projectile.friendly = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 1800;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.aiStyle = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage = 0;
            Projectile.scale = 0;
        }
        public override void AI()
        {
            Projectile.scale = LumUtils.InverseLerp(0, 30, Time);
            Projectile.damage = (int)(Projectile.originalDamage * LumUtils.InverseLerp(40, FuseTime-40, Time));
            Projectile.velocity *= DriftDamp;

            Projectile.rotation += 0.08f * (Projectile.direction == 0 ? 1f : Projectile.direction) * LumUtils.InverseLerp(FuseTime, 0, Time) ;

            if (Time > FuseTime - 15)
            {
                float pulseT = (Time - (FuseTime - 15)) / 15f;
                Projectile.scale = 1f + 0.12f * (1f - pulseT) * 40;
            }

            if (Time >= FuseTime)
            {
                Detonate();
                Projectile.Kill();
            }

            Time++;
        }

        private void Detonate()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            const float InitialSpeed = 7.5f;

            float angleStep = MathHelper.TwoPi / ThornCount;

            for (int i = 0; i < ThornCount; i++)
            {
                float angle = angleStep * i + Projectile.rotation;
                Vector2 velocity = angle.ToRotationVector2() * InitialSpeed*2;

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<SeekingThorn>(),
                    Projectile.damage/4,
                    10f
                    
                );
            }

            SoundEngine.PlaySound(SoundID.Item14 with { PitchVariance = 0.4f }, Projectile.Center).WithVolumeBoost(4);
        }

        private static int FindClosestPlayer(Vector2 from, float maxDist)
        {
            int best = -1;
            float bestD = maxDist;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (!p.active || p.dead)
                    continue;

                float d = Vector2.Distance(from, p.Center);
                if (d < bestD)
                {
                    bestD = d;
                    best = i;
                }
            }

            return best;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D thornTex = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            Texture2D coreTex = GennedAssets.Textures.GreyscaleTextures.HollowCircleSoftEdge;

            Vector2 DrawPos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(coreTex, DrawPos, null, Color.White with { A = 0 }, 0f, coreTex.Size() * 0.5f, 0.1f * Projectile.scale, 0);


            const float Radius = 70;

            float angleStep = MathHelper.TwoPi / ThornCount;

            float pulse = 1f + 0.1f * MathF.Sin(Time * 0.25f);

            for (int i = 0; i < ThornCount; i++)
            {
                float angle = angleStep * i + Projectile.rotation;

                Vector2 offset = angle.ToRotationVector2() * Radius * pulse;
                Vector2 pos = DrawPos + offset * LumUtils.InverseLerp(0, FuseTime, Time) ;

                float rotation = angle + MathHelper.PiOver2;

                Main.EntitySpriteDraw(thornTex, pos, null, Color.White * 0.85f, rotation, thornTex.Size() * 0.5f,  new Vector2(2f, 60f)*Projectile.scale, 0);
            }
            Utils.DrawBorderString(Main.spriteBatch, Projectile.damage.ToString(), DrawPos, Color.AntiqueWhite);
            return false;
        }

    }

}
