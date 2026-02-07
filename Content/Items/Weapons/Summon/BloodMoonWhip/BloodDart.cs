using Luminance.Assets;
using Luminance.Common.Utilities;
using NoxusBoss.Assets;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip;

internal class BloodDart : ModProjectile
{
    public enum NeedleState
    {
        Thrown,

        StuckInEnemy,

        Dislodge
    }

    private const int DislodgeTime = 60;

    public int timeOffset;

    public int index;

    private Vector2 offset;

    private bool locatedTarget;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public int Time
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public NeedleState CurrentState
    {
        get => (NeedleState)Projectile.ai[1];
        set => Projectile.ai[1] = (float)value;
    }

    public NPC StuckNPC
    {
        get => Projectile.ai[2] == -1? null : Main.npc[(int)Projectile.ai[2]];
        set => Projectile.ai[2] = value.whoAmI;
    }

    public ref Player Owner => ref Main.player[Projectile.owner];

    public override bool? CanDamage()
    {
        return CurrentState != NeedleState.StuckInEnemy;
    }

    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.localNPCHitCooldown = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.DamageType = DamageClass.SummonMeleeSpeed;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 600;
        Projectile.extraUpdates = 2;
    }

    public override void AI()
    {
        Statemachine();
        Time++;
    }

    private void Statemachine()
    {
        switch (CurrentState)
        {
            case NeedleState.Thrown:
                Normal();

                break;
            case NeedleState.StuckInEnemy:

                if (StuckNPC == null || !StuckNPC.active)
                {
                    Projectile.Kill();

                    return;
                }

                Projectile.timeLeft++;
                Projectile.Center = StuckNPC.Center - offset.RotatedBy(StuckNPC.rotation);
                Projectile.velocity = Vector2.Zero;

                break;
            case NeedleState.Dislodge:
                Dislodge();

                break;
        }
    }

    private void Normal()
    {
        // Base rotation so it faces its current velocity when not homing
        if (Projectile.velocity.LengthSquared() > 0.01f)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        var locatedTarget = false;

        // Find the closest target.
        var npcDistCompare = 25000f; // Large initial value
        var index = -1;

        foreach (var n in Main.ActiveNPCs)
        {
            float extraDistance = n.width / 2 + n.height / 2;

            // Big-ish detection radius so it can choose something sane
            if (!n.CanBeChasedBy(Projectile) || !Projectile.WithinRange(n.Center, 400f + extraDistance))
            {
                continue;
            }

            var currentNPCDist = Vector2.Distance(n.Center, Projectile.Center);

            if (currentNPCDist < npcDistCompare && Collision.CanHit(Projectile.Center, 1, 1, n.Center, 1, 1))
            {
                npcDistCompare = currentNPCDist;
                index = n.whoAmI;
            }
        }

        if (index != -1)
        {
            locatedTarget = true;
            var target = Main.npc[index];

            var startHomingDistance = 460f;
            var distanceToTarget = Vector2.Distance(Projectile.Center, target.Center);

            if (distanceToTarget <= startHomingDistance)
            {
                var desiredAngle = (target.Center - Projectile.Center).ToRotation();
                var currentAngle = Projectile.velocity.ToRotation();

                // Max turn per tick 
                var maxTurn = MathHelper.ToRadians(12f); // smaller = lazier, bigger = snappier

                // Rotate current angle toward desired angle with a hard cap

                var newAngle = currentAngle.AngleLerp(desiredAngle, 0.05f); //Terraria.Utils.AngleTowards(currentAngle, desiredAngle, maxTurn);

                // Keep speed roughly constant (or clamp to a minimum so it doesn't stall)
                var speed = Projectile.velocity.Length();

                if (speed < 8f)
                {
                    speed = 8f; // minimum cruising speed
                }

                Projectile.velocity = newAngle.ToRotationVector2() * speed;
                Projectile.rotation = newAngle;
            }
            // else: it's too far away → just keep flying straight
        }
    }

    private void Dislodge()
    {
        if (Time < DislodgeTime * Projectile.MaxUpdates + timeOffset)
        {
            if (Time == 0)
            {
                Projectile.tileCollide = false;
                Projectile.velocity = Owner.Center.AngleTo(StuckNPC.Center).ToRotationVector2().RotatedByRandom(MathHelper.ToRadians(30)) * 18;
            }

            if (Projectile.scale < 1)
            {
                Projectile.scale = float.Lerp(Projectile.scale, 1, 0.05f);
            }

            Projectile.velocity *= 0.9f;
            Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.Center.AngleTo(StuckNPC.Center), 0.07f);
        }

        if (Time > DislodgeTime * Projectile.MaxUpdates - 1 + timeOffset)
        {
            if (Time == DislodgeTime * Projectile.MaxUpdates + timeOffset)
            {
                Projectile.ResetLocalNPCHitImmunity();

                SoundEngine.PlaySound
                (
                    SoundID.DD2_BallistaTowerShot with
                    {
                        Pitch = 0.4f
                    },
                    Projectile.Center
                );
            }

            if (StuckNPC == null)
            {
                Projectile.velocity = Projectile.rotation.ToRotationVector2() * 30f;
                return;
            }

            if (StuckNPC.dontTakeDamage || !StuckNPC.active)
            {
                Projectile.ai[2] = -1;
                return;
            }

            if (StuckNPC != null)
            {
                Projectile.velocity = Projectile.Center.AngleTo(StuckNPC.Center).ToRotationVector2() * 30f;
            }
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (CurrentState == NeedleState.Thrown)
        {
            offset = target.Center - Projectile.Center;
            StuckNPC = target;
            CurrentState = NeedleState.StuckInEnemy;
        }
        else
        {
            for (var i = 0; i < 40; i++)
            {
                Dust.NewDust(Projectile.Center, 30, 30, DustID.Blood, Projectile.velocity.X, Projectile.velocity.Y);
            }

            SoundEngine.PlaySound
            (
                GennedAssets.Sounds.Common.Glitch with
                {
                    Pitch = 0.1f
                },
                target.Center
            );

            if (StuckNPC != null && StuckNPC == target)
            {
                Projectile.Kill();
            }
            else
            {
                Projectile.ResetLocalNPCHitImmunity();
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var tex = ModContent.Request<Texture2D>(this.GetPath()).Value; //TextureAssets.Item[ItemID.ThrowingKnife].Value;

        var DrawPos = Projectile.Center - Main.screenPosition;

        float opacity = 1;

        if (StuckNPC != null)
        {
            opacity = StuckNPC.Opacity;
        }

        var scale = new Vector2(1, 1) * Projectile.scale;
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

        for (var i = 0; i < 12; i++)
        {
            var thing = MathF.Sin(Main.GlobalTimeWrappedHourly * 10.1f + Projectile.whoAmI * 10) * Projectile.scale + 1;
            var a = Color.Lerp(Color.Purple, Color.Crimson, thing);
            Main.EntitySpriteDraw(tex, DrawPos + new Vector2(2, 0).RotatedBy(MathHelper.TwoPi * (i / 12f)), null, a * opacity, Projectile.rotation + MathHelper.PiOver2, tex.Size() / 2, scale, 0);
        }

        Main.spriteBatch.ResetToDefault();
        Main.EntitySpriteDraw(tex, DrawPos, null, lightColor * opacity, Projectile.rotation + MathHelper.PiOver2, tex.Size() / 2, scale, 0);

        return base.PreDraw(ref lightColor);
    }
}