using HeavenlyArsenal.Content.Buffs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.ArsenalPlayer
{
    class StimPlayer : ModPlayer
    {
        public float addictionChance;
        public float stimsUsed;
        public bool Withdrawl;
        public bool Addicted;
       
        private int LoseStimTimer;
        private int timeSinceLastStim;
        private const int StimDuration = 15 * 60;
        private const int AddictionCheckInterval = 60 * 60;
        private const int WithdrawlChance = 3; // 1/3 chance
        private const int WithdrawlDuration = 30 * 60;

        private int WithdrawlTime;

        public override void Initialize()
        {           
            timeSinceLastStim = 0;
        }

        public override void PostUpdate()
        {
            addictionChance = MathHelper.Clamp(5*stimsUsed/10,0,100);
           // Main.NewText($"Stims Used: {stimsUsed}, Addiction chance: {addictionChance}, Widthdrawl: {Withdrawl}, Addicted {Addicted}, Time since last Stim: {timeSinceLastStim}, addictionCheckInterval = {AddictionCheckInterval}, withdrawltime: {WithdrawlTime}, LoseStimTimer: {LoseStimTimer }", Color.AntiqueWhite);
            if (Withdrawl)
            {
                WithdrawlTime++;
                if (!Player.HasBuff(ModContent.BuffType<StimWithdrawl_Debuff>())) 
                {
                    WithdrawlTime = 1;
                    Player.AddBuff(ModContent.BuffType<StimWithdrawl_Debuff>(), WithdrawlDuration, false, false); // Apply debuff
                   
                }
                if(WithdrawlTime >= WithdrawlDuration)
                {
                    Withdrawl = false;
                }
            }
            else if(Addicted)
            {
                // Handle addiction effects here if needed
                Player.AddBuff(ModContent.BuffType<StimAddicted_Debuff>(), 100, true, false);
                timeSinceLastStim++;

                // Check for addiction if enough time has passed
                if (timeSinceLastStim >= AddictionCheckInterval)
                {
                    
                    
                        Withdrawl = true;
                        Addicted = false; // Reset addiction status
                    
                    timeSinceLastStim = 0;
                }
            }

            if (LoseStimTimer == 1200 && stimsUsed > 0)
            {
                stimsUsed -= 1;
                LoseStimTimer = -1;
            }

            LoseStimTimer++;
        }

        public void UseStim()
        {
            stimsUsed++;
            timeSinceLastStim = 0;

            // Update addiction chance
            LoseStimTimer = 0;

            // Check for addiction
            if (Main.rand.NextFloat() < addictionChance / 100f)
            {
                Addicted = true;
                Withdrawl = false;
            }
        }

        public void EndWithdrawl()
        {

            Addicted = false;
            Withdrawl = false;
            stimsUsed = 0f;
            addictionChance = 0f;
        }

        public override void UpdateDead()
        {

            Addicted = false;
            Withdrawl = false;
            stimsUsed = 0f;
            addictionChance = 0f;
            timeSinceLastStim = 0;
        }
    }
}
