using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.World.GameScenes.AvatarUniverseExploration;
using HeavenlyArsenal.Common.utils;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using CalamityMod.Particles;
using NoxusBoss.Core.Utilities;
using System.Collections.Generic;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using HeavenlyArsenal.Core.Systems;
using NoxusBoss.Content.NPCs.Bosses.CeaselessVoid;
using Humanizer;

namespace HeavenlyArsenal.Core.Globals
{



    public class TradeVFXGlobalItem : GlobalItem
    {

      
        public override void UpdateInventory(Item item, Player player)
        {
            if (VoidTradingSystem.TradeInputRegistry.InputItemTypes.Contains(item.type))
            {
                HeavenlyArsenal.forceOpenRift = true;
                // Check if the Avatar Universe condition is met.
                if (AvatarUniverseExplorationSystem.InAvatarUniverse)
                {
                    // Set the time in the universe variable to 0
                    player.GetValueRef<int>(AvatarUniverseExplorationSky.TimeInUniverseVariableName).Value = 0;
                }
            }
            else
            {
                HeavenlyArsenal.forceOpenRift = false;
            }
            base.UpdateInventory(item, player);
        }
        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            
            
            // Check if the item type is one that has a trade registered and if the Avatar Universe condition is met.
            if (!VoidTradingSystem.TradeInputRegistry.InputItemTypes.Contains(item.type)|| !AvatarUniverseExplorationSystem.InAvatarUniverse)
                return true;


            Player player = Main.LocalPlayer;

            //TODO: Fade out the further away you are from the rift

            float FadeInterp= 0;



            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            player.GetValueRef<int>(AvatarUniverseExplorationSky.TimeInUniverseVariableName).Value = 0;
            Texture2D itemTexture = TextureAssets.Item[item.type].Value;
            Rectangle itemFrame = (Main.itemAnimations[item.type] == null)? itemTexture.Frame(): Main.itemAnimations[item.type].GetFrame(itemTexture);

            Vector2 particleDrawCenter = position + new Vector2(0f, 10f);
            Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;
            
            Main.EntitySpriteDraw(glow, particleDrawCenter- Main.screenPosition, glow.Frame(), Color.Red with { A = 200 }, 0, glow.Size() * 0.5f, new Vector2(0.12f, 0.25f), 0, 0);
            Texture2D innerRiftTexture = AssetDirectory.Textures.VoidLake.Value;
            Color edgeColor = new Color(1f, 0.06f, 0.06f);
            float timeOffset = (Main.myPlayer * 2.5552343f + item.type * 0.05f);

            ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
            riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.2f + timeOffset);
            riftShader.TrySetParameter("baseCutoffRadius", 0.24f);
            riftShader.TrySetParameter("swirlOutwardnessExponent", 0.2f);
            riftShader.TrySetParameter("swirlOutwardnessFactor", 3f);
            riftShader.TrySetParameter("vanishInterpolant", 0.01f +FadeInterp);
            riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
            riftShader.TrySetParameter("edgeColorBias", 0.1f);
            riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
            riftShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap);
            riftShader.Apply();

            Main.spriteBatch.Draw(innerRiftTexture, particleDrawCenter, null, Color.White, 0 + MathHelper.Pi, innerRiftTexture.Size() * 0.5f, new Vector2(0.1f, 0.05f), 0, 0);

            Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            spriteBatch.Draw(itemTexture, position, itemFrame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
         
            return false;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (VoidTradingSystem.TradeInputRegistry.InputItemTypes.Contains(item.type))
                item.SetNameOverride("Trade Item");
            base.ModifyTooltips(item, tooltips);
        }
    }

    public class TradeGlobalItemReturn : GlobalItem
    {
        public override bool InstancePerEntity => true;


        
    }

}

