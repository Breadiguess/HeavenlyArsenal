using CalamityMod;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.WeeabouScythe;

[LegacyName("FlowerShuriken")]
internal class Closure : ModItem
{
    public override void SetDefaults()
    {
        DamageClass d;
        var calamity = ModLoader.GetMod("CalamityMod");
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
        Item.knockBack = 2;
        Item.useAnimation = 4;

        Item.noUseGraphic = false;
        Item.shoot = ModContent.ProjectileType<ClosureStealth>();
        Item.shootSpeed = 20;
    }

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ItemNoGravity[Item.type] = true;
        ItemID.Sets.AnimatesAsSoul[Item.type] = true;
        ItemID.Sets.BonusAttackSpeedMultiplier[Item.type] = 0.76f;
        Item.ResearchUnlockCount = 1;
        ItemID.Sets.gunProj[Item.type] = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.Calamity().StealthStrikeAvailable())
        {
            return true;
        }

        return false;

        //FlowerShuriken_Proj.CurrentFlower++;
        return base.Shoot(player, source, position, velocity, type, damage, knockback);
    }
}