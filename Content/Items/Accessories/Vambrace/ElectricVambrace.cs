using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.Vambrace
{
   
    public class ElectricVambrace : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 56;
            Item.defense = 8;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.accessory = true;
        }
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 8));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }


        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<DischargePlayer>().Active = true;
            var modPlayer = player.Calamity();
            modPlayer.transformer = true;
            modPlayer.aSpark = true;

            //modPlayer.DashID = ElectricVambraceDash.ID;
        }


        public override void AddRecipes()
        {
            CreateRecipe().
            AddIngredient<TheTransformer>(1).
            AddIngredient<AmidiasSpark>(1).
            AddIngredient<SlagsplitterPauldron>(1).
            AddIngredient<LeviathanAmbergris>(1).
            AddIngredient<AscendantSpiritEssence>(8).
            AddTile<CosmicAnvil>().
            AddCondition(Condition.NearWater).
            Register();
        }
    }
    public class ElectricVambracePlayer : ModPlayer
    {
        internal bool Active;
        public bool HasReducedDashFirstFrame 
        { 
            get; 
            private set; 
        }
        

        
        public override void Load()
        { 

        }
       

        public override void PostUpdateMiscEffects()
        {
            if (Active)
            {

            }
        }
     
    }

   

}
