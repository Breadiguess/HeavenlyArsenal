using HeavenlyArsenal.Common;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.CCR_Weapon
{
    public enum NoxusWeaponState
    {
        Charge,
        Slash,
        Stab
    }
    class NoxusWeapon : ModItem
    {
        public static HeavenlyArsenalServerConfig Config => ModContent.GetInstance<HeavenlyArsenalServerConfig>();

        public override bool IsLoadingEnabled(Mod mod)
        {
            // Check config setting
            bool enabledInConfig = ModContent.GetInstance<HeavenlyArsenalServerConfig>().EnableSpecialItems;
            bool isOtherModLoaded = ModLoader.HasMod("CalRemix");

            return enabledInConfig || isOtherModLoaded;
        }



        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.WoodenSword);
            Item.shoot = ModContent.ProjectileType<NoxusWeaponProjectile>();
        }


    }
    public class NoxusWeaponProjectile : ModProjectile
    {
        #region Setup
        public NoxusWeaponState CurrentState;
        public ref float Time => ref Projectile.ai[0];

        public override string Texture => "HeavenlyArsenal/Content/Items/Weapons/Melee/CCR_Weapon/NoxusWeapon";
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }
        public override void SetStaticDefaults()
        {

        }
        #endregion

        #region AI
        /*
         * now the real question is, what do i want to do with this weapon?
         * i like the idea of a reverse flamethrower, but the noxus spray already kind of already fits the general flamethrower vibe.
         * or, it could be a knife?
         * 
         */
        public override void AI()
        {
            switch (CurrentState)
            {
                case NoxusWeaponState.Charge:

                    break;
                case NoxusWeaponState.Slash:

                    break;
                case NoxusWeaponState.Stab:

                    break;
            }
        }
        #endregion

        public override bool PreDraw(ref Color lightColor)
        {
            return base.PreDraw(ref lightColor);
        }
    }

}

