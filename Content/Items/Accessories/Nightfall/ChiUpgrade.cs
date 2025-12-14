using System.Collections.Generic;
using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Rarities;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Accessories.Nightfall;

internal class ChiUpgrade : ModItem
{
    public override string LocalizationCategory => "Items.Accessories";

    public override string Texture => "HeavenlyArsenal/Content/Items/Accessories/Nightfall/nightfall";

    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 5));
        ItemID.Sets.AnimatesAsSoul[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.defense = 4;
        Item.rare = ModContent.RarityType<Turquoise>();

        Item.value = Item.sellPrice(0, 10, 3);

        Item.width = 28;
        Item.height = 28;
    }

    public override void UpdateEquip(Player player)
    {
        player.GetModPlayer<NightfallPlayer>().NightfallActive = true;
        player.Calamity().trinketOfChi = true;

        player.GetCritChance<GenericDamageClass>() += 4;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        var player = Main.LocalPlayer;

        // Check if the accessory is equipped
        if (player != null && player.GetModPlayer<NightfallPlayer>().NightfallActive)
        {
            // Example debug values, replace with actual logic if available
            var damageBucketTotal = player.GetModPlayer<NightfallPlayer>().DamageBucketTotal;
            var damageBucketMax = NightfallPlayer.DamageBucketMax;
            var interpolant = Math.Clamp(damageBucketTotal / damageBucketMax, 0f, 1f);

            var critIncrease = player.GetModPlayer<NightfallPlayer>().CritModifier;

            var HitCooldown = NightfallPlayer.CooldownMax;
            // Create an interpolant out of damage bucket total / damagebucketmax
            // Increase the crit chance of the player based on that interpolant x 100

            var debugLine1 = new TooltipLine(Mod, "DebugDamageBucket", $"[DEBUG] Damage Bucket: {damageBucketTotal} / {damageBucketMax}")
            {
                OverrideColor = Color.Red
            };

            var debugLine2 = new TooltipLine(Mod, "DebugCrit", $"[DEBUG] Modified Crit Chance: {critIncrease}%")
            {
                OverrideColor = Color.Red
            };

            var debugLine3 = new TooltipLine(Mod, "DebugBurstCooldown", $"[DEBUG] HitCooldown Max: {HitCooldown / 60f} seconds")
            {
                OverrideColor = Color.Red
            };

            tooltips.Add(debugLine1);
            tooltips.Add(debugLine2);
            tooltips.Add(debugLine3);
        }
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<TrinketofChi>())
            .AddIngredient(ItemID.SoulofMight, 15)
            .AddIngredient(ItemID.SoulofNight, 8)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}