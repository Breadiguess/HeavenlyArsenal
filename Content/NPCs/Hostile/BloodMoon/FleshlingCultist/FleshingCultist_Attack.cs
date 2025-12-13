using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.FleshlingCultist
{
    partial class FleshlingCultist : BloodMoonBaseNPC
    {
        //todo: put this into NPC.ai[2]l
        public enum Behaviors
        {
            WillingSacrifice,

            Worship,

            Defend,

            BlindRush
        }

        public Behaviors CurrentState { get; set; }


        void StateMachine()
        {
            if (CurrentState != Behaviors.Worship)
            {
                isWorshipping = false;
                if (canBeSacrificed == false)
                    canBeSacrificed = true;
            }
            else
            {
                canBeSacrificed = false;
            }
            switch (CurrentState)
            {
                case Behaviors.WillingSacrifice:
                    WillingSacrifice();
                    break;
                case Behaviors.Worship:
                    Worship();
                    break;
                case Behaviors.Defend:

                    break;

                case Behaviors.BlindRush:
                    BlindRush();
                    break;
            }
        }

        void WillingSacrifice()
        {
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

            Cult d = CultistCoordinator.GetCultOfNPC(NPC);
            if (d != null)
            {
                if (NPC.Center.Distance(d.Leader.Center) > 200)
                {
                    SacrificePrio++;
                    NPC.velocity.X = NPC.AngleTo(d.Leader.Center).ToRotationVector2().X * 2;
                }
            }

            else
                CurrentState = Behaviors.BlindRush;


        }

        void Worship()
        {
            if (CultistCoordinator.GetCultOfNPC(NPC) != null)
            {
                Cult a = CultistCoordinator.GetCultOfNPC(NPC);
                if (a == null)
                {
                    CurrentState = Behaviors.BlindRush;
                    return;

                }
                int iD = a.Cultists.IndexOf(NPC);
                float offset = iD % 2 == 0 ? 1 : -1;
                offset *= (iD + 1) * 65;
                //Main.NewText(iD + $", {NPC.whoAmI}, offset: {offset}");
                Vector2 DesiredPosition = a.Leader.Center + new Vector2(offset, NPC.Bottom.Y - a.Leader.Bottom.Y);

                NPC.velocity.X = NPC.AngleTo(DesiredPosition).ToRotationVector2().X * 2;

                //Dust b = Dust.NewDustPerfect(DesiredPosition, DustID.Cloud, Vector2.Zero, newColor: Color.AntiqueWhite);

                if (DesiredPosition.Distance(NPC.Center) < 10f)
                {
                    NPC.knockBackResist = 0;
                }
                if (NPC.Distance(DesiredPosition) < 20)
                {
                    isWorshipping = true;
                }
                else
                    isWorshipping = false;
                Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);


                if (NPC.GetGlobalNPC<RitualBuffNPC>().hasRitualBuff)
                    CurrentState = Behaviors.BlindRush;

                if (isWorshipping && Time % 60 == 0)
                {
                    BloodParticle particle = BloodParticle.pool.RequestParticle();

                    particle.Prepare(NPC.Center, 120, a.Leader);
                    ParticleEngine.ShaderParticles.Add(particle);
                }
            }
            else
                CurrentState = Behaviors.BlindRush;
        }

        void BlindRush()
        {
            if (playerTarget == null)
                FindPlayer();
            NPC.velocity.X = float.Lerp(NPC.velocity.X,NPC.AngleTo(playerTarget.Center).ToRotationVector2().X * 6, 0.2f);
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
            float horizontalRange = 100f;

            // Check if horizontally close but vertically offset
            if (Math.Abs(playerTarget.Center.X - NPC.Center.X) < horizontalRange &&
                playerTarget.Center.Y < NPC.Center.Y - 16f && 
                NPC.velocity.Y == 0) 
            {
                // Jump
                NPC.velocity.Y = -10f; // Adjust jump strength
                NPC.netUpdate = true; // Sync in multiplayer
            }

            float pushRadius = 40f; // detection radius for overlap
            float pushStrength = 0.3f; // how strong the repulsion is

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];
                if (other.active && other.whoAmI != NPC.whoAmI && other.type == NPC.type)
                {
                    float dist = Vector2.Distance(NPC.Center, other.Center);
                    if (dist < pushRadius && dist > 0f)
                    {
                        // Compute a small push vector away from the other NPC
                        Vector2 pushDir = (NPC.Center - other.Center).SafeNormalize(Vector2.Zero);
                        float pushAmount = (pushRadius - dist) / pushRadius; // stronger when closer
                        NPC.velocity += pushDir * pushStrength * pushAmount;
                    }
                }
            }
        }

        void FindPlayer()
        {
            playerTarget = Main.player[NPC.FindClosestPlayer()];
        }
    }
}
