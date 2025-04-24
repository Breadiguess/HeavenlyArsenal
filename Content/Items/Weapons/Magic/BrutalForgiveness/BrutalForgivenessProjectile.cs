using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Core.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.BrutalForgiveness;

public class BrutalForgivenessProjectile : ModProjectile
{
    /// <summary>
    /// The owner of this seed.
    /// </summary>
    public ref Player Owner => ref Main.player[Projectile.owner];

    /// <summary>
    /// How long this seed has existed for.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 3600;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.hide = true;
        Projectile.manualDirectionChange = true;
        Projectile.DamageType = DamageClass.Magic;
    }

    public override void AI()
    {
        // Begin dying if no longer holding the click button or otherwise cannot use the item.
        if (!Owner.channel || Owner.dead || !Owner.active || Owner.noItems || Owner.CCed)
        {
            Projectile.Kill();
            return;
        }

        SetPlayerItemAnimations();

        if (Time % 6f == 5f)
        {
            if (!Owner.CheckMana(Owner.HeldMouseItem(), -1, true))
                Projectile.Kill();

            else
            {
                ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, 1.7f);
                SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.SliceTelegraph with { MaxInstances = 16, PitchVariance = 0.3f }, Projectile.Center).WithVolumeBoost(0.5f);
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 vineVelocity = Projectile.SafeDirectionTo(Main.MouseWorld).RotatedByRandom(0.006f) * Main.rand.NextFloat(10f, 11f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vineVelocity, ModContent.ProjectileType<BrutalVine>(), Projectile.damage, 0f, Projectile.owner);
                }
            }
        }

        Time++;
    }

    public void SetPlayerItemAnimations()
    {
        if (Main.myPlayer == Projectile.owner)
        {
            int idealDirection = (int)Projectile.HorizontalDirectionTo(Main.MouseWorld);
            if (Projectile.direction != idealDirection)
            {
                Projectile.direction = idealDirection;
                Projectile.netUpdate = true;
            }
        }

        Owner.heldProj = Projectile.whoAmI;
        Owner.SetDummyItemTime(3);
        Owner.ChangeDir(Projectile.direction);
        Projectile.Center = Owner.Center + Vector2.UnitX * Projectile.direction * 4f;
        Owner.itemLocation = Projectile.Center;
        Projectile.timeLeft = 2;
    }
}
