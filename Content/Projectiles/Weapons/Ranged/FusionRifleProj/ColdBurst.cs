using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.DataStructures;
using rail;
using System.Security.Cryptography;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using NoxusBoss.Content.Buffs;
using CalamityMod.Buffs.DamageOverTime;
using Luminance.Core.Graphics;
using static NoxusBoss.Assets.GennedAssets.Textures;
using static NoxusBoss.Assets.GennedAssets;
using static Luminance.Common.Utilities.Utilities;
using NoxusBoss.Assets;
using System.Linq;
using Luminance.Assets;

using HeavenlyArsenal.common;
using HeavenlyArsenal.Content.Items.Weapons.Ranged;
using NoxusBoss.Content.Particles.Metaballs;
using Dust = Terraria.Dust;
using HeavenlyArsenal.Content.Particles.Metaballs;


namespace HeavenlyArsenal.Content.Projectiles.Weapons.Ranged.FusionRifleProj
{
    internal class ColdBurst : ModProjectile//, IPixelatedPrimitiveRenderer
    {
        private Vector2[] oldPos;
        public int Time
        {
            get;
            set;
        }

        public ref Terraria.Player Owner => ref Main.player[Projectile.owner];
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public static float SmoothStep(float edge0, float edge1, float value)
        {
            // Clamp the input value to the range [0, 1]
            value = Math.Clamp((value - edge0) / (edge1 - edge0), 0f, 1f);

            // Perform the smoothstep interpolation
            return value * value * (3f - 2f * value);
        }

        

        public bool FirstHit
        {
            get;
            set;
        }

        public Color BloodColorFunction(float completionRatio)
        {
            return Projectile.GetAlpha(new Color(82, 1, 23));
        }


        public override void SetStaticDefaults()
        {
            //Main.projFrames[Projectile.type] = 35;
            ProjectileID.Sets.CanHitPastShimmer[Projectile.type] = true;
            //ProjectileID.Sets.
           // ProjectileID.Sets.min
        }
        // Store the target NPC using Projectile.ai[0]
        private NPC HomingTarget
        {
            get => Projectile.ai[0] == 0 ? null : Main.npc[(int)Projectile.ai[0] - 1];
            set
            {
                Projectile.ai[0] = value == null ? 0 : value.whoAmI + 1;
            }
        }
        public override void SetDefaults()
        {
            
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = Projectile.height = 78;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown =100; // 20 ticks before the same npc can be hit again


            AIType = ProjectileID.Bullet;
            Projectile.idStaticNPCHitCooldown = 8;
            
           
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            float homingRange = 600f;
            float closestDist = homingRange;
            int targetIndex = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(Projectile) && !npc.friendly)
                {
                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        targetIndex = i;
                    }
                }
            }
            if (targetIndex != -1)
            {
                NPC target = Main.npc[targetIndex];
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float homingStrength = 0.1f;   // tweak this for more or less turning
                Projectile.velocity = Vector2.Normalize(Projectile.velocity + toTarget * homingStrength) * Projectile.velocity.Length();
            }
            Projectile.velocity *= 1.51f;
        }


        public override bool PreAI()
        {
            if (oldPos == null)
                oldPos = Enumerable.Repeat(Projectile.Center, 9).ToArray();

            for (int i = oldPos.Length - 2; i > 0; i--)
            {
                oldPos[i] = oldPos[i - 1];
            }

            oldPos[0] = Projectile.Center + Projectile.velocity * 2;
            return false;
        }

       

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //target.AddBuff(ModContent.BuffType<MiracleBlight>(), 600);
            if (FirstHit)
            {
                //todo: access globalNPC volatileRounds and set the hit target to true
                FirstHit = false;
                
               
                Owner.GetModPlayer<FusionRiflePlayer>().BurstCounter++;
                Projectile.velocity = Vector2.Zero;
                for (int i = oldPos.Length - 2; i > 0; i--)
                {
                    oldPos[i] = target.Center + Projectile.oldVelocity;
                }

                oldPos[0] = target.Center;
                Projectile.damage = -1;
                Projectile.timeLeft = 25;
            }

            if (Owner.GetModPlayer<FusionRiflePlayer>().VolatileRounds && target.GetGlobalNPC<VolatileRounds>().VolatileCooldown <=0 )
            {
                target.GetGlobalNPC<VolatileRounds>().VolatileActive = true;

                if (target.GetGlobalNPC<VolatileRounds>().VolatileSafe <= 0 && target.GetGlobalNPC<VolatileRounds>().VolatileCooldown <= 0)
                {
                    
                    target.GetGlobalNPC<VolatileRounds>().VolatileSafe = 20;
                    target.GetGlobalNPC<VolatileRounds>().VolatileTimer = 60 * 3;
                }

            }

            var metalball = ModContent.GetInstance<FusionRifle_Hit>();
            for (int i = 0; i <1; i++)
            {
                float gasSize = Projectile.width/1.3f;// * Main.rand.NextFloat(0.32f, 1.6f);
                metalball.CreateParticle(Projectile.Center + Projectile.velocity * 0.1f, Vector2.Zero, gasSize);
            }
            for (int i = 0; i< 4; i++)
            {
                Dust.NewDust(target.Center - Projectile.oldVelocity, 40, 40, DustID.Snow, Projectile.velocity.X*10, Projectile.velocity.Y*10, 100, Color.AntiqueWhite, 1);
            }
        }
            


        public override void OnSpawn(IEntitySource source)
        {
            FirstHit = true;
        }
        public override void OnKill(int timeLeft)
        {

        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Glowball = GennedAssets.Textures.GreyscaleTextures.BloomFlare;
            float GlowScale = 0.1f;
            Vector2 glowScale = new Vector2(0.4f, 0.2f);
            Vector2 Gorigin = new Vector2(Glowball.Size().X / 2, Glowball.Size().Y / 2);
            

            if(FirstHit)
                Main.spriteBatch.Draw(Glowball, Projectile.Center + Projectile.velocity/2 - Main.screenPosition, null,
                     (Color.Violet with { A = 0 }) * 0.2f, Projectile.velocity.ToRotation(), Gorigin, glowScale, SpriteEffects.None, 0f);
            



            if (oldPos == null || oldPos.Length == 0)
                return true; // Prevent null reference by exiting early if oldPos is not initialized  

            float WidthFunction(float p) => 50f * MathF.Pow(p, 0.9f) * (1f - p * 0.9f);
            Color ColorFunction(float p) => new Color(60, 60, 150, 200);

            ManagedShader trailShader = ShaderManager.GetShader("HeavenlyArsenal.FusionRifle_Bullet");
            trailShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly + Projectile.velocity.Length() / 8f + Projectile.identity / 72.113f);
            trailShader.TrySetParameter("spin", 0);//0.5f * Math.Sign(Projectile.velocity.X));
            trailShader.TrySetParameter("brightness", 1.5f);
            trailShader.SetTexture(Noise.CrackedNoiseA, 0, SamplerState.LinearWrap);
            trailShader.SetTexture(Noise.FireNoiseB, 1, SamplerState.LinearWrap);
            trailShader.SetTexture(Noise.DendriticNoiseZoomedOut, 2, SamplerState.LinearWrap);

            PrimitiveRenderer.RenderTrail(oldPos, new PrimitiveSettings(WidthFunction, ColorFunction, _ => Vector2.Zero, Shader: trailShader, Smoothen: false), oldPos.Length);

            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            base.PostDraw(lightColor);
        }

        /*
        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            Texture2D BubblyNoise = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/Noise/BubblyNoise").Value;
            Texture2D DendriticNoiseZoomedOut = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/Noise/DendriticNoiseZoomedOut").Value;

            Rectangle viewBox = Projectile.Hitbox;
            Rectangle screenBox = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
            viewBox.Inflate(540, 540);
            if (!viewBox.Intersects(screenBox))
                return;

            float WidthFunction(float p) => 50f * MathF.Pow(p, 0.66f) * (1f - p * 0.5f);
            Color ColorFunction(float p) => new Color(215, 30, 35, 200);

            ManagedShader trailShader = ShaderManager.GetShader("HeavenlyArsenal.AvatarRifleBulletAuroraEffect");
            trailShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * Projectile.velocity.Length() / 8f + Projectile.identity * 72.113f);
            trailShader.TrySetParameter("spin", 2f * Math.Sign(Projectile.velocity.X));
            trailShader.TrySetParameter("brightness", 1.5f);
            trailShader.SetTexture(GennedAssets.Textures.Noise.DendriticNoiseZoomedOut, 0, SamplerState.LinearWrap);
            trailShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoiseDetailed, 1, SamplerState.LinearWrap);
            trailShader.SetTexture(GennedAssets.Textures.Noise.DendriticNoiseZoomedOut, 2, SamplerState.LinearWrap);

            PrimitiveRenderer.RenderTrail(oldPos, new PrimitiveSettings(WidthFunction, ColorFunction, _ => Vector2.Zero, Shader: trailShader, Smoothen: false), oldPos.Length);
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 9);
        }
        */





    }
}
