using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Thing
{
    partial class IkCreature
    {
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return false;
            foreach(var leg in Legs)
            {
                leg.Draw(spriteBatch, screenPos, drawColor);
            }
            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    }
}
