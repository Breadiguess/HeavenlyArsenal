using CalamityMod;
using HeavenlyArsenal.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon
{
    internal class CrystalArrow : ModProjectile
    {
        public ref float flip => ref Projectile.localAI[0];
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Timer => ref Projectile.ai[0];
        public int StuckID
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public int Charge
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }
        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.Size = new Vector2(60, 60);
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 2;
            StuckID = -1;
            Projectile.timeLeft = 1800;
            Projectile.extraUpdates = 3;
        }

        public override void OnSpawn(IEntitySource source)
        {
            //Main.NewText("Projectile was spawned correctly: " + Projectile.ToString());
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Charge = 0;
            Projectile.damage = 0;
            return base.OnTileCollide(oldVelocity);
        }
        public override void AI()
        {
            if (Projectile.velocity.Length() > 3)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
           

            if (StuckID != 0)
            {
                StickAndExhume();
            }
            Timer++;
        }
       

        public void StickAndExhume()
        {
            NPC victim = Main.npc[StuckID];
            if (victim == null)
                return;
            Projectile.tileCollide = false;
            Projectile.Center = victim.Center;

            if(Timer <= Charge)
            {
                float baseRotation = Owner.MountedCenter.AngleTo(victim.Center);
                int rings = Charge;
                int pointsPerRing = 3;
                float baseRadius = 100f;

                for (int ring = 0; ring <= rings; ring++)
                {
                    float radius = baseRadius * ring;
                    float ringRotation = ring * 0.4f;
                    float totalRotation = baseRotation + ringRotation;

                    for (int i = 0; i < pointsPerRing; i++)
                    {
                        float angle = MathHelper.TwoPi * i / pointsPerRing + totalRotation;

                        Vector2 offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
                        Vector2 pos = victim.Center + offset;

                        Dust d = Dust.NewDustPerfect(pos, DustID.Cloud);
                        d.velocity = Vector2.Zero;

                        Vector2 aimDir = (victim.Center - pos).SafeNormalize(Vector2.UnitY);
                        Vector2 projVel = aimDir * Owner.HeldItem.shootSpeed;

                        Projectile.NewProjectile(
                           Projectile.GetSource_FromThis(),
                           pos,
                           projVel,
                           ModContent.ProjectileType<EntropicCrystal>(),
                           Owner.HeldItem.damage / 10,
                           Owner.HeldItem.knockBack,
                           Owner.whoAmI,
                           ai0: 1,
                           ai1: 0f
                       );
                    }
                }
            }
            






            /*
            if (victim.active && Timer % 5 == 0)
            {

                int CrystalAmount = Charge > 4 ? (int)Charge / 2 : (int)Charge;
                float spawnHeight = 600f;
                float horizSpread = 200f;
                int delayPerCrystal = 3;  

                for (int i = 0; i < CrystalAmount; i++)
                {
                    // random X offset
                    float offsetX = Main.rand.NextFloat(-horizSpread, horizSpread);
                    Vector2 spawnPos = new Vector2(victim.Center.X + offsetX, victim.Center.Y - spawnHeight);


                    Vector2 aimDir = (victim.Center - spawnPos).SafeNormalize(Vector2.UnitY);
                    aimDir = Vector2.Lerp(Vector2.UnitY, aimDir, 0.5f);
                    Vector2 projVel = aimDir * Owner.HeldItem.shootSpeed;


                    float startDelay = i * delayPerCrystal;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos,
                        projVel,
                        ModContent.ProjectileType<EntropicCrystal>(),
                        Owner.HeldItem.damage/10,
                        Owner.HeldItem.knockBack,
                        Owner.whoAmI,
                        ai0: startDelay,
                        ai1: 0f
                    );


                }

            }*/
            if (Timer > 360 || !victim.active)
               ShatterArrow();
        }
        public void ShatterArrow()
        {
            //SoundEngine.PlaySound(GennedAssets.Sounds.Custom.LowTierGodLightning with { PitchVariance = 2});
            Projectile.Kill();

        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            if (target.lifeMax < hit.Damage)
                //do nothing 
                return;
            StuckID = target.whoAmI;
            Timer = 0;
            Projectile.damage = -1;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.isAPreviewDummy)
                return base.PreDraw(ref lightColor);
            Texture2D Arrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/CrystalArrow").Value;
            Texture2D GlowArrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/CrystalArrow_Glow").Value;

            Texture2D IntenseArrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/CrystalArrow_HeadGlow").Value;
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;

            Vector2 Origin = new Vector2(Arrow.Width, Arrow.Height / 2);

            float Rot = Projectile.rotation;
            float Scale = Projectile.scale;
            SpriteEffects Flip = flip == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(Arrow, DrawPos, null, lightColor, Rot, Origin, Scale, SpriteEffects.None);

            Main.EntitySpriteDraw(GlowArrow, DrawPos, null, Color.White, Rot, Origin, Scale, SpriteEffects.None);
            ;

            Texture2D debug = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            Vector2 ArrowHead = Projectile.Center + new Vector2(10, 0).RotatedBy(Projectile.rotation);

            for (int i = 0; i < Charge; i++)
            {

                float Wane = Math.Abs(MathF.Sin(Main.GlobalTimeWrappedHourly * 10.1f + 10 - i * 10) * 0.2f) + 0.8f;
                Vector2 IntenseArrowOrigin = new Vector2(IntenseArrow.Width, IntenseArrow.Height / 2);

                float thing = (i * 5 - 15) * MathF.Cos(Main.GlobalTimeWrappedHourly * 10.1f) * 0.2f;

                Vector2 Adjusted = ArrowHead + new Vector2(-10, thing).RotatedBy(Projectile.rotation);
                float adjustedRot = Adjusted.AngleTo(ArrowHead);

                //Main.EntitySpriteDraw(debug, ArrowHead - Main.screenPosition, null, Color.Green, 0, debug.Size() / 2, 4, 0);
                //Main.EntitySpriteDraw(debug, Adjusted - Main.screenPosition, null, Color.Red, 0, debug.Size() / 2, 4, 0);
                Main.EntitySpriteDraw(IntenseArrow, Adjusted - Main.screenPosition, null, Color.AntiqueWhite * 0.6f, adjustedRot, IntenseArrowOrigin, Wane, 0);
            }
            return false;
        }
    }
}
