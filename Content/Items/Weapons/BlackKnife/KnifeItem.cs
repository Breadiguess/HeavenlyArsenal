using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.BlackKnife;

internal class KnifeItem : ModItem
{
    private int SwingStage;

    public override string LocalizationCategory => "Items.Weapons";

    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(3, 5));
    }

    public override void SetDefaults()
    {
        Item.DamageType = DamageClass.Generic;

        Item.damage = 3000;
        Item.shoot = ModContent.ProjectileType<KnifeSlash>();
        Item.shootSpeed = 2;

        Item.crit = 96;
        Item.useStyle = ItemUseStyleID.HiddenAnimation;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.noUseGraphic = true;
        Item.noMelee = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Item.useStyle = SwingStage == 0 ? ItemUseStyleID.Swing : ItemUseStyleID.RaiseLamp;
        var a = Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, velocity, type, damage, knockback);
        a.ai[2] = SwingStage;
        SwingStage++;

        if (SwingStage > 1)
        {
            SwingStage = 0;
        }

        return false;
    }
}