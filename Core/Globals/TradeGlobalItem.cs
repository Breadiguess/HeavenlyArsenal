using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.World.GameScenes.AvatarUniverseExploration;
using HeavenlyArsenal.Common.utils;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using HeavenlyArsenal.Common.Scenes;
using CalamityMod.Particles;
using NoxusBoss.Core.Utilities;
using System.Collections.Generic;

namespace HeavenlyArsenal.Core.Globals
{
    public class TradeVFXGlobalItem : GlobalItem
    {
        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // Check if the item type is one that has a trade registered and if the Avatar Universe condition is met.
            if (!VoidTradingSystem.TradeInputRegistry.InputItemTypes.Contains(item.type)|| !AvatarUniverseExplorationSystem.InAvatarUniverse)
                return true;

           
            Player player = Main.LocalPlayer;

            
            player.GetValueRef<int>(AvatarUniverseExplorationSky.TimeInUniverseVariableName).Value = 0;
          

            // Retrieve the item texture and its frame.
            Texture2D itemTexture = TextureAssets.Item[item.type].Value;
            Rectangle itemFrame = (Main.itemAnimations[item.type] == null)
                                    ? itemTexture.Frame()
                                    : Main.itemAnimations[item.type].GetFrame(itemTexture);

            float currentPower = 0f;
            // Calculate the center point where to draw the particle effect.
            Vector2 particleDrawCenter = position + new Vector2(1f, 6f);

            // Set the particle interpolation speed using a lerp between two values.
            //EnchantmentEnergyParticles.InterpolationSpeed = MathHelper.Lerp(0.035f, 0.1f, currentPower);
            //EnchantmentEnergyParticles.DrawSet(particleDrawCenter + Main.screenPosition);

            Texture2D a = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierCooldown_Icon").Value;
            // Draw the actual item.
            spriteBatch.Draw(itemTexture, position, itemFrame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(a, particleDrawCenter, itemFrame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            // Return false to skip the default drawing (since we handled it).
            return false;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(item, tooltips);
        }
    }

    public class TradeGlobalItemReturn : GlobalItem
    {
        public override bool InstancePerEntity => true;


        
    }

}

