using System.Collections.Generic;
using CalamityMod;
using Luminance.Assets;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight;

public class RaptureBeam : ModProjectile
{
    public bool SetupComplete;

    public Color BaseColor { get; set; }

    public Entity Target { get; set; }

    public ref Player Owner => ref Main.player[Projectile.owner];

    public ref float Time => ref Projectile.ai[0];

    public ref float Thing => ref Projectile.ai[1];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    /// <summary>
    ///     brbr
    /// </summary>
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1000;
    }

    public override void SetDefaults()
    {
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.timeLeft = 60;
        Projectile.Size = new Vector2(100);
        Projectile.damage = 300;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.ai[1] = 0;
        Projectile.OriginalCritChance = 5;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }

    public override void AI()
    {
        if (!SetupComplete)
        {
            if (Target == null)
            {
                Projectile.Kill();

                return;
            }

            Projectile.rotation = Projectile.Center.AngleTo(Target.Center);
            SetupComplete = true;
        }

        Time++;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (target.SuperArmor)
        {
            modifiers = modifiers with
            {
                SuperArmor = false
            };
        }

        var value = AddableFloat.Zero + 1f;
        modifiers.ScalingArmorPenetration = value;

        var DamageMulti = MultipliableFloat.One * 1.3f;
        modifiers.TargetDamageMultiplier = DamageMulti;
        modifiers.Defense = StatModifier.Default * 0.0f;

        target.Calamity().unbreakableDR = false;
        target.Calamity().DR = 0;
        modifiers.CritDamage = StatModifier.Default + 0.2f;

        //i see your parasite wank and despair, l-man
        if (ModLoader.HasMod("SRPTerraria") && target.ModNPC != null)
        {
            var npcType = target.ModNPC.GetType();

            var parasiteBase = ModLoader.TryGetMod("SRPTerraria", out var srpMod)
                ? srpMod.Code.GetType("SRPTerraria.Content.NPCs.Parasites.ParasiteBaseNPC")
                : null;

            if (parasiteBase != null && parasiteBase.IsAssignableFrom(npcType))
            {
                // adapt to this, fucker

                CombatText.NewText(target.getRect(), Color.Orange, Projectile.damage, true);

                target.life -= Projectile.damage;
                target.checkDead();
                target.HitEffect();
            }
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        var dist = 2000f;

        var offset = new Vector2(dist * Projectile.scale * 10, 0).RotatedBy(Projectile.rotation);
        float _ = 0;

        return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center - offset / 2, Projectile.Center + offset, 120f, ref _);

        ;
    }

    public float Placeholdername(float value)
    {
        if (Time > Projectile.timeLeft / 2)
        {
            value = float.Lerp(value, 0, 0.1f);
        }
        else
        {
            value = float.Lerp(value, 1, 0.1f);
        }

        return value;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var Beam = MiscTexturesRegistry.BloomLineTexture.Value;

        Thing = Placeholdername(Thing);
        var SizeScalar = Thing;

        var DrawPos = Projectile.Center - Main.screenPosition;
        var Origin = Beam.Size() * 0.5f;
        var Scale = new Vector2(0.9f * SizeScalar, 0.4f * Target.width);

        var Rot = Projectile.rotation + MathHelper.PiOver2;

        var BeamColor = Color.White with
        {
            A = 0
        };

        Main.EntitySpriteDraw(Beam, DrawPos, null, BeamColor, Rot, Origin, Scale, SpriteEffects.None);

        //Utils.DrawBorderString(Main.spriteBatch, Time.ToString(), DrawPos, Color.AntiqueWhite);
        return false;
    }
}