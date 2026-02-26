using NoxusBoss.Content.NPCs.Enemies.RiftEclipse.Frightweavers;
using NoxusBoss.Content.Particles;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using static System.MathF;
using static NoxusBoss.Core.Fixes.LuminanceFindGroundVerticalFix;
namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    public class Frightweaver : ModNPC
    {
        #region Fields and Properties


        public Vector2 GravityDirection = Vector2.UnitY;

        public SpiderLeg[] Legs;

        public Player Target => Main.player[NPC.target];

        public ref float DashTimer => ref NPC.ai[0];

        public ref float DashDelay => ref NPC.ai[1];

        public bool WasWalkingUpward
        {
            get => NPC.ai[2] == 1f;
            set => NPC.ai[2] = value.ToInt();
        }

        public static LazyAsset<Texture2D> MyTexture
        {
            get;
            private set;
        }

        public static LazyAsset<Texture2D> UpperLegTexture
        {
            get;
            private set;
        }

        public static LazyAsset<Texture2D> LowerLegTexture
        {
            get;
            private set;
        }

        public const float LegSizeFactor = 0.6f;

        /// <summary>
        /// The acceleration of the spider's dash.
        /// </summary>
        public const float DashAcceleration = 0.31f;

        /// <summary>
        /// The maximum speed at which the spider can dash.
        /// </summary>
        public const float MaxDashSpeed = 7.2f;

        /// <summary>
        /// The default quantity of gravity imposed upon the spider.
        /// </summary>
        public const float DefaultGravity = 0.2f;

        /// <summary>
        /// The amount of deceleration imposed upon forward motion when the spider is undergoing spring motion due to being too far from/near to the ground.
        /// </summary>
        public const float ForwardDecelerationDuringSpringMotion = 0.04f;

        /// <summary>
        /// The amount of acceleration used when the spider begins walking up walls.
        /// </summary>
        public const float WallClimbAcceleration = 0.1f;

        /// <summary>
        /// The maximum speed that the spider can travel at when climbing up walls.
        /// </summary>
        public const float MaxWallClimbSpeed = 4.5f;

        /// <summary>
        /// How long, in frames, dashes last.
        /// </summary>
        public static readonly int DashDuration = SecondsToFrames(1.5f);

        /// <summary>
        /// The minimum amount of time a dash delay can last.
        /// </summary>
        public static readonly int MinDashDelayDuration = SecondsToFrames(1.5f);

        /// <summary>
        /// The maximum amount of time a dash delay can last.
        /// </summary>
        public static readonly int MaxDashDelayDuration = SecondsToFrames(3.5f);

        #endregion Fields and Properties

        #region Initialization

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            if (Main.netMode != NetmodeID.Server)
            {
                MyTexture = LazyAsset<Texture2D>.FromPath(Texture);
                UpperLegTexture = LazyAsset<Texture2D>.FromPath($"{Texture}UpperLeg");
                LowerLegTexture = LazyAsset<Texture2D>.FromPath($"{Texture}LowerLeg");
            }
        }

        public override void SetDefaults()
        {
            NPC.npcSlots = 3f;

            // Set up hitbox data.
            NPC.width = 42;
            NPC.height = 26;

            // Define stats.
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 10000;

            // Do not use any default AI states.
            NPC.aiStyle = -1;
            AIType = -1;

            // Use 33% knockback resistance.
            NPC.knockBackResist = 0.66f;

            // Disable gravity.
            NPC.noGravity = true;

            // Define hit and death sounds.
            NPC.HitSound = SoundID.NPCHit20;
            NPC.HitSound = SoundID.NPCDeath22;

            // Initialize legs.
            Legs = new SpiderLeg[6];
            for (var i = 0; i < Legs.Length; i++)
            {
                var horizontalOffset = 0f;
                if (i == 0)
                    horizontalOffset = -102f;
                if (i == 1)
                    horizontalOffset = -82f;
                if (i == 2)
                    horizontalOffset = -72f;
                if (i == 3)
                    horizontalOffset = 70f;
                if (i == 4)
                    horizontalOffset = 88f;
                if (i == 5)
                    horizontalOffset = 110f;

                Vector2 legOffset = new(horizontalOffset, 96f);
                Legs[i] = new(LegSizeFactor * legOffset, LegSizeFactor, legOffset.Length() * 0.685f);
                Legs[i].Leg[0].Rotation = legOffset.X < 0f ? MathHelper.Pi : 0f;
                Legs[i].Leg[1].Rotation = Legs[i].Leg[0].Rotation;
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            DashDelay = 180f;
            NPC.TargetClosest();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement($"Mods.{Mod.Name}.Bestiary.{Name}")
            });
        }

        #endregion Initialization

        #region AI
        public override void AI()
        {
            // Reset the gravity direction to down every frame.
            GravityDirection = Vector2.UnitY;

            var walkTowardsPlayer = DetermineDashEligibility();
            var forwardDirectionToPlayer = Vector2.UnitX * NPC.SafeDirectionTo(Target.Center).X.NonZeroSign();

            // Check if the spider should walk up walls or not and act accordingly.
            var walkingUpward = CheckIfShouldWalkUpWalls(forwardDirectionToPlayer);
            if (walkingUpward)
            {
                GravityDirection = forwardDirectionToPlayer;
                walkTowardsPlayer = false;

                if (NPC.velocity.Y >= -MaxWallClimbSpeed)
                    NPC.velocity.Y -= WallClimbAcceleration;
            }

            if (walkingUpward != WasWalkingUpward)
            {
                WasWalkingUpward = walkingUpward;
                if (walkingUpward)
                {
                    NPC.velocity.Y -= 7f;
                    NPC.position.X += NPC.SafeDirectionTo(Target.Center).X * 16f;
                }

                NPC.netUpdate = true;
            }

            Vector2 forwardDirection = new(GravityDirection.Y, GravityDirection.X);
            Vector2 absoluteForwardDirection = new(Abs(GravityDirection.Y), Abs(GravityDirection.X));
            Vector2 absoluteGravityDirection = new(Abs(GravityDirection.X), Abs(GravityDirection.Y));
            var groundPosition = FindGround(NPC.Center.ToTileCoordinates(), GravityDirection).ToWorldCoordinates();
            var distanceFromGround = Vector2.Distance(NPC.Center, groundPosition);

            if (distanceFromGround >= LegSizeFactor * 62f)
            {
                NPC.velocity += GravityDirection * DefaultGravity;
                NPC.velocity -= NPC.velocity * absoluteForwardDirection * ForwardDecelerationDuringSpringMotion;
            }
            else if (distanceFromGround <= LegSizeFactor * 48f)
            {
                NPC.velocity -= GravityDirection * DefaultGravity;
                NPC.velocity -= NPC.velocity * absoluteForwardDirection * ForwardDecelerationDuringSpringMotion;
            }
            else
            {
                NPC.velocity -= NPC.velocity * absoluteGravityDirection * 0.16f;

                if (walkTowardsPlayer)
                {
                    var perpendicularDistanceFromPlayer = Abs(SignedDistanceToLine(NPC.Center, Target.Center, forwardDirection));

                    // Slow down near the target.
                    if (perpendicularDistanceFromPlayer <= 120f)
                        NPC.velocity -= NPC.velocity * forwardDirection * 0.06f;

                    // Move forward.
                    else if (Abs(Vector2.Dot(NPC.velocity, forwardDirection)) < MaxDashSpeed)
                        NPC.velocity += NPC.SafeDirectionTo(Target.Center) * forwardDirection * DashAcceleration;

                    // Slow down if the speed limit has been exceeded.
                    else
                        NPC.velocity -= NPC.velocity * forwardDirection * 0.04f;
                }
                else if (!walkingUpward)
                    NPC.velocity -= NPC.velocity * forwardDirection * 0.05f;
            }

            // Disable gravity if minimal movement is happening.
            NPC.noTileCollide = NPC.velocity.Length() <= 1.9f && !walkingUpward;

            // Look forward.
            var idealRotation = NPC.velocity.X * 0.05f + NPC.velocity.Y * NPC.spriteDirection * 0.097f + forwardDirection.ToRotation();
            if (NPC.velocity.Length() >= 0.5f)
                NPC.spriteDirection = Vector2.Dot(NPC.velocity, forwardDirection).NonZeroSign();
            if (Abs(forwardDirection.Y) >= 0.9f)
            {
                idealRotation += MathHelper.Pi;
                NPC.spriteDirection *= -1;
            }
            NPC.rotation = NPC.rotation.AngleTowards(idealRotation, 0.09f).AngleLerp(idealRotation, 0.03f);

            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < Legs.Length; j++)
                    Legs[j]?.Update(NPC);
            }
        }

        public bool CheckIfShouldWalkUpWalls(Vector2 forwardDirectionToPlayer)
        {
            // Check if the target can be detected. If they can, there's no reason to walk on walls, as adhering to natural gravity is sufficient to reach them.
            if (Collision.CanHitLine(NPC.Center, 1, 1, Target.Center, 1, 1))
                return false;

            // Check up to 90 pixels forward and determine if there's an obstacle within that distance.
            var obstacleAhead = false;
            var forwardCheckDistance = 90f;
            var obstaclePosition = NPC.Center + forwardDirectionToPlayer * forwardCheckDistance;
            while (!Collision.CanHit(NPC.Center, 1, 1, obstaclePosition, 1, 1))
            {
                // If there was an obstacle, make note of that and step back up in front of the obstacle.
                obstaclePosition -= forwardDirectionToPlayer * 16f;
                obstacleAhead = true;
            }
            obstaclePosition += forwardDirectionToPlayer * 16f;

            // If there is not obstacle, terminate this method immediately- There would be no wall to walk on in the first place.
            if (!obstacleAhead)
                return false;

            // Lastly, check how far up the height of the found obstacle is.
            // If it's too short, ignore it.
            var minObstacleHeight = 200f;
            var obstacleHeight = FindGroundVertical(obstaclePosition.ToTileCoordinates()).ToWorldCoordinates().Distance(obstaclePosition);
            if (obstacleHeight < minObstacleHeight)
                return !Collision.SolidCollision(NPC.Center, 1, 64);

            return true;
        }

        public bool DetermineDashEligibility()
        {
            if (DashDelay <= 0f)
            {
                // Jump at the player as a dash begins.
                NPC.velocity.X = NPC.SafeDirectionTo(Target.Center).X * 10f;
                NPC.velocity.Y -= 3.2f;

                // Start the dash and prepare a random cooldown delay for how long it takes for the spider to perform the next dash.
                DashDelay = Main.rand.Next(120, 210);
                DashTimer = 1f;
                NPC.netUpdate = true;
            }

            // Increment the dash timer if it has been started.
            // Once it reaches its maximum, the spider will stop and the dash delay countdown will begin.
            // During the delay countdown it will slow down to a halt, creating the stereotypical erratic, burst-like movement real spiders are known for.
            if (DashTimer >= 1f)
            {
                DashTimer++;
                if (DashTimer >= DashDuration)
                {
                    DashTimer = 0f;
                    NPC.netUpdate = true;
                }
                return true;
            }

            DashDelay--;
            return false;
        }
        #endregion AI

        #region Drawing

        public static void DrawLeg(SpriteBatch spriteBatch, Texture2D legTexture, Vector2 start, Vector2 end, Color color, float width, SpriteEffects direction)
        {
            // Draw nothing if the start and end are equal, to prevent division by 0 problems.
            if (start == end)
                return;

            var rotation = (end - start).ToRotation();
            Vector2 scale = new(Vector2.Distance(start, end) / legTexture.Width, width);
            start.Y += 2f;

            NoxusBoss.Core.Utilities.Utilities.DrawLineBetter(spriteBatch, start+Main.screenPosition, end+Main.screenPosition, Color.White, 4);

            //spriteBatch.Draw(legTexture, start, null, color, rotation, legTexture.Size() * Vector2.UnitY * 0.5f, scale, direction, 0f);
        }

        public void DrawLegSet(SpiderLeg[] legs, Color lightColor, Vector2 screenPos)
        {
            for (var i = 0; i < legs.Length; i++)
            {
                if (legs[i] is null)
                    continue;

                var leg = legs[i].Leg;
                if (leg.JointCount <= 0)
                    continue;

                Vector2 previousPosition = leg.StartingPoint;
                for (var j = 0; j < leg.JointCount; j++)
                {
                    var legTexture = (j == 0 ? UpperLegTexture : LowerLegTexture).Value;
                    var direction = (leg.EndEffectorPosition.X - NPC.Center.X).NonZeroSign() == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
                    DrawLeg(Main.spriteBatch, legTexture, previousPosition - screenPos, previousPosition + leg[j].Offset - screenPos, lightColor, 1f, direction);
                    previousPosition += leg[j].Offset;
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Draw legs.
            if (Legs is not null)
            {
                if (NPC.IsABestiaryIconDummy)
                {
                    for (var j = 0; j < Legs.Length; j++)
                        Legs[j]?.Update(NPC);
                }

                DrawLegSet(Legs, NPC.GetAlpha(drawColor), screenPos);
            }

            // Draw the spider.
            var texture = TextureAssets.Npc[Type].Value;
            var drawPosition = NPC.Center - screenPos;
            var direction = NPC.spriteDirection.ToSpriteDirection();
            Main.EntitySpriteDraw(texture, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, direction);

            return false;
        }

        #endregion Drawing

        #region Hit Effects

        public override void HitEffect(NPC.HitInfo hit)
        {
            return;
            if (Main.netMode == NetmodeID.Server)
                return;

            // Create blood and gore when killed.
            if (NPC.life <= 0)
            {
                // Create blood.
                for (var i = 0; i < 24; i++)
                {
                    var bloodVelocity = Main.rand.NextVector2CircularEdge(10f, 12f) + new Vector2(hit.HitDirection * 9.1f, -12f);
                    var bloodColor = Color.Lerp(new(119, 28, 28), new(89, 13, 24), Main.rand.NextFloat());
                    BloodParticle blood = new(NPC.Center + Main.rand.NextVector2Circular(18f, 18f), bloodVelocity, 24, Main.rand.NextFloat(0.7f, 1.25f), bloodColor);
                    blood.Spawn();
                }

                // Create leg gores.
                foreach (var spiderLeg in Legs)
                {
                    var kinematicLeg = spiderLeg.Leg;
                    Vector2 previousPosition = kinematicLeg.StartingPoint;
                    for (var i = 0; i < kinematicLeg.JointCount; i++)
                    {
                        for (var j = 0; j < 2; j++)
                        {
                            var goreName = $"Frightweaver{(i == 0 ? "Upper" : "Lower")}Leg{j + 1}";
                            var goreID = ModContent.Find<ModGore>(Mod.Name, goreName).Type;
                            Gore.NewGorePerfect(NPC.GetSource_Death(), previousPosition, Main.rand.NextVector2CircularEdge(2f, 5f) + new Vector2(hit.HitDirection * 3f, -4f), goreID, NPC.scale);
                        }

                        previousPosition += kinematicLeg[i].Offset;
                    }
                }

                // Create body gores.
                for (var i = 0; i < 2; i++)
                {
                    var goreName = $"Frightweaver{i + 1}";
                    var goreID = ModContent.Find<ModGore>(Mod.Name, goreName).Type;
                    Gore.NewGorePerfect(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2CircularEdge(4f, 4f) + new Vector2(hit.HitDirection * 3f, -3.2f), goreID, NPC.scale);
                }
            }
        }

        #endregion Hit Effects
    }
}
