using CalamityMod.Graphics.Renderers;
using HeavenlyArsenal.Common.Utilities;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using NoxusBoss.Assets;
using NoxusBoss.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Accessories.BloodyLeechScarf
{
    internal class LeechScarf_DrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.ArmOverItem);
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {

            if (drawInfo.drawPlayer.GetValueRef<bool>(LeechScarf_Item.WearingAccessory))
            {
                return true;
            }
            return false;
        }
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
           
            Texture2D texture = ModContent.Request<Texture2D>($"{Mod.Name}/Content/Items/Accessories/BloodyLeechScarf/LeechScarf_Item_Shoulder").Value;

            var Position = drawInfo.BodyPosition() +new Vector2(0);
                           

            Color color = Color.White;
            var d = new DrawData(texture, Position,
            drawInfo.drawPlayer.legFrame, color, 0f, drawInfo.drawPlayer.legFrame.Size()/2, 1f, drawInfo.playerEffect);
            
            drawInfo.DrawDataCache.Add(d);
        }
    }
}
