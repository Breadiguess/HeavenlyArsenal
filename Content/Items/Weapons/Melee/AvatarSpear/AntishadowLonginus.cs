using NoxusBoss.Assets;
using NoxusBoss.Content.Particles.Metaballs;
using Terraria.Audio;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.AvatarSpear;

public class AntishadowLonginus : ModProjectile
{
    public ref float Time => ref Projectile.ai[0];

    public ref float Target => ref Projectile.ai[1];

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 20;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.extraUpdates = 0;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 50;
        Projectile.localAI[0] = Main.rand.Next(6);
        Projectile.localAI[1] = Main.rand.Next(6);
    }

    public override void AI()
    {
        Projectile.velocity *= 0.95f;

        var targetRotation = Projectile.velocity.ToRotation();

        var valid = true;
        NPC targetNPC = null;

        if (Target > 0 && Target <= Main.npc.Length)
        {
            targetNPC = Main.npc[(int)(Target - 1)];

            if (!targetNPC.active || targetNPC.lifeMax < 5 || targetNPC.friendly || targetNPC.dontTakeDamage)
            {
                valid = false;
            }
        }

        if (targetNPC != null && valid)
        {
            Projectile.velocity += Projectile.DirectionTo(targetNPC.Center) * Utils.GetLerpValue(20, 120, Time, true);
            targetRotation = targetRotation.AngleLerp(Projectile.AngleTo(targetNPC.Center), Utils.GetLerpValue(0, 30, Time, true));

            if (Time > 40)
            {
                Projectile.extraUpdates = 5;
            }
        }
        else
        {
            Projectile.Kill();
        }

        Projectile.rotation = Projectile.rotation.AngleLerp(targetRotation + MathHelper.PiOver2, 0.5f);

        Time++;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        var metaball = ModContent.GetInstance<BloodMetaball>();

        for (var i = 0; i < 10; i++)
        {
            var bloodSpawnPosition = Projectile.Center + Main.rand.NextVector2Circular(5, 5) * Projectile.scale;
            var bloodVelocity = Main.rand.NextVector2Circular(12f, 12f);
            metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(10f, 40f), Main.rand.NextFloat(2f));
        }

        SoundEngine.PlaySound
        (
            GennedAssets.Sounds.Avatar.PortalPierce with
            {
                Volume = 0.4f,
                Pitch = 0.7f,
                MaxInstances = 0
            },
            Projectile.Center
        );

        Projectile.Kill();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var glow = AssetDirectory.Textures.BigGlowball.Value;

        Main.EntitySpriteDraw
        (
            glow,
            Projectile.Center - Main.screenPosition,
            null,
            Color.DarkRed with
            {
                A = 100
            } *
            0.5f,
            Projectile.rotation,
            glow.Size() / 2,
            new Vector2(0.1f, 0.15f) * Projectile.scale,
            0
        );

        for (var i = 0; i < 4; i++)
        {
            DrawSpear
            (
                new Vector2(2, 0).RotatedBy(Projectile.rotation + i / 4f * MathHelper.TwoPi),
                Color.Red with
                {
                    A = 100
                }
            );
        }

        DrawSpear(Vector2.Zero, Color.Black);

        return false;
    }

    private void DrawSpear(Vector2 offset, Color color)
    {
        var texture = TextureAssets.Projectile[Type].Value;

        var headFrame = texture.Frame(3, 2, (int)Projectile.localAI[0] % 3);
        var poleFrame = texture.Frame(3, 2, (int)Projectile.localAI[1] % 3, 1);

        var effect = Projectile.localAI[0] > 2 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var poleEffect = Projectile.localAI[1] > 2 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var poleScale = MathF.Cbrt(Utils.GetLerpValue(5, 30, Time, true));

        Main.EntitySpriteDraw
        (
            texture,
            Projectile.Center + offset - Main.screenPosition,
            poleFrame,
            color,
            Projectile.oldRot[Projectile.oldRot.Length / 3],
            new Vector2(poleFrame.Width / 2, 8),
            Projectile.scale * poleScale,
            poleEffect
        );

        Main.EntitySpriteDraw
            (texture, Projectile.Center + offset - Main.screenPosition, headFrame, color, Projectile.rotation, new Vector2(headFrame.Width / 2, headFrame.Height - 4), Projectile.scale, effect);
    }
}