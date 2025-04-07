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


namespace HeavenlyArsenal.Content.Projectiles.Weapons.Ranged.FusionRifleProj
{
    internal class FusionRifle_Projectile : ModProjectile//, IPixelatedPrimitiveRenderer
    {
        private Vector2[] oldPos;
        public int Time
        {
            get;
            set;
        }


        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public static float SmoothStep(float edge0, float edge1, float value)
        {
            // Clamp the input value to the range [0, 1]
            value = Math.Clamp((value - edge0) / (edge1 - edge0), 0f, 1f);

            // Perform the smoothstep interpolation
            return value * value * (3f - 2f * value);
        }

        public float BloodWidthFunction(float completionRatio)
        {
            float baseWidth = Projectile.width * 0.66f;
            float smoothTipCutoff = SmoothStep(0f, 1f, MathHelper.Lerp(0.09f, 0.3f, completionRatio));
            return smoothTipCutoff * baseWidth;
        }

        public Color BloodColorFunction(float completionRatio)
        {
            return Projectile.GetAlpha(new Color(82, 1, 23));
        }


        public override void SetStaticDefaults()
        {
            //Main.projFrames[Projectile.type] = 35;
        }

        public override void SetDefaults()
        {
            
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            //Projectile.ApplyStatsFromSource = true;

            Projectile.usesLocalNPCImmunity = true;
            //Projectile.localNPCHitCooldown = -1; // 1 hit per npc max
            Projectile.localNPCHitCooldown =1; // 20 ticks before the same npc can be hit again


            AIType = ProjectileID.Bullet;
            Projectile.idStaticNPCHitCooldown = 8;
           
        }
        public override void AI()
        {
         
           Projectile.aiStyle = 0;

            // Lighting.AddLight(Projectile.position, 0.2f, 0.2f, 0.6f);
            // Lighting.Brightness(1, 1);
            Terraria.Dust.NewDustPerfect(Projectile.Left, DustID.AncientLight, Vector2.Zero,150,Color.AntiqueWhite,1);
            //Terraria.Dust.NewDustPerfect(Projectile.position,
                //+ new Vector2(Projectile.velocity.X, 0f), 
             //   DustID.AncientLight, Projectile.velocity, 150, default, 1f);   //spawns dust behind it, this is a spectral light blue dust

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity *= 1.01f;


            //if (++Projectile.frameCounter >= 1)
            //{
             //   Projectile.frameCounter = 0;
              //  // Or more compactly Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
               // if (++Projectile.frame >= Main.projFrames[Projectile.type])
               //     Projectile.frame = 0;   
           // }



        }

        public override bool PreAI()
        {
            if (oldPos == null)
                oldPos = Enumerable.Repeat(Projectile.Center, 20).ToArray();

            for (int i = oldPos.Length - 2; i > 0; i--)
            {
                oldPos[i] = oldPos[i - 1];
            }

            oldPos[0] = Projectile.Center + Projectile.velocity * 2;
            return false;
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), // Adding Poisoned to target
                 600); // for 5 seconds (60 ticks = 1 second)
        }



        public override void OnSpawn(IEntitySource source)
        {
           // FusionRifleHoldout.BurstCount++;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig.WithVolumeScale(0.5f).WithPitchOffset(0.8f), Projectile.position);
            //for (var i = 0; i < 6; i++)
            //{
            //    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AncientLight, 0f, 0f, 100, default(Color), 1f);
            //}
        }



        public override bool PreDraw(ref Color lightColor)
        {
           
            float WidthFunction(float p) => 50f * MathF.Pow(p, 0.66f) * (1f - p * 0.5f);
            //Color ColorFunction(float p) => new Color(8, 6, 20, 200);
            Color ColorFunction(float p) => new Color(60, 0, 150, 200);

            ManagedShader trailShader = ShaderManager.GetShader("HeavenlyArsenal.FusionRifle_Bullet");
            trailShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * Projectile.velocity.Length() / 8f + Projectile.identity/72.113f);
            trailShader.TrySetParameter("spin", 1f* Math.Sign(Projectile.velocity.X));
            trailShader.TrySetParameter("brightness",  1.5f);
            trailShader.SetTexture(Noise.TurbulentNoise, 0, SamplerState.LinearWrap);
            trailShader.SetTexture(Noise.FireNoiseB, 1, SamplerState.LinearWrap);
            trailShader.SetTexture(Noise.DendriticNoiseZoomedOut, 2, SamplerState.LinearWrap);

            PrimitiveRenderer.RenderTrail(oldPos, new PrimitiveSettings(WidthFunction, ColorFunction, _ => Vector2.Zero, Shader: trailShader, Smoothen: false), oldPos.Length);


            return true;
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
