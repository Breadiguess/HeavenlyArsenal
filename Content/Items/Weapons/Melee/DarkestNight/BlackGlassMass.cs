using System.Collections.Generic;
using NoxusBoss.Assets;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight;

public class BlackGlassMass : ModProjectile
{
    // registry of active masses (only indices)
    public static List<int> ActiveMasses = new();

    public Color[] GlowColor;

    public int MaxMass = 550;

    public ref Player Owner => ref Main.player[Projectile.owner];

    public ref float Time => ref Projectile.ai[0];

    public int TotalMass
    {
        get => (int)Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }

    public override string GlowTexture => "HeavenlyArsenal/Content/Items/Weapons/Melee/DarkestNight/BlackGlass_Glow";

    public override void OnSpawn(IEntitySource source)
    {
        GlowColor = new Color[100];

        for (var i = 0; i < 100; i++)
        {
            GlowColor[i] = RainbowColorGenerator.TrailColorFunction(i / 100f);
        }

        if (!ActiveMasses.Contains(Projectile.whoAmI))
        {
            ActiveMasses.Add(Projectile.whoAmI);
        }
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 4;
    }

    public override void SetDefaults()
    {
        Projectile.hostile = false;
        Projectile.friendly = true;

        Projectile.Size = new Vector2(10, 10);
        Projectile.DamageType = DamageClass.Melee;
        Projectile.tileCollide = false;
        Projectile.extraUpdates = 0;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
        overWiresUI.Add(index);
    }

    public override void AI()
    {
        // merge into other masses nearby
        var mergeRadius = 32f;
        var mergeRadiusSq = mergeRadius * mergeRadius;

        for (var i = ActiveMasses.Count - 1; i >= 0; i--)
        {
            var id = ActiveMasses[i];

            if (id == Projectile.whoAmI)
            {
                continue;
            }

            if (id < 0 || id >= Main.maxProjectiles)
            {
                ActiveMasses.RemoveAt(i);

                continue;
            }

            var other = Main.projectile[id];

            if (!other.active)
            {
                ActiveMasses.RemoveAt(i);

                continue;
            }

            if (other.owner != Projectile.owner)
            {
                continue;
            }

            var dsq = Vector2.DistanceSquared(Projectile.Center, other.Center);

            if (dsq <= mergeRadiusSq)
            {
                var otherMass = other.ModProjectile as BlackGlassMass;

                if (otherMass == null)
                {
                    continue;
                }

                // absorb as much as we can without exceeding MaxMass
                var transferable = Math.Min(otherMass.TotalMass, MaxMass - TotalMass);

                if (transferable <= 0)
                {
                    continue;
                }

                TotalMass += transferable;
                otherMass.TotalMass -= transferable;

                if (otherMass.TotalMass <= 0)
                {
                    other.Kill();
                }

                // if we've reached MaxMass, stop absorbing
                if (TotalMass >= MaxMass)
                {
                    break;
                }
            }
        }

        var scale = TotalMass / (float)MaxMass;
        Projectile.scale = scale;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var Base = ModContent.Request<Texture2D>(Texture).Value;
        var Glow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Melee/DarkestNight/BlackGlass_Glow").Value;
        Texture2D Glow2 = GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall;
        var DrawPos = Projectile.Center - Main.screenPosition;
        var Origin = Base.Size() * 0.5f;
        var Grigin = Glow.Size() * 0.5f;
        var Rot = Projectile.rotation + MathHelper.PiOver2;

        var Scale = new Vector2(4) * Projectile.scale;
        var GlowScale = new Vector2(4f) * Projectile.scale;

        var flip = Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        var GlowMulti = float.Lerp(0, 1f, Math.Clamp(Time / 20, 0, 1));

//            Main.EntitySpriteDraw(Glow2, DrawPos, null, GlowColor[0] * 0.2f, Rot, Glow2.Size() * 0.5f, GlowScale, flip);

//            Main.EntitySpriteDraw(Base, DrawPos, null, Color.AntiqueWhite, Rot, Origin, Scale, flip);

        // Utils.DrawBorderString(Main.spriteBatch, TotalMass.ToString(), DrawPos, Color.AntiqueWhite);
        return false;
    }
}