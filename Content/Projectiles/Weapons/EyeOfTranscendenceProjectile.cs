using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons;

public class EyeOfTranscendenceProjectile : ModProjectile
{
    // --- Configuration Constants ---
    private const float NemesisChance = 0.10f; // 10% chance to do double damage.
    private const int MaxChainCount = 3;       // Maximum targets to chain to.
    private const int NumFractalPoints = 100;    // Number of points along the fractal path.
    private const float MoveSpeed = 15f;         // Speed of the projectile along the path.

    // --- Custom Fields ---
    private List<Vector2> pathPoints;
    private int currentPathIndex;
    private int chainsDone;


    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public static bool KeybindPressed = false;


    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.aiStyle = 0; // Custom AI.
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Magic;  // Adjust damage type as needed.
        Projectile.penetrate = -1; // Infinite penetration.
        Projectile.timeLeft = 600; // Lasts 10 seconds by default.
        Projectile.light = 0.5f;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false; // Allow it to travel through tiles.
    }

    public override void OnSpawn(IEntitySource source)
    {
        // When spawned, generate a fractal path towards the designated target.
        // Here, we use the player's mouse position as a simple target.
        Vector2 targetPosition = Main.MouseWorld;
        pathPoints = GenerateFractalPath(Projectile.Center, targetPosition, NumFractalPoints);
        currentPathIndex = 0;
        chainsDone = 0;
    }

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];

        // If the special keybind is pressed, annihilate all enemies on-screen.
        if (KeybindPressed)
        {
            AnnihilateEnemiesOnScreen(player);
            KeybindPressed = false;
        }

        // Follow the fractal path:
        if (currentPathIndex < pathPoints.Count)
        {
            MoveAlongPath();
            CreateFractalDust(Projectile.Center);
        }
        else
        {
            // Path complete—either hold position or continue with simple forward motion.
            Projectile.velocity *= 0.98f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // Determine if Nemesis effect triggers.
        if (Main.rand.NextFloat() < NemesisChance)
        {
            // Apply double damage by modifying damage (this example assumes hit.Damage is mutable).
            // In actual practice you might want to call target.StrikeNPC with adjusted values.
            hit.Damage *= 2;

            // Optionally: create a visual or sound effect to highlight the Nemesis effect.
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
        }
        else
        {
            // No critical—trigger a fractal explosion.
            CreateFractalExplosion(Projectile.Center, Projectile.damage);
        }

        // Attempt to chain to a new enemy if we haven't reached max chains.
        if (chainsDone < MaxChainCount)
        {
            NPC newTarget = FindNearestEnemy(target.Center, 400f); // search radius can be adjusted.
            if (newTarget != null && newTarget.whoAmI != target.whoAmI)
            {
                // Spawn a new projectile chain to the next target.
                Vector2 spawnPosition = Projectile.Center;
                Vector2 targetPos = newTarget.Center;

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition,
                    (targetPos - spawnPosition).SafeNormalize(Vector2.Zero) * MoveSpeed,
                    Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner,
                    ai0: 0, ai1: chainsDone + 1);
            }
        }
        // Kill the current projectile after impact.
        Projectile.Kill();
    }

    public override void Kill(int timeLeft)
    {
        // On projectile death, create an explosion of visual effects.
        CreateFractalDust(Projectile.Center);
        // Optionally: Play a sound effect.
        SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
    }

    /// <summary>
    /// Moves the projectile along the pre-generated fractal path.
    /// </summary>
    private void MoveAlongPath()
    {
        if (currentPathIndex >= pathPoints.Count)
            return;

        Vector2 targetPoint = pathPoints[currentPathIndex];
        Vector2 direction = targetPoint - Projectile.Center;
        float distance = direction.Length();

        if (distance < MoveSpeed)
        {
            Projectile.Center = targetPoint;
            currentPathIndex++;
        }
        else
        {
            direction.Normalize();
            Projectile.velocity = direction * MoveSpeed;
        }
    }

    /// <summary>
    /// Generates a pseudo-fractal path between two points.
    /// In this simple example we create a sequence of randomized waypoints.
    /// </summary>
    private List<Vector2> GenerateFractalPath(Vector2 start, Vector2 end, int points)
    {
        List<Vector2> fractalPoints = new List<Vector2>();
        fractalPoints.Add(start);

        // Calculate the difference vector
        Vector2 diff = end - start;

        for (int i = 1; i < points; i++)
        {
            float t = i / (float)points;
            // Linear interpolation
            Vector2 lerped = Vector2.Lerp(start, end, t);
            // Add randomness perpendicular to the straight path.
            float deviation = (float)(Main.rand.NextDouble() - 0.5) * 50f * (1 - t);
            Vector2 perpendicular = new Vector2(-diff.Y, diff.X);
            if (perpendicular != Vector2.Zero)
                perpendicular.Normalize();
            lerped += perpendicular * deviation;
            fractalPoints.Add(lerped);
        }

        fractalPoints.Add(end);
        return fractalPoints;
    }

    /// <summary>
    /// Creates dust effects along the projectile's path to simulate a fractal/astral effect.
    /// </summary>
    private void CreateFractalDust(Vector2 position)
    {
        // Adjust dust type and properties as needed.
        int dustIndex = Dust.NewDust(position, 1, 1, DustID.MagicMirror);
        Main.dust[dustIndex].velocity *= 0.5f;
        Main.dust[dustIndex].scale = 1.2f;
    }

    /// <summary>
    /// Creates a fractal explosion effect for AoE damage.
    /// Note: The actual damage application may need to be implemented in a separate explosion projectile.
    /// </summary>
    private void CreateFractalExplosion(Vector2 center, int explosionDamage)
    {
        // Create visual dust explosion
        for (int i = 0; i < 50; i++)
        {
            Vector2 velocity = Main.rand.NextVector2CircularEdge(5f, 5f);
            int dust = Dust.NewDust(center, 10, 10, DustID.Fireworks);
            Main.dust[dust].velocity = velocity;
            Main.dust[dust].scale = 1.5f;
        }

        // Deal AoE damage: iterate over NPCs in a radius and apply damage.
        float explosionRadius = 100f;
        foreach (NPC npc in Main.npc)
        {
            if (npc.active && !npc.friendly && npc.Distance(center) < explosionRadius)
            {
               
                NPC.HitInfo hitInfo = new NPC.HitInfo
                {
                    Damage = explosionDamage,
                    Knockback = 5f,
                    HitDirection = 0
                };
                npc.StrikeNPC(hitInfo);
            }
        }
    }

    /// <summary>
    /// Finds the nearest hostile NPC within the specified radius from a center point.
    /// Returns null if none are found.
    /// </summary>
    private NPC FindNearestEnemy(Vector2 center, float radius)
    {
        NPC nearest = null;
        float closestDist = radius;
        foreach (NPC npc in Main.npc)
        {
            if (npc.active && !npc.friendly && !npc.dontTakeDamage)
            {
                float distance = Vector2.Distance(center, npc.Center);
                if (distance < closestDist)
                {
                    closestDist = distance;
                    nearest = npc;
                }
            }
        }
        return nearest;
    }

    /// <summary>
    /// Annihilate all hostile enemies on the screen.
    /// This is a placeholder; you might want to add visual effects or sound cues.
    /// </summary>
    private void AnnihilateEnemiesOnScreen(Player player)
    {
        foreach (NPC npc in Main.npc)
        {
            if (npc.active && !npc.friendly && npc.Distance(player.Center) < 800f) // assuming 800 pixels as "on-screen"
            {
                // Create a HitInfo object to pass to StrikeNPC
                NPC.HitInfo hitInfo = new NPC.HitInfo
                {
                    Damage = npc.lifeMax, // Deal maximum damage to kill the NPC
                    Knockback = 0f,      // No knockback
                    HitDirection = 0     // Neutral hit direction
                };

                // Kill the NPC using the HitInfo object
                npc.StrikeNPC(hitInfo);
            }
        }
        // Optionally, create a screen shake or visual effect here.
        SoundEngine.PlaySound(SoundID.Item29, player.Center);
    }
}
