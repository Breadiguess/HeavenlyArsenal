using CalamityMod.Graphics.Metaballs;
using HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon;
using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Particles.Metaballs
{
    public class SludgeBall : Metaball
    {
        public class SludgeParticle
        {
            public float Size;

            public Vector2 Velocity;

            public Vector2 Center;
            public Color LightColor;
        }
        public static readonly List<SludgeParticle> SludgeParticles = new();

        //public override MetaballDrawLayerType DrawContext => MetaballDrawLayerType.AfterProjectiles;
        public override Color EdgeColor => Color.White;


      

        public override IEnumerable<Texture2D> Layers
        {
            get
            {

                {
                    yield return GennedAssets.Textures.Noise.SwirlNoise2;
                }
            }
        }


        public override bool AnythingToDraw
        {
            get => Main.projectile.Any(p => p.active && (p.type == ModContent.ProjectileType<NowhereGoop>()));
            

        }


        public override MetaballDrawLayer DrawContext => MetaballDrawLayer.AfterProjectiles;

        public static void CreateParticle(Vector2 spawnPosition, Vector2 velocity, float size)
        {
            SludgeParticles.Add(new()
            {
                Center = spawnPosition,
                Velocity = velocity,
                Size = size
            });
        }

        public override void Update()
        {
            foreach (SludgeParticle particle in SludgeParticles)
            {
                particle.Velocity *= 0.99f;
                particle.Size *= 0.8f;
                particle.Center += particle.Velocity;
                particle.LightColor = Lighting.GetColor(particle.Center.ToTileCoordinates());
            }
            SludgeParticles.RemoveAll(p => p.Size <= 2f);

        }

        public override void DrawInstances()
        {

            Texture2D circle = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Particles/Metaballs/BasicCircle").Value;
            Color a = Color.White;//new Color(40, 40, 40);
            foreach (SludgeParticle particle in SludgeParticles)
                Main.spriteBatch.Draw(circle, particle.Center - Main.screenPosition, null, a, 0f, circle.Size() * 0.5f, new Vector2(particle.Size) / circle.Size(), 0, 0f);

            foreach (Projectile p in Main.projectile.Where(p => p.active))
            {
                Color c = Color.White * p.Opacity;
                if (p.type == ModContent.ProjectileType<NowhereGoop>() && p.hide != true)
                {
                    c.A = 0;
                    p.ModProjectile.PreDraw(ref c);
                }
            }
        }
    }
}
