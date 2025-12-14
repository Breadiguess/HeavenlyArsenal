using Luminance.Common.Utilities;
using NoxusBoss.Content.NPCs.Friendly;
using Terraria.GameContent;

namespace HeavenlyArsenal.Common.utils;

internal class Baldlyn : GlobalNPC
{
    private readonly int solynType = ModContent.NPCType<Solyn>();

    public override bool InstancePerEntity => true; // Corrected by using a property override instead of assignment  

    //public Texture2D baldlyn => ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/NPCs/Friendly/Baldlyn");
    public override void SetStaticDefaults()
    {
        // TextureAssets.Npc[solynType] = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/NPCs/Friendly/Baldlyn");
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.type == solynType)
        {
            //Solyn solyn = npc.ModNPC as Solyn;

            //Utils.DrawBorderString(spriteBatch, solyn.CurrentState.ToString(), npc.Center - screenPos, Color.AntiqueWhite, anchory: 2);
        }

        if (npc.type == solynType && Main.zenithWorld)
        {
            var drawPosition = npc.Center - screenPos + Vector2.UnitY * (npc.gfxOffY - 6f);

            if (npc.IsShimmerVariant)
            {
                var shimmerTexture = ModContent.Request<Texture2D>($"{TextureAssets.Npc[solynType].Value}_Shimmer").Value;
                Main.EntitySpriteDraw(shimmerTexture, drawPosition, null, npc.GetAlpha(drawColor), npc.rotation, shimmerTexture.Size() * 0.5f, npc.scale, 0);

                return false;
            }

            // Draw Solyn  
            var glowmaskColor = Color.White;
            var frame = npc.frame;
            var texture = TextureAssets.Npc[solynType].Value;
            var baldlyn = (Texture2D)ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/NPCs/Friendly/Baldlyn");

            var direction = npc.spriteDirection.ToSpriteDirection() ^ SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(baldlyn, drawPosition, frame, npc.GetAlpha(drawColor), npc.rotation, frame.Size() * 0.5f, npc.scale, direction);

            return false;
        }

        // Default draw behavior for other NPCs  
        return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
    }
}