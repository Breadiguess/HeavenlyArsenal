using System.Collections.Generic;
using System.Linq;
using HeavenlyArsenal.Common.Ui;
using HeavenlyArsenal.Content.Projectiles;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.Projectiles;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Localization;

namespace HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath;

public class VoidCrestOathPlayer : ModPlayer
{
    private static readonly HashSet<int> BlacklistedProjectiles = new()
    {
        ModContent.ProjectileType<DeadStar>(),
        ModContent.ProjectileType<DeadStarIron>(),
        ModContent.ProjectileType<FrostColumn>(),
        ModContent.ProjectileType<OtherworldlyThorn>(),
        ModContent.ProjectileType<BlackHoleHostile>(),
        ModContent.ProjectileType<SwordConstellation>(),
        ModContent.ProjectileType<ControlledStar>()

        // Add other projectile IDs here
    };

    public override void ResetEffects()
    {
        voidCrestOathEquipped = false;
        Vanity = false;
        TrollMode = false;
        Hide = false;
    }

    public override void PostUpdateMiscEffects()
    {
        if (Cooldown <= 1 && !Vanity)
        {
            ResourceInterp = float.Lerp(ResourceInterp, InterceptCount / MaxInterceptCount, 0.2f);
        }

        if (Vanity)
        {
            ResourceInterp = float.Lerp(ResourceInterp, 1, 0.2f);
        }

        if (!voidCrestOathEquipped)
        {
            return;
        }

        if (Main.specialSeedWorld)
        {
            TrollMode = true;
        }

        if (TrollMode)
        {
            if (!Main.rand.NextBool(19840) && !Player.dead && !Player.immune)
            {
                return;
            }

            var deathMessage = Language.GetTextValue("Mods.HeavenlyArsenal.PlayerDeathMessages.VoidCrest" + Main.rand.Next(1, 3 + 1), Player.name);
            Player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(deathMessage)), 10000.0, 0);
        }

        if (Vanity)
        {
            return;
        }

        var value = InterceptCount / MaxInterceptCount;

        // Main.NewText(InterceptCount);
        // Main.NewText(value);
        WeaponBar.DisplayBar(Color.White, Color.Red, value, BarOffset: new Vector2(0, -40));

        if (Cooldown <= 0)
        {
            ManageTargeting();
        }
        else
        {
            ResourceInterp = 0;
            Cooldown--;

            if (Cooldown == 1)
            {
                InterceptCount = MaxInterceptCount;
                SoundEngine.PlaySound(GennedAssets.Sounds.Common.Glitch);
            }

            RegenerateInterceptCount();
        }
    }

    /// <summary>
    ///     Manages the targeting and interception of hostile projectiles within a specified radius.
    /// </summary>
    /// <remarks>
    ///     This method identifies hostile projectiles within the tracking radius, calculates
    ///     interception costs,  and determines whether the player can afford to intercept them. If
    ///     interception is
    ///     possible, it spawns  interceptor projectiles to neutralize the targets. The method also handles
    ///     regeneration
    ///     of interception  resources when no projectiles are intercepted during the current tick.
    ///     Projectiles are
    ///     prioritized based on their distance to the player, and certain projectiles are excluded  from
    ///     targeting
    ///     based on predefined conditions (e.g., blacklisted types, friendly projectiles, or projectiles
    ///     already
    ///     intercepted).
    /// </remarks>
    private void ManageTargeting()
    {
        InterceptCost = 30;

        trackedProjectileIndices.Clear();
        targetedProjectiles.RemoveAll(id => !Main.projectile[id].active || Main.projectile[id].friendly);

        InterceptRegenRate = 1;

        var interceptedSomethingThisTick = false;

        foreach (var proj in Main.ActiveProjectiles)
        {
            if (BlacklistedProjectiles.Contains(proj.type))
            {
                continue;
            }

            var distance = Vector2.Distance(proj.Center, Player.Center);

            if (proj.hostile &&
                proj.type != interceptorType &&
                !proj.friendly &&
                distance <= TrackingRadius)
            {
                trackedProjectileIndices.Add(proj.whoAmI);
            }
        }

        trackedProjectileIndices.Sort((a, b) => Vector2.Distance(Main.projectile[a].Center, Player.Center).CompareTo(Vector2.Distance(Main.projectile[b].Center, Player.Center)));

        foreach (var index in trackedProjectileIndices.ToList())
        {
            var proj = Main.projectile[index];

            if (!proj.active || proj.friendly || proj.hostile == false || proj.type == interceptorType || proj.type == ModContent.ProjectileType<SmallTeslaArc>())
            {
                continue;
            }

            var distance = Vector2.Distance(proj.Center, Player.Center);

            if (distance > InterceptDistance)
            {
                continue;
            }

            // Calculate cost scaling
            var sizeFactor = proj.width * proj.height / 10000f;
            var cost = InterceptCost * Math.Max(1f, sizeFactor);

            // Only intercept if affordable
            if (InterceptCount > cost && !targetedProjectiles.Contains(proj.whoAmI))
            {
                interceptedSomethingThisTick = true;
                // Main.NewText($"adding {proj.Name} {proj.whoAmI} to targetedProjectiles");
                targetedProjectiles.Add(proj.whoAmI);

                targetedProjectiles.Sort
                (
                    (a, b) =>
                    {
                        var pa = Main.projectile[a];
                        var pb = Main.projectile[b];

                        return pa.whoAmI.CompareTo(pb.whoAmI);
                    }
                );

                var alreadyTargeted = Main.projectile.Any
                (
                    p =>
                        p.active &&
                        p.type == interceptorType &&
                        p.ModProjectile is VoidCrest_Spear interceptor &&
                        interceptor.TargetId == proj.whoAmI
                );

                if (!alreadyTargeted)
                {
                    InterceptCount -= cost;

                    var d = proj.Center - proj.velocity;

                    var adjuster = new Vector2(70 + proj.width + proj.velocity.X * proj.MaxUpdates, proj.velocity.Y * proj.MaxUpdates).RotatedBy
                            (proj.velocity.ToRotation())
                        .RotatedByRandom(MathHelper.ToRadians(35)); //.RotatedByRandom(MathHelper.PiOver2);

                    var adjustedSpawn = proj.Center + adjuster; // Main.rand.NextVector2CircularEdge(20, 20);

                    var a = adjuster + proj.Center;
                    var Velocity = adjustedSpawn.AngleTo(proj.Center + proj.velocity * 4 * proj.MaxUpdates).ToRotationVector2() * 105;

                    var dad = Projectile.NewProjectileDirect
                    (
                        Player.GetSource_FromThis(),
                        adjustedSpawn,
                        Velocity,
                        interceptorType,
                        -1,
                        1f,
                        Player.whoAmI
                    );

                    if (dad.ModProjectile is VoidCrest_Spear mom)
                    {
                        mom.TargetId = proj.whoAmI;
                        mom.BaseSize = proj.Size.X / proj.scale;
                    }

                    if (InterceptCount <= 1)
                    {
                        InterceptCount = 0;
                        Cooldown = CooldownMax;

                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Clap, Player.Center);

                        return;
                    }
                }
            }
        }

        if (!interceptedSomethingThisTick)
        {
            RegenerateInterceptCount();
        }
    }

    /// <summary>
    ///     Regenerates InterceptCount until it reaches the maximum.
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

    #region values

    /// <summary>
    ///     the max (8 second) cooldown after which Voidcrest must recharge.
    /// </summary>
    public int CooldownMax = 16 * 60;

    /// <summary>
    /// </summary>
    public int Cooldown;

    /// <summary>
    ///     True if the accessory is actively equipped in an accessory slot.
    ///     (Not in a vanity slot.)
    /// </summary>
    public bool voidCrestOathEquipped;

    public bool Hide;

    /// <summary>
    ///     If true, the accessory is in a vanity slot and only provides cosmetic effects.
    /// </summary>
    public bool Vanity;

    public float ResourceInterp;

    /// <summary>
    ///     The resource used to "pay" for intercepts.
    ///     It will decrease when an interception happens and regenerate slowly.
    /// </summary>
    public float InterceptCount;

    /// <summary>
    ///     Maximum resource value for intercepting hostile projectiles.
    /// </summary>
    public float MaxInterceptCount = 1000f;

    /// <summary>
    ///     How much resource is consumed per intercept.
    /// </summary>
    public float InterceptCost = 10f;

    /// <summary>
    ///     How much resource is regenerated per tick if no intercept is occurring.
    /// </summary>
    public float InterceptRegenRate = 4;

    /// <summary>
    ///     A list to keep track of hostile projectile indices that we are watching.
    ///     We rebuild this every tick.
    /// </summary>
    public List<int> trackedProjectileIndices = new();

    /// <summary>
    ///     Detection radius (in pixels) for adding enemy projectiles to the tracking list.
    /// </summary>
    private readonly float TrackingRadius = 1050f;

    /// <summary>
    ///     Distance at which an intercept is triggered (in pixels).
    /// </summary>
    private readonly float InterceptDistance = 325f;

    /// <summary>
    ///     Interceptor projectile type.
    /// </summary>
    private int interceptorType => ModContent.ProjectileType<VoidCrest_Spear>();

    /// <summary>
    ///     The current Projectile to be intercepted
    /// </summary>
    public int TobeDestroyed
    {
        //todo: output the proj so that i can handle the destroy logic in the interceptor because i hate everyone
        get;
        set;
    }

    public List<int> targetedProjectiles = new();

    public bool TrollMode;

    #endregion
}