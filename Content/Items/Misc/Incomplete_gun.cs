using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.NPCs.Bosses.Avatar;
using NoxusBoss.Core.Graphics;
using NoxusBoss.Core.World.WorldSaving;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using Terraria.DataStructures;
using NoxusBoss.Content.Rarities;
using Terraria;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Content.Items.Materials;
using HeavenlyArsenal.Content.Projectiles.Misc;

namespace HeavenlyArsenal.Content.Items.Misc
{
    internal class Incomplete_gun : ModItem
    {

        public override void SetDefaults()
        {

            Item.rare = ModContent.RarityType<HotPink>();

            Item.damage = -1;
            Item.shootSpeed = 40f;
            Item.width = 40;
            Item.height = 32;
            Item.useTime = 40;
            Item.reuseDelay = 40;




            Item.consumeAmmoOnFirstShotOnly = true;
            Item.useAnimation = 0;
            Item.noUseGraphic = true;
            Item.useTurn = true;
            Item.channel = true;


            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6;

            //Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            //Item.shoot = ModContent.ProjectileType<FusionRifle_Projectile>();
            Item.shoot = ModContent.ProjectileType<incomplete_gunHoldout>();
            Item.ChangePlayerDirectionOnShoot = true;
            Item.noMelee = true;
        }
        private bool fusionOut = false;


        public override void HoldItem(Player player)
        {
            if (!fusionOut)
            {
                // Spawn the projectile
                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item), // Source of the projectile
                    player.Center.X,               // X coordinate of the spawn location
                    player.Center.Y,               // Y coordinate of the spawn location
                    0f, 0f,                        // Velocity (set to 0 for stationary)
                    ModContent.ProjectileType<incomplete_gunHoldout>(), // Type of the projectile
                    Item.damage,                   // Damage of the projectile
                    Item.knockBack,                // Knockback of the projectile
                    player.whoAmI                  // Owner of the projectile
                );
                fusionOut = true; // Set the flag to true to prevent further spawns
            }
        }
        public override void UpdateInventory(Player player)
        {
            // Reset the flag if the item is not being held
            if (player.HeldItem.type != Item.type)
            {
                fusionOut = false;
            }
        }






        public override void AddRecipes()
        {
            CreateRecipe().
                AddTile<DraedonsForge>().
                AddIngredient(ModContent.ItemType<Auric_Catalyst>()).
                AddIngredient(ModContent.ItemType<shadowspec_GunParts>()).
                AddIngredient(ModContent.ItemType<AuricBar>(), 5).
                Register();
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // Rewrite tooltips post-Nameless Deity.
            if (BossDownedSaveSystem.HasDefeated<NamelessDeityBoss>())
            {
                // Remove the default tooltips.
                tooltips.RemoveAll(t => t.Name.Contains("Tooltip"));

                // Generate and use custom tooltips.
                string specialTooltip = this.GetLocalizedValue("TooltipPostNamelessDeity");
                TooltipLine[] tooltipLines = specialTooltip.Split('\n').Select((t, index) =>
                {
                    Item.rare = ModContent.RarityType<AvatarRarity>();
                    return new TooltipLine(Mod, $"NamelessDeityTooltip{index + 1}", t);
                }).ToArray();

                // Color the last tooltip line.
                tooltipLines.Last().OverrideColor = DialogColorRegistry.NamelessDeityTextColor;
                tooltips.AddRange(tooltipLines);
                return;
            }

            // Make the final tooltip line about needing to pass the test use Nameless' dialog.
            TooltipLine tooltip = tooltips.FirstOrDefault(t => t.Name == "Tooltip1");
            if (tooltip is not null)
                tooltip.OverrideColor = DialogColorRegistry.NamelessDeityTextColor;
        }


        //prevent player from shooting (duh)
        public override bool CanShoot(Player player) => false;




    }
}
