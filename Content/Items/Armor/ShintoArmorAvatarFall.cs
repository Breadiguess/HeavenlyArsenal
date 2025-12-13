using HeavenlyArsenal.Core.Systems;
using Luminance.Assets;
using Luminance.Core.Graphics;
using Luminance.Core.Sounds;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.Particles;
using NoxusBoss.Content.Particles.Metaballs;
using NoxusBoss.Core.GlobalInstances;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.Items.Armor
{
    public class ShintoArmorAvatarFall : ModPlayer
    {
        public bool Active;
        public float SlamPower;
        public bool Slamming;
        private float CachedVelocity;
        /// The looped sound used for FALLING at a 'normal' pace.
        /// </summary>
        public LoopedSoundInstance? FastFallLoop
        {
            get;
            set;
        }
        public override void ResetEffects()
        {
            // Slam resets each tick unless actively rebuilt.
            Active = false;
            Slamming = false;

        }

        public override void PostUpdateRunSpeeds()
        {
            bool inAir = !WorldGen.SolidOrSlopedTile(Main.tile[(Player.Bottom / 16f).ToPoint()])
                          && !Collision.SolidCollision(Player.position, Player.width, Player.height);

            if (Active)
            {
                // Slam charging

                float fallSpeedInterpolant = InverseLerp(25f, 130f, Player.velocity.Y);
                if (Player.controlDown && Player.velocity.Y > 1f && !Player.mount.Active)
                {
                    SlamPower++;
                    // Update the looping sound.

                }
                else
                {
                    SlamPower = 0;
                }

                if (SlamPower == 1)
                {
                    FastFallLoop?.Stop();
                    FastFallLoop = LoopedSoundManager.CreateNew(GennedAssets.Sounds.Avatar.GravitySlamFallLoop);

                }
                SlamPower = MathHelper.Clamp(SlamPower, 0, 40);

                if (SlamPower > 3)
                    Player.GetModPlayer<PlayerDataManager>().MaxFallSpeedBoost.Value = 170f;
                // Detect slam landing
                if (!inAir && SlamPower >= 40)
                    Slamming = true;

                if (SlamPower > 0)
                    FastFallLoop?.Update(Player.Center, sound =>
                    {
                        sound.Volume = float.Lerp(-0.15f, 2.9f, fallSpeedInterpolant);
                        sound.Pitch = fallSpeedInterpolant * 0.74f;
                      
                    });
                else
                    if (FastFallLoop != null)
                    FastFallLoop.Stop();

                
                if (CachedVelocity < Math.Abs(Player.velocity.Y))
                    CachedVelocity = Math.Max(Math.Abs(Player.velocity.Y), CachedVelocity);

                if (Slamming)
                {
                    // 4x player height
                   
                    int distance = (int)(Player.height * 4f) + (int)(Math.Abs(Player.velocity.Y * 4));

                    // Convert world coords to tile coords
                    Point start = Player.Center.ToTileCoordinates();
                    Point end = new Point(start.X, start.Y + distance / 16);

                    Point? hit = LineAlgorithm.RaycastTo(start.X, start.Y, end.X, end.Y);

                    if (hit.HasValue)
                    {
                        Player.immune = true;
                        Player.immuneTime = 12;
                        Player.immuneNoBlink = true;

                    }
                }
                

               
                if (Slamming)
                {
                    //a bit silly, but very fun!
                    float downwardVel = CachedVelocity; 
                    float radius = MathHelper.Clamp(downwardVel / 60, 1f, 5f);
                    int baseDamage = (int)(Player.statLifeMax2 + Math.Pow(downwardVel, 4));
                   
                    Projectile slam = Projectile.NewProjectileDirect(
                        Player.GetSource_FromThis(),
                        Player.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<SlamHitbox>(),
                        baseDamage,
                        0f,
                        Player.whoAmI,
                        radius,
                        downwardVel
                    );
                    slam.scale = radius;
                    Player.immune = true;
                    Player.immuneTime = 12;
                    Player.immuneNoBlink = true;

                    CachedVelocity = 0;



                    if (FastFallLoop != null)
                        FastFallLoop?.Stop();
                   
                    ModContent.GetInstance<TileDistortionMetaball>().CreateParticle(Player.Bottom, Vector2.Zero, 120f);
                    SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.GravitySlamImpact, Player.Center);

                    int dist = (int)(500 * fallSpeedInterpolant);
                    for (float dx = -dist; dx < dist; dx += Main.rand.NextFloat(14f, 31f))
                    {
                        float dustSizeInterpolant = Main.rand.NextFloat();
                        float dustScale = float.Lerp(1f, 2f, dustSizeInterpolant);
                        Vector2 dustVelocity = (-Vector2.UnitY * Main.rand.NextFloat(30f, 44f) + Main.rand.NextVector2Circular(10f, 20f)) / dustScale;

                        Point groundSearchPoint = (Player.Center + Vector2.UnitX * dx).ToTileCoordinates();
                        Point groundTilePosition = AvatarOfEmptiness.FindGroundVerticalPlatforms(groundSearchPoint);
                        groundTilePosition.Y++;

                        Tile tile = Framing.GetTileSafely(groundTilePosition);
                        Color dustColor = AvatarOfEmptiness.DoBehavior_RubbleGravitySlam_CalculateImpactColor(tile);

                        Vector2 groundPosition = groundTilePosition.ToWorldCoordinates();
                        SmallSmokeParticle dust = new SmallSmokeParticle(groundPosition, dustVelocity, dustColor, Color.Transparent, dustScale, 200f);
                        dust.Spawn();

                        for (int j = 0; j < 3; j++)
                            WorldGen.KillTile_MakeTileDust(groundTilePosition.X, groundTilePosition.Y, tile);
                    }

                    for (int i = 0; i < 32; i++)
                    {
                        Vector2 dustSpawnPosition = Player.Center + Vector2.UnitY * 20f + Main.rand.NextVector2Circular(10f, 4f);
                        Tile groundTilePosition = Framing.GetTileSafely((dustSpawnPosition + Vector2.UnitY * 24f).ToTileCoordinates());
                        Color dustColor = AvatarOfEmptiness.DoBehavior_RubbleGravitySlam_CalculateImpactColor(groundTilePosition);

                        Vector2 dustVelocity = Vector2.UnitX.RotatedByRandom(0.2f) * Main.rand.NextFloat(5f, 67f) * Main.rand.NextFromList(-1f, 1f);
                        float dustScale = dustVelocity.Length() / 32f;
                        SmallSmokeParticle impactDust = new SmallSmokeParticle(dustSpawnPosition, dustVelocity, dustColor, Color.Transparent, dustScale, 200f);
                        impactDust.Spawn();
                    }

                    ScreenShakeSystem.StartShakeAtPoint(Player.Bottom, downwardVel / 5);
                    SlamPower = 0;
                }

            }
            else
            {
                SlamPower = 0;
            }
        }
    }
    public class SlamHitbox : ModProjectile
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetDefaults()
        {
            Projectile.width = 300;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 10;
            Projectile.hide = true;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public float Radius => Projectile.ai[0];
        public float VelocityPower => Projectile.ai[1];

        public override void AI()
        {
            //Projectile.scale = Projectile.ai[0];
            // //Projectile.width = (int)(300 * Projectile.scale);
            // Projectile.height = (int)(160 * Projectile.scale);
            if (Projectile.timeLeft == 10)
                Projectile.Center = Main.player[Projectile.owner].Center;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {

        }
    }
}

