using HeavenlyArsenal.Common.Keybinds;
using Terraria.GameInput;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.SwordData
{
    public partial class SwordCombatPlayer : ModPlayer
    {
        public SwordMode CurrentMode;
        public SwordAttackID? CurrentAttack;
        public int AttackTimer;
        public bool IsCharging;
        public float CurrentResource;
        public int MaxResource;

        public bool Active;


        // Shadowfy / counter
        public bool CanCounter;
        public int CounterWindow;

        public SwordAttackID? QueuedAttack;
        public void QueueAttack(SwordAttackID id)
        {
            QueuedAttack = id;
        }
        private void HandleInput()
        {
            if (Active == false)
            {
                QueuedAttack = null;
                return;
            }

            if (Player.itemTime > 0)
                return;

            // Mode Switch
            if (Player.controlUseTile && !IsCharging)
            {
                // Debug Text
                CurrentMode = CurrentMode == SwordMode.Small? SwordMode.Large: SwordMode.Small;

                Main.NewText($"CurrentMode: {CurrentMode}");
                QueuedAttack = null;
                return;
            }
           

            if (CurrentResource == MaxResource && KeybindSystem.DualModeActivete.JustPressed && CurrentMode == SwordMode.Large)
            {
                // Debug Text
                Main.NewText($"CurrentMode: {CurrentMode}");
                CurrentMode = SwordMode.Dual;
            }
            else if (CurrentMode == SwordMode.Dual && CurrentResource == 0)
            {
                CurrentMode = SwordMode.Large;
            }

            // Small mode attacks
            // Light attacks
            if (CurrentMode == SwordMode.Small)
            {
                if (Player.controlUseItem)
                {
                    // Debug Text
                    QueueAttack(SwordAttackID.Small_Light1);
                    CurrentAttack = SwordAttackID.Small_Light1;
                    Main.NewText($"QueuedAttack: {QueuedAttack}\nCurrentAttack: {CurrentAttack}\nAttackTimer: {AttackTimer}");
                    if (CurrentAttack == null)
                    {
                        QueueAttack(SwordAttackID.Small_Light1);
                        CurrentAttack = SwordAttackID.Small_Light1;
                        AttackTimer = 30;
                    }
                    /*
                    else if (CurrentAttack == SwordAttackID.Small_Light1)
                    {
                        QueueAttack(SwordAttackID.Small_Light2);
                        CurrentAttack = SwordAttackID.Small_Light2;
                        AttackTimer = 30;
                    }
                    else if (CurrentAttack == SwordAttackID.Small_Light2)
                    {
                        // Due to double combo attack. Attack twice.
                        QueueAttack(SwordAttackID.Small_Light3);
                        CurrentAttack = SwordAttackID.Small_Light3;
                        AttackTimer = 75;
                    }
                    else if (CurrentAttack == SwordAttackID.Small_Light3)
                    {
                        QueueAttack(SwordAttackID.Small_Light4);
                        CurrentAttack = SwordAttackID.Small_Light4;
                        AttackTimer = 90;
                    }
                    else if(CurrentAttack == SwordAttackID.Small_Light4)
                    {

                        QueueAttack(SwordAttackID.Small_Light1);
                        CurrentAttack = SwordAttackID.Small_Light1;
                    }
                    // Shadowfy
                    else if (Player.controlUseTile && CanShadowfy())
                    {
                        QueueAttack(SwordAttackID.Small_Shadowfy);
                        CanCounter = true;
                        AttackTimer = 90;
                        CounterWindow = 60;
                    }*/
                }
            }

            if (CurrentMode == SwordMode.Large)
            {
                // Debug Text
                //Main.NewText($"QueuedAttack: {QueuedAttack}\nCurrentAttack: {CurrentAttack}\nAttackTimer: {AttackTimer}");

                // Large mode attacks
                if (Player.controlUseItem && !IsCharging)
                {
                    if (QueuedAttack == null)
                    {
                        QueueAttack(SwordAttackID.Large_Light);
                        CurrentAttack = SwordAttackID.Large_Light;
                        AttackTimer = 60;
                        return;
                    }
                }

                // Parry
                if (Player.controlUseTile && QueuedAttack == null)
                {
                    QueueAttack(SwordAttackID.Large_ParryCounter);
                    IsCharging = true;
                    CanCounter = true;
                    AttackTimer = 120;
                    AttackTimer++;
                }

                // Charge attack
                if (Player.controlUseTile && QueuedAttack == SwordAttackID.Large_ParryCounter)
                {
                    QueueAttack(SwordAttackID.Large_ChargeSwing);
                    IsCharging = true;
                    CanCounter = false;
                    AttackTimer = 120;
                }
                if (Player.controlUseTile && QueuedAttack == SwordAttackID.Large_ChargeWave)
                {
                    QueueAttack(SwordAttackID.Large_ChargeWave);
                    IsCharging = true;
                    AttackTimer = 120;
                }
            }

            if (CurrentMode == SwordMode.Dual)
            {
                // Debug Text
                Main.NewText($"QueuedAttack: {QueuedAttack}\nCurrentAttack: {CurrentAttack}\nAttackTimer: {AttackTimer}");

                // Dual mode attacks
                if (Player.controlUseItem)
                {
                    if (QueuedAttack == null)
                    {
                        QueueAttack(SwordAttackID.Dual_Light1);
                        AttackTimer = 90;
                    }
                    if (QueuedAttack == SwordAttackID.Dual_Light1)
                    {
                        QueueAttack(SwordAttackID.Dual_Light2);
                        AttackTimer = 120;
                    }
                }

                // Charge attack
                if (Player.controlUseTile)
                {
                    QueueAttack(SwordAttackID.Dual_ChargeDash);
                    IsCharging = true;
                }
            }

            if (AttackTimer == 0)
            {
                QueuedAttack = null;
            }
        }

        public void SwapSword()
        {
            CurrentMode = CurrentMode == SwordMode.Small ? SwordMode.Large : SwordMode.Small;

        }

        private bool CanShadowfy()
        {
            var isSmallModeAttack =
                QueuedAttack == SwordAttackID.Small_Light1
                || QueuedAttack == SwordAttackID.Small_Light2
                || QueuedAttack == SwordAttackID.Small_Light3
                || QueuedAttack == SwordAttackID.Small_Light4;

            return isSmallModeAttack && KeybindSystem.ShadowTeleport.JustPressed;
        }

        public override void Load()
        {
            On_Player.OnHurt_Part1 += CancelIfParrying;
        }

        private void CancelIfParrying(On_Player.orig_OnHurt_Part1 orig, Player self, Player.HurtInfo info)
        {
            SwordCombatPlayer sword = self.GetModPlayer<SwordCombatPlayer>();

            orig(self, info);
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Player.HeldItem.type == ModContent.ItemType<TrioSword_Item>())
            {
                Active = true;
                HandleInput();
            }
            else
            {
                QueuedAttack = null;
            }
        }

        public override void ResetEffects()
        {
            Active = false;
            MaxResource = 100;

            if (CurrentResource < 0)
                CurrentResource = 0;

            AttackTimer--;

            if (CounterWindow > 0)
                CounterWindow--;
            else
                CanCounter = false;
        }
    }
}
