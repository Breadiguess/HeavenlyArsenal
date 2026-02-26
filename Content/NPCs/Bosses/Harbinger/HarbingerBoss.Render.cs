using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Harbinger
{
    public partial class HarbingerBoss
    {

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Bosses/Harbinger/image").Value;
            Main.EntitySpriteDraw(texture, Main.LocalPlayer.Center - screenPos, null, Color.White, 0, Vector2.Zero, 0.2f, 0);




            

            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    }

}
