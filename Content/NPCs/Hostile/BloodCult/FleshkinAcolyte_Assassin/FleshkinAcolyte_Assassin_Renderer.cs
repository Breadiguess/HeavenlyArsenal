using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodCult.FleshkinAcolyte_Assassin
{
    public partial class FleshkinAcolyte_Assassin
    {


        public override void FindFrame(int frameHeight)
        {

            if(CurrentState == Behaviors.sneak)
            {
                const int WalkFrameStart = 0;
                const int WalkFrameEnd = 5;

                const int SneakFrameStart = 6;
                const int SneakFrameEnd = 12;

                const float MoveThreshold = 0.1f;

                var moving = MathF.Abs(NPC.velocity.X) > MoveThreshold;
                var sneaking = StealthAmount > 25f;

                if (!moving)
                {
                    if (sneaking)
                    {
                        NPC.frame.Y = SneakFrameStart;
                        return;
                    }

                    NPC.frame.Y = WalkFrameStart;
                    return;
                }

                var start = WalkFrameStart;
                var end = WalkFrameEnd;
                var animSpeed = 6f;

                if (sneaking)
                {
                    start = SneakFrameStart;
                    end = SneakFrameEnd;
                    animSpeed = 9f;
                }

                var frameCount = end - start + 1;
                var animTick = (int)MathF.Floor(Time / animSpeed);
                var frameOffset = animTick % frameCount;
                if (frameOffset < 0)
                {
                    frameOffset += frameCount;
                }

                var frameIndex = start + frameOffset;
                NPC.frame.Y = frameIndex;
            }


            if (CurrentState == Behaviors.slash)
            {

            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;


            Rectangle frame = tex.Frame(1, Main.npcFrameCount[Type]+1, 0, NPC.frame.Y);
            Main.EntitySpriteDraw(tex, NPC.Center - screenPos,
                frame,
                drawColor * NPC.Opacity,
                NPC.rotation,
                frame.Size() / 2+ new Vector2(0,20),
                NPC.scale,
                NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                0
            );

            //Utils.DrawBorderString(spriteBatch, StealthAmount.ToString(), NPC.Center - screenPos, drawColor);
            return false;
        }
    }
}
