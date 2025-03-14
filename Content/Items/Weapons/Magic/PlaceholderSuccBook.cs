

using NoxusBoss.Content.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using HeavenlyArsenal.Content.Projectiles.Magic;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic
{
    public class PlaceholderSuccBook : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 48;
            Item.damage = 1750;
            Item.DamageType = DamageClass.Magic;
            Item.rare = ModContent.RarityType<AvatarRarity>();
            //Item.rare = ModContent.RarityType<VioletRarity>();
            Item.useStyle = ItemUseStyleID.Thrust;
            Item.mana = 15;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.value = Item.sellPrice(gold: 20);
            Item.shoot = ModContent.ProjectileType<Succ>();
            Item.shootSpeed = 4f;
           // if (ModLoader.HasMod(HUtils.CalamityMod)) {
           //     ModRarity r;
           //     Mod calamity = ModLoader.GetMod(HUtils.CalamityMod);
           //     calamity.TryFind("Violet", out r);
           //     Item.rare = r.Type;
           // }
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<Succ>()] <= 0;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.manaCost = 0f;

            if (player.ownedProjectileCounts[ModContent.ProjectileType<Succ>()] <= 0) {
                if (player.altFunctionUse == 0) {
                    Projectile.NewProjectileDirect(source, position, velocity, type, damage, 0, player.whoAmI);
                }
            }

            return false;
        }

        public override void AddRecipes()
        {
            
        }
    }
}
