using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Items.Armor.Statigel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.Nightfall
{
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
            Item.rare = ModContent.RarityType<CalamityMod.Rarities.Turquoise>();

            Item.value = Terraria.Item.sellPrice(0, 10, 3, 0);

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
            Player player = Main.LocalPlayer;
            // Check if the accessory is equipped
            if (player != null && player.GetModPlayer<NightfallPlayer>().NightfallActive)
            {
                // Example debug values, replace with actual logic if available
                int damageBucketTotal = player.GetModPlayer<NightfallPlayer>().DamageBucketTotal;
                int damageBucketMax = NightfallPlayer.DamageBucketMax;
                float interpolant = Math.Clamp(damageBucketTotal/damageBucketMax, 0f, 1f);

                int critIncrease = player.GetModPlayer<NightfallPlayer>().CritModifier;

                int HitCooldown = NightfallPlayer.CooldownMax;
                // Create an interpolant out of damage bucket total / damagebucketmax
                // Increase the crit chance of the player based on that interpolant x 100
               
                
                TooltipLine debugLine1 = new TooltipLine(Mod, "DebugDamageBucket", $"[DEBUG] Damage Bucket: {damageBucketTotal} / {damageBucketMax}")
                {
                    OverrideColor = Microsoft.Xna.Framework.Color.Red
                };
                TooltipLine debugLine2 = new TooltipLine(Mod, "DebugCrit", $"[DEBUG] Modified Crit Chance: {critIncrease}%")
                {
                    OverrideColor = Microsoft.Xna.Framework.Color.Red
                };

                TooltipLine debugLine3 = new TooltipLine(Mod, "DebugBurstCooldown", $"[DEBUG] HitCooldown Max: {HitCooldown / 60f} seconds")
                {
                    OverrideColor = Microsoft.Xna.Framework.Color.Red
                };
                
                tooltips.Add(debugLine1);
                tooltips.Add(debugLine2);
                tooltips.Add(debugLine3);
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
              .AddIngredient(ModContent.ItemType<TrinketofChi>(),1)
              .AddIngredient(ItemID.SoulofMight,15)
              .AddIngredient(ItemID.SoulofNight, 8)
              .AddTile(TileID.MythrilAnvil)
              .Register();
        }
    }
}
