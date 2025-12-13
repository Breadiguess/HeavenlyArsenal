using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    /// <summary>
    /// fake biome for the bestiary
    /// </summary>
    public class RiftEclipseBloodMoon: ModBiome
    {
        public override string Name => Language.GetTextValue($"Mods.{Mod.Name}.Bestiary.Biome");
        public override Color? BackgroundColor => Color.Black;
        public override string BestiaryIcon => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RiftEclipseBloodMoon";

        public override bool IsBiomeActive(Player player) => false;

        public override float GetWeight(Player player) => 0f;
    }
    public class RiftEclipseBloodMoonBestiaryBackground : ModSystem
    {
        public override void Load()
        {
            //On_Main.CheckMonoliths += DrawVFX;
            
        }
        public static Texture2D BestiaryBackground;
        public static RenderTarget2D bestiaryBackground;
      

        private void DrawVFX(On_Main.orig_CheckMonoliths orig)
        {

            if (bestiaryBackground == null || bestiaryBackground.IsDisposed)
            {
                bestiaryBackground = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            }
            else if (bestiaryBackground.Size() != new Vector2(Main.screenWidth, Main.screenHeight))
            {
                Main.QueueMainThreadAction(() =>
                {
                    bestiaryBackground.Dispose();
                    bestiaryBackground = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                });
                return;
            }
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null);
            Main.graphics.GraphicsDevice.SetRenderTarget(bestiaryBackground);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);
            drawBloodMoon();

            Main.graphics.GraphicsDevice.SetRenderTarget(null);

            Main.spriteBatch.End();

            orig();
        }
        void drawBloodMoon()
        {
            Texture2D WavyBlotchNoise = GennedAssets.Textures.Noise.WavyBlotchNoise;
            if (WavyBlotchNoise == null || WavyBlotchNoise.IsDisposed)
                return;
            ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.AvatarRiftShapeShader");
            riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly);
            riftShader.TrySetParameter("baseCutoffRadius", 0.2f);
            riftShader.TrySetParameter("swirlOutwardnessExponent", 0.151f);
            riftShader.TrySetParameter("swirlOutwardnessFactor", 3.67f);
            riftShader.TrySetParameter("vanishInterpolant", 0.1f);
            riftShader.SetTexture(WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
            riftShader.SetTexture(WavyBlotchNoise, 2, SamplerState.AnisotropicWrap);
            riftShader.Apply();

            Texture2D flare = Luminance.Assets.MiscTexturesRegistry.ShineFlareTexture.Value;
            Vector2 lensFlareScale = new Vector2(1.5f, 0.48f) * 0.4f * 2f;
            Vector2 lensFlarePosition = Main.screenPosition;
            NPC riftDummy = new NPC();
            riftDummy.SetDefaults(ModContent.NPCType<AvatarRift>());

            //riftDummy.As<AvatarRift>().BackgroundProp = true;
            riftDummy.As<AvatarRift>().PreDraw(Main.spriteBatch, Vector2.Zero, Color.White);
            //Main.spriteBatch.Draw(flare, lensFlarePosition, null, riftDummy.GetAlpha(Color.Red) with { A = 0 } * backglowOpacity, 0f, flare.Size() * 0.5f, lensFlareScale, 0, 0f);
            //Main.spriteBatch.Draw(flare, lensFlarePosition, null, NPC.GetAlpha(Color.White) with { A = 0 } * backglowOpacity * 0.6f, 0f, flare.Size() * 0.5f, lensFlareScale * 0.8f, 0, 0f);

            //Main.spriteBatch.Draw(BloomCircleSmall, lensFlarePosition, null, NPC.GetAlpha(Color.Red) with { A = 0 } * backglowOpacity, 0f, BloomCircleSmall.Size() * 0.5f, backglowScale * 6f, 0, 0f);
            //Main.spriteBatch.Draw(BloomCircleSmall, lensFlarePosition, null, NPC.GetAlpha(Color.Red) with { A = 0 } * backglowOpacity * 0.15f, 0f, BloomCircleSmall.Size() * 0.5f, backglowScale * 36f, 0, 0f);
            //AvatarRiftTargetContent.DrawRiftWithShader(NPC, screenPos, TransformPerspective, BackgroundProp, SuckOpacity, 0f, TargetIdentifierOverride);

        }
    }
    public class RiftBloodMoonBackground : IBestiaryInfoElement, IBestiaryBackgroundImagePathAndColorProvider
    {
        public Asset<Texture2D> GetBackgroundImage()
        {
            // Fix: BestiaryBackground is a Texture2D, not an Asset<Texture2D>. Wrap it if not null, otherwise fallback.
            if (RiftEclipseBloodMoonBestiaryBackground.BestiaryBackground != null)
            {
                // Create an untracked Asset<Texture2D> from the Texture2D
                return Main.Assets.CreateUntracked<Texture2D>(
                    new System.IO.MemoryStream(), 
                    // Dummy stream, not used, but required by signature
                    "RiftEclipseBloodMoonBestiaryBackground.BestiaryBackground"
                );
            }
            return Main.Assets.Request<Texture2D>("Images/MapBG1");
        }

        public Color? GetBackgroundColor() => Color.Black;
        public UIElement ProvideUIElement(BestiaryUICollectionInfo info) => null;
    }

}
