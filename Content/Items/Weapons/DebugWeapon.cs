using HeavenlyArsenal.Content.Projectiles.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons
{
    class DebugWeapon :ModItem
    {
        public override string Texture => "HeavenlyArsenal/Content/Projectiles/Misc/ChaliceOfFun_Juice";
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.NightsEdge);
            Item.damage = 40939;
            Item.DamageType = DamageClass.Generic;
            
            Item.shoot = ModContent.ProjectileType<EyeOfTranscendenceProjectile>();
        }

    }
}
