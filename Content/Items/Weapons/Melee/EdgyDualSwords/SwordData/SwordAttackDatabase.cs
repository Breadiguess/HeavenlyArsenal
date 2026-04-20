using HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.Projectiles;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.SwordData
{
    public static class SwordAttackDatabase
    {
        public static readonly Dictionary<SwordAttackID, SwordAttackDef> Attacks;

        static SwordAttackDatabase()
        {
            #region Small Mode Attacks
            Attacks = new()
            {
                [SwordAttackID.Small_Light1] = new SwordAttackDef
                {
                    ID = SwordAttackID.Small_Light1,
                    Mode = SwordMode.Small,
                    EnergyGain = 4,
                    DashSpeed = 6f,
                    ProjectileLifeTime = 70,
                    DamageMultiplier = 0.85f,
                    ProjectileType = ModContent.ProjectileType<TrioSwordProjectile>()
                },

                [SwordAttackID.Small_Light2] = new SwordAttackDef
                {
                    ID = SwordAttackID.Small_Light2,
                    Mode = SwordMode.Small,
                    EnergyGain = 4,
                    DashSpeed = 6f,
                    DamageMultiplier = 0.9f,
                    ProjectileType = ModContent.ProjectileType<TrioSwordProjectile>()
                },

                [SwordAttackID.Small_Light3] = new SwordAttackDef
                {
                    ID = SwordAttackID.Small_Light3,
                    Mode = SwordMode.Small,
                    EnergyGain = 2,
                    DashSpeed = 3f,
                    DashDirection = Main.LocalPlayer.DirectionTo(Main.MouseWorld),
                    DamageMultiplier = 0.65f,
                    ProjectileType = ModContent.ProjectileType<TrioSwordProjectile>()
                },

                [SwordAttackID.Small_Shadowfy] = new SwordAttackDef
                {
                    ID = SwordAttackID.Small_Shadowfy,
                    Mode = SwordMode.Small,
                    EnergyCost = 0,
                    DashSpeed = -5f,
                    GrantsIFrames = true,
                    //ProjectileType = ModContent.ProjectileType<ShadowfyDash>()
                },
                #endregion

                #region Large Mode Attacks
                [SwordAttackID.Large_Light] = new SwordAttackDef
                {
                    ID = SwordAttackID.Large_Light,
                    Mode = SwordMode.Large,

                    ProjectileLifeTime = 70,
                    EnergyGain = 6,
                    DamageMultiplier = 1.25f,
                    //ProjectileType = ModContent.ProjectileType<>() // Need projectile
                },

                [SwordAttackID.Large_ChargeSwing] = new SwordAttackDef
                {
                    ID = SwordAttackID.Large_ChargeSwing,
                    Mode = SwordMode.Large,
                    EnergyGain = 12,
                    DamageMultiplier = 2f,
                    IsChargeAttack = true,
                    //ProjectileType = ModContent.ProjectileType<>()
                },

                [SwordAttackID.Large_ChargeWave] = new SwordAttackDef
                {
                    ID = SwordAttackID.Large_ChargeWave,
                    Mode = SwordMode.Large,
                    DamageMultiplier = 5f,
                    //ProjectileType = ModContent.ProjectileType<>()
                },

                [SwordAttackID.Large_ParryCounter] = new SwordAttackDef
                {
                    ID = SwordAttackID.Large_ParryCounter,
                    Mode = SwordMode.Large,
                    EnergyCost = 0,
                    IsCounter = true,
                    CanParry = true,
                    //ProjectileType = ModContent.ProjectileType<>()
                },
                #endregion

                #region Dual Mode Attacks
                [SwordAttackID.Dual_Light1] = new SwordAttackDef
                {
                    ID = SwordAttackID.Dual_Light1,
                    Mode = SwordMode.Dual,
                    DamageMultiplier = 1.1f,
                    //ProjectileType = ModContent.ProjectileType<>()
                },

                [SwordAttackID.Dual_Light2] = new SwordAttackDef
                {
                    ID = SwordAttackID.Dual_Light2,
                    Mode = SwordMode.Dual,
                    DamageMultiplier = 1.2f,
                    //ProjectileType = ModContent.ProjectileType<>()
                },

                [SwordAttackID.Dual_ChargeDash] = new SwordAttackDef
                {
                    ID = SwordAttackID.Dual_ChargeDash,
                    Mode = SwordMode.Dual,
                    IsChargeAttack = true,
                    DamageMultiplier = 3f,
                    //ProjectileType = ModContent.ProjectileType<>()
                }
                #endregion
            };
        }
    }
}
