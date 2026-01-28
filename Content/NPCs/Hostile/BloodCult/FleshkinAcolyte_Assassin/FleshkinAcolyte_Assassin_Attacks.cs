using CalamityMod;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodCult.FleshkinAcolyte_Assassin
{
    public partial class FleshkinAcolyte_Assassin : BloodMoonBaseNPC
    {


        public override void AI()
        {
            StateMachine();
            Time++;
        }


        public enum Behaviors
        {
            sneak,
            slash,
            jumpSlash,
            throwDarts,

            retreat,
            deployLarvaeBomb,
        }

        Behaviors CurrentState
        {
            get => (Behaviors)NPC.ai[1];
            set => NPC.ai[1] = (float)value;
        }

        public void StateMachine()
        {
            switch (CurrentState)
            {
                case Behaviors.sneak:
                    DoSneakBehavior();
                    break;
                case Behaviors.slash:
                    DoSlashBehavior();
                    break;
                case Behaviors.jumpSlash:
                    DoJumpSlashBehavior();
                    break;
                case Behaviors.throwDarts:
                    DoThrowDartsBehavior();
                    break;
                case Behaviors.retreat:
                    DoRetreatBehavior();
                    break;
                case Behaviors.deployLarvaeBomb:
                    DoDeployLarvaeBombBehavior();
                    break;
            }
        }
        //todo: check the light or something of the area its in. if the space is bright, then stealth drops. otherwise, go to stealth max.
        private void DoSneakBehavior()
        {
            // Determine brightness at NPC's position
            int tileX = (int)(NPC.Center.X / 16f);
            int tileY = (int)(NPC.Center.Y / 16f);
            tileX = Math.Clamp(tileX, 0, Main.maxTilesX - 1);
            tileY = Math.Clamp(tileY, 0, Main.maxTilesY - 1);

            var lightColor = Lighting.GetColor(tileX, tileY);
            float brightness = (lightColor.R + lightColor.G + lightColor.B) / (3f * 255f);

            if (brightness > 0.5f)
            {
                StealthAmount = Math.Max(0, StealthAmount - 4);
            }
            else if (brightness > 0.4f)
            {
                StealthAmount = Math.Max(0, StealthAmount - 1);
            }
            else
            {
                StealthAmount = Math.Min(_stealthMax, StealthAmount + 1);
            }

            if (currentTarget == null)
            {
                NPC.TargetClosest(true);
                currentTarget = Main.player[NPC.target];
            }
            else
            {
                // slow because stealthy
                // if stealth is too low, just start straight up running at them
                const int StealthRunThreshold = 20;
                const float sneakSpeed = 2f;
                const float runSpeed = 6f;

                float dirX = NPC.DirectionTo(currentTarget.Center).X;

              
                NPC.velocity.X = dirX * float.Lerp(sneakSpeed, runSpeed, 1-LumUtils.InverseLerp(0,StealthRunThreshold, StealthAmount));
              

                NPC.direction = NPC.velocity.X.DirectionalSign();
                NPC.spriteDirection = NPC.direction;


                if (NPC.Distance(currentTarget.Center) < 75)
                {
                    NPC.velocity = Vector2.Zero;
                    CurrentState = Behaviors.slash;
                    Time = 0;
                }
            }



        }

        private void DoSlashBehavior()
        {
            StealthAmount = (int)(100 * (1 - LumUtils.InverseLerp(0, 10, Time)));
            NPC.frame.Y = (int)(12 + 4 * LumUtils.InverseLerp(10, 30, Time));

            if (Time > 30)
            {
                if (Main.rand.NextBool())
                    CurrentState = Behaviors.deployLarvaeBomb;
                else
                    CurrentState = Behaviors.retreat;
                    Time = -1;
            }
        }

        private void DoJumpSlashBehavior()
        {

        }

        private void DoThrowDartsBehavior()
        {

        }

        private void DoRetreatBehavior()
        {
            NPC.direction = -NPC.velocity.X.DirectionalSign();
            NPC.spriteDirection = NPC.direction;
            NPC.velocity.X = -NPC.HorizontalDirectionTo(currentTarget.Center) * 5;

            if (Time > 40 * 5)
            {
                NPC.velocity.Y *= 0.3f;
                CurrentState = Behaviors.sneak;
                Time = 0;
                return;
            }
            if (Time % 40 == 0 && Time! < 40 * 5)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Common.TwinkleMuffled with { Pitch = -1 }, NPC.Center);
                NPC.velocity.Y -= 6;
            }

        }

        private void DoDeployLarvaeBombBehavior()
        {

            NPC.knockBackResist = 0;
            NPC.frame.Y = (int)(20 + 4 * LumUtils.InverseLerp(0, 40, Time));

            if (Time == 40)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Common.TwinkleMuffled with { Pitch = -1 }, NPC.Center);
                for (int i = 0; i < 5; i++)
                {
                    NPC a = NPC.NewNPCDirect(NPC.GetSource_FromThis(), NPC.Center, ModContent.NPCType<UmbralLarvae_Egg>());
                    //todo: evenly distribute the eggs 
                    a.velocity = (-MathHelper.PiOver2 + (i/5f * MathHelper.PiOver2) -MathHelper.ToRadians(45)).ToRotationVector2() * 5;
                }
                for (int i = 0; i < 60; i++)
                {
                    LarvaeBombSmoke particle = new LarvaeBombSmoke();
                    Vector2 velocity = Main.rand.NextVector2Square(1, 2).RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.8f, 1.4f);
                    particle.Prepare(NPC.Center, velocity, 180, 1, NPC.Center);
                    ParticleEngine.ShaderParticles.Add(particle);
                }

            }
            if(Time > 47)
            {

                Reposition();
                CurrentState = Behaviors.sneak;
                StealthAmount = _stealthMax;
                Time = -1;
            }
        }

        //todo: find a solid (ish) spot to place the npc, and then set their coords to it. cannot be too close to where the npc already is, and preferably away from "CurrentTarget".
        private void Reposition()
        {
            const int maxAttempts = 200;
            const float minDistancePx = 100f;
            const float minDistanceFromTargetPx = 200f;
            const float maxDistancePx = 800f;

            if (currentTarget == null)
            {
                NPC.TargetClosest(true);
                currentTarget = Main.player[NPC.target];
            }

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                float angle = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
                float distance = Main.rand.NextFloat(minDistancePx, maxDistancePx);
                Vector2 candidate = NPC.Center + angle.ToRotationVector2() * distance;

                int tileX = (int)(candidate.X / 16f);
                int tileY = (int)(candidate.Y / 16f);

                tileX = Math.Clamp(tileX, 10, Main.maxTilesX - 11);
                tileY = Math.Clamp(tileY, 10, Main.maxTilesY - 11);

                // Find first solid tile downward (ground)
                int groundTileY = -1;
                for (int y = tileY; y < Main.maxTilesY - 10; y++)
                {
                    Tile t = Framing.GetTileSafely(tileX, y);
                    if (t.HasTile && Main.tileSolid[t.TileType])
                    {
                        groundTileY = y;
                        break;
                    }
                }

                if (groundTileY == -1)
                {
                    continue; // no ground found under this candidate
                }

               
                Tile tileAbove = Framing.GetTileSafely(tileX, groundTileY - 1);
                if (tileAbove.HasTile && Main.tileSolid[tileAbove.TileType])
                {
                    continue;
                }

                Vector2 spawnCenter = new Vector2(tileX * 16f + 8f, (groundTileY - 1) * 16f + NPC.height / 2f);

                if (Vector2.Distance(spawnCenter, NPC.Center) < minDistancePx)
                {
                    continue;
                }

                if (currentTarget != null && Vector2.Distance(spawnCenter, currentTarget.Center) < minDistanceFromTargetPx)
                {
                    continue;
                }

                Vector2 topLeft = spawnCenter - new Vector2(NPC.width / 2f, NPC.height / 2f);
                if (Collision.SolidCollision(topLeft, NPC.width, NPC.height))
                {
                    continue;
                }

                NPC.Center = spawnCenter + new Vector2(0, NPC.Size.Y);
                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;
                return;
            }

            if (currentTarget != null)
            {
                Vector2 away = NPC.Center - currentTarget.Center;
                if (away == Vector2.Zero)
                {
                    away = new Vector2(1f, 0f);
                }

                away.Normalize();
                NPC.Center += away * maxDistancePx;
                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;
            }
        }
    }
}
