
using HeavenlyArsenal.Core;
using Luminance.Common.Easings;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Artillery_Crab
{

    public partial class BloodCrab
    {
        public enum Behavior
        {
            debug,
            CheckVictimRange,

            CheckFlowControl,
            MeleeCharge,
            CrushTargetToDeath,
            PrepareSquidMissiles,
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


                case Behavior.CheckFlowControl:
                    CheckFlowControl();
                    break;
                case Behavior.CheckVictimRange:
                    CheckVictimRange();

                    break;
                case Behavior.MeleeCharge:
                    MeleeCharge();

                    break;
                case Behavior.PrepareSquidMissiles:
                    PrepareSquidMissiles();
                    break;

                case Behavior.SquidMissiles:
                    FireSquidMissiles();
                    break;
            }
        }
        /// <summary>
        /// brain
        /// </summary>
        private void CheckFlowControl()
        {
            if (Blood >= MaxBlood)
                CurrentState = Behavior.PrepareSquidMissiles;
            if(SquidMissileLoadedCount > 0)
            {
                Time = -1;

                CurrentState = Behavior.SquidMissiles;
            }
        }

        private void CheckVictimRange()
        {
            ClawDesiredLoc = NPC.Center + new Vector2(-70, 70);
            CurrentState = Behavior.CheckFlowControl;
        }
        #region Smash player with Claw

        private PiecewiseCurve piecewiseCurve;
        private BezierCurve curve;
        private bool _MeleePiecewiseInitialized => piecewiseCurve is not null;
        private bool _MeleeCurveInitialized => curve is not null;
        private void MeleeCharge()
        {
            if (!_MeleeCurveInitialized)
            {
                Vector2[] controls = new Vector2[]
                {
                    new Vector2(-4, -10),
                    new Vector2(0, -10),
                    new Vector2(10, -10),
                    new Vector2(12,20)
                };
                curve = new BezierCurve(controls);
            }
            if (!_MeleePiecewiseInitialized)
            {
                piecewiseCurve = new PiecewiseCurve();
                piecewiseCurve.Add(EasingCurves.Elastic, EasingType.Out, 0.2f, 0.5f);
                piecewiseCurve.Add(EasingCurves.Elastic, EasingType.InOut, -1f, 0.8f);
                piecewiseCurve.Add(EasingCurves.Exp, EasingType.In, 1, 1f);
            }
            if (Target == null)
                Target = Main.LocalPlayer;


            ClawOpenAmount = piecewiseCurve.Evaluate(Time / 120f);
            ClawDesiredLoc = Target.Center;//claw.Skeleton.Root + claw.Skeleton.JointPositions[^1].DirectionTo(Target.Center)*100;
            Dust.NewDustPerfect(ClawDesiredLoc, DustID.Cloud, Vector2.Zero);
            piecewiseCurve = null;
            curve = null;
            if (Time > 120)
            {
                Time = -1;
            }
        }
        #endregion
        private void FindBombardLocation()
        {

        }

        private void Bombard()
        {
        }

        private void AntiAirMeasures()
        {

        }
        #region SquidMissiles
        private int SquidMissileLoadedCount;
        private void PrepareSquidMissiles()
        {
            const int SquidMissileCost = 40;

            if (Blood > 0)
            {
                SquidMissileLoadedCount++;

                Blood -= SquidMissileCost;
                if (Blood < 0)
                {
                    
                    SquidMissileLoadedCount--;
                    Blood += SquidMissileCost;
                    CurrentState = Behavior.CheckFlowControl;
                }
                
            }

        }
        private void FireSquidMissiles()
        {
            if (Time < 60)
            {
                NPC.rotation += Main.rand.NextFloat(-1, 1) * 0.1f * LumUtils.InverseLerp(0, 60, Time);
                NPC.position += Main.rand.NextVector2Square(-1, 1) * LumUtils.InverseLerp(0, 60, Time) * 0.5f;
            }
            if (Time == 61)
            {
                NPC.position += Vector2.UnitY * 30;
            }
            if (Time > 62)
            {
              
                SoundEngine.PlaySound(GennedAssets.Sounds.Mars.RailgunFire with { Pitch = -2, PitchVariance = 2, MaxInstances = 16 }, NPC.Center).WithVolumeBoost(0.2f);
                for (int i = 0; i < SquidMissileLoadedCount; i++)
                {
                    int p = Main.myPlayer;
                    int proj = Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Top + new Vector2(Main.rand.NextFloat(-60, 61) * 0.4f, 0),
                        Vector2.Zero,                 // OnSpawn sets initial velocity
                        ModContent.ProjectileType<SquidMissile>(),
                        NPC.defDamage,
                        20f,
                        0,
                        0f,
                        p
                    );
                }
                SquidMissileLoadedCount = 0;

                CurrentState = Behavior.debug;
            }
           



        }
        #endregion
    }
}
