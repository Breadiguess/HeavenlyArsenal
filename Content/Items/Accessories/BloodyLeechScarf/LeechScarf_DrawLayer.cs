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
        public override void Load()
        {
            On_PlayerDrawLayers.DrawPlayer_28_ArmOverItem += On_PlayerDrawLayers_DrawPlayer_28_ArmOverItem;
        }

        private void On_PlayerDrawLayers_DrawPlayer_28_ArmOverItem(On_PlayerDrawLayers.orig_DrawPlayer_28_ArmOverItem orig, ref PlayerDrawSet drawinfo)
        {
            if (drawinfo.drawPlayer.GetValueRef<bool>(LeechScarf_Item.WearingAccessory))
            {
                Draw(ref drawinfo);
                return;
                
            }

            orig(ref drawinfo);
        }

        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.ArmOverItem);
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            
            Player guy = drawInfo.drawPlayer;

            return guy.GetModPlayer<LeechScarf_Player>().Active;
        }
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
           
            Texture2D texture = ModContent.Request<Texture2D>($"{Mod.Name}/Content/Items/Accessories/BloodyLeechScarf/LeechScarf_Item_Shoulder").Value;

            var Position = drawInfo.BodyPosition() +new Vector2(0);


            Color color = Lighting.GetColor(drawInfo.drawPlayer.Center.ToTileCoordinates().X, drawInfo.drawPlayer.Center.ToTileCoordinates().Y, Color.White);
            var d = new DrawData(texture, Position,
            drawInfo.drawPlayer.legFrame, color, 0f, drawInfo.drawPlayer.legFrame.Size()/2, 1f, drawInfo.playerEffect);
            d.shader = drawInfo.cNeck;
            drawInfo.DrawDataCache.Add(d);
        }
    }
}
