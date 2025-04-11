using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Core.Physics.ClothManagement;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
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
        } = new ClothSimulation(Vector3.Zero, 10, 8, 3f, 50f, 0.1f);


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
        private RenderTarget2D RobeMapTarget;

        private static event Action<SpriteBatch> drawToTarget;
        private const int frontSize = 200;
        private const int backSize = 600;

        public override void Initialize()
        {
            Main.QueueMainThreadAction(() =>
            {
                drawToTarget += DrawRobeToTarget;
                //frenziedParticles = new MonoParticleSystem<FrenziedFlameParticle>(200);
                RobeMapTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, backSize, backSize);
                RobeTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, backSize, backSize);
                //frenziedTargetFront = new RenderTarget2D(Main.graphics.GraphicsDevice, frontSize, frontSize);
            });
        }

        public void DrawRobeToTarget(SpriteBatch spritebatch)
        {
            if (Player != null)
            {
                if (!IsReady() || !ShaderManager.HasFinishedLoading) // God damn Luminance you slowpoke
                    return;

                Main.spriteBatch.GraphicsDevice.SetRenderTarget(RobeMapTarget);
                Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);

                Vector2 robePosition = Player.Center + new Vector2(4 * Player.direction, -50f).RotatedBy(Player.fullRotation);

                Matrix world = Matrix.CreateTranslation(-robePosition.X + backSize / 2, -robePosition.Y + backSize / 2, 0f);
                
                Matrix projection = Matrix.CreateOrthographicOffCenter(0, backSize, 600, 0, -1000, 1000);
                Matrix matrix = world * projection;

                ManagedShader clothShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssasinRobeShader");
                clothShader.TrySetParameter("opacity", LumUtils.InverseLerp(60f, 120f, ExistenceTimer));
                clothShader.TrySetParameter("transform", matrix);
                clothShader.Apply();
                Robe.Render();

                Main.spriteBatch.End();

                Main.spriteBatch.GraphicsDevice.SetRenderTarget(RobeTarget);
                Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);

                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);
                Vector3[] palette =
                [
                    new Vector3(1.5f),
                    new Vector3(0f, 1f, 1.2f),
                    new Vector3(1f, 0f, 0f),
                ];
                ManagedShader overlayShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssassinColorProcessingShader");
                overlayShader.TrySetParameter("eyeScale", 1f);
                overlayShader.TrySetParameter("gradient", palette);
                overlayShader.TrySetParameter("gradientCount", palette.Length);
                overlayShader.TrySetParameter("textureSize", RobeMapTarget.Size() * 2);
                overlayShader.TrySetParameter("edgeColor", Color.Red.ToVector4());
                overlayShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
                overlayShader.Apply();

                Main.spriteBatch.Draw(RobeMapTarget, new Vector2(backSize / 2), null, Color.Black, 0f, RobeMapTarget.Size() * 0.5f, 2f, 0, 0);

                Main.spriteBatch.End();
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
            Robe.DampeningCoefficient = 0.17f;

            int steps = 15;
            float windSpeed = Math.Clamp(Main.WindForVisuals  * 8f, -1.3f, 0f);
            Vector2 robePosition = Player.Center + new Vector2(0, -50f * Player.gravDir).RotatedBy(Player.fullRotation);
            robePosition += Main.OffsetsPlayerHeadgear[(int)(Player.bodyFrame.Y / Player.bodyFrame.Height)] + Player.velocity;
            Vector3 wind = Vector3.UnitX * (LumUtils.AperiodicSin(ExistenceTimer * 0.029f) * 0.67f + windSpeed) * 1.74f;
            for (int i = 0; i < steps; i++)
            {
                for (int x = 0; x < Robe.Width; x++)
                {
                    for (int y = 0; y < 2; y++)
                        ConstrainParticle(robePosition + new Vector2((6 - x) * Player.direction, 0), Robe.particleGrid[x, y], 0f);
                }

                Robe.Simulate(0.06f, false, Vector3.UnitY * 5f + wind * Player.direction);
            }
        }

        private void ConstrainParticle(Vector2 anchor, ClothPoint? point, float angleOffset)
        {
            if (point is null)
                return;

            point.Position = new Vector3(anchor, 0f);
            point.IsFixed = false;
        }       
    }
}
