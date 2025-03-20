using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.BrutalForgiveness;

public class BrutalForgiveness : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 12;
        Item.height = 12;
        Item.DamageType = DamageClass.Magic;
        Item.damage = 7777;
        Item.knockBack = 3f;
        Item.useTime = 25;
        Item.useAnimation = 25;
        Item.autoReuse = true;
        Item.mana = 15;
        Item.holdStyle = 0;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.channel = true;
        Item.noUseGraphic = true;
        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<BrutalForgivenessProjectile>();
        Item.shootSpeed = 10f;
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
        Item.value = Item.buyPrice(gold: 2);
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
}
