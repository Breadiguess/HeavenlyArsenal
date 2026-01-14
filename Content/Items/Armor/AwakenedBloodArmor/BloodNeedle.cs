using System.Collections.Generic;
using System.Linq;
using CalamityMod;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players;
using HeavenlyArsenal.Core;
using Luminance.Assets;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles.Metaballs;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor;

public enum NeedleAi
{
    Idle,

    Attack,

    Retract
}

internal class BloodNeedle : ModProjectile
{
    public override void AI()
    {
        #region misc

        Projectile.damage = (int)Player.GetDamage(DamageClass.Generic).ApplyTo(600);
        Index = 0;

        foreach (var proj in Main.projectile.Where(n => n.active && n.owner == Player.whoAmI && n.type == Type))
        {
            if (proj.whoAmI > Projectile.whoAmI)
            {
                Index++;
            }
        }

        Projectile.timeLeft = 20;

        #endregion

        DespawnChecks();
        TargetAndAttack();

        if (Player.GetModPlayer<AwakenedBloodPlayer>().BloodBoostActive)
        {
            Projectile.extraUpdates = 2;
        }
        else

        {
            Projectile.extraUpdates = 3;
        }

        ManageTentacles();
        Time++;
    }

    public void DespawnChecks()
    {
        if (Player.dead)
        {
            Projectile.Kill();
        }

        if (!Player.active || Player.GetModPlayer<AwakenedBloodPlayer>().CurrentForm != AwakenedBloodPlayer.Form.Offense || Player.GetModPlayer<AwakenedBloodPlayer>().AwakenedBloodSetActive != true)
        {
            // If the player is no longer wearing the armor, retract the needles towards the player and then delete them.
            var toPlayer = Player.MountedCenter - Projectile.Center;

            if (toPlayer.Length() < 50f)
            {
                // If the needle is close enough to the player, kill the projectile.
                Projectile.Center = Player.MountedCenter;
                Projectile.Kill();
            }
            else
            {
                Projectile.Center = Vector2.Lerp(Projectile.Center, Player.MountedCenter, 0.76f);
            }
        }
    }

    public void ManageTentacles()
    {
        var FromOwner = Projectile.Center - Player.MountedCenter;
        float DistTooBig = 225;

        if (tentacle == null)
        {
            tentacle = new Rope(Projectile.Center, Player.MountedCenter, 15, 10f, Vector2.Zero);
        }

        for (var i = 1; i < tentacle.segments.Length - 1; i++)
        {
            if (Main.rand.NextBool(85) && i < tentacle.segments.Length - tentacle.segments.Length / 6)
            {
                var blood = Dust.NewDustPerfect(tentacle.segments[i].position, DustID.CrimtaneWeapons, new Vector2(0, -3f), 10, Color.Crimson);
                blood.noGravity = true;
                blood.rotation = Main.rand.NextFloat(-89, 89);
            }

            if (i <= 10 && TimeInner <= 0 && FromOwner.Length() < DistTooBig)
            {
                tentacle.segments[i].position += new Vector2(10 * -Player.direction, -10 + Index % 2);
            }
            else if (Attack == 1 && i <= 10)
            {
                tentacle.segments[i].position += new Vector2(-Projectile.velocity.X, AttackVariation);
            }
        }

        //Projectile.direction = Player.direction;
        if (TimeInner <= 0)
        {
            tentacle.gravity = new Vector2(0, 1);
        }
        else
        {
            tentacle.gravity = new Vector2(-Projectile.velocity.X / tentacle.segments.Length, -Projectile.velocity.Y);
        }

        tentacle.damping = Utils.GetLerpValue(40, 20, Player.velocity.Length(), true) * 0.65f;

        ///agony

        var tex = TextureAssets.Projectile[Type].Value;
        var frameHeight = tex.Height / HeadFrameCount;
        var HeadFrame = new Rectangle(0, headFrame * frameHeight, tex.Width, 192);
        var scale = Projectile.scale;

        var localTip = new Vector2
        (
            0f,
            HeadFrame.Height / 5
        );

        var tipOffset = localTip.RotatedBy(Projectile.rotation);

        var jitter = new Vector2(0, (float)Math.Cos((Time + Index * 100) / 40));

        tentacle.segments[0].position = Projectile.Center - tipOffset + Projectile.velocity;
        tentacle.segments[1].position = tentacle.segments[0].position - tipOffset + Projectile.velocity;
        tentacle.segments[^1].position = Player.MountedCenter + jitter / 2;

        Projectile.Center += jitter / 10;
        tentacle.Update();
    }

    public void TargetAndAttack()
    {
        var FromOwner = Projectile.Center - Player.MountedCenter;
        float TargetRange = 380;

        if (Player.GetModPlayer<AwakenedBloodPlayer>().BloodBoostActive)
        {
            TargetRange = 420;
        }

        if (Target == null && Attack == 0f)
        {
            var found = Projectile.FindTargetWithinRange(TargetRange);

            if (found != null)
            {
                Target = found;
            }
        }

        if (Target != null)
        {
            var npcDeadOrInactive = !Target.active || Target.life <= 0 || Target.friendly;

            var armorLost = !Player.active ||
                            Player.GetModPlayer<AwakenedBloodPlayer>().CurrentForm != AwakenedBloodPlayer.Form.Offense ||
                            !Player.GetModPlayer<AwakenedBloodPlayer>().AwakenedBloodSetActive;

            var tooFarFromPlayer = Vector2.Distance(Target.Center, Player.MountedCenter) > TargetRange + 100f;

            if (npcDeadOrInactive || armorLost || (tooFarFromPlayer && Attack == 0f))
            {
                Target = null;
            }
        }

        TargetName = Target;

        // If the target is more than 520 away from the player, discard the target.
        if (Target != null && (AttackCooldown! > 0 || Vector2.Distance(Target.Center, Player.MountedCenter) > TargetRange + 100))
        {
            Target = null;
        }

        // Compute home position
        var SignFromOwner = Math.Sign(FromOwner.X);
        var sideOffset = Player.direction * (Index % 2 == 0 ? -90f : 100f);
        var homePos = Player.MountedCenter + new Vector2(sideOffset, 10 * (Index % 2)) - Player.velocity * 2f;

        if (Index >= 2 && Target == null)
        {
            homePos = Player.MountedCenter + new Vector2(sideOffset, Index % 1 - (50 + Player.direction * SignFromOwner * (Index % 2))) - Player.velocity * 2f;
        }
        else
        {
            homePos = Player.MountedCenter + new Vector2(sideOffset, (Index % 2 == 0 ? 1 : 0) - 60) - Player.velocity * 20f;
        }

        if (Target == null && Attack != 0)
        {
            Attack = 0;
        }

        var trackSpeed = 1f;
        var stabSpeed = 50f;
        var windUpDuration = !Player.GetModPlayer<AwakenedBloodPlayer>().BloodBoostActive ? 15f : 7f;
        var retractSpeed = 1f;

        float DistTooBig = 225;

        if (Target != null && AttackCooldown <= 0)
        {
            var direction = Projectile.SafeDirectionTo(Target.Center);

            if (Attack == 0f)
            {
                // Begin wind-up
                AttackVariation = Main.rand.NextFloat(-20, -10);
                var distanceToTarget = (Projectile.Center - Target.Center).Length();

                if (distanceToTarget < TargetRange - 100)
                {
                    // Smoothly pull back before stabbing forward
                    var pullBackVelocity = direction * -8; // Pull back in the opposite direction
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

                if (Player.GetModPlayer<AwakenedBloodPlayer>().BloodBoostActive)
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
                var angleDifference = MathHelper.ToDegrees(Math.Abs(MathHelper.WrapAngle(Projectile.rotation - (direction.ToRotation() - MathHelper.PiOver2))));

                if (TimeInner < windUpDuration || angleDifference > 30f)
                {
                    // wind-up (no movement)
                    Projectile.velocity = Vector2.Zero;
                }
                else
                {
                    if (TimeInner == windUpDuration + 1)
                    {
                        SoundEngine.PlaySound
                        (
                            AssetDirectory.Sounds.Projectiles.BloodNeedle.NeedleStrike with
                            {
                                Pitch = 0.1f,
                                PitchVariance = 0.4f,
                                MaxInstances = 16
                            }
                        );
                    }

                    Projectile.velocity = direction * stabSpeed;
                }

                TimeInner++;
            }
            else if (Attack == 2f)
            {
                // retract
                var toHome = homePos - Projectile.Center;
                //todo: Add a little bit of angle variance for visual flair
                //float angleVariance = MathHelper.ToRadians(Main.rand.NextFloat(-15f, 15f)); // Random angle between -5 and 5 degrees
                var adjustedDirection = toHome.SafeNormalize(Vector2.Zero);
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
            var preferredAngle = MathHelper.ToRadians(-Player.direction * 50);
            Projectile.rotation = Projectile.rotation.AngleLerp(preferredAngle, 0.1f);

            if (FromOwner.Length() > DistTooBig)
            {
                Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.ToRotation() + MathHelper.PiOver2, 0.35f);
            }
        }
    }

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
    ///     Keeps track of which tendril this is.
    /// </summary>
    public ref float Index => ref Projectile.ai[2];

    /// <summary>
    ///     randomized value to add visual flair to the tendril's attacks
    /// </summary>
    public ref float AttackVariation => ref Projectile.localAI[0];

    private bool canDamage;

    /// <summary>
    ///     time (in ticks) between attacks from this needle
    /// </summary>
    public float AttackCooldown;

    public float MaxAttackCooldown = 30; //in ticks

    /// <summary>
    ///     For the visual effect on the needles
    /// </summary>
    public float pulsateAmount;

    public ref Player Player => ref Main.player[Projectile.owner];

    public Rope tentacle;

    public BezierCurve Tendril;

    /// <summary>
    ///     stores the target chosen, mainly used for debug
    /// </summary>
    private NPC TargetName;

    /// <summary>
    ///     The target of the needle. This is set when the needle gets a target.
    ///     it is used to prevent the needle from switching targets while it is attacking.
    /// </summary>
    public NPC Target { get; set; }

    public float ClotInterp => 0;

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
        var frenzy = Player.GetModPlayer<AwakenedBloodPlayer>().BloodBoostActive;
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

    private bool CanHit()
    {
        return Attack == 1 && AttackCooldown <= 0;
    }

    public override bool? CanHitNPC(NPC target)
    {
        return CanHit();
        //(Attack == 1 && AttackCooldown <= 0 && (!Main.zenithWorld || !target.friendly));
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        base.ModifyDamageHitbox(ref hitbox);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        AttackCooldown += 3; //MaxAttackCooldown - 2 + Main.rand.Next(0, 2);
        var metaball = ModContent.GetInstance<BloodMetaball>();

        for (var i = 0; i < 5; i++)
        {
            var bloodSpawnPosition = target.Center;
            var bloodVelocity = (Main.rand.NextVector2Circular(8f, 8f) + Projectile.velocity / 5) * Main.rand.NextFloat(0.4f, 4.2f);
            metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(10f, 40f), Main.rand.NextFloat(2f));
        }
        /*
        if (Player.GetModPlayer<BloodArmorPlayer>().Frenzy)
        {
            Player.statLife++;
        }
        */

        if (target.life < damageDone)
        {
            Target = null;
            AttackCooldown = 0;
            Attack = 3;
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Player.GetModPlayer<AwakenedBloodPlayer>().BloodBoostActive)
        {
            modifiers.SetCrit();
        }

        target.AddBuff(ModContent.BuffType<LacerationBuff>(), 240);
    }

    public override bool? CanCutTiles()
    {
        return CanHit();
    }

    #endregion

    #region drawcode

    /// <summary>
    ///     Gives each piece of clot a randomized position on the tendril.
    /// </summary>
    /// <param name="desiredCount"></param>
    private void EnsureClotOffsetCount(int desiredCount)
    {
        if (clotOffsets == null)
        {
            clotOffsets = new List<Vector2>(desiredCount);
        }

        if (clotFrames == null)
        {
            clotFrames = new List<int>(desiredCount);
        }

        // Append random offsets if we have fewer than needed:
        while (clotOffsets.Count < desiredCount)
        {
            var maxOffset = 5f; // adjust “scatter radius” as you like
            var ox = Main.rand.NextFloat(-maxOffset, maxOffset);
            var oy = Main.rand.NextFloat(-maxOffset, maxOffset);
            clotOffsets.Add(new Vector2(ox, oy));
        }

        while (clotFrames.Count < desiredCount)
        {
            var totalFrames = 1;
            var frame = Main.rand.Next(0, totalFrames + 1);
            clotFrames.Add(frame);
        }

        while (clotFrames.Count > desiredCount)
        {
            clotFrames.RemoveAt(clotFrames.Count - 1);
        }

        // If we somehow have more entries than segments, trim the end:
        while (clotOffsets.Count > desiredCount)
        {
            clotOffsets.RemoveAt(clotOffsets.Count - 1);
        }
    }

    //todo: create a method that checks if the player is in frenzy. then, animate the bloodneedle_head and hold it on the last frame.
    //when the frenzy is over, run backwards through the animation until its on the first frame again.
    //7 frames
    private void DrawLine()
    {
        var texture = TextureAssets.FishingLine.Value;
        var frame = texture.Frame();
        var origin = new Vector2(frame.Width / 2, 2);

        var points = new List<Vector2>();

        points.AddRange(tentacle.GetPoints());
        points.Add(Player.MountedCenter);

        var pos = points[0] + new Vector2(-2, 0);

        for (var i = 0; i < points.Count - 1; i++)
        {
            var element = points[i];
            var diff = points[i + 1] - element;

            var rotation = diff.ToRotation() - MathHelper.PiOver2;
            var color = Lighting.GetColor(element.ToTileCoordinates(), Color.Crimson);
            var scale = new Vector2(2, (diff.Length() + 2) / frame.Height);

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None);

            pos += diff;
        }
    }

    /// <summary>
    ///     Draws a glowing light when called, and the size and strength is determined by
    ///     Main.GlobalWTimeWrappedHourly.
    /// </summary>
    /// <param name="lightColor"></param>
    public void drawHeartbeat(Vector2 Position, Color lightColor, float bpm = 60f, bool invert = false)
    {
        var heartTex = AssetDirectory.Textures.BigGlowball.Value;
        var healthFactor = MathHelper.Clamp(1f - Player.statLife / (float)Player.statLifeMax2, 0.5f, 1.5f); // Lower health = faster heartbeat
        var erraticFactor = 0.9f + Main.rand.NextFloat(-0.1f, 0.1f); // Add slight randomness to the beat timing
        var beatSpeed = 2f * healthFactor * erraticFactor; // Adjust beat speed based on health and randomness

        // Calculate the length of one beat in seconds
        var beatInterval = 60f / bpm;

        // Global time in seconds
        var globalTime = Main.GlobalTimeWrappedHourly;

        // Position in the current beat cycle [0 .. beatInterval)
        var t = globalTime % beatInterval;

        if (invert)
        {
            t = beatInterval - t;
        }

        // Normalize into a 0–1 range over the full interval
        var norm = t / beatInterval;

        // Determine scale factors over beat phases:
        // 0.00–0.25 : grow    (0 to max)
        // 0.25–0.40 : hold max
        // 0.40–0.65 : shrink (max back to rest)
        // 0.65–1.00 : rest   (at rest size)
        var restScale = 0.075f;
        var maxScale = 0.15f;
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

        var scale = new Vector2(pulsate * 0.9f, pulsate * 1.1f);

        var glowOffset = new Vector2(0, 0f);
        var drawPos = Position + glowOffset;

        Main.EntitySpriteDraw
        (
            heartTex,
            drawPos,
            heartTex.Frame(),
            Color.Crimson.MultiplyRGB(lightColor),
            Projectile.rotation,
            heartTex.Size() * 0.5f,
            scale,
            SpriteEffects.None
        );
        //Utils.DrawBorderString(Main.spriteBatch, "O", drawPos, Color.White);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var head = ModContent.Request<Texture2D>(Texture).Value;
        //todo: create frame and make sure that it looks decent.x

        var frameHeight = head.Height / HeadFrameCount;
        var HeadFrame = head.Frame(1, 8, 0, headFrame);
        var origin = new Vector2(head.Width / 2f, frameHeight / 2f);
        var drawPos = Projectile.Center - Main.screenPosition;

        if (Player.GetModPlayer<AwakenedBloodPlayer>().BloodBoostActive)
        {
            drawHeartbeat(drawPos, lightColor, 20);
        }

        var sprite = Player.direction * Player.gravDir < 0 ? SpriteEffects.FlipHorizontally : 0;

        var headDrawPos = new Vector2(0, 0) + Projectile.Center - Main.screenPosition;

        if (!Projectile.isAPreviewDummy && Index == 1)
        {
            /*
                Utils.DrawBorderString(Main.spriteBatch, "| Attack variation: " + AttackVariation.ToString(), Projectile.Center - Vector2.UnitY * 120 - Main.screenPosition, Color.White);
                Utils.DrawBorderString(Main.spriteBatch, "| Index: " + Index.ToString() + " | Direction: " + Projectile.direction.ToString() + " | TimeInner: " + TimeInner.ToString(), Projectile.Center - Vector2.UnitY * 140 - Main.screenPosition, Color.White);
                Utils.DrawBorderString(Main.spriteBatch, "| Attack: " + Attack.ToString(), Projectile.Center - Vector2.UnitY * (100) - Main.screenPosition, Color.White);

                Utils.DrawBorderString(Main.spriteBatch, "| Velocity: " + Projectile.velocity.ToString(), Projectile.Center - Vector2.UnitY * (220) - Main.screenPosition, Color.White);

                Utils.DrawBorderString(Main.spriteBatch, "| Atttack Cooldown: " + AttackCooldown.ToString(), Projectile.Center - Vector2.UnitY * (180) - Main.screenPosition, Color.White);
                if (TargetName != null)
                    Utils.DrawBorderString(Main.spriteBatch, "| " + TargetName.ToString(), Projectile.Center - Vector2.UnitY * (160) - Main.screenPosition, Color.White);
            */
        }

        DrawLine();
        DrawTentacles(ref lightColor);

        Main.EntitySpriteDraw(head, headDrawPos, HeadFrame, lightColor, Projectile.rotation, origin, new Vector2(1f, 1f), sprite);

        return false;
    }

    public void DrawTentacles(ref Color lightColor)
    {
        var body = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/RegSeg").Value;
        Texture2D Clot = GennedAssets.Textures.GreyscaleTextures.WhitePixel; //ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/Scab").Value;

        if (tentacle != null)
        {
            var Tendril = Player.direction * Player.gravDir < 0 ? 0 : SpriteEffects.FlipVertically;

            var points = new List<Vector2>();

            points.AddRange(tentacle.GetPoints());
            points.Add(Player.MountedCenter);
            var tentacleVel = -Projectile.rotation.ToRotationVector2() * 16;
            EnsureClotOffsetCount(points.Count);
            // DrawLine(points);
            var pulsateIndex = (int)(Main.GlobalTimeWrappedHourly * 20) % points.Count; // Determine which point to pulsate
            pulsateAmount = (float)(Math.Cos(Main.GlobalTimeWrappedHourly) % 1f * 0.2f);

            var BodyframeHeight = body.Height / BodyFrameCount;
            var BodyFrame = new Rectangle(0, bodyFrame * BodyframeHeight, body.Width, BodyframeHeight);
            var Borigin = new Vector2(body.Width / 2f, BodyframeHeight / 2f);

            //for (int i = points.Count - 1; i > 0; i--)
            for (var i = 1; i < points.Count - 1; i++)
            {
                var rot = points[i].AngleTo(points[i - 1]);
                var currentPulsate = 0f;

                // Apply pulsate amount to the current point, and half to adjacent points
                if (i == pulsateIndex)
                {
                    currentPulsate = pulsateAmount;
                }
                else if (i == pulsateIndex - 2 || i == pulsateIndex + 2)
                {
                    currentPulsate = pulsateAmount * 0.75f;
                }

                var stretch = new Vector2
                (
                    (1.3f - (float)i / points.Count * 0.6f) * Projectile.scale,
                    i > points.Count ? points[i].Distance(points[i - 1]) / (body.Height / 2f) : 1.1f + currentPulsate
                );

                var ScabFrame = new Rectangle(0, clotFrames[i], 16, 16);
                var Clorigin = Clot.Size() * 0.5f;

                Main.EntitySpriteDraw(body, points[i] - Main.screenPosition, BodyFrame, lightColor, rot, Borigin, stretch, Tendril);
                var fixedOffset = clotOffsets[i];
                var clotDrawPos = points[i] + fixedOffset;
            }
        }
    }

    #endregion
}

#region Laceration

public class LacerationBuff : ModBuff
{
    //why did I make it an undertale reference
    /// <summary>
    ///     strength of debuff, essentially.
    /// </summary>
    public int LOVE { get; set; } = 0;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
    {
        Utils.DrawBorderString(spriteBatch, LOVE.ToString(), drawParams.Position, Color.White);
    }

    public override bool ReApply(NPC npc, int time, int buffIndex)
    {
        var root = LacerationNPC.GetRealLifeOrSelf(npc);
        root.GetGlobalNPC<LacerationNPC>().LOVE++;

        return false;
    }

    public override void Update(NPC npc, ref int buffIndex)
    {
        var root = LacerationNPC.GetRealLifeOrSelf(npc);
        root.GetGlobalNPC<LacerationNPC>().Time++;
    }

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
    }
}

public class LacerationNPC : GlobalNPC
{
    public int Time;

    public override bool InstancePerEntity => true;

    public int LOVE { get; set; }

    public static NPC GetRealLifeOrSelf(NPC npc)
    {
        if (npc.realLife >= 0 && Main.npc[npc.realLife].active)
        {
            return Main.npc[npc.realLife];
        }

        return npc;
    }

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
        var root = GetRealLifeOrSelf(npc);

        if (root.HasBuff<LacerationBuff>())
        {
            var g = root.GetGlobalNPC<LacerationNPC>();

            if (g.Time % 75 == 0)
            {
                root.lifeRegen -= 2 * g.LOVE;
                damage += 10 * g.LOVE;
                root.SimpleStrikeNPC(damage, 1, false, 0, null, true, 0, true);
            }
        }
    }

    public override void DrawEffects(NPC npc, ref Color drawColor)
    {
        if (npc.HasBuff<LacerationBuff>())
        {
            var dustcount = (int)Math.Round((decimal)(LOVE / 100));

            for (var i = 0; i < dustcount; i++)
            {
                int type = npc.Organic() ? DustID.Blood : DustID.DesertPot;
                var a = Dust.NewDustPerfect(npc.Center, type);
                a.position = npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height);
            }
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