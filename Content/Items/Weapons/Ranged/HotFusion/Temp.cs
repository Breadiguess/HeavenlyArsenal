using HeavenlyArsenal.Content.Items.Weapons.Ranged.ColdFusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.HotFusion
{
    class Temp : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items";
        public override string Texture => "HeavenlyArsenal/Content/Projectiles/Misc/ChaliceOfFun_Juice";

        public override void SetDefaults()
        {
            Item.width = Item.height = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = Item.useAnimation = 20;
            Item.shoot = ModContent.ProjectileType<ColdBurst>();
            Item.shootSpeed = 16f;
            Item.DamageType = DamageClass.Generic;
            Item.damage = 1;
        }
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }
    }
}
