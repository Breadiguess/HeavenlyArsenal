using System.Collections.Generic;
using System.Linq;
using CalamityMod.Enums;
using CalamityMod.Graphics.Metaballs;
using HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon;

namespace HeavenlyArsenal.Content.Particles.Metaballs.NoxusGasMetaball;

public class NoxusGasMetaball : Metaball
{
    public class GasParticle
    {
        public float Size;

        public Vector2 Velocity;

        public Vector2 Center;
    }

    public static readonly List<GasParticle> GasParticles = new();

    //public override MetaballDrawLayerType DrawContext => MetaballDrawLayerType.AfterProjectiles;

    public override Color EdgeColor => Color.MediumPurple;

    public override IEnumerable<Texture2D> Layers
    {
        get
        {
            {
                yield return ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Particles/Metaballs/NoxusGasLayer1").Value;
            }
        }
    }

    public override bool AnythingToDraw
    {
        get
        {
            // Only draw if there is at least one active EntropicBlast projectile
            var entropicBlastType = ModContent.ProjectileType<EntropicBlast>();
            var entropicCrystalType = ModContent.ProjectileType<EntropicBlast>();

            return Main.projectile.Any(p => (p.active && p.type == entropicBlastType) || p.type == entropicCrystalType);
        }
    }

    public override GeneralDrawLayer DrawLayer => GeneralDrawLayer.BeforeProjectiles;

    public static void CreateParticle(Vector2 spawnPosition, Vector2 velocity, float size)
    {
        GasParticles.Add
        (
            new GasParticle
            {
                Center = spawnPosition,
                Velocity = velocity,
                Size = size
            }
        );
    }

    public override void Update()
    {
        foreach (var particle in GasParticles)
        {
            particle.Velocity *= 0.99f;
            particle.Size *= 0.93f;
            particle.Center += particle.Velocity;
        }

        GasParticles.RemoveAll(p => p.Size <= 2f);
    }

    public override void DrawInstances()
    {
        var circle = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Particles/Metaballs/BasicCircle").Value;

        foreach (var particle in GasParticles)
        {
            Main.spriteBatch.Draw(circle, particle.Center - Main.screenPosition, null, Color.Purple, 0f, circle.Size() * 0.5f, new Vector2(particle.Size) / circle.Size(), 0, 0f);
        }

        foreach (var p in Main.projectile.Where(p => p.active))
        {
            var c = Color.Purple * p.Opacity;

            if (p.type == ModContent.ProjectileType<EntropicBlast>() && p.hide != true)
            {
                c.A = 0;
                p.ModProjectile.PreDraw(ref c);
            }
        }
    }
}