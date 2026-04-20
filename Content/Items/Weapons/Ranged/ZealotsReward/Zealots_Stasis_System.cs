using HeavenlyArsenal.Content.Items.Armor.BaseArmor;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ZealotsReward
{
    
    internal class Zealots_Stasis_System : ModSystem
    {

        
        public static RenderTarget2D FrozenNpcTarget;

        public static bool DrawingFrozenTarget;

        private static bool _queuedRebuild;
        private static int _width;
        private static int _height;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            On_Main.CheckMonoliths += BuildFrozenTarget;
            On_Main.DoDraw_DrawNPCsOverTiles += DrawFrozenOverlay;

            Main.QueueMainThreadAction(RebuildTarget);
        }

        public override void Unload()
        {
            if (Main.dedServ)
                return;

            On_Main.CheckMonoliths -= BuildFrozenTarget;
            On_Main.DoDraw_DrawNPCsOverTiles -= DrawFrozenOverlay;
            Main.QueueMainThreadAction(DisposeTarget);
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ || Main.instance?.GraphicsDevice is null)
                return;

            var pp = Main.instance.GraphicsDevice.PresentationParameters;
            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;

            if (width <= 0 || height <= 0)
                return;

            if (FrozenNpcTarget is null || width != _width || height != _height)
                _queuedRebuild = true;

            if (_queuedRebuild && !Main.gameMenu)
            {
                _queuedRebuild = false;
                Main.QueueMainThreadAction(RebuildTarget);
            }
        }

        private static void RebuildTarget()
        {
            if (Main.dedServ || Main.instance?.GraphicsDevice is null)
                return;

            GraphicsDevice gd = Main.instance.GraphicsDevice;
            var pp = gd.PresentationParameters;

            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;

            if (width <= 0 || height <= 0)
                return;

            FrozenNpcTarget?.Dispose();
            FrozenNpcTarget = new RenderTarget2D(
                gd,
                width,
                height,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.PreserveContents
            );

            _width = width;
            _height = height;
        }
        private static void DisposeTarget()
        {

            FrozenNpcTarget?.Dispose();
            FrozenNpcTarget = null;
            DrawingFrozenTarget = false;
        }

        private void BuildFrozenTarget(On_Main.orig_CheckMonoliths orig)
        {
            orig();

            if (Main.dedServ || Main.gameMenu || FrozenNpcTarget is null)
                return;



            GraphicsDevice gd = Main.instance.GraphicsDevice;
            RenderTargetBinding[] oldTargets = gd.GetRenderTargets();

            gd.SetRenderTarget(FrozenNpcTarget);
            gd.Clear(Color.Transparent);

            Main.spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            /*
             * float2 pixelSize;
float time;
float outlineThickness;
float4 outlineColor;
float4 iceTint;
float noiseScale;
float noiseStrength;
float frostContrast;
float frostThreshold;
            */


      
            DrawingFrozenTarget = true;
            try
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active)
                        continue;

                    if (!npc.TryGetGlobalNPC(out Zealots_Stasis_NPC stasis))
                        continue;

                    if (!stasis.DrawToFrozenRT)
                        continue;
              
                    Main.instance.DrawNPCDirect(Main.spriteBatch, npc, npc.behindTiles, Main.screenPosition);
                }
            }
            finally
            {
                DrawingFrozenTarget = false;
            }

            Main.spriteBatch.End();
            gd.SetRenderTargets(oldTargets);
        }

        private void DrawFrozenOverlay(On_Main.orig_DoDraw_DrawNPCsOverTiles orig, Main self)
        {
            orig(self);

            if (Main.dedServ || Main.gameMenu || FrozenNpcTarget is null)
                return;


            
            Main.spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Matrix.Identity
            );
            var iceShader = ShaderManager.GetShader("HeavenlyArsenal.IceShader");

            iceShader.SetTexture(FrozenNpcTarget, 0);
            iceShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoiseDetailed, 1, SamplerState.PointWrap);

            Vector2 targetSize = new Vector2(FrozenNpcTarget.Width, FrozenNpcTarget.Height);
            Vector2 zoom = Main.GameViewMatrix.Zoom;
            Vector2 invZoom = new Vector2(1f / zoom.X, 1f / zoom.Y);

            float worldToNoise = 0.003f;
            float noiseScale = 0.9f;

            Vector2 noiseUVScale = targetSize * invZoom * worldToNoise * noiseScale;
            Vector2 noiseUVOffset = Main.screenPosition * worldToNoise * noiseScale;

            iceShader.TrySetParameter("pixelSize", new Vector2(1f / FrozenNpcTarget.Width, 1f / FrozenNpcTarget.Height));
            iceShader.TrySetParameter("noiseUVScale", noiseUVScale);
            iceShader.TrySetParameter("noiseUVOffset", noiseUVOffset);

            iceShader.TrySetParameter("outlineColor", Color.CadetBlue.ToVector4());
            iceShader.TrySetParameter("iceTint", new Color(140, 190, 255).ToVector4());

            float thing = 1;
            iceShader.TrySetParameter("outlineThickness", 5f * thing);
            iceShader.TrySetParameter("frostThreshold", 0.12f * thing);
            iceShader.TrySetParameter("frostContrast", 1f * thing);
            iceShader.TrySetParameter("shellOpacity", 0.35f * thing);
            iceShader.TrySetParameter("ridgeStrength", 1.25f * thing);
            iceShader.TrySetParameter("rimStrength", 0.15f * thing);

            iceShader.TrySetParameter("interiorDesaturation", 0.35f * thing);
            iceShader.TrySetParameter("interiorDarkening", 0.82f);
            iceShader.TrySetParameter("interiorColdTintStrength", 0.18f * thing);

            iceShader.Apply();


            Main.spriteBatch.Draw(FrozenNpcTarget, new Vector2(0, 0), Color.White);

            Main.spriteBatch.End();
        }
    }
}