using System.Collections.Generic;
using System.IO;
using System.Linq;
using CalamityMod;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using Luminance.Assets;
using Terraria.Audio;
using Player = Terraria.Player;

namespace HeavenlyArsenal.Content.Items.Accessories.Vambrace;

public class DischargePlayer : ModPlayer
{
    public bool Active;

    public List<NPC> StruckNPCS = new();

    public List<Projectile> ProjectileOrder = new();

    public bool SpawnedCharge { get; set; }

    public bool isVambraceDashing { get; set; }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Active && proj.type == ModContent.ProjectileType<ElectricalDischarge>())
        {
            var discharge = proj.ModProjectile as ElectricalDischarge;

            if (discharge == null)
            {
                return;
            }

            //attach the ncp to the struck npcs to use as 
            StruckNPCS.Append(target);

            if (discharge.Source)
            {
                discharge.ChainCount++;
            }
        }
    }

    public override void PostUpdate()
    {
        if (Active)
        {
            if (Player.miscCounter % 8 == 7 && Player.dashDelay > 0) // Reduced dash cooldown by 38%
            {
                Player.dashDelay--;
            }

            //Console.WriteLine(Player.dashDelay);
            //Main.NewText($"{Player.dashDelay}");
            if (Player.dashDelay == -1)
            {
                if (Player.miscCounter % 6 == 0 && Player.velocity != Vector2.Zero)
                {
                    var damage = 180;//r.ApplyArmorAccDamageBonusesTo(Player.GetBestClassDamage().ApplyTo(170));
                }

                Player.endurance += 0.20f;

                if (!isVambraceDashing) // Dash isn't reduced, this is used to determine the first frame of dashing
                {
                    SoundEngine.PlaySound
                    (
                        SoundID.DD2_BetsyFireballImpact with
                        {
                            Volume = 0.4f,
                            PitchVariance = 0.4f
                        },
                        Player.Center
                    );

                    var damage = 750;// Player.ApplyArmorAccDamageBonusesTo(Player.GetBestClassDamage().ApplyTo(750));
                    isVambraceDashing = true;

                    if (!SpawnedCharge)
                    {
                        if (Player.ownedProjectileCounts[ModContent.ProjectileType<ElectricalDischarge>()] <= 0)
                        {
                            var proj = Projectile.NewProjectileDirect
                                (Player.GetSource_FromThis(), Player.Center + Player.velocity * 1.25f, Vector2.Zero, ModContent.ProjectileType<ElectricalDischarge>(), damage, 10f, Player.whoAmI);

                            var discharge = proj.ModProjectile as ElectricalDischarge;

                            if (discharge == null)
                            {
                                return;
                            }

                            discharge.Source = true;
                            discharge.ChainCount = 10;
                        }

                        SpawnedCharge = true;
                    }
                }

                else
                {
                    isVambraceDashing = false;
                }
            }

            if (Player.dashDelay == 0)
            {
                SpawnedCharge = false;
            }
        }
    }

    public override void PreUpdate()
    {
        if (!Active)
        {
            return;
        }

        foreach (var npc in StruckNPCS.ToList())
        {
            if (npc != null) { }
        }
    }

    public override void ResetEffects()
    {
        Active = false;
    }
}

internal class ElectricalDischarge : ModProjectile
{
    public static int MaxTargets = 20;

    public List<NPC> localTargets = new();

    public bool HasHit;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    /// <summary>
    ///     if this bool is true, this is the daddy to the rest of the projectiles.
    /// </summary>
    public bool Source { get; set; }

    public NPC Target { get; set; }

    public Player Owner => Main.player[Projectile.owner];

    public ref float ChainCount => ref Projectile.ai[0];

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(Source);
        writer.Write(HasHit);
    }

    public override void SetDefaults()
    {
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.Size = new Vector2(120, 120);
        Projectile.DamageType = DamageClass.Generic;
        Projectile.extraUpdates = 3;
        Projectile.timeLeft = 180;

        Projectile.CritChance = -4;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.penetrate = -1;
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.NoLiquidDistortion[Projectile.type] = true;
    }

    public override void AI()
    {
        if (Source)
        {
            if (!HasHit)
            {
                Projectile.Center = Owner.Center + Owner.velocity * 0.25f;
            }
            else
            {
                for (var i = 0; i < Owner.GetModPlayer<DischargePlayer>().StruckNPCS.Count - 1; i++)
                {
                    var proj = Projectile.NewProjectileDirect
                        (Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ElectricalDischarge>(), Projectile.damage, 0.4f);

                    var discharge = proj.ModProjectile as ElectricalDischarge;

                    if (discharge != null)
                    {
                        discharge.Source = false;
                        discharge.localTargets = localTargets;
                        discharge.Target = Owner.GetModPlayer<DischargePlayer>().StruckNPCS[i];
                    }

                    Owner.GetModPlayer<DischargePlayer>().ProjectileOrder.Add(proj);
                    var lightningPos = localTargets[i].Center + Main.rand.NextVector2Circular(24, 24);

                    var particle = HeatLightning.pool.RequestParticle();
                    particle.Prepare(lightningPos, Main.rand.NextVector2Circular(10, 10), Main.rand.NextFloat(-2f, 2f), 10 * 3, Main.rand.NextFloat(0.5f, 1f));
                    ParticleEngine.Particles.Add(particle);
                }

                Projectile.active = false;
            }
        }
        else if (Target != null)
        {
            Projectile.Size = new Vector2(75, 75);
            Projectile.penetrate = 1;
            Projectile.Center = Vector2.Lerp(Projectile.Center, Target.Center, 0.75f);

            if (Vector2.Distance(Projectile.Center, Target.Center) <= 1)
            {
                //todo:: add a particle effect, and then kill the npc
                Projectile.active = false;
            }
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        var dPlayer = Owner.GetModPlayer<DischargePlayer>();
        HasHit = true;

        if (dPlayer.StruckNPCS.Count == 0 && Source)
        {
            for (var i = 0; i < Main.maxNPCs; i++)
            {
                var nearbyNPCs = Main.npc.Where
                    (
                        n => n.CanBeChasedBy(Projectile) &&
                             Vector2.Distance(Projectile.Center, n.Center) <= 600f &&
                             dPlayer.StruckNPCS.Contains(n)
                    )
                    .OrderBy(n => Vector2.Distance(Projectile.Center, n.Center));

                foreach (var npc in nearbyNPCs)
                {
                    dPlayer.StruckNPCS.Add(npc);

                    if (dPlayer.StruckNPCS.Count >= MaxTargets)
                    {
                        break;
                    }
                }
            }
        }

        base.OnHitNPC(target, hit, damageDone);
    }

    public override bool? CanDamage()
    {
        return !HasHit;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var DrawPos = Owner.Center - Main.screenPosition;

        if (localTargets.Count > 0)
        {
            for (var i = 0; i < localTargets.Count; i++)
            {
                Utils.DrawBorderString(Main.spriteBatch, localTargets[i].FullName + ", " + localTargets[i].whoAmI, DrawPos - Vector2.UnitY * (100 + i * 20), Color.AntiqueWhite);
            }
        }

        return false;
    }
}