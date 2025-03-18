using System;
using CalamityMod;
using CalamityMod.Dusts;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using HeavenlyArsenal.ArsenalPlayer;
using static Luminance.Common.Utilities.Utilities;
using CalamityMod.CalPlayer;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Projectiles.Misc
    
{
    public class  VambraceDash: ModProjectile, ILocalizedModType, IPixelatedPrimitiveRenderer
    {

        public int Time
        {
            get;
            set;
        }

        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];
        private static float ExplosionRadius = 75f;


        public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.BeforeProjectiles;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
        }


        public override void SetDefaults()
        {
            //These shouldn't matter because its circular
            Projectile.width = 75;
            Projectile.height = 75;
            Projectile.friendly = true;
            Projectile.DamageType = AverageDamageClass.Default;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 4;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 22;
            
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            HeavenlyArsenalPlayer modPlayer = player.GetModPlayer<HeavenlyArsenalPlayer>();

           
            Time++;
            if ((Main.myPlayer == Projectile.owner) && modPlayer.isVambraceDashing)
            {
                //Console.WriteLine("VambraceDash!!!");
                //Projectile.oldPos = 
                Projectile.timeLeft += 4;
                Projectile.position = new Vector2(Owner.MountedCenter.X+player.velocity.X
                    -Projectile.width/2
                    //+(Owner.direction)
                    ,Owner.MountedCenter.Y-25f);
            }
        }


        public override void OnSpawn(IEntitySource source)
        {
            int projectileType = ModContent.ProjectileType<VambraceDash>();
            int count = 0;

            // Loop through all projectiles
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == projectileType)
                {
                    count++;
                }
            }

            // Output the count to the in-game chat
            if (Main.netMode != NetmodeID.Server) // Ensure it's not a server-only environment
            {
                Main.NewText($"Number of VambraceDash projectiles: {count}", Color.Cyan);
            }
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 240);
            //target.AddBuff(ModContent.BuffType<Buffs.StatDebuffs.ArmorCrunch>(), 300);



            //SoundEffect VambraceHit = ModContent.Request<SoundEffect>("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_fire2").Value;
            SoundEngine.PlaySound(SoundID.DD2_LightningBugZap with { Volume = 0.6f, PitchVariance = 0.4f }, Projectile.Center);
            
            //for (int i = 0; i <= 8; i++)
            //{
            //    Dust dust = Dust.NewDustPerfect(target.Center, Main.rand.NextBool() ? 174 : 127, new Vector2(0, -2).RotatedByRandom(MathHelper.ToRadians(30f)) * Main.rand.NextFloat(2f, 4.5f), 0, default, Main.rand.NextFloat(2.8f, 3.4f));
            //    dust.noGravity = false;
            //}
            //for (int i = 0; i <= 5; i++)
            //{
            //    Dust dust2 = Dust.NewDustPerfect(target.Center, Main.rand.NextBool() ? 174 : 127, new Vector2(0, -3).RotatedByRandom(MathHelper.ToRadians(8f)) * Main.rand.NextFloat(1f, 5f), 0, default, Main.rand.NextFloat(2.8f, 3.4f));
            //    dust2.noGravity = false;
            //}

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<VambraceDischarge>(), Projectile.damage / 2, 15f, Projectile.owner);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(new Vector2(Owner.position.X + 4 * Owner.direction, Owner.position.Y), ExplosionRadius, targetHitbox);
        public override bool? CanDamage() => base.CanDamage();
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = Math.Sign(Owner.direction);
        }

        public override bool? CanCutTiles() => false;



        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D BloomCircleSmall = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/TrailStreaks/StreakMagma").Value;


            float scaleFactor = Projectile.width / 50f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + Projectile.velocity;
            //Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.DarkRed) with { A = 0 } * 0.2f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 1.2f, 0, 0f);
            //Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.Red) with { A = 0 } * 0.4f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 0.64f, 0, 0f);
            //Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.Orange) with { A = 0 } * 0.4f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 0.3f, 0, 0f);
            return false;
        }



        public float ElectricWidth(float completionRatio)
        {
            float baseWidth = 30f;
            float smoothTipCutoff = MathHelper.SmoothStep(0f, 1f, InverseLerp(0.09f, 0.1f, completionRatio));
            return smoothTipCutoff * baseWidth;
        }

        public Color BloodColorFunction(float completionRatio)
        {
            return Projectile.GetAlpha(new Color(255, 240, 255));
        }


        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            Texture2D BubblyNoise = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/TrailStreaks/StreakMagma").Value;
            Texture2D DendriticNoiseZoomedOut = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/Noise/DendriticNoiseZoomedOut").Value;



            Rectangle viewBox = Projectile.Hitbox;
            Rectangle screenBox = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
            viewBox.Inflate(540, 540);
            if (!viewBox.Intersects(screenBox))
                return;

            float lifetimeRatio = Time / 240f;
            float dissolveThreshold = InverseLerp(0.67f, 1f, lifetimeRatio) * 0.5f;

            ManagedShader BloodShader = ShaderManager.GetShader("HeavenlyArsenal.VambraceDash_Shader");
            BloodShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly + Projectile.identity * 72.113f);
            BloodShader.TrySetParameter("dissolveThreshold", dissolveThreshold);
            BloodShader.TrySetParameter("accentColor", new Vector4(1.21f, 2.38f, 2.55f, 0f));
            BloodShader.SetTexture(BubblyNoise, 1, SamplerState.LinearWrap);
            BloodShader.SetTexture(DendriticNoiseZoomedOut, 2, SamplerState.LinearWrap);

            

            PrimitiveSettings settings = new PrimitiveSettings(ElectricWidth, BloodColorFunction, _ => Projectile.Size * 0.5f + Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.56f, Pixelate: true, Shader: BloodShader);
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 9);
        }
    }
}