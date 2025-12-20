using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Items.Weapons.BlackKnife;

public class KnifeItem : ModItem
{
    public int Stage { get; set; }

    public override string LocalizationCategory => "Items.Weapons";

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(3, 5));
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.noUseGraphic = true;
        Item.noMelee = true;
        
        Item.DamageType = DamageClass.Generic;

        Item.damage = 3000;
        Item.crit = 96;
        
        Item.shootSpeed = 2f;
        Item.shoot = ModContent.ProjectileType<KnifeSlash>();


        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.HiddenAnimation;
    }

    public override void LoadData(TagCompound tag)
    {
        base.LoadData(tag);

        Stage = tag.GetInt(nameof(Stage));
    }

    public override void SaveData(TagCompound tag)
    {
        base.SaveData(tag);
        
        tag[nameof(Stage)] = Stage;
    }

    public override ModItem Clone(Item newEntity)
    {
        var instance = base.Clone(newEntity);
        
        if (instance is not KnifeItem clone)
        {
            return instance;
        }

        clone.Stage = Stage;

        return clone;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // TODO: Find a suitable method to change the item's use style.
        Item.useStyle = Stage == 0 ? ItemUseStyleID.Swing : ItemUseStyleID.RaiseLamp;
        
        var projectile = Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, velocity, type, damage, knockback, player.whoAmI, 0f, 0f, Stage);

        Stage++;

        if (Stage <= 1)
        {
            return false;
        }

        Stage = 0;
        
        return false;
    }
}