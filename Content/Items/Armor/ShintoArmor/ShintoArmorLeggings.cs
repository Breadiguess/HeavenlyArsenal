using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Items.Armor.Statigel;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Content.Items.Materials;
using NoxusBoss.Content.Rarities;

namespace HeavenlyArsenal.Content.Items.Armor.ShintoArmor;

[AutoloadEquip(EquipType.Legs)]
public class ShintoArmorLeggings : ModItem
{
    public static readonly int MoveSpeedBonus = 5;

    //public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MoveSpeedBonus);
   
    public override string LocalizationCategory => "Items.Armor.ShintoArmor";

    public override void SetStaticDefaults()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlot] = true;
        }
    }

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = Item.sellPrice(7, 43, 0, 2); // How many coins the item is worth
        Item.rare = ModContent.RarityType<AvatarRarity>();
        Item.defense = 55;
    }

    public override void UpdateEquip(Player player)
    {
        player.GetDamage(DamageClass.Generic) += 0.20f;
        player.moveSpeed += 0.5f;
        player.runAcceleration *= 1.2f;
        player.maxRunSpeed *= 1.2f;
        player.accRunSpeed *= 0.5f;
        player.runSlowdown *= 2f;
        var modPlayer = player.Calamity();
        player.moveSpeed += 0.3f;

        player.autoJump = true;
        player.jumpSpeedBoost += 1.6f;
        player.noFallDmg = true;
        player.GetModPlayer<ShintoArmorPlayer>().VoidBeltEquipped = true;
        modPlayer.DashID = ShintoArmorDash.ID;    
        player.dashType = 0;
        player.spikedBoots = 2;
    }

    public override void AddRecipes()
    {
        var recipe = CreateRecipe()
            .AddIngredient(ModContent.ItemType<AvatarMaterial>())
            .AddIngredient<DemonshadeGreaves>()
            .AddIngredient(ItemID.NinjaPants)
            .AddIngredient(ItemID.CrystalNinjaLeggings)
            .AddIngredient<StatigelGreaves>()
            .AddTile<DraedonsForge>();

        HeavenlyArsenal.TryAddModIngredient(recipe, "CalamityHunt", "ShogunPants");

        recipe.AddIngredient<StatisVoidSash>();
        recipe.Register();

       
    }
}