using CalamityMod.Graphics.Renderers;
using CalamityMod.Items;
using CalamityMod.Rarities;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.Cosmetic
{
    [AutoloadEquip(EquipType.Back)]
    public class AuraCosmetic :ModItem
    {
        public new string LocalizationCategory => "Items.Accessories";
        
        public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 56;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.accessory = true;

            //ArmorIDs.Head.Sets.IsTallHat[Item.headSlot] = true;
        }

    }

    public class AuraCosmeticDraw : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.HeadBack);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.head == EquipLoader.GetEquipSlot(Mod, nameof(AuraCosmetic), EquipType.Face);
        //ggrrrr

        public override bool IsHeadLayer => true;

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Accessories/AuraCosmetic").Value;

            Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;

            Vector2 drawPos = Main.screenPosition - new Vector2(drawInfo.drawPlayer.headPosition.X - 30f, drawInfo.drawPlayer.headPosition.Y);

            float VisualScale = 0.1f;
            Texture2D innerRiftTexture = AssetDirectory.Textures.VoidLake.Value;


            Main.EntitySpriteDraw(glow, drawPos, glow.Frame(), Color.Red with { A = 200 }, drawInfo.drawPlayer.headRotation, glow.Size() * 0.5f, new Vector2(0.12f, 0.25f) * VisualScale, 0, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, Color.White, drawInfo.drawPlayer.headRotation, innerRiftTexture.Size() * 0.5f, new Vector2(0.2f, 0.4f) * VisualScale, 0, 0);
            
            Texture2D ChromaticSpires = GennedAssets.Textures.GreyscaleTextures.ChromaticSpires;

            float spireScale = 0.1f;
            float spireOpacity = drawInfo.drawPlayer.opacityForAnimation;
            Vector2 drawPosition = drawInfo.drawPlayer.Center+new Vector2(0,-20f) - Main.screenPosition ;
            //Main.NewText($"drawpos: {drawPosition}, head pos: {drawInfo.drawPlayer.headPosition}");
            float rotation = drawInfo.drawPlayer.headRotation + MathHelper.ToRadians(-45);

            Main.spriteBatch.Draw(ChromaticSpires, drawPosition, null, (Color.Violet with { A = 0 }) * spireOpacity, rotation+ MathHelper.PiOver4, ChromaticSpires.Size() * 0.5f, spireScale, 0, 0f);
            Main.spriteBatch.Draw(ChromaticSpires, drawPosition, null, (Color.Violet with { A = 0 }) * spireOpacity, rotation+MathHelper.ToRadians(180) + MathHelper.PiOver4, ChromaticSpires.Size() * 0.5f, spireScale, 0, 0f);
            /*
            Main.spriteBatch.PrepareForShaders();
            //new Texture Placeholder = GennedAssets.Textures.Extra.Code;
            ManagedShader postProcessingShader = ShaderManager.GetShader("HeavenlyArsenal.FusionRifleClothPostProcessingShader");
            postProcessingShader.TrySetParameter("textureSize", 300);
            postProcessingShader.TrySetParameter("edgeColor", new Color(208, 37, 40).ToVector4());
            postProcessingShader.SetTexture(GennedAssets.Textures.SecondPhaseForm.Beads3, 0, SamplerState.LinearWrap);
            postProcessingShader.Apply();
            /*
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            Texture2D innerRiftTexture = AssetDirectory.Textures.VoidLake.Value;
            Color edgeColor = new Color(1f, 0.06f, 0.06f);
            */


            /*

            ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
            riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.2f);
            riftShader.TrySetParameter("baseCutoffRadius", 0.1f);
            riftShader.TrySetParameter("swirlOutwardnessExponent", 0.42f);
            riftShader.TrySetParameter("swirlOutwardnessFactor", 5f);
            riftShader.TrySetParameter("vanishInterpolant", 1f);
            riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
            riftShader.TrySetParameter("edgeColorBias", 0.15f);
            riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
            riftShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap);
            riftShader.Apply();

            Main.spriteBatch.Draw(innerRiftTexture, drawPos, null, Color.White, drawInfo.drawPlayer.headRotation + MathHelper.Pi, innerRiftTexture.Size() * 0.5f, new Vector2(0.2f, 0.4f) * VisualScale, 0, 0);
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White, 0f, texture.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
            */
            /*
            Main.NewText($"Drawpos: {drawPos}", Color.AntiqueWhite);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            */
        }
    }
}
