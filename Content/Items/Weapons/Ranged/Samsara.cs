using CalamityMod;
using HeavenlyArsenal.Content.Projectiles.Weapons.Ranged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged
{
    class Samsara : ModItem
    {
        public override void SetDefaults()
        {
            Item.shoot = ModContent.ProjectileType<PossibilitySeed>();
            Item.shootSpeed = 20f;
            Item.width = 20;
            Item.height = 20;
            Item.useStyle = 5;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 100;
            Item.crit = 20;
            Item.knockBack = 5f;
            Item.value = 10000;
            Item.rare = 2;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.noMelee = true;

            Item.Calamity().devItem = true;

        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }




    }
}
