using CalamityMod.NPCs.OldDuke;
using CalamityMod.Particles;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.Audio;
using Particle = CalamityMod.Particles.Particle;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight;

public class Collapse : GlobalNPC
{
    public bool LucilleWouldProbablyApprove;

    public int CollapseTimer;

    public int CollapseStage;

    public int CollapseCooldownMax = 60 * 4;

    public int CollapseCooldownTimer;

    public bool Collapsing;

    public RoaringNight ShellSword;

    public bool Active
    {
        get => CollapseStage > 0;
        set => CollapseStage = value ? 1 : 0;
    }

    public Player Collapser { get; set; }

    public override bool InstancePerEntity => true;

    public override void PostAI(NPC npc)
    {
        if (CollapseCooldownTimer > 0)
        {
            CollapseCooldownTimer--;

            return;
        }

        if (!Active)
        {
            return;
        }

        if (CollapseStage >= 3)
        {
            if (ShellSword == null)
            {
                if (Collapser.heldProj != -1)
                {
                    ShellSword = Main.projectile[Collapser.heldProj].ModProjectile as RoaringNight;
                }
            }

            CollapseTimer++;

            if (CollapseTimer % 2 == 0)
            {
                if (CollapseTimer < 50)
                {
                    //if (CollapseTimer == 2){
                    var particle = CollapseParticle.pool.RequestParticle();

                    var GlowColor = RainbowColorGenerator.TrailColorFunction(CollapseTimer / 60f) * 0.2f;
                    var rotation = npc.rotation + MathHelper.ToRadians(Main.rand.NextFloat(0, 361));
                    var Scale = 0.5f + CollapseTimer / 17f;
                    particle.Prepare(npc.Center, Vector2.Zero, rotation, 120 - CollapseTimer, Scale, 0, CollapseTimer / 50f, GlowColor, npc);
                    ParticleEngine.ShaderParticles.Add(particle);
                    //}
                }

                Vector2 AdjustedSpawn;
                Dust b;

                for (var i = 0; i < 50; i++)
                {
                    var ds = Main.rand.NextFloat(10, 200) * (60 * 1.75f) / (CollapseTimer + 1);
                    AdjustedSpawn = Main.rand.NextVector2CircularEdge(ds, ds) + npc.Center;
                    b = Dust.NewDustDirect(AdjustedSpawn, 1, 30, DustID.AncientLight);
                    b.noGravity = true;
                    b.velocity = AdjustedSpawn.AngleTo(npc.Center).ToRotationVector2() * Vector2.Distance(AdjustedSpawn, npc.Center) * 0.2f;
                    b.color = RainbowColorGenerator.TrailColorFunction(Main.rand.NextFloat());
                }
            }

            //initial sound 
            if (CollapseTimer == 1)
            {
                if (npc.type == ModContent.NPCType<OldDuke>())
                {
                    LucilleWouldProbablyApprove = Main.rand.NextBool(2);

                    if (LucilleWouldProbablyApprove)
                    {
                        SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.Chuckle, npc.Center);
                    }
                }

                SoundEngine.PlaySound
                (
                    AssetDirectory.Sounds.Items.Weapons.Rapture.Collapse with
                    {
                        Volume = 0.8f,
                        PitchVariance = 0.2f
                    }
                );
            }

            if (CollapseTimer == 60 * 1.5f)
            {
                SoundEngine.PlaySound
                (
                    AssetDirectory.Sounds.Items.Weapons.Rapture.CollapseImpact with
                    {
                        Volume = 1.2f,
                        PitchVariance = 0.2f
                    }
                );
            }

            if (CollapseTimer >= 60 * 1.75f)
            {
                Particle b = new DetailedExplosion(npc.Center, Vector2.Zero, Color.AntiqueWhite * 0.5f, Vector2.One, 0, 0, 4.6f, 60);

                GeneralParticleHandler.SpawnParticle(b);

                var particle = LightFlash.pool.RequestParticle();
                var GlowColor = RainbowColorGenerator.TrailColorFunction(Main.rand.NextFloat());
                particle.Prepare(npc.Center, Vector2.Zero, npc.rotation + MathHelper.ToRadians(Main.rand.Next(60)), 60, Main.rand.NextFloat(1.6f, 2.4f), GlowColor);
                ParticleEngine.ShaderParticles.Add(particle);

                var particle2 = LightFlash.pool.RequestParticle();
                particle2.Prepare(npc.Center, Vector2.Zero, npc.rotation + MathHelper.ToRadians(Main.rand.Next(60)), 60, Main.rand.NextFloat(3f, 5.4f), GlowColor, true);
                ParticleEngine.ShaderParticles.Add(particle2);

                Collapsing = true;

                Rapture(npc);
                CollapseStage = 0;
                CollapseTimer = 0;
                CollapseCooldownTimer = CollapseCooldownMax;
                Collapsing = false;
            }
        }
    }

    public override void OnKill(NPC npc)
    {
        if (CollapseStage > 3 && CollapseTimer > 0 && CollapseCooldownTimer == 0)
        {
            SoundEngine.PlaySound
            (
                AssetDirectory.Sounds.Items.Weapons.Rapture.CollapseImpact with
                {
                    Volume = 1.2f,
                    PitchVariance = 0.2f
                }
            );

            Rapture(npc);
        }
    }

    private void Rapture(NPC npc)
    {
        if (Collapser != null)
        {
            ScreenShakeSystem.StartShakeAtPoint(npc.Center, 30);
            var RotationOffset = Main.rand.NextFloat(0, 360);
            //Main.NewText($"{RotationOffset}");
            RotationOffset = MathHelper.ToRadians(RotationOffset);

            if (LucilleWouldProbablyApprove)
            {
                npc.StrikeInstantKill();
                CombatText.NewText(npc.getRect(), Color.Red, int.MaxValue, true);
            }
            else
            {
                Collapser.StrikeNPCDirect(npc, npc.CalculateHitInfo(10_000 * 3, 0));
            }

            //spawn beams
            for (var i = 0; i < 12; i++)
            {
                var SpawnPos = new Vector2(i * 4, 0).RotatedBy(RotationOffset + MathHelper.ToRadians(i * 30));
                SpawnPos += npc.Center;

                int Damage;

                if (ShellSword != null && ShellSword.CreatorItem != null)
                {
                    Damage = Collapser.GetWeaponDamage(ShellSword.CreatorItem) / 4;
                }
                else
                {
                    Damage = Collapser.HeldItem.damage / 4;
                }

                Damage = (int)(Damage * 1.05f);
                Damage += (int)(npc.GetLifePercent() * 0.05f);

                var a = Projectile.NewProjectileDirect
                (
                    Collapser.GetSource_FromThis(),
                    SpawnPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<RaptureBeam>(),
                    Damage,
                    30
                );

                if (ShellSword != null)
                {
                    a.CritChance = ShellSword.CreatorItem.crit;
                }

                var beam = a.ModProjectile as RaptureBeam;
                beam.Target = npc;
                beam.BaseColor = RainbowColorGenerator.TrailColorFunction(Main.rand.NextFloat());
            }

            //dust vfx
            Dust c;

            for (var i = 0; i < 250; i++)
            {
                c = Dust.NewDustDirect(npc.Center, 160, 160, DustID.AncientLight, Main.rand.NextFloat(-100, 101), Main.rand.NextFloat(-100, 101));
                c.noGravity = true;
                c.fadeIn = 1.5f;
                c.color = RainbowColorGenerator.TrailColorFunction(Main.rand.NextFloat());
            }
        }
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit)
    {
        if (CollapseStage >= 3 && Collapsing)
        {
            //CalamityMod.Particles.Particle a = new DetailedExplosion(npc.Center, Vector2.Zero, Color.AntiqueWhite, Vector2.One, 0, 0, 10, 60, true);

            var trailPos = npc.Center;
            float trailScale = 1;
            var trailColor = Color.DarkRed;
            var Direction = new Vector2(56);
            Particle Trail = new SparkParticle(trailPos, Direction, false, 35, trailScale, trailColor);
            GeneralParticleHandler.SpawnParticle(Trail);
            //GeneralParticleHandler.SpawnParticle(a);
        }
    }

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        if (CollapseCooldownTimer > 0)
        {
            return;
        }

        if (projectile.type == ModContent.ProjectileType<RoaringNight>())
        {
            CollapseStage++;
            ShellSword = projectile.ModProjectile as RoaringNight;
        }

        if (projectile.type == ModContent.ProjectileType<BlindingLight>() || projectile.type == ModContent.ProjectileType<RaptureBeam>())
        {
            if (Main.rand.NextBool(4))
            {
                CollapseStage++;
            }

            ShellSword = projectile.ModProjectile as RoaringNight;
        }

        Collapser = Main.player[projectile.owner];
        /*
        if (CollapseStage >= 3)
        {
            Player Owner = Main.player[projectile.owner];

            if (Owner != null)
            {
                Owner.StrikeNPCDirect(npc, npc.CalculateHitInfo(damageDone * 3, 0));
                float RotationOffset = Main.rand.NextFloat(0, 360);
                Main.NewText($"{RotationOffset}");
                RotationOffset = MathHelper.ToRadians(RotationOffset);
                for(int i = 0; i< 12; i++)
                {
                    Vector2 SpawnPos = new Vector2(i * 4, 0).RotatedBy(RotationOffset + MathHelper.ToRadians(i*30));
                    SpawnPos += npc.Center;

                    int Damage = Owner.GetWeaponDamage(Owner.HeldItem);
                    Projectile a = Projectile.NewProjectileDirect(Owner.GetSource_FromThis(), SpawnPos,
                    Vector2.Zero, ModContent.ProjectileType<RaptureBeam>(), Damage, 0);
                    RaptureBeam beam = a.ModProjectile as RaptureBeam;
                    beam.Target = npc;
                    beam.BaseColor = TrailColorFunction(Main.rand.NextFloat());

                }
            }

            CollapseStage = 0;

        }*/
    }
}