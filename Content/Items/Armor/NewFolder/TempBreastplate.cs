using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.NewFolder
{
    [AutoloadEquip(EquipType.Body)]
    public class TempBreastplate : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor";
        public override void SetStaticDefaults()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            int equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);

            ArmorIDs.Body.Sets.HidesTopSkin[equipSlot] = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlot] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.defense = 28;
            Item.rare = ModContent.RarityType<PureGreen>();
        }

        public override void UpdateEquip(Player player)
        {
            var modPlayer = player.Calamity();
            player.GetDamage<GenericDamageClass>() += 0.12f;
            player.GetCritChance<GenericDamageClass>() += 8;
            modPlayer.omegaBlueChestplate = true;
            modPlayer.noLifeRegen = true;
            modPlayer.omegaBlueSet = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<ReaperTooth>(5).
                AddIngredient<DepthCells>(18).
                AddIngredient<RuinousSoul>(3).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
