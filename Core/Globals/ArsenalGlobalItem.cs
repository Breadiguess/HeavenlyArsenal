using CalamityMod.Items;
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.UI.CalamitasEnchants;
using CalamityMod;
using HeavenlyArsenal.ArsenalPlayer;
using HeavenlyArsenal.Content.Items.Weapons.Melee;
using HeavenlyArsenal.Content.Projectiles.Weapons.Melee.AvatarSpear;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Steamworks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using NoxusBoss.Core.World.GameScenes.AvatarUniverseExploration;
using System.Collections.Generic;


namespace HeavenlyArsenal.Core.Globals;

public class ArsenalGlobalItem : GlobalItem
{
    public delegate void ModifyItemLootDelegate(Item item, ItemLoot loot);

    public static event ModifyItemLootDelegate? ModifyItemLootEvent;

    public static List<Enchantment> EnchantmentList { get; internal set; } = new List<Enchantment>();


    public override void ModifyItemLoot(Item item, ItemLoot loot)
    {
        ModifyItemLootEvent?.Invoke(item, loot);
    }

    

    // TODO: try to mess around with the Items name while empowered
    /*
    public override void SetDefaults(Item item)
    {
          
        if (item.netID == ModContent.ItemType<AvatarLonginus>())
        {
            foreach (Player.GetModPlayer<AvatarSpearHeatPlayer>().Active in )
            {
                if (Player.GetModPlayer<AvatarSpearHeatPlayer>().Active)
                    item.SetNameOverride("");
            }
            
        }
    }
    */
}
