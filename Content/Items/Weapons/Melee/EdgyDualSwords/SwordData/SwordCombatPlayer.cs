using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.SwordData
{
    public class    SwordCombatPlayer : ModPlayer
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
            if (Player.controlUseItem && QueuedAttack == null)
            {
                QueueAttack(SwordAttackID.Small_Light1);
            }

            if (Player.controlUseTile && CanShadowfy())
            {
                QueueAttack(SwordAttackID.Small_Shadowfy);
            }
        }

        private bool CanShadowfy()
        {
            return true;
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



        public override void ResetEffects()
        {
            Active = false;

            if (CounterWindow > 0)
                CounterWindow--;
            else
                CanCounter = false;
        }
    }
}
