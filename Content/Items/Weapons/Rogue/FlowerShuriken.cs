using CalamityMod;
using HeavenlyArsenal.Content.Projectiles.Weapons.Rogue.ND_Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue
{
    class FlowerShuriken : ModItem
    {
        public override string Texture => "HeavenlyArsenal/Content/Items/Weapons/Magic/avatar_FishingRod";
        public override void SetDefaults()
        {
            DamageClass d;
            Mod calamity = ModLoader.GetMod("CalamityMod");
            calamity.TryFind("RogueDamageClass", out d);
            Item.DamageType = d;
           

            Item.crit = 70;
            Item.damage = 4000;
            Item.useStyle = 1;
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 4;
            Item.reuseDelay = 4;
            Item.noMelee = true;
            Item.Calamity().devItem = true;
            Item.useAnimation = 4;

            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<FlowerShuriken_Proj>();
            Item.shootSpeed = 20;

        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemNoGravity[Item.type] = true;
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
            ItemID.Sets.BonusAttackSpeedMultiplier[Item.type] = 0.76f;
            Item.ResearchUnlockCount = 1;
            //ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
            //ItemID.Sets.AnimatesAsSoul[Type] = true;
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(0, 1));
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //FlowerShuriken_Proj.CurrentFlower++;
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }


    }
}
