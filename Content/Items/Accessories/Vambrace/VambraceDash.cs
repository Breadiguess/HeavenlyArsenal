using CalamityMod;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.Items.Accessories.Vambrace
    
{
    public class  VambraceDash: ModProjectile
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
            Projectile.DamageType = DamageClass.Default;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 30;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 22;
            
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            ElectricVambracePlayer modPlayer = player.GetModPlayer<ElectricVambracePlayer>();

           
            Time++;
            if (Main.myPlayer == Projectile.owner && modPlayer.isVambraceDashing)
            {
               
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
                //Main.NewText($"Number of VambraceDash projectiles: {count}", Color.Cyan);
            }
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 240);
            //target.AddBuff(ModContent.BuffType<Buffs.StatDebuffs.ArmorCrunch>(), 300);



            //SoundEffect VambraceHit = ModContent.Request<SoundEffect>("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_fire2").Value;
            SoundEngine.PlaySound(SoundID.DD2_LightningBugZap with { Volume = 0.6f, PitchVariance = 0.4f }, Projectile.Center);
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


    
    }
}