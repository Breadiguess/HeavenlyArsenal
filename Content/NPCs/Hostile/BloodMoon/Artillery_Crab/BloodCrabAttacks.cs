
using Terraria;
using Terraria.Audio;
using static System.Net.Mime.MediaTypeNames;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Artillery_Crab
{

    public partial class BloodCrab
    {
        public enum Behavior
        {
            debug,
            CheckVictimRange,

            MeleeCharge,
            CrushTargetToDeath,
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
                    FireSquidMissiles();
                    break;
            }
        }

        private void CheckVictimRange()
        { 

        }

        private void MeleeCharge()
        {

        }

        private void FindBombardLocation()
        {

        }

        private void Bombard()
        {
        }

        private void AntiAirMeasures()
        {

        }

        private void FireSquidMissiles()
        {
            //temporary
            if(Time% 120== 0)
            {
                //SoundEngine.PlaySound(GennedAssets.Sounds.Mars.RailgunFire with { Pitch = -2, PitchVariance = 2, MaxInstances = 16 }, NPC.Center);
                for (int i = 0; i < 7; i++)
                {
                    int p = Main.myPlayer;
                    int proj = Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Top + new Vector2(Main.rand.NextFloat(-60, 61) * 0.2f, 0),
                        Vector2.Zero,                 // OnSpawn sets initial velocity
                        ModContent.ProjectileType<SquidMissile>(),
                        NPC.defDamage,
                        20f,
                        0,
                        0f,
                        p                             // ai[1] = target player index
                    );
                }

                CurrentState = Behavior.debug;
            }

        }
    }
}
