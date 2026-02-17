using HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.SwordData
{
    public static class SwordAttackDatabase
    {
        public static readonly Dictionary<SwordAttackID, SwordAttackDef> Attacks;

        static SwordAttackDatabase()
        {
            Attacks = new()
            {
                [SwordAttackID.Small_Light1] = new SwordAttackDef
                {
                    ID = SwordAttackID.Small_Light1,
                    Mode = SwordMode.Small,
                    EnergyGain = 4,
                    DashSpeed = 6f,
                    DamageMultiplier = 0.85f,
                    ProjectileType = ModContent.ProjectileType<SmallBladeSlash>()
                },

                [SwordAttackID.Small_Shadowfy] = new SwordAttackDef
                {
                    ID = SwordAttackID.Small_Shadowfy,
                    Mode = SwordMode.Small,
                    EnergyCost = 0,
                    DashSpeed = -5f,
                    GrantsIFrames = true,
                    ProjectileType = ModContent.ProjectileType<ShadowfyDash>()
                },

                // etc.
            };
        }
    }

    
}
