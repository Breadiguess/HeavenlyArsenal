using CalamityMod.Projectiles.Ranged;
using Luminance.Assets;
using NoxusBoss.Content.Rarities;
using System.Collections.Generic;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    public class Aoe_Rifle_Item : ModItem
    {
        // IDEAS:
        // 1. hitscan rifle, with chinese kanji and multiple other languages for "finality" or "The end" or some other shtick on impact with something 
        // 2. authority?
        // 3. WOUND THE WORLD

        public override string LocalizationCategory => "Items.Weapons.Ranged";

        public override string Texture => MiscTexturesRegistry.ChromaticBurstPath;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.gunProj[Type] = true;   
        }
        public override void SetDefaults()
        {
            Item.value = Terraria.Item.buyPrice(4, 20, 10, 4);
            Item.damage = 20_000;
            Item.rare = ModContent.RarityType<AvatarRarity>();
            
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.useAmmo = AmmoID.Bullet;
            Item.shootSpeed = 40;    
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.shoot = ModContent.ProjectileType<Aoe_Rifle_HeldProj>();
            Item.noUseGraphic = true;
            Item.useTime = 60;
            Item.useAnimation = 60;
            
        }

        public override void HoldItem(Player player)
        {
            if (player.ownedProjectileCounts[Item.shoot] <1)
            {
                Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, Vector2.Zero, Item.shoot, 0, 0);

            }
        }
        public override bool CanUseItem(Player player) => false;
        public override bool CanShoot(Player player) => false;


        public override void UpdateInventory(Player player)
        {

        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

        }
    }
}
