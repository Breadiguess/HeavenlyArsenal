using System.Collections.Generic;
using System.Linq;
using HeavenlyArsenal.Content.Projectiles.Weapons.Magic;
using HeavenlyArsenal.Core.Items;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Content.Tiles;
using NoxusBoss.Core.GlobalInstances;
using Terraria.GameContent.ItemDropRules;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic;

public class avatar_FishingRod : ModItem
{
    public override string Texture => "HeavenlyArsenal/Content/Items/Weapons/Magic/avatar_FishingRod";

    public override string LocalizationCategory => "Items.Weapons.Magic";

    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 30;
        Item.DamageType = DamageClass.Magic;
        Item.damage = 4445;
        Item.knockBack = 3f;
        Item.useTime = 25;
        Item.useAnimation = 25;
        Item.autoReuse = true;
        Item.mana = 15;
        // No UseSound

        Item.holdStyle = 0; // Custom hold style
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.channel = true;
        Item.noUseGraphic = true;
        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<avatar_FishingRodProjectile>();
        Item.shootSpeed = 10f;
        Item.rare = ModContent.RarityType<AvatarRarity>();
        Item.value = Item.buyPrice(gold: 2);
    }

    public override bool CanUseItem(Player player)
    {
        return player.ownedProjectileCounts[Item.shoot] <= 0;
    }

    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Coral, 2)
            .AddTile<GardenFountainTile>()
            .Register();
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        // Find the fishing power tooltip from localization
        var fishingPowerTooltip = tooltips.FirstOrDefault(t => t.Mod == Mod.Name && t.Name == "FishingPower");

        // Find the main tooltip
        var mainTooltip = tooltips.FirstOrDefault(t => t.Mod == "Terraria" && t.Name == "Tooltip0");

        if (fishingPowerTooltip != null)
        {
            // Set a different color for the fishing power tooltip
            fishingPowerTooltip.OverrideColor = Color.Cyan;

            // Move the fishing power tooltip to the top of the list
            //tooltips.Remove(fishingPowerTooltip);
            tooltips.Insert(0, fishingPowerTooltip);
        }

        if (mainTooltip != null && fishingPowerTooltip != null)
        {
            // Ensure "Fishing Power" isn't duplicated in the main tooltip
            //mainTooltip.Text = mainTooltip.Text.Replace(fishingPowerTooltip.Text + "\n\n", "");
        }
    }

    public override void SetStaticDefaults()
    {
        GlobalNPCEventHandlers.ModifyNPCLootEvent += (npc, npcLoot) =>
        {
            if (npc.type == ModContent.NPCType<AvatarOfEmptiness>())
            {
                var normalOnly = new LeadingConditionRule(new Conditions.NotExpert());

                {
                    normalOnly.OnSuccess(ItemDropRule.Common(Type));
                }

                npcLoot.Add(normalOnly);
            }
        };

        ItemLootGlobalItem.RegisterLoot(AvatarOfEmptiness.TreasureBagID, ItemDropRule.Common(Type));
    }

    // The following prevents the rod from using mana when first used

    public override void Load()
    {
        On_Player.ItemCheck_PayMana += DoNotPayMana;
    }

    private bool DoNotPayMana(On_Player.orig_ItemCheck_PayMana orig, Player self, Item sItem, bool canUse)
    {
        if (sItem.type == ModContent.ItemType<avatar_FishingRod>())
        {
            return self.CheckMana(sItem.mana);
        }

        return orig(self, sItem, canUse);
    }
}