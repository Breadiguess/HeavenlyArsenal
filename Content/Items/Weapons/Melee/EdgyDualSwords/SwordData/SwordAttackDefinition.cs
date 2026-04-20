namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.SwordData
{
    public sealed class SwordAttackDef
    {
        public SwordAttackID ID;
        public SwordMode Mode;

        // Resource interaction
        public int EnergyCost;
        public int EnergyGain;


        public int ProjectileLifeTime;

        // Movement
        public float DashSpeed;
        public Vector2 DashDirection;
        public bool GrantsIFrames;

        // Combat behavior
        public bool IsChargeAttack;
        public bool CanParry;
        public bool IsCounter;

        public int ParryStartWindow;
        public int ParryEndWindow;


        public float DamageMultiplier;
        public float KnockbackMultiplier;
        public float UseTimeMultiplier = 1;


        // Projectile
        public int ProjectileType;
    }

}
