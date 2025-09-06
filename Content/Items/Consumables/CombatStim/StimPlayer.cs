using HeavenlyArsenal.Common;
using HeavenlyArsenal.Content.Buffs.Stims;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Consumables.CombatStim
{
    class StimPlayer : ModPlayer
    {
        public int stimsUsed;
        public int MAX_STIMS_USED = 10;

        public bool Withdrawl;
        private int WithdrawlDuration = 32 * 60;
        private int WithdrawlTime = 0;

        public bool Addicted;
        private int AddictionCheckInterval = 20 * 60;
        public float addictionChance;

        public int LoseStimTimer = 0;
        public int timeSinceLastStim = 0;
      

        public override void Initialize()
        {           
            timeSinceLastStim = 0;
        }

        public override void PostUpdate()
        {
            LoseStimTimer++;
            //Main.NewText($"Stims Used: {stimsUsed}, Addiction chance: {addictionChance}, Widthdrawl: {Withdrawl}, Addicted {Addicted}, Time since last Stim: {timeSinceLastStim}, addictionCheckInterval = {AddictionCheckInterval}, withdrawltime: {WithdrawlTime}, LoseStimTimer: {LoseStimTimer }", Color.AntiqueWhite);

            addictionChance =(float) Math.Clamp(Math.Pow(stimsUsed, stimsUsed/2),0,100);
                
                // MathHelper.Clamp(10*stimsUsed/3+stimsUsed,0,100);
            if (Withdrawl)
            {
                if (Player.HasBuff(ModContent.BuffType<StimAddicted_Debuff>()))
                    Player.ClearBuff(ModContent.BuffType<StimAddicted_Debuff>());
                //YOU CAN'T HAVE ADDICTED AND WITHDRAWL AT THE SAME TIME STOP IT
                if (Addicted)
                {
                   Withdrawl = false;
                   return;
                }
                

                if (!Player.HasBuff(ModContent.BuffType<StimWithdrawl_Debuff>())) 
                {
                    WithdrawlTime = 1;
                    Player.AddBuff(ModContent.BuffType<StimWithdrawl_Debuff>(), WithdrawlDuration, false, false); // Apply debuff
                   
                }
                if(WithdrawlTime >= WithdrawlDuration)
                {
                    Withdrawl = false;
                }

                //some extra stuff for psychosis
                if (HeavenlyArsenalClientConfig.Instance.StimVFX)
                {
                    //only you can hear it, so you look like a freak
                    if (Main.netMode != NetmodeID.Server)
                        if (WithdrawlTime % Main.rand.Next(50,100) == 0 && !Player.HasBuff<CombatStimBuff>())
                        {
                            ScreenShakeSystem.StartShake(1f, shakeStrengthDissipationIncrement: 1f);

                            Vector2 VoiceSpawnOffset = Player.Center + Main.rand.NextVector2CircularEdge(400,460);
                            SoundStyle a = AssetDirectory.Sounds.Items.CombatStim.PsychosisWhisper;
                            float sound = Main.soundVolume;

                            SoundEngine.PlaySound(a with { Volume = 0.2f, PitchVariance = 0.5f, MaxInstances = 0, PlayOnlyIfFocused = true, Type = SoundType.Sound}, VoiceSpawnOffset);
                           

                        }
                }

                WithdrawlTime++;
            }

            else if(Addicted)
            {

                timeSinceLastStim++;
                if (Player.HasBuff(ModContent.BuffType<StimWithdrawl_Debuff>()))
                    Player.ClearBuff(ModContent.BuffType<StimWithdrawl_Debuff>());

                if (!Player.HasBuff(ModContent.BuffType<StimAddicted_Debuff>()))
                {
                    
                    Player.AddBuff(ModContent.BuffType<StimAddicted_Debuff>(), AddictionCheckInterval, true, false); // Apply debuff

                }
                if (timeSinceLastStim >= AddictionCheckInterval)
                { 
                    Withdrawl = true;
                    Addicted = false;
                }
            }

            if (LoseStimTimer == 2200 && stimsUsed > 0)
            {
                stimsUsed -= 1;
                LoseStimTimer = -1;
            }

        }

        
        public void UseStim()
        {

            stimsUsed++;
            timeSinceLastStim = 0;

            // Update addiction chance
            LoseStimTimer = 0;
            WithdrawlTime = 0;

            // Check for addiction
            if (Main.rand.NextFloat() < addictionChance / 100f)
            {
                Addicted = true;
                Withdrawl = false;
            }
            if (Addicted)
            {
                Player.ClearBuff(ModContent.BuffType<StimAddicted_Debuff>());

            }
        }

        public void EndWithdrawl()
        {

            Addicted = false;
            Withdrawl = false;
            stimsUsed = 0;
            addictionChance = 0f;
        }

        public override void UpdateDead()
        {

            Addicted = false;
            Withdrawl = false;
            stimsUsed = 0;
            addictionChance = 0f;
            timeSinceLastStim = 0;
        }
    }
}
