using NoxusBoss.Content.Rarities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity
{
    [AutoloadEquip(EquipType.Body)]
    class LightCultist_Robe : ModItem
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

        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                EquipLoader.AddEquipTexture(Mod, "HeavenlyArsenal/Content/Items/Armor/Vanity/LightCultist_Waist", EquipType.Waist, this);
            }
        }
        public override void SetStaticDefaults()
        {
            // HidesHands defaults to true which we don't want.
            var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            ArmorIDs.Body.Sets.HidesArms[equipSlot] = true;
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlot] = true;
        }

        public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
        {
            robes = true;
            equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
            //ArmorIDs.Legs.Sets.
        }
    }
}
