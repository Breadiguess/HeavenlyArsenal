using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeavenlyArsenal.Utilities.Extensions;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity.ScavSona
{
    internal class ScavSona_FloppyHair_Renderer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition()
        {
            return new AfterParent(PlayerDrawLayers.HairBack);
        }
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {

            var Owner = drawInfo.drawPlayer;

            DrawData a = new DrawData(ScavSona_FloppyHair_Player.ScavSona_Hair_Target, drawInfo.GetHeadDrawPosition(), null, Color.White, 0, ScavSona_FloppyHair_Player.ScavSona_Hair_Target.Size() / 2, 1, 0, 0);
            //a.color = drawInfo.colorHair;
            //a.shader = drawInfo.hairDyePacked;
            drawInfo.DrawDataCache.Add(a);
        }
    }
}
