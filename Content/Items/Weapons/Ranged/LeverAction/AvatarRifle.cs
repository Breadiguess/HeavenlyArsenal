namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.LeverAction;

public class AvatarRifle : ModItem
{
    public override string LocalizationCategory => "Items.Weapons.Ranged";

    public override string Texture => "HeavenlyArsenal/Content/Items/Weapons/Ranged/LeverAction/AvatarRifle";

    public override void SetDefaults()
    {
        Item.damage = 30;
        Item.DamageType = DamageClass.Ranged;
        Item.shoot = ModContent.ProjectileType<AvatarRifle_Held>();
        Item.shootSpeed = 1;

        Item.useAmmo = AmmoID.Bullet;

        Item.useAnimation = 5;
        Item.noUseGraphic = true;
        Item.useTurn = true;
        Item.channel = true;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.knockBack = 6;
    }

    public override bool CanShoot(Player player)
    {
        return false;
    }

    public override void HoldItem(Player player)
    {
        if (player.ownedProjectileCounts[Item.shoot] < 1)
        {
            var proj = Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, Vector2.Zero, Item.shoot, 10, 0);
        }
    }
}