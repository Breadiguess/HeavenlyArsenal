using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab
{
    partial class BloodCrab
    {

        public override void FindFrame(int frameHeight)
        {
            
        }
        void RenderSquidLauncher(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            RenderSquidLauncher(spriteBatch, screenPos, drawColor);

            Utils.DrawBorderString(spriteBatch, CurrentState.ToString(), NPC.Center - screenPos, Color.AntiqueWhite);
            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    }
}
