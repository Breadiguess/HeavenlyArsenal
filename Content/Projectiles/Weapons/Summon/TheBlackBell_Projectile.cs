using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Buffs;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using Terraria.DataStructures;
using CalamityMod.Projectiles.BaseProjectiles; // Ensure this is the namespace for your Rope class

using NoxusBoss.Content.Particles;
using CalamityMod.Enums;
using CalamityMod.Particles;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Summon
{
    public class TheBlackBell_Projectile : ModProjectile
    {
        private Rope rope; // Instance of the Rope
        private const int segmentCount = 20; // Number of rope segments
        private const float segmentLength = 7f; // Length of each rope segment
        private readonly Vector2 gravity = new Vector2(0, 0.2f); // Simulate rope sag due to gravity
        //private int counter = 0;
        private int cooldownTimer = 0; // Timer for cooldown management
        private const int cooldownDuration = 30; // Cooldown duration in ticks (30 ticks = 0.5 seconds at 60 FPS)
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1;
            Projectile.penetrate = -1; // Infinite lifespan
            Projectile.tileCollide = false; // Don't collide with tiles
            Projectile.ignoreWater = true;
            Player player = Main.player[Projectile.owner];


            rope = new Rope(new Vector2(player.Center.X, player.Center.Y), Projectile.Center, segmentCount, segmentLength, gravity);
        }
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            
            Projectile.position = new Vector2(player.Center.X, player.Center.Y - 50f);
            player.AddBuff(ModContent.BuffType<TheBlackBell_Buff>(), 60, true, false);
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Calculate the desired position of the minion based on the player's position
            Vector2 desiredPosition = player.Center + new Vector2(-60f*player.direction,0f); // Hover 50 pixels behind the player
            Vector2 directionToDesired = desiredPosition - Projectile.Center;

            // Set drag threshold for triggering the hit effect
            float dragThreshold = 30f; // Adjust this value for sensitivity

            // Drag behavior: smoothly move toward the desired position
            float dragSpeed = 0.07f; // Lower values create stronger lag (being "dragged")
            Projectile.velocity += directionToDesired * dragSpeed;
            Projectile.velocity *= 0.5f; // Friction to smooth movement

            // Check if dragged too fast (exceeds threshold)
            if (Projectile.velocity.Length() > dragThreshold)
            {
                TriggerHitEffect();
            }

            // Ensure the minion stays active as long as the player has the buff
            if (player.HasBuff(ModContent.BuffType<TheBlackBell_Buff>())) // Replace 'YourBuffClass' with your actual buff class
            {
                //Projectile.Kill();
            }
            else
            {
                Projectile.Kill();
            }

            // Update the rope's start and end positions
            if (rope != null)
            {
                rope.segments[0].position = player.Center; // Pin the first segment to the player
                rope.segments[^1].position = Projectile.Center; // Pin the last segment to the minion
                rope.Update(); // Update the rope physics
            }
            if (cooldownTimer > 0)
                cooldownTimer--;
            // Check for collisions with Projectiles
            // Check for collisions with projectiles using the Colliding method
            foreach (Projectile otherProjectile in Main.ActiveProjectiles)
            {
                if (otherProjectile.owner == player.whoAmI && // Player-owned
                    otherProjectile.DamageType == DamageClass.SummonMeleeSpeed &&(
                    otherProjectile.Hitbox.Intersects(Projectile.Hitbox) ||// Check hitbox intersection
                     Colliding(otherProjectile.Hitbox, Projectile.Hitbox) == true)) // Check custom collision

                {
                  
                    TriggerHitEffect();
                    cooldownTimer = cooldownDuration; // Reset cooldown timer
                }
            }
            //Player player = Main.player[Projectile.owner];
            //if player.Dash.
            //if player.dashType.GetType == DashCollisionType.ShieldSlam
            //{

            //}


        }

        // Method to trigger the hit effect
        private void TriggerHitEffect()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.NewProjectile(Projectile.GetSource_FromThis(null), Projectile.Center.X, Projectile.Center.Y, 0f, 0f, ModContent.ProjectileType<PsychedelicFeather>(), -1, 0, player.whoAmI);
            // Visual effects when dragged too fast or hit
            for (int i = 0; i < 12; i++)
            {

                DeltaruneExplosionParticle deltaruneExplosionParticle = new DeltaruneExplosionParticle(Projectile.Center, Vector2.Zero, Color.AntiqueWhite, 48, 1);
                //GeneralParticleHandler.SpawnParticle(deltaruneExplosionParticle);
                
                float angle = Main.rand.NextFloat(MathHelper.TwoPi); // Randomized angle for dust effects
                Dust.NewDustPerfect(
                    Projectile.Center + angle.ToRotationVector2() * 20,
                    DustID.RuneWizard, 
                    angle.ToRotationVector2() * Main.rand.NextFloat(2f, 6f), // Randomized velocity
                    0,
                    new Color(0.8f, 0.2f, 0.2f), // Custom color for the dust
                    Main.rand.NextFloat(1.2f, 1.8f) // Randomized scale
                );
            }
        }


        //Projectile.NewProjectile(Projectile.GetSource_FromThis(null), Projectile.Center.X, Projectile.Center.Y, 0f, 0f, ModContent.ProjectileType<PsychedelicFeather>(), -1, 0, player.whoAmI);
        //foreach (Projectile otherProjectile in Main.ActiveProjectiles)
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Instrument = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Projectiles/Weapons/Summon/TheBlackBell_Projectile").Value;           
            // Ensure the rope exists
            if (rope != null)
            {
                // Fetch the rope's segment positions
                Vector2[] points = rope.GetPoints();
                for (int i = 0; i < points.Length - 1; i++)
                {
                    Vector2 start = points[i];
                    Vector2 end = points[i + 1];

                    // Calculate rotation for the segment
                    float rotation = (end - start).ToRotation();

                    // Request the texture
                    Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/TheBlackBell_String").Value;

                    // Define the origin and calculate scale based on segment length
                    Vector2 origin = new Vector2(texture.Width / 2f, 0f); // Top-center origin
                    float segmentLength = (end - start).Length(); // Length of the current segment

                    // Draw the segment
                    Main.spriteBatch.Draw(
                        texture,
                        start - Main.screenPosition, // Position adjusted for screen
                        null, // Full texture
                        lightColor, // Pass the provided light color
                        rotation+MathHelper.PiOver2, // Rotation to align with the segment
                        origin, // Origin point for rotation and positioning
                        new Vector2(1f, segmentLength / texture.Height), // Scale: width stays 1, height matches segment length
                        SpriteEffects.None, // No special effects
                        0f // Draw order
                    );
                }
            }

            return true; // Allow the minion to be drawn normally
        }
    }
}
