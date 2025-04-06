using HeavenlyArsenal.Core.Physics.ClothManagement;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor
{
    class ShintoArmorCapePlayer : ModPlayer
    {
        private float ExistenceTimer;

        public ClothSimulation Robe
        {
            get;
            set;
        } = new ClothSimulation(Vector3.Zero, 22, 21, 4.4f, 60f, 0.019f);

        public override void Load()
        {
            On_Main.CheckMonoliths += DrawAllTargets;
        }
        private void DrawAllTargets(On_Main.orig_CheckMonoliths orig)
        {
            drawToTarget?.Invoke(Main.spriteBatch);

            Main.spriteBatch.GraphicsDevice.SetRenderTarget(null);
            Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);

            orig();
        }

        private RenderTarget2D RobeTarget;


        private static event Action<SpriteBatch> drawToTarget;
        private const int frontSize = 200;
        private const int backSize = 600;

        public override void Initialize()
        {
            Main.QueueMainThreadAction(() =>
            {
              
                //frenziedParticles = new MonoParticleSystem<FrenziedFlameParticle>(200);
                RobeTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, backSize, backSize);
                //frenziedTargetFront = new RenderTarget2D(Main.graphics.GraphicsDevice, frontSize, frontSize);
            });
        }

        public void DrawRobeToTarget(SpriteBatch spritebatch)
        {
            if (Player != null)
            {
                if (!IsReady())
                    return;
                Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);
                Main.spriteBatch.GraphicsDevice.SetRenderTarget(RobeTarget);
                Matrix world = Matrix.CreateTranslation(-Player.Center.X + WotGUtils.ViewportSize.X * 0.5f, -Player.Center.Y + WotGUtils.ViewportSize.Y * 0.5f, 0f);
                Matrix projection = Matrix.CreateOrthographicOffCenter(0f, WotGUtils.ViewportSize.X, WotGUtils.ViewportSize.Y, 0f, -1000f, 1000f);
                Matrix matrix = world * projection;

                ManagedShader clothShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssasinRobeShader");
                clothShader.TrySetParameter("opacity", LumUtils.InverseLerp(60f, 120f, ExistenceTimer));
                clothShader.TrySetParameter("transform", matrix);
                clothShader.Apply();
                Robe.Render();

            }
        }
        public bool IsReady() => RobeTarget != null;
        public DrawData GetRobeTarget() => new DrawData(RobeTarget, Vector2.Zero, null, Color.White, -Player.fullRotation, RobeTarget.Size() * 0.5f, 1f, 0);
        //public DrawData GetFrenzyTargetFront() => new DrawData(frenziedTargetFront, Vector2.Zero, frenziedTargetFront.Frame(), Color.White, -Player.fullRotation, frenziedTargetFront.Size() * 0.5f, 1f, 0);


        public override void PostUpdateMiscEffects()
        {
            UpdateCloth();
            ExistenceTimer++;
        }
        private void UpdateCloth()
        {
            int steps = 25;
            float windSpeed = Math.Clamp(Main.WindForVisuals  * 8f, -1.3f, 0f);
            Vector2 robePosition = Player.Center + new Vector2(14f, -50f).RotatedBy(Player.fullRotation);
            Vector3 wind = Vector3.UnitX * (LumUtils.AperiodicSin(ExistenceTimer * 0.029f) * 0.67f + windSpeed) * 1.74f;
            for (int i = 0; i < steps; i++)
            {
                for (int x = 0; x < Robe.Width; x += 2)
                {
                    for (int y = 0; y < 2; y++)
                        ConstrainParticle(robePosition, Robe.particleGrid[x, y], 0f);
                }

                Robe.Simulate(0.051f, false, Vector3.UnitY * 3.2f + wind);
            }
        }

       

        private void ConstrainParticle(Vector2 anchor, ClothPoint? point, float angleOffset)
        {
            if (point is null)
                return;

            float xInterpolant = point.X / (float)Robe.Width;
            float angle = MathHelper.Lerp(MathHelper.PiOver2, MathHelper.TwoPi - MathHelper.PiOver2, xInterpolant);

            Vector3 ring = new Vector3(MathF.Cos(angle + angleOffset) * 50f, 0f, -MathF.Cos(angle) * 10f);
            ring.Y += point.Y * 6f;

            point.Position = new Vector3(anchor, 0f) + ring;
            point.IsFixed = true;
        }

        
    }
}
