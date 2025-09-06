
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BadSun
{
    internal class BadSunItem : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetDefaults()
        {
            Item.useTime = 2;
            Item.useStyle = 4;
            Item.useAnimation = 2;
            Item.damage = 30000;
            Item.DamageType = DamageClass.Summon;
            Item.shoot = ModContent.ProjectileType<DoomedSerenity>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
        }
        public override void SetStaticDefaults()
        {
            ItemID.Sets.StaffMinionSlotsRequired[Item.type] = 1;
        }

        public override bool CanShoot(Player player) =>

            true;//player.ownedProjectileCounts[ModContent.ProjectileType<DoomedSerenity>()] <= 0;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<DoomedSerenity>()] >= 1)
            {
                // If the player already has a DoomedSerenity, call HandleReposition() to reposition the existing projectile
                foreach (Projectile proj in Main.projectile)
                {
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<DoomedSerenity>())
                    {
                        // Assumes DoomedSerenity has a public method HandleReposition(Player player, Vector2 position)
                        if (proj.ModProjectile is DoomedSerenity doomedSerenity)
                        {
                            
                            doomedSerenity.HandleReposition(Main.MouseWorld);
                        }
                        break;
                    }
                }
                return false;
            }
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }
        public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
        {
            //todo: overwrite the 
            return base.PreDrawTooltip(lines, ref x, ref y);
        }
    }
}
