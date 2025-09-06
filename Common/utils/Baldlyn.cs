using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Friendly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Common.utils
{
    class Baldlyn : GlobalNPC
    {
        private int solynType = ModContent.NPCType<Solyn>();
        //public Texture2D baldlyn => ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/NPCs/Friendly/Baldlyn");
        public override void SetStaticDefaults()
        {
          // TextureAssets.Npc[solynType] = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/NPCs/Friendly/Baldlyn");
           
        }

        public override bool InstancePerEntity => true; // Corrected by using a property override instead of assignment  

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.type == solynType && (Main.zenithWorld ))
            {
                Vector2 drawPosition = npc.Center - screenPos + Vector2.UnitY * (npc.gfxOffY - 6f);
                if (npc.IsShimmerVariant)
                {
                    Texture2D shimmerTexture = ModContent.Request<Texture2D>($"{TextureAssets.Npc[solynType].Value}_Shimmer").Value;
                    Main.EntitySpriteDraw(shimmerTexture, drawPosition, null, npc.GetAlpha(drawColor), npc.rotation, shimmerTexture.Size() * 0.5f, npc.scale, 0);
                    return false;
                }

                // Draw Solyn  
                Color glowmaskColor = Color.White;
                Rectangle frame = npc.frame;
                Texture2D texture = TextureAssets.Npc[solynType].Value;
                Texture2D baldlyn = (Texture2D)ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/NPCs/Friendly/Baldlyn");

                SpriteEffects direction = npc.spriteDirection.ToSpriteDirection() ^ SpriteEffects.FlipHorizontally;

                Main.EntitySpriteDraw(baldlyn, drawPosition, frame, npc.GetAlpha(drawColor), npc.rotation, frame.Size() * 0.5f, npc.scale, direction);

                return false;
            }

            // Default draw behavior for other NPCs  
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
    }
}
