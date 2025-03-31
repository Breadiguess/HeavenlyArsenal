using CalamityMod.Rarities;
using HeavenlyArsenal.Content.Projectiles.Weapons.Melee.AvatarSpear;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
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
        Item.width = 40;
        Item.height = 32;
        Item.useTime = 40;
        Item.reuseDelay = 40;

        Item.useAnimation = 0;
        Item.useTurn = true;
        Item.channel = true;
        Item.knockBack = 6;
        Item.autoReuse = true;
        Item.ChangePlayerDirectionOnShoot = true;
        Item.noMelee = false;
        Item.noUseGraphic = false;
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
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center.X, player.Center.Y, 0f, 0f, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
        }
    }
}
