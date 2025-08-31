using CalamityMod;
using CalamityMod.NPCs.TownNPCs;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles.Metaballs;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Animations;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor
{
    public enum NeedleAi
    {
        Idle,
        Attack,
        Retract
    }

    class BloodNeedle : ModProjectile
    {
        #region setup
        private List<Vector2> clotOffsets;
        private List<int> clotFrames;
        public NeedleAi CurrentState;

        private Vector2 hitPosition;
        public override string Texture => "HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/BloodNeedle_Head";
        public float TimeInner;
        public ref float Time => ref Projectile.ai[0];
        public ref float Attack => ref Projectile.ai[1];
        /// <summary>
        /// Keeps track of which tendril this is.
        /// </summary>
        public ref float Index => ref Projectile.ai[2];
        /// <summary>
        /// randomized value to add visual flair to the tendril's attacks
        /// </summary>
        public ref float AttackVariation => ref Projectile.localAI[0];
        private bool canDamage;
        /// <summary>
        /// time (in ticks) between attacks from this needle
        /// </summary>
        public float AttackCooldown;
        public float MaxAttackCooldown = 30;//in ticks
        /// <summary>
        /// For the visual effect on the needles
        /// </summary>
        public float pulsateAmount = 0;
        public ref Player Player => ref Main.player[Projectile.owner];
        public Rope tentacle;

        public BezierCurve Tendril;
        /// <summary>
        /// stores the target chosen, mainly used for debug
        /// </summary>
        private NPC TargetName;

        /// <summary>
        /// The target of the needle. This is set when the needle gets a target. 
        /// it is used to prevent the needle from switching targets while it is attacking.
        /// </summary>
        public NPC Target
        {
            get;
            set;
        }

        public float ClotInterp => Player.GetModPlayer<BloodArmorPlayer>().Clot;
        public override void SetStaticDefaults()
        {

            ProjectileID.Sets.TrailingMode[Type] = 3;
            ProjectileID.Sets.ImmediatelyUpdatesNPCBuffFlags[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.damage = 500;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.width = Projectile.height = 64;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.knockBack = 0;
        }
        #endregion


        public override void AI()
        {
            #region misc
            
            Projectile.damage = (int)(Player.GetDamage(DamageClass.Generic).ApplyTo(600));
            Index = 0;
            foreach (Projectile proj in Main.projectile.Where(n => n.active && n.owner == Player.whoAmI && n.type == Type))
            {
                if (proj.whoAmI > Projectile.whoAmI) Index++;
            }
            Projectile.timeLeft = 20;
            #endregion
            #region targeting and stuff

            Vector2 FromOwner = Projectile.Center - Player.MountedCenter;
            float TargetRange = 380;
            if (Player.GetModPlayer<BloodArmorPlayer>().Frenzy)
                TargetRange = 420;
           

            if (Target == null && Attack == 0f)
            {
                NPC found = Projectile.FindTargetWithinRange(TargetRange);
                if (found != null)
                    Target = found;
            }

            
            if (Target != null)
            {
                bool npcDeadOrInactive = !Target.active || Target.life <= 0 || Target.friendly;
                bool armorLost = !Player.active
                                 || Player.GetModPlayer<BloodArmorPlayer>().CurrentForm != BloodArmorForm.Offense
                                 || !Player.GetModPlayer<BloodArmorPlayer>().BloodArmorEquipped;
                bool tooFarFromPlayer = Vector2.Distance(Target.Center, Player.MountedCenter) > (TargetRange + 100f);

                
                if (npcDeadOrInactive || armorLost || (tooFarFromPlayer && Attack == 0f))
                {
                    Target = null;
                }
            }

            
            TargetName = Target;


            // If the target is more than 520 away from the player, discard the target.
            if ((Target != null && (AttackCooldown! > 0 || Vector2.Distance(Target.Center, Player.MountedCenter) > TargetRange + 100)) || (!Player.active || Player.GetModPlayer<BloodArmorPlayer>().CurrentForm != BloodArmorForm.Offense || Player.GetModPlayer<BloodArmorPlayer>().BloodArmorEquipped != true)) Target = null;
            


            // Compute home position
            int SignFromOwner = Math.Sign(FromOwner.X);
            float sideOffset = Player.direction * (Index % 2 == 0 ? -90f : 100f);
            Vector2 homePos = Player.MountedCenter + new Vector2(sideOffset, 10*(Index%2)) - Player.velocity * 2f;

            if (Index >= 2 && Target == null) homePos = Player.MountedCenter + new Vector2(sideOffset, Index % 1 - (50 + Player.direction * SignFromOwner * (Index % 2))) - Player.velocity * 2f;
            else homePos = Player.MountedCenter + new Vector2(sideOffset, (Index % 2 == 0 ? 1 : 0)-60) - Player.velocity * 20f;
            
            if (Target == null && Attack != 0) Attack = 0;
            float trackSpeed = 1f;
            float stabSpeed = 50f;
            const float windUpDuration = 5f;
            float retractSpeed = 1f; // faster retract
            
            float DistTooBig = 225;
            if (Target != null && (AttackCooldown <= 0))
            {
                Vector2 direction = Projectile.SafeDirectionTo(Target.Center);

                if (Attack == 0f)
                {
                    // Begin wind-up
                    AttackVariation = Main.rand.NextFloat(-10, 10);
                    float distanceToTarget = (Projectile.Center - Target.Center).Length();
                    if (distanceToTarget < TargetRange - 100)
                    {
                        // Smoothly pull back before stabbing forward
                        Vector2 pullBackVelocity = direction * -8; // Pull back in the opposite direction
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, pullBackVelocity, 1f);
                    }


                    if (Index == 1)
                    {
                        //Main.NewText($"{Projectile.velocity}");
                    }
                    if (TimeInner > 12)
                    {
                        Attack = 1f;
                        TimeInner = 0;
                    }
                    TimeInner++;
                    if (Player.GetModPlayer<BloodArmorPlayer>().Frenzy)
                    {
                       
                        TimeInner++;
                    }
                   
                    Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.ToRotation() + MathHelper.PiOver2, 0.75f);
                }

                if (Attack == 1f)
                {


                    Projectile.rotation = Projectile.rotation.AngleLerp(direction.ToRotation() - MathHelper.PiOver2, 0.25f);
                    //Todo: if within 30 degrees of the direction.to rotation
                    Projectile.velocity = direction * stabSpeed;
                    // Check if within 30 degrees of the direction's rotation
                    float angleDifference = MathHelper.ToDegrees(Math.Abs(MathHelper.WrapAngle(Projectile.rotation - (direction.ToRotation() - MathHelper.PiOver2))));
                    if (TimeInner < windUpDuration || angleDifference > 30f)
                    {
                        // wind-up (no movement)
                        Projectile.velocity = Vector2.Zero;
                    }
                    else
                    {
                        Projectile.velocity = direction * stabSpeed;
                    }
                    TimeInner++;
                }
                else if (Attack == 2f)
                {
                    // retract
                    Vector2 toHome = homePos - Projectile.Center;
                    //todo: Add a little bit of angle variance for visual flair
                    //float angleVariance = MathHelper.ToRadians(Main.rand.NextFloat(-15f, 15f)); // Random angle between -5 and 5 degrees
                    Vector2 adjustedDirection = toHome.SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, adjustedDirection * toHome.Length() * retractSpeed, trackSpeed);
                    //Main.NewText($"{TimeInner}");
                    if (Vector2.Distance(Projectile.Center, homePos + Player.velocity) < 20f)
                    {
                        // reset

                        Attack = 0f;
                        TimeInner = 0f;
                        Projectile.Center = homePos;
                    }
                }
                else if (Attack == 3f)
                {
                    Attack = 1;
                    TimeInner = 0;
                }
            }
            else
            {
                if (AttackCooldown > 0)
                {
                    AttackCooldown--;
                }
                TimeInner = 0f;
                Projectile.Center += Player.velocity * 0.65f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, (homePos - Projectile.Center) * 0.1f, trackSpeed);
                float preferredAngle = MathHelper.ToRadians(-Player.direction * 50);
                Projectile.rotation = Projectile.rotation.AngleLerp(preferredAngle, 0.1f);



                if (FromOwner.Length() > DistTooBig)
                {

                    Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.ToRotation() + MathHelper.PiOver2, 0.35f);
                }
            }
            #endregion


            // Tentacle rope logic
            #region cool tentacle stuff
            if (Tendril == null)
                Tendril = new BezierCurve(Projectile.Center);


            if (tentacle == null)
            {
                tentacle = new Rope(Projectile.Center, Player.MountedCenter, 15, 10f, Vector2.Zero);
            }
            for (int i = 1; i < tentacle.segments.Length - 1; i++)
            {
                if (Main.rand.NextBool(85) && i < tentacle.segments.Length - tentacle.segments.Length / 6)
                {
                    Dust blood = Dust.NewDustPerfect(tentacle.segments[i].position, DustID.CrimtaneWeapons, new Vector2(0, -3f), 10, Color.Crimson, 1);
                    blood.noGravity = true;
                    blood.rotation = Main.rand.NextFloat(-89, 89);
                }
                if (i <= 10 && (TimeInner <= 0) && FromOwner.Length() < DistTooBig)
                {
                    tentacle.segments[i].position += new Vector2(10 * -Player.direction, -10 + Index % 2);
                }
                else if (Attack == 1 && i <= 10)
                {
                    tentacle.segments[i].position += new Vector2(-Projectile.velocity.X, AttackVariation);
                }
            }

            Vector2 jitter = new Vector2(
                MathF.Sin(Time * 0.05f + Index * 1.5f),
                MathF.Cos(Time * 0.05f + Index * 1.5f)
            ) * Main.rand.NextFloat(2);

            //Projectile.direction = Player.direction;
            if (TimeInner <= 0)
                tentacle.gravity = new Vector2(0, 1);
            else
                tentacle.gravity = new Vector2(-Projectile.velocity.X / tentacle.segments.Length, -Projectile.velocity.Y);

            tentacle.damping = Utils.GetLerpValue(40, 20, Player.velocity.Length(), true) * 0.65f;


            ///agony


            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / HeadFrameCount;
            Rectangle HeadFrame = new Rectangle(0, headFrame * frameHeight, tex.Width, 192);
            float scale = Projectile.scale;
            Vector2 localTip = new Vector2(
                0f,
                HeadFrame.Height / 5
            );
            Vector2 tipOffset = localTip.RotatedBy(Projectile.rotation);

            tentacle.segments[0].position = Projectile.Center - tipOffset + Projectile.velocity;
            tentacle.segments[1].position = tentacle.segments[0].position - tipOffset + Projectile.velocity;
            tentacle.segments[^1].position = Player.MountedCenter + jitter;
            #endregion

            Projectile.Center += jitter / 10;
            tentacle.Update();

            Time++;
            #region despawnConditions
            if (Player.dead)
            {
                Projectile.Kill();
            }
            if (!Player.active || Player.GetModPlayer<BloodArmorPlayer>().CurrentForm != BloodArmorForm.Offense || Player.GetModPlayer<BloodArmorPlayer>().BloodArmorEquipped != true)
            {
                // If the player is no longer wearing the armor, retract the needles towards the player and then delete them.
                Vector2 toPlayer = Player.MountedCenter - Projectile.Center;

                if (toPlayer.Length() < 50f)
                {
                    // If the needle is close enough to the player, kill the projectile.
                    Projectile.Center = Player.MountedCenter;
                    Projectile.Kill();
                }
                else Projectile.Center = Vector2.Lerp(Projectile.Center, Player.MountedCenter, 0.76f);
                return;
            }


            #endregion
        }
        #region Animation
        private int headFrame;
        private int headFrameCounter;
        private const int HeadFrameCount = 8;
        private const int FrameSpeed = 5;

        private int bodyFrame;
        private int bodyFrameCounter;
        private const int BodyFrameCount = 2;
        private const int BodyFrameSpeed = 1;
        public override void PostAI()
        {

            bool frenzy = Player.GetModPlayer<BloodArmorPlayer>().Frenzy;
            headFrameCounter++;



            if (headFrameCounter >= FrameSpeed)
            {
                headFrameCounter = 0;
                if (frenzy)
                {
                    bodyFrame = Utils.Clamp(bodyFrame + 1, 0, BodyFrameCount - 1);
                    headFrame = Utils.Clamp(headFrame + 1, 0, HeadFrameCount - 1);
                }
                else
                {
                    // reverse
                    bodyFrame = Utils.Clamp(bodyFrame - 1, 0, BodyFrameCount - 1);
                    headFrame = Utils.Clamp(headFrame - 1, 0, HeadFrameCount - 1);
                }
            }



        }
        #endregion
        #region Assorted Stuff
        private int clotFrame;
      

        public override void OnKill(int timeLeft)
        {
            
            base.OnKill(timeLeft);
        }
        private bool CanHit() => (Attack == 1 && AttackCooldown <= 0);
        
        public override bool? CanHitNPC(NPC target) => CanHit();//(Attack == 1 && AttackCooldown <= 0 && (!Main.zenithWorld || !target.friendly));

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            base.ModifyDamageHitbox(ref hitbox);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!Player.GetModPlayer<BloodArmorPlayer>().Frenzy)
                AttackCooldown += MaxAttackCooldown - 2 + Main.rand.Next(0, 2);
            else
                AttackCooldown += 10;
           
            BloodMetaball metaball = ModContent.GetInstance<BloodMetaball>();
            for (int i = 0; i < 5; i++)
            {
                Vector2 bloodSpawnPosition = Projectile.Center + new Vector2(30 * Projectile.scale).RotatedBy(Projectile.rotation);
                Vector2 bloodVelocity = (Main.rand.NextVector2Circular(8f, 8f) + Projectile.velocity / 5) * Main.rand.NextFloat(0.2f, 1.2f);
                metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(10f, 40f), Main.rand.NextFloat(2f));
            }
            //float threshold = target.Size.Length() / 2f + Projectile.width;

            //TimeInner = 0f;
            if (Player.GetModPlayer<BloodArmorPlayer>().Frenzy)
            {
                Player.statLife++;
            }


            if(target.life < damageDone)
            {
                Target = null;
                AttackCooldown = 0;
                Attack = 3;
            }
            if(!Player.GetModPlayer<BloodArmorPlayer>().Frenzy)
                for (int i = 0; i < 1; i++) Player.GetModPlayer<BloodArmorPlayer>().AddBloodUnit(-5);

        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Player.GetModPlayer<BloodArmorPlayer>().Frenzy)
            {
                modifiers.SetCrit();
            }
            target.AddBuff(ModContent.BuffType<LacerationBuff>(), 240, false);

        }

        public override bool? CanCutTiles() => CanHit();
        #endregion
        #region drawcode
        /// <summary>
        /// Gives each piece of clot a randomized position on the tendril.
        /// </summary>
        /// <param name="desiredCount"></param>
        private void EnsureClotOffsetCount(int desiredCount)
        {
            if (clotOffsets == null)
                clotOffsets = new List<Vector2>(desiredCount);
            if (clotFrames == null)
                clotFrames = new List<int>(desiredCount);
            // Append random offsets if we have fewer than needed:
            while (clotOffsets.Count < desiredCount)
            {
                float maxOffset = 5f; // adjust “scatter radius” as you like
                float ox = Main.rand.NextFloat(-maxOffset, maxOffset);
                float oy = Main.rand.NextFloat(-maxOffset, maxOffset);
                clotOffsets.Add(new Vector2(ox, oy));
                
            }
            while (clotFrames.Count < desiredCount)
            {
                int totalFrames = 5;
                int frame = Main.rand.Next(0, totalFrames+1);
                clotFrames.Add(frame);
                
            }
            while (clotFrames.Count > desiredCount)
                clotFrames.RemoveAt(clotFrames.Count - 1);
            // If we somehow have more entries than segments, trim the end:
            while (clotOffsets.Count > desiredCount)
                clotOffsets.RemoveAt(clotOffsets.Count - 1);
        }
        //todo: create a method that checks if the player is in frenzy. then, animate the bloodneedle_head and hold it on the last frame.
        //when the frenzy is over, run backwards through the animation until its on the first frame again.
        //7 frames
        private void DrawLine(List<Vector2> list)
        {
            //Texture2D texture = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/SegmentTendon").Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(frame.Width / 2, frame.Width / 2);

            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation();
                Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.AntiqueWhite);
                Vector2 scale = new Vector2(0.75f, 5);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

                pos += diff;
            }
        }
        /// <summary>
        /// Draws a glowing light when called, and the size and strength is determined by Main.GlobalWTimeWrappedHourly.
        /// </summary>
        /// <param name="lightColor"></param>
        public void drawHeartbeat(Vector2 Position, Color lightColor, float bpm = 60f, bool invert = false)
        {
            Texture2D heartTex = AssetDirectory.Textures.BigGlowball.Value;
            float healthFactor = MathHelper.Clamp(1f - (Player.statLife / (float)Player.statLifeMax2), 0.5f, 1.5f); // Lower health = faster heartbeat
            float erraticFactor = 0.9f + Main.rand.NextFloat(-0.1f, 0.1f); // Add slight randomness to the beat timing
            float beatSpeed = 2f * healthFactor * erraticFactor; // Adjust beat speed based on health and randomness

            // Calculate the length of one beat in seconds
            float beatInterval = 60f / bpm;

            // Global time in seconds
            float globalTime = (float)Main.GlobalTimeWrappedHourly;

            // Position in the current beat cycle [0 .. beatInterval)
            float t = globalTime % beatInterval;
            if (invert)
                t = beatInterval - t;

            // Normalize into a 0–1 range over the full interval
            float norm = t / beatInterval;

            // Determine scale factors over beat phases:
            // 0.00–0.25 : grow    (0 to max)
            // 0.25–0.40 : hold max
            // 0.40–0.65 : shrink (max back to rest)
            // 0.65–1.00 : rest   (at rest size)
            float restScale = 0.075f;
            float maxScale = 0.15f;
            float pulsate;

            if (norm < 0.25f)
            {
                // Grow
                pulsate = MathHelper.Lerp(restScale, maxScale, norm / 0.25f);
            }
            else if (norm < 0.40f)
            {
                // Hold
                pulsate = maxScale;
            }
            else if (norm < 0.65f)
            {
                // Shrink
                pulsate = MathHelper.Lerp(maxScale, restScale, (norm - 0.40f) / 0.25f);
            }
            else
            {
                // Rest
                pulsate = restScale;
            }

            Vector2 scale = new Vector2(pulsate * 0.9f, pulsate * 1.1f);

            Vector2 glowOffset = new Vector2(0, 0f);
            Vector2 drawPos = Position + glowOffset;

            Main.EntitySpriteDraw(heartTex, drawPos, heartTex.Frame(), Color.Crimson.MultiplyRGB(lightColor), Projectile.rotation, heartTex.Size() * 0.5f, scale,
                SpriteEffects.None
            );
            //Utils.DrawBorderString(Main.spriteBatch, "O", drawPos, Color.White);

        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D head = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D body = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/RegSeg").Value;
            Texture2D Clot = GennedAssets.Textures.GreyscaleTextures.WhitePixel;//ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/Scab").Value;
            //todo: create frame and make sure that it looks decent.x

           

            int frameHeight = head.Height / HeadFrameCount;
            Rectangle HeadFrame = new Rectangle(0, headFrame * frameHeight, head.Width, frameHeight);
            Vector2 origin = new Vector2(head.Width / 2f, frameHeight / 2f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            if (Player.GetModPlayer<BloodArmorPlayer>().Frenzy)
                drawHeartbeat(drawPos, lightColor, 20, false);

            SpriteEffects sprite = Player.direction * Player.gravDir < 0 ? SpriteEffects.FlipHorizontally : 0;
            SpriteEffects Tendril = Player.direction * Player.gravDir < 0 ? 0 : SpriteEffects.FlipVertically;

            Vector2 headDrawPos = new Vector2(0, 0) + Projectile.Center - Main.screenPosition;
            //todo: integrate, and make use of. 
            /*
            Effect effect = AssetDirectory.PrimShaders.TextureMap;
            effect.Parameters["scroll"].SetValue(0);
            effect.Parameters["repeats"].SetValue(Player.MountedCenter / (float)body.Width());
            effect.Parameters["sampleTexture"].SetValue(body.Value);
            VineTrailDrawer?.Render(effect, -Main.screenPosition);
            */
            if(!Projectile.isAPreviewDummy)
            {/*
                Utils.DrawBorderString(Main.spriteBatch, "| Attack variation: " + AttackVariation.ToString(), Projectile.Center - Vector2.UnitY * 120 - Main.screenPosition, Color.White);
                Utils.DrawBorderString(Main.spriteBatch, "| Index: " + Index.ToString() + " | Direction: " + Projectile.direction.ToString() + " | TimeInner: " + TimeInner.ToString(), Projectile.Center - Vector2.UnitY * 140 - Main.screenPosition, Color.White);
                Utils.DrawBorderString(Main.spriteBatch, "| Attack: " + Attack.ToString(), Projectile.Center - Vector2.UnitY * (100) - Main.screenPosition, Color.White);

                Utils.DrawBorderString(Main.spriteBatch, "| Velocity: " + Projectile.velocity.ToString(), Projectile.Center - Vector2.UnitY * (220) - Main.screenPosition, Color.White);

                Utils.DrawBorderString(Main.spriteBatch, "| Atttack Cooldown: " + AttackCooldown.ToString(), Projectile.Center - Vector2.UnitY * (180) - Main.screenPosition, Color.White);
                if (TargetName != null)
                    Utils.DrawBorderString(Main.spriteBatch, "| " + TargetName.ToString(), Projectile.Center - Vector2.UnitY * (160) - Main.screenPosition, Color.White);
            */}
            if (tentacle != null)
            {

                List<Vector2> points = new List<Vector2>();

                points.AddRange(tentacle.GetPoints());
                points.Add(Player.MountedCenter);
                Vector2 tentacleVel = -Projectile.rotation.ToRotationVector2() * 16;
                EnsureClotOffsetCount(points.Count);
                // DrawLine(points);
                int pulsateIndex = (int)(Main.GlobalTimeWrappedHourly * 20) % points.Count; // Determine which point to pulsate
                pulsateAmount = (float)(Math.Cos(Main.GlobalTimeWrappedHourly) % 1f * 0.2f);

                int BodyframeHeight = body.Height / BodyFrameCount;
                Rectangle BodyFrame = new Rectangle(0, bodyFrame * BodyframeHeight, body.Width, BodyframeHeight);
                Vector2 Borigin = new Vector2(body.Width / 2f, BodyframeHeight / 2f);

                //for (int i = points.Count - 1; i > 0; i--)
                for (int i = 1; i < points.Count -1; i++)
                {

                    float rot = points[i].AngleTo(points[i - 1]);
                    float currentPulsate = 0f;

                    // Apply pulsate amount to the current point, and half to adjacent points
                    if (i == pulsateIndex)
                    {
                        currentPulsate = pulsateAmount;
                    }
                    else if (i == pulsateIndex - 2 || i == pulsateIndex + 2)
                    {
                        currentPulsate = pulsateAmount * 0.75f;
                    }

                    Vector2 stretch = new Vector2((1.3f - (float)i / points.Count * 0.6f) * Projectile.scale,
                        i > points.Count ? points[i].Distance(points[i - 1]) / (body.Height / 2f) : 1.1f + currentPulsate);

                    Rectangle ScabFrame = new Rectangle(0, clotFrames[i], 16, 16);
                    Vector2 Clorigin = new Vector2(ScabFrame.Width / 2, (ScabFrame.Height / 5) / 2);

                    Main.EntitySpriteDraw(body, points[i] - Main.screenPosition, BodyFrame, lightColor, rot, Borigin, stretch, Tendril, 0);
                    Vector2 fixedOffset = clotOffsets[i];
                    Vector2 clotDrawPos = points[i] + fixedOffset;
                    Main.EntitySpriteDraw(Clot, clotDrawPos - Main.screenPosition, ScabFrame, lightColor, rot, Clorigin * 0.5f, Player.GetModPlayer<BloodArmorPlayer>().Clot, SpriteEffects.None, 0);
                }
            }
            Main.EntitySpriteDraw(head, headDrawPos, HeadFrame, lightColor, Projectile.rotation, origin, new Vector2(1f, 1f), sprite, 0);


            return false;
        }

        #endregion
    }

    #region Laceration
    public class LacerationBuff : ModBuff
    {
        //why did I make it an undertale reference
        /// <summary>
        /// strength of debuff, essentially.
        /// </summary>
        public int LOVE { get; set; } = 0;


        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {
            Utils.DrawBorderString(spriteBatch, LOVE.ToString(), drawParams.Position, Color.White);
        }


        public override bool ReApply(NPC npc, int time, int buffIndex)
        {
            npc.GetGlobalNPC<LacerationNPC>().LOVE++;
            time++;

            return false;
        }


        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<LacerationNPC>().Time++;
            
        }

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }
    }
    public class LacerationNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public int LOVE { get; set; } = 0;
        public int Time;
        public override void PostAI(NPC npc)
        {
            if (npc.HasBuff<LacerationBuff>())
            {
                Time++;

            }

        }
        public override void ResetEffects(NPC npc)
        {
            if (!npc.HasBuff<LacerationBuff>())
            {
                Time = 0;
                LOVE = 0;
            }
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (npc.HasBuff<LacerationBuff>())
            {
                if (Time % 75 == 0)
                {
                    npc.lifeRegen -= 2 * LOVE;
                    damage += 10 * LOVE;
                    npc.SimpleStrikeNPC(damage, 1, false, 0, null, true, 0, true);
                    //Main.NewText($"OWW!! {npc.FullName}.{npc.whoAmI} just took {damage} Bleed damage at tier {LOVE} !!!");
                }
            }

        }
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (npc.HasBuff<LacerationBuff>())
            {
                
            }
            
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.HasBuff<LacerationBuff>())
            {
                //Utils.DrawBorderString(spriteBatch, "I'm bleeding out!! | " + LOVE.ToString() + " | TimeLeft: " + Time.ToString(), npc.Center - screenPos, Color.White);

            }
          
        }
    }
    #endregion
}
