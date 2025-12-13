    
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab
{
    partial class BloodCrab
    {
        public enum Behavior
        {
            debug,
            CheckVictimRange,

            //if close up, victim range  -> meleeCharge
            MeleeCharge,

            FindBombardLocation,
            Bombard,

            AntiAirMeasures,

            SquidMissiles

        }
        public Behavior CurrentState;

        public void StateMachine()
        {
            switch (CurrentState)
            {
                case Behavior.debug:
                    CurrentState = Behavior.CheckVictimRange;
                    break;

                case Behavior.CheckVictimRange:
                    CheckVictimRange();
                    break;
                case Behavior.MeleeCharge:
                    MeleeCharge();
                    break;
                case Behavior.FindBombardLocation:
                    FindBombardLocation();
                    break;
                case Behavior.Bombard:
                    Bombard();
                    break;

                case Behavior.AntiAirMeasures:
                    AntiAirMeasures();
                    break;

                case Behavior.SquidMissiles:
                    break;
            }
        }

        void CheckVictimRange()
        {
            if (currentTarget == null || !currentTarget.active)
            {
                int target = NPC.FindClosestPlayer();
                if (Main.player[target] == null || !Main.player[target].active)
                    return;
                else
                    currentTarget = Main.player[target];
            }
            Vector2 Dist = NPC.Center - currentTarget.Center;

            if (Dist.X > 400)
            {

                if (Dist.Y + 30 > 300 && Dist.Y - 60 > 300)
                    CurrentState = Behavior.FindBombardLocation;
                else
                    CurrentState = Behavior.AntiAirMeasures;
            }
            else if (Dist.X <= 400)
            {
                if (Dist.Y + 30 < 100 && Dist.Y - 30 < 100)
                {
                    CurrentState = Behavior.MeleeCharge;
                }
            }
        }
        void MeleeCharge()
        {
            if (currentTarget == null)
            {
                CurrentState = Behavior.CheckVictimRange;
                return;
            }
            Vector2 dist = NPC.Center - currentTarget.Center;
            dist.SafeNormalize(Vector2.UnitX);
            if(Time < 20)
            {
                NPC.spriteDirection = Math.Sign(dist.X);
                NPC.direction = NPC.spriteDirection;
            }
            if (Time > 20 && currentTarget.Distance(NPC.Center) < 400)
            {
                NPC.velocity.X = 10 * -NPC.direction;
                Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
            }
            if(Time > 70)
            {
                CurrentState = Behavior.CheckVictimRange;
            }
            


        }
        void FindBombardLocation()
        {
            CurrentState = Behavior.CheckVictimRange;
        }

        void Bombard()
        {

        }

        void AntiAirMeasures()
        {
            CurrentState = Behavior.CheckVictimRange;
        }
    }
}
