using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.NPCs.Hostile
{
    public class BloodMoonBalancing : ModSystem
    {
        public static float DefenseMultiplier = 1f;
        public static float DamageMultiplier = 1f;
        public static float HealthMultiplier = 1f;

        //todo: find all bosses that have been defeated after moonlord. then, we will adjust the values accordingly. 
        public override void PreUpdateEntities()
        {

           
            if (DownedBossSystem.downedPrimordialWyrm)
            {

                return;
            }
            if(DownedBossSystem.downedCalamitas || DownedBossSystem.downedExoMechs)
            {
                DamageMultiplier = 1.2f;
                HealthMultiplier = 1.2f;
                DefenseMultiplier = 1.4f;
                return;
            }
            if (DownedBossSystem.downedYharon)
            {
                DamageMultiplier = 1.0f;
                HealthMultiplier = 1.2f;
                DefenseMultiplier = 1.0f;
                return;
            }
            if (DownedBossSystem.downedDoG)
            {
                DamageMultiplier = 1.0f;
                HealthMultiplier = 0.6f;
                DefenseMultiplier = 0.8f;
            }
            if (DownedBossSystem.downedPolterghast)
            {
                DamageMultiplier = 0.8f;
                HealthMultiplier = 0.5f;
                DefenseMultiplier = 0.7f;
                return;
            }
            if (DownedBossSystem.downedProvidence)
            {
                DamageMultiplier = 0.7f;
                HealthMultiplier = 0.4f;
                DefenseMultiplier = 0.6f;
                return;
            }
            if (DownedBossSystem.downedGuardians)
            {
                DamageMultiplier = 0.55f;
                HealthMultiplier = 0.4f;
                DefenseMultiplier = 0.42f;
                return;
            }
            else
            {
                DamageMultiplier = 0.55f;
                HealthMultiplier = 0.4f;
                DefenseMultiplier = 0.42f;
                return;
            }
        }

        public override void OnWorldUnload()
        {
            DefenseMultiplier = 1f;
            DamageMultiplier = 1f;
            HealthMultiplier = 1f;
        }
    }
}
