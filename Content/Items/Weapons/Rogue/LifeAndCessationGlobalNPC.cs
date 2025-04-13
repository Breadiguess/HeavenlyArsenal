using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.RenderTargets;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.RocheLimit;

public class LifeAndCessationGlobalNPC : GlobalNPC
{
    private static int timeSinceLastRTAccess;

    private static Vector2? itemVelocityOverride;

    /// <summary>
    /// Whether this NPC is currently being shredded by a black hole.
    /// </summary>
    public bool BeingShredded
    {
        get;
        set;
    }

    /// <summary>
    /// The amount by which this NPC should be downscaled due to entering a black hole.
    /// </summary>
    public float DownscaleFactor
    {
        get;
        set;
    } = 1f;

    /// <summary>
    /// The disintegration render target for NPCs.
    /// </summary>
    public static InstancedRequestableTarget DisintegrationTarget
    {
        get;
        set;
    }

    public override bool InstancePerEntity => true;

    public override void SetStaticDefaults()
    {
     //leftovers becuase im using the stupid black hole as a base
     //   On_Main.DrawNPC += DecreaseTargetScale;
     //   On_Main.DrawNPCs += ApplyDisintegrationEffect;
     
    }

   

    // Lobotomize targets that are preoccupied with being fucking shredded to pieces by a black hole.
    public override bool PreAI(NPC npc) => !BeingShredded;

    public override void PostAI(NPC npc)
    {
        bool wasShreddedBefore = BeingShredded;
        BeingShredded = false;
        DownscaleFactor = MathHelper.Lerp(DownscaleFactor, 1f, 0.12f);
        if (!npc.CanBeChasedBy() && !wasShreddedBefore)
            return;

        float minDistance = 9999999f;
        int blackHoleID = ModContent.ProjectileType<RocheLimitBlackHole>();
        var blackHoles = LumUtils.AllProjectilesByID(blackHoleID);
        Projectile? closestBlackHole = null;
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            if (projectile.type == blackHoleID)
            {
                float distanceToBlackHole = projectile.Distance(npc.Center);
                if (distanceToBlackHole < minDistance)
                {
                    closestBlackHole = projectile;
                    minDistance = distanceToBlackHole;
                }
            }
        }

        if (closestBlackHole is not null && npc.WithinRange(closestBlackHole.Center, 900f))
        {
            Vector2 suctionOrigin = closestBlackHole.Center;

            float suctionInterpolant = closestBlackHole.As<RocheLimitBlackHole>().BlackHoleDiameter / RocheLimitBlackHole.MaxBlackHoleDiameter;
            float suctionAcceleration = suctionInterpolant * 0.09f;
            npc.velocity = Vector2.Lerp(npc.velocity, npc.SafeDirectionTo(suctionOrigin) * suctionInterpolant * 80f, suctionAcceleration);

            if (npc.realLife == -1)
            {
                float idealDownscaling = EasingCurves.Exp.Evaluate(EasingType.Out, LumUtils.InverseLerp(150f, 700f, npc.Distance(suctionOrigin)));
                DownscaleFactor = MathHelper.Lerp(1f, idealDownscaling, suctionInterpolant);
            }

            // It's time to die.
            float shredDistance = closestBlackHole.As<RocheLimitBlackHole>().BlackHoleDiameter * 0.33f;
            if (npc.WithinRange(suctionOrigin, shredDistance) && suctionInterpolant >= 0.85f)
            {
                BeingShredded = true;

                npc.velocity = Vector2.Zero;
                npc.Center = suctionOrigin;
            }

            // Hits are inputted manually to ensure maximum control over the NPC's death, which needs to be more interesting than just splaying a bunch of gore and loot.
            if (closestBlackHole.Colliding(closestBlackHole.Hitbox, npc.Hitbox) || BeingShredded)
            {
                int damage = closestBlackHole.damage;
                bool willDie = npc.life - damage <= 0; // This calculation doesn't care about defense and DR but honestly who cares? filthy liar,then why do they have DR still
                if (willDie)
                {
                    npc.active = false;

                    Vector2 fallbackJetDirection = Main.rand.NextVector2Unit().RotateTowards(closestBlackHole.AngleTo(Main.player[closestBlackHole.owner].Center), MathHelper.Pi * 0.4f);
                    Vector2 jetDirection = npc.velocity.SafeNormalize(fallbackJetDirection);
                    try
                    {
                        itemVelocityOverride = jetDirection * 65f;
                        npc.NPCLoot();
                    }
                    finally
                    {
                        itemVelocityOverride = null;
                    }

                    closestBlackHole.As<RocheLimitBlackHole>().ReleaseJet(jetDirection);
                }
                else
                {
                    SoundStyle? oldHitSound = npc.HitSound;
                    SoundStyle? oldDeathSound = npc.DeathSound;
                    try
                    {
                        npc.HitSound = null;
                        npc.DeathSound = null;
                        Main.player[closestBlackHole.owner].addDPS(npc.SimpleStrikeNPC(damage, 0));
                    }
                    finally
                    {
                        npc.HitSound = oldHitSound;
                        npc.DeathSound = oldDeathSound;
                    }
                }
            }
        }
    }
}
