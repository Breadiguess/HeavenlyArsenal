using HeavenlyArsenal.Common.Utilities;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Rarities;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity
{
    [AutoloadEquip(EquipType.Head)]
    class LightCultist_Helmet : ModItem
    {
        public override string LocalizationCategory => "Items.Armor.Vanity.LightCultist";
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 28;
            Item.rare = ModContent.RarityType<NamelessDeityRarity>();
            Item.value = 0;
            Item.vanity = true;
            Item.maxStack = 1;
        }
    }

    class lightCultist_Drawlayer: PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.FrontAccFront);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.armor[10].IsAir && drawInfo.drawPlayer.armor[0].ModItem is LightCultist_Helmet || drawInfo.drawPlayer.armor[10].ModItem is LightCultist_Helmet;

        public override bool IsHeadLayer => false;



        protected override void Draw(ref PlayerDrawSet drawInfo)
        {

            Asset<Texture2D> texture = ModContent.Request<Texture2D>($"HeavenlyArsenal/Content/Items/Armor/Vanity/Halo");

            DrawData drawData = new DrawData(texture.Value, drawInfo.HeadPosition(), drawInfo.drawPlayer.headFrame, drawInfo.colorHair, drawInfo.drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect, 0);
            //drawData.shader = drawInfo.hairDyePacked;
            drawInfo.DrawDataCache.Add(drawData);

        }
    }
}
