using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.ArsenalPlayer
{
    internal class GenderSwapPlayer : ModPlayer
    {
        public bool IsTrans;

        public override void SaveData(TagCompound tag)
        {
            tag["HasUsedGenderSwapPotion"] = IsTrans;
        }
        public override void LoadData(TagCompound tag)
        {
            IsTrans = tag.GetBool("HasUsedGenderSwapPotion");
        }

        public override void CopyClientState(ModPlayer targetCopy)
        {
            var clone = targetCopy as GenderSwapPlayer;
            clone.IsTrans = IsTrans;
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            var clone = clientPlayer as GenderSwapPlayer;
            if (clone.IsTrans != IsTrans)
            {
            }
        }
    }
    internal class GenderSwapGlobalItem : GlobalItem
    {
        public override void OnConsumeItem(Item item, Player player)
        {
            if(item.type == ItemID.GenderChangePotion && !player.GetModPlayer<GenderSwapPlayer>().IsTrans)
                player.GetModPlayer<GenderSwapPlayer>().IsTrans = true;
        }
    }
}
