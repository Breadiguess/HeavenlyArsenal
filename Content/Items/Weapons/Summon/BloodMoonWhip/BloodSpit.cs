using CalamityMod;
using Luminance.Assets;
using Luminance.Common.Utilities;
using NoxusBoss.Assets;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip;

public class BloodSpit : ModProjectile
{
    public int NPCIndex
    {
        get => (int)temp;
        set => temp = value;
    }

    public ref float temp => ref Projectile.ai[1];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.hostile = false;
        Projectile.friendly = true;

        Projectile.damage = 100;
        Projectile.Size = new Vector2(14, 14);
        Projectile.DamageType = DamageClass.Summon;

        Projectile.tileCollide = false;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 430;
        // Projectile.stopsDealingDamageAfterPenetrateHits = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 4;
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        ProjectileID.Sets.MinionShot[Type] = true;
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 7;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
        var npc = Main.npc[NPCIndex];

        if (npc.active && npc != null)
        {
            Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.AngleTo(npc.Center), 0.455f);
            Projectile.velocity = Projectile.rotation.ToRotationVector2() * 10;
        }

        for (var i = 0; i < 3; i++)
        {
            Dust.NewDustDirect(Projectile.Center, 20, 20, DustID.Blood, -Projectile.velocity.X, -Projectile.velocity.Y);
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }

    public override void OnKill(int timeLeft)
    {
        base.OnKill(timeLeft);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;
        Texture2D PlaceholderName = GennedAssets.Textures.GreyscaleTextures.BloomLine;

        var DrawPos = Projectile.Center - Main.screenPosition;

        var Origin = tex.Size() * 0.5f;
        var Other = new Vector2(PlaceholderName.Width / 2, 0);
        var Rot = Projectile.rotation + MathHelper.PiOver2;

        var color = Color.Crimson;
        Main.spriteBatch.SetBlendState(BlendState.Additive);
        Main.EntitySpriteDraw(tex, DrawPos, null, color, Projectile.rotation, Origin, 0.5f, SpriteEffects.None);
        //Main.EntitySpriteDraw(PlaceholderName, DrawPos, null, color, Rot, Other, 0.1f, SpriteEffects.None);

        Main.spriteBatch.ResetToDefault();

        return false;
    }
}