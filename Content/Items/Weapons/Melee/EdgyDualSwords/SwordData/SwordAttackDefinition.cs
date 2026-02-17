using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.SwordData
{
    public sealed class SwordAttackDef
    {
        public SwordAttackID ID;
        public SwordMode Mode;

        // Resource interaction
        public int EnergyCost;
        public int EnergyGain;

        // Timing
        public int StartupFrames;
        public int ActiveFrames;
        public int RecoveryFrames;

        // Movement
        public float DashSpeed;
        public Vector2 DashDirection;
        public bool GrantsIFrames;

        // Combat behavior
        public bool IsChargeAttack;
        public bool CanParry;
        public bool IsCounter;
        public float DamageMultiplier;
        public float KnockbackMultiplier;

        // Projectile
        public int ProjectileType;
    }

}
