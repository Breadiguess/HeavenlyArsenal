using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Accessories.RadiantRampart;

[AutoloadEquip(EquipType.Shield)]   
public class RadiantRampartItem : ModItem
{
    public override string LocalizationCategory => "Items.Accessories";

    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 6));
        ItemID.Sets.AnimatesAsSoul[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 64;
        Item.height = 62;
        Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
        Item.defense = 12;
        Item.defense += 8;
        Item.accessory = true;
        Item.rare = ModContent.RarityType<CalamityMod.Rarities.HotPink>();
    }

    public override void AddRecipes()
    {
        var recipe = CreateRecipe()
            .AddIngredient(ModContent.ItemType<RampartofDeities>())
            .AddIngredient(ModContent.ItemType<Radiance>())
            
            .AddTile(ModContent.TileType<DraedonsForge>());

        recipe.Register();
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<RampartPlayer>().Equipped = true;
        var modPlayer = player.Calamity();

        // Directly inherits both Cross Necklace and Deific Amulet iframe boosts
        player.longInvince = true;
        modPlayer.dAmulet = true;

        // Large hit iframe effect taken from Seraph Tracers
        modPlayer.rampartOfDeities = true;

        // Ice Barrier buff inherited from Frozen Turtle Shell
        if (player.statLife <= player.statLifeMax2 * 0.5)
        {
            player.AddBuff(BuffID.IceBarrier, 5);
        }

        // Knockback immunity inherited from Paladin's Shield
        player.noKnockback = true;

        // Paladin's Shield application
        if (player.statLife > player.statLifeMax2 * 0.25f)
        {
            player.hasPaladinShield = true;

            if (player.whoAmI != Main.myPlayer && player.miscCounter % 10 == 0)
            {
                var myPlayer = Main.myPlayer;

                if (Main.player[myPlayer].team == player.team && player.team != 0)
                {
                    var teamPlayerXDist = player.position.X - Main.player[myPlayer].position.X;
                    var teamPlayerYDist = player.position.Y - Main.player[myPlayer].position.Y;

                    if ((float)Math.Sqrt(teamPlayerXDist * teamPlayerXDist + teamPlayerYDist * teamPlayerYDist) < 800f)
                    {
                        Main.player[myPlayer].AddBuff(BuffID.PaladinsShield, 20);
                    }
                }
            }
        }

        player.statLifeMax2 += 70;

        // Abyss light, debuff near-immunity, life regen effects, and massively enhances debuff halving
        modPlayer.purity = true;

        // Inherits effects from Honey Dew and Living Dew
        modPlayer.alwaysHoneyRegen = true;
        modPlayer.honeyDewHalveDebuffs = true;
        modPlayer.livingDewHalveDebuffs = true;

        // Add light if the other accessories aren't equipped and visibility is turned on
        if (!(modPlayer.rOoze || modPlayer.aAmpoule) && !hideVisual)
        {
            Lighting.AddLight(player.Center, new Vector3(1.32f, 1.32f, 1.82f));
        }
    }
}

public class RampartPlayer : ModPlayer
{
    public bool Equipped;

    public override void ResetEffects()
    {
        Equipped = false;
    }

    public override void PostUpdateMiscEffects() { }
}