using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Misc
{
    class GoodAppleSling : ModItem
    {
        public override string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.useAmmo = AmmoID.Dart;
            Item.useTime = 1;
            Item.useStyle = 0;

            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.width = Item.height = 32;
            Item.damage = 13;
            Item.crit = -100;
            Item.shoot = ModContent.ProjectileType<GoodAppleSlingHeld>();
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemNoGravity[Item.type] = true;

            ItemID.Sets.ItemIconPulse[Item.type] = false;
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(1, 1));
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
        }

        private bool SlingOut(Player player) => player.ownedProjectileCounts[Item.shoot] > 0;

        public override void HoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (!SlingOut(player))
                {
                    Projectile Sling = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
                   // spear.rotation = -MathHelper.PiOver2 + 1f * player.direction;
                }
            }
        }

    }


    class GoodAppleAmmo : GlobalItem
    {
        public override bool? CanBeChosenAsAmmo(Item ammo, Item weapon, Player player)
        {
            if (ModLoader.TryGetMod("CalamityHunt", out Mod calamityHunt)) 
            {
                if (ammo.type == calamityHunt.Find<ModItem>("BadApple").Type && weapon.ModItem is GoodAppleSling) 
                {
                    return true;
                }
            }
            if (ammo.ModItem is GoodApple && weapon.ModItem is GoodAppleSling)
            {
                return true;
            }
            else
            {
                return base.CanBeChosenAsAmmo(ammo, weapon, player);
            }
        }
    }
}
