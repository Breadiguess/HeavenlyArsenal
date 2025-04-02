using CalamityMod.Rarities;
using HeavenlyArsenal.Content.Projectiles.Weapons.Melee.AvatarSpear;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee;

public class AvatarLonginus : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.Spears[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.rare = ModContent.RarityType<HotPink>();

        Item.damage = 10000;
        Item.shootSpeed = 40f;
        Item.crit = 84;
        Item.width = 40;
        Item.height = 32;
        Item.useTime = 40;
        Item.reuseDelay = 40;

        Item.useAnimation = 0;
        Item.useTurn = true;
        Item.channel = true;
        Item.knockBack = 3;
        Item.autoReuse = true;
        Item.ChangePlayerDirectionOnShoot = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.shoot = ModContent.ProjectileType<AvatarLonginusHeld>();
    }


    //TODO: replace the current sound styles with different ones more actualized of the spear

    public static readonly SoundStyle ThrustSound = new("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_charge3");

    public static readonly SoundStyle HitSound = new("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_fire");

    public static readonly SoundStyle FullyChargedSound = new("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_FullyCharged");

    private bool SpearOut(Player player) => player.ownedProjectileCounts[Item.shoot] > 0;

    public override void HoldItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            if (!SpearOut(player))
            {
                Projectile spear = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
                spear.rotation = -MathHelper.PiOver2 + 1f * player.direction;
            }
        }
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => false;

    
}
