using System.Collections.Generic;
using HeavenlyArsenal.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath
{
    public class VoidCrestOathPlayer : ModPlayer
    {
        /// <summary>
        /// True if the accessory is actively equipped in an accessory slot.
        /// (Not in a vanity slot.)
        /// </summary>
        public bool voidCrestOathEquipped;

        /// <summary>
        /// If true, the accessory is in a vanity slot and only provides cosmetic effects.
        /// </summary>
        public bool NotVanity;

        /// <summary>
        /// True if the conflicting accessory WarBannerOftheSun is equipped.
        /// (Set by that accessory’s UpdateAccessory.)
        /// </summary>
        public bool warBannerOftheSunEquipped;

        /// <summary>
        /// The resource used to "pay" for intercepts.
        /// It will decrease when an interception happens and regenerate slowly.
        /// </summary>
        public float InterceptCount;

        /// <summary>
        /// Maximum resource value for intercepting hostile projectiles.
        /// </summary>
        public float MaxInterceptCount = 100f;

        /// <summary>
        /// How much resource is consumed per intercept.
        /// </summary>
        public float InterceptCost = 10f;

        /// <summary>
        /// How much resource is regenerated per tick if no intercept is occurring.
        /// </summary>
        public float InterceptRegenRate = 0.5f;

        /// <summary>
        /// A list to keep track of hostile projectile indices that we are watching.
        /// We rebuild this every tick.
        /// </summary>
        public List<int> trackedProjectileIndices = new List<int>();

        /// <summary>
        /// Detection radius (in pixels) for adding enemy projectiles to the tracking list.
        /// </summary>
        private const float TrackingRadius = 300f;

        /// <summary>
        /// Distance at which an intercept is triggered (in pixels).
        /// </summary>
        private const float InterceptDistance = 50f;

        /// <summary>
        /// Interceptor projectile type. Make sure to define this in your mod.
        /// </summary>
        private int interceptorType => ModContent.ProjectileType<VoidCrestInterceptorProjectile>();

        public override void ResetEffects()
        {
            
            voidCrestOathEquipped = false;
           
            warBannerOftheSunEquipped = false;
        }

        public override void PostUpdate()
        {
            // If the conflicting accessory is equipped, skip all interception logic.
            if (warBannerOftheSunEquipped)
                return;

           
            if (!voidCrestOathEquipped || NotVanity)
            {
                
                RegenerateInterceptCount();
                return;
            }

            // Rebuild the tracking list each tick.
            trackedProjectileIndices.Clear();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active)
                    continue;

                // Only consider projectiles that:
                //  • Are hostile (enemy projectiles)
                //  • Are not friendly (not shot by player)
                //  • Were not spawned by the player (owner check)
                if (proj.hostile && !proj.friendly && proj.owner != Player.whoAmI)
                {
                    float distance = Vector2.Distance(proj.Center, Player.Center);
                    if (distance >= TrackingRadius)
                    {
                        trackedProjectileIndices.Add(i);
                    }
                }
            }

            if (Main.myPlayer == Player.whoAmI)
            {
                Main.NewText($"[VoidCrest] Hostile projectiles in range: {trackedProjectileIndices.Count}", Color.Yellow);
            }


            // Iterate through tracked projectiles
            bool interceptedSomethingThisTick = false;
            foreach (int index in trackedProjectileIndices)
            {
                if (index < 0 || index >= Main.maxProjectiles)
                    continue;

                Projectile proj = Main.projectile[index];
                if (proj == null || !proj.active)
                    continue;

                float distance = Vector2.Distance(proj.Center, Player.Center);

                // ✅ Debug: Display tracked hostile projectiles
                if (Main.myPlayer == Player.whoAmI)
                {
                    CombatText.NewText(Player.Hitbox, Color.Cyan, $"Tracking proj {proj.type} | dist: {distance:F0}");
                }

                if (distance <= InterceptDistance)
                {
                    if (InterceptCount >= InterceptCost)
                    {
                        // ✅ Debug: Intercept triggered
                        if (Main.myPlayer == Player.whoAmI)
                        {
                            Main.NewText($"[VoidCrest] Intercepting projectile {proj.type} at distance {distance:F1}", Color.Red);
                        }

                        Projectile.NewProjectile(
                            Player.GetSource_FromThis(),
                            proj.Center,
                            Vector2.Zero,
                            interceptorType,
                            50,
                            1f,
                            Player.whoAmI
                        );

                        CreateInterceptVisualEffect(proj.Center);
                        InterceptCount -= InterceptCost;
                        proj.Kill();

                        interceptedSomethingThisTick = true;
                    }
                    else
                    {
                        // ✅ Debug: Not enough resource
                        if (Main.myPlayer == Player.whoAmI)
                        {
                            Main.NewText("[VoidCrest] Not enough InterceptCount!", Color.OrangeRed);
                        }
                    }
                }
            }


            // If no intercept happened this tick, regenerate the intercept resource.
            if (!interceptedSomethingThisTick)
                RegenerateInterceptCount();
        }

        /// <summary>
        /// Regenerates InterceptCount until it reaches the maximum.
        /// </summary>
        private void RegenerateInterceptCount()
        {
            if (InterceptCount < MaxInterceptCount)
            {
                InterceptCount += InterceptRegenRate;
                if (InterceptCount > MaxInterceptCount)
                {
                    InterceptCount = MaxInterceptCount;
                }
            }
        }

        /// <summary>
        /// Creates a visual effect (dust, particles, etc.) at the specified position.
        /// 
        /// </summary>
        /// <param name="position">The world position for the visual effect.</param>
        private void CreateInterceptVisualEffect(Vector2 position)
        {
            // Example using Terraria dust
            for (int d = 0; d < 20; d++)
            {
                int dustIndex = Dust.NewDust(
                    position,
                    10, 10,
                    DustID.MagicMirror,
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-3f, 3f),
                    100,
                    default,
                    1.5f
                );
                Main.dust[dustIndex].noGravity = true;
            }
        }
    }
}
