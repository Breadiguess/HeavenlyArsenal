using System.Collections.Generic;
using CalamityMod;
using HeavenlyArsenal.Common.utils;
using Luminance.Assets;
using NoxusBoss.Assets;
using NoxusBoss.Core.Physics.VerletIntergration;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.Temp;

public class Bola : ModProjectile
{
    #region Values / setup

    public int BolaCount = 3;

    public NPC BoundTarget { get; set; }

    public bool StealthStrike;

    public enum BolaState
    {
        Windup,

        Throw,

        Tangled
    }

    public BolaState CurrentState = BolaState.Windup;

    public ref float Time => ref Projectile.ai[0];

    public ref float Charge => ref Projectile.ai[1];

    public float SwingStrength => MathF.Pow(1 + Charge / 180, 4);

    public int MaxCharge
    {
        get
        {
            if (Owner.Calamity().wearingRogueArmor)
            {
                return (int)(Owner.Calamity().rogueStealthMax * 100);
            }

            return 100;
        }
    }

    public ref float ArmRot => ref Projectile.localAI[0];

    public ref Player Owner => ref Main.player[Projectile.owner];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.stopsDealingDamageAfterPenetrateHits = true;
        Projectile.timeLeft = 540;
        Projectile.Size = new Vector2(20, 20);
    }

    public override void OnSpawn(IEntitySource source)
    {
        ClearStealth();

        Owner.StartChanneling();

        //Projectile.rotation = Projectile.velocity.ToRotation();
        Balls = new List<Vector2>(BolaCount);

        for (var i = 0; i < Balls.Capacity; i++)
        {
            var Pos = new Vector2(30, 0).RotatedBy(MathHelper.TwoPi * i);
            Balls.Add(Pos);
        }

        calculateCenter();
        bolaRope = new List<Ties>(Balls.Count);

        for (var i = 0; i < Balls.Count; i++)
        {
            bolaRope.Add(new Ties(Balls[i], tieCenter));
        }
    }

    #endregion

    // TODOS FOR TOMORROW:
    // 1. MAKE STEALTH BUILD SLOWLY OVER TIME. DON'T FORGET TO FACTOR IN MAX STEALTH AND STEALTH ACCELERATION.
    // 2. MAYBE REWRITE THE CODE FOR THE BOLAS SO THAT INSTEAD OF DOING COSTLY ROPE PHYSICS SIMS, ITS JUST VERLET STRINGS.
    // THIS WILL MAKE ME FEEL A BIT BETTER.
    // 3. START DANGLING, THEN MOVE TO SPINNING THE BOLAS WHILE CHARGING. REMEMBER TO TRY TO GET THIS OFFSET A 45 DEGREE ANGLE
    // - THINK LIKE HOW VOIDCREST OATH'S HALO LOOKS.
    // STARTS OFF SLOW, BUT RAPIDLY SPINS UP. 
    // MAYBE IT ENDS UPLOOKING LIKE A STREAK OF LIGHT? LIKE ITS MOVING SO FAST THAT YOU CAN ONLY SEE A CONTINUOUS CIRCLE.
    // THAT WOULD PROBABLY MAKE IT A LOT EASIER TO CODE, ACTUALLY.
    //
    // 4. HERE'S A LIST OF SEVERAL IDEAS I'VE HAD FOR THE STEALTH STRIKE:
    //      a. OPENS A PORTAL TO THE DEAD UNIVERSE AND DRAGS THE TARGET INSIDE. THIS DEALS A "YES" AMOUNT OF DAMAGE.
    //      b. WRAPS THE TARGET IN SHADOWY CLOTH AND THEN DRAINS THEM. MAYBE BETTER ON NON STEALTH STRIKE?
    //      c. TRANSFORMATION. AT MAX CHARGE, THE BOLAS TRANSFORM INTO A DIVINE WEAPON AND DEALS A "YESSER" AMOUNT OF DAMAGE. 
    //       HONESTLY ONE OF THE MORE BORING OPTIONS. I DON'T WANT TO MAKE ANOTHER ENDGAME JAVELIN.
    //      
    // HONESTLY IM LOOKING FORWARD TO THIS. THIS SEEMS LIKE A FUN WEAPON CONCEPT, AND I CAME UP WITH IT ALL ON MY OWN.
    // OTHER ASSORTED IDEAS INCLUDE:
    // ALT FIRE SCATTERING SOME KIND OF TRAP AROUND THAT YOU CAN THEN SEND NPCS INTO WITH THE BOLA.
    // SOME KIND OF CALTROPS? EVIL AND FUCKED UP CALTROPS.
    // PORTALS RIPPING OUT OF SPACE AND SPEARS IMPALING AND RIPPING APART ENEMIES BY TRYING TO DRAG THEM IN DIFFERENT DIRECTIONS.
    // PROBALBY NOT GONNA BE USED BECAUSE I'VE OVERUSED THAT ASTHETIC/THEMATIC/VISUAL SO MUCH ALREADY.
    // HITTING THE GROUND WITH THE BOLA CREATES A TRAP/SPRAYS THE ORBS AROUND.
    // I THINK THATS IT. 
    // I LOVE YOU.

    #region AI

    public override void AI()
    {
        calculateCenter();
        StateMachine();

        Updateballs();
        UpdateRope();
        Time++;
    }

    private void StateMachine()
    {
        switch (CurrentState)
        {
            case BolaState.Windup:
            {
                ManageCharge();

                break;
            }
            case BolaState.Throw:
            {
                ManageThrow();

                break;
            }
            case BolaState.Tangled:
            {
                ManageTangled();

                break;
            }
        }
    }

    private void ManageCharge()
    {
        Projectile.timeLeft++;

        if (Owner.Calamity().wearingRogueArmor)
        {
            Owner.Calamity().rogueStealth = Charge / 100;
        }

        var toMouse = Owner.MountedCenter.AngleTo(Main.MouseWorld);
        ArmRot = MathHelper.ToRadians(-Owner.direction * -150 + MathHelper.ToDegrees(toMouse)) + MathHelper.ToRadians(MathF.Sin(Time / 10));

        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, ArmRot);
        var HandPos = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, ArmRot);

        Projectile.velocity = Owner.MountedCenter.AngleTo(Main.MouseWorld).ToRotationVector2() * 0.1f;
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.Center = HandPos;

        if (Owner.channel)
        {
            Charge = Math.Clamp(Charge + CalculateStealthMulti(), 0, MaxCharge);
            //Main.NewText($"Charge: {Charge}, \n Stealth: {Owner.Calamity().rogueStealth}");
        }

        if (!Owner.channel)
        {
            if (Charge < 10)
            {
                Projectile.Kill();
            }
            else
            {
                if (Owner.Calamity().StealthStrikeAvailable())
                {
                    Owner.Calamity().ConsumeStealthByAttacking();
                    StealthStrike = true;
                }

                CurrentState = BolaState.Throw;
                Time = 0;
                Projectile.velocity = Owner.MountedCenter.AngleTo(Main.MouseWorld).ToRotationVector2() * 20 * Charge / 100;
            }
        }
    }

    private void ManageThrow()
    {
        var val = calculateSpeed() * Charge / 100;
        Projectile.rotation = Projectile.velocity.ToRotation();

        if (Time > 60)
        {
            Projectile.velocity.Y += 1f;
        }
    }

    private void ManageTangled()
    {
        if (BoundTarget == null)
        {
            Projectile.active = false;
        }

        Projectile.Center = BoundTarget.Center;
    }

    #endregion

    #region Collisions

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (BoundTarget == null)
        {
            BoundTarget = target;
        }

        if (CurrentState == BolaState.Throw)
        {
            CurrentState = BolaState.Tangled;
        }

        if (StealthStrike)
        {
            //oh boy here we go

            var A = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<DeadUniverse_Rift>(), 1, 0);
            var b = A.ModProjectile as DeadUniverse_Rift;

            if (b.goToBrazil.TrappedNPCs.Contains(target))
            {
                b.goToBrazil.TrappedNPCs.Add(target);
            }

            target.GetGlobalNPC<BrazilVictim>().Rift = b;
            target.GetGlobalNPC<BrazilVictim>().Banisher = Owner;

            Projectile.Kill();
        }
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        hitbox.Location += new Vector2(10, 0).RotatedBy(Main.GlobalTimeWrappedHourly).ToPoint();
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        return false;
    }

    #endregion

    #region HelpersAndStructs

    private void ClearStealth()
    {
        Owner.Calamity().rogueStealth = 0;
    }

    private float CalculateStealthMulti()
    {
        if (!Owner.Calamity().wearingRogueArmor)
        {
            return 1;
        }

        var thing = 0.5f;

        if (Owner.velocity.Length() > 0.01f)
        {
            thing *= Owner.Calamity().stealthGenMoving;
        }

        thing *= Owner.Calamity().stealthAcceleration;
        //todo: make it take exponentially longer past 75

        if (Charge > 75)
        {
            thing = MathF.Pow(1.1f, thing);
        }

        return thing;
    }

    public struct Ties
    {
        public VerletSimulatedSegment a;

        public Rope String;

        public Vector2 Start;

        public Vector2 End;

        public Ties(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
            String = new Rope(Start, End, 12, 2, Vector2.Zero, 5);
        }
    }

    public Vector2 tieCenter;

    public List<Vector2> BallTrail = new(10);

    public List<Vector2> Balls;

    public List<Ties> bolaRope;

    private Vector2 calculateCenter()
    {
        if (CurrentState == BolaState.Windup)
        {
            var HandPos = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, ArmRot);

            return HandPos;
        }

        var Center = Vector2.Zero;
        float x = 0;
        float y = 0;

        if (Balls != null && Balls.Count > 0)
        {
            for (var i = 0; i < Balls.Count; i++)
            {
                x += Balls[i].X;
                y += Balls[i].Y;
            }

            x /= Balls.Count;
            y /= Balls.Count;
            Center = new Vector2(x, y);
        }

        return Center;
    }

    private float calculateSpeed()
    {
        if (CurrentState == BolaState.Windup)
        {
            return 1;
        }

        // Get the projectile's overall speed (magnitude of velocity vector)
        var speed = Projectile.velocity.Length();

        // Convert speed into a multiplier. 
        // Example: 1f = normal speed, <1f = slower, >1f = faster.
        // Adjust the divisor (like 10f here) based on your design.
        var multiplier = speed / 20f;

        multiplier = MathHelper.Clamp(speed / 20f, 0.0f, 2f);

        return multiplier;
    }

    private void Updateballs()
    {
        float xScale;
        float yScale;
        Vector2 local;
        Vector2 world;

        for (var i = 0; i < Balls.Count; i++)
        {
            //okay, no.
            //so the current issue is that this doesn't feel like its weighted properly.
            //i'd say its becuase of the fact that the string is updated based on the bola, rather than the other way around. 
            //ughh i shouldn't have stayed up until almost 1. that was a huge mistake.
            if (CurrentState == BolaState.Windup)
            {
                var val2 = MathF.Pow(CalculateStealthMulti() + Charge / 100, 3);
                var rotationSpeed = 0.15f * val2;
                var orbitRadius = 30f;
                var ovalX = orbitRadius * 1.5f;
                var ovalY = orbitRadius * 0.5f;

                var angle = Projectile.direction * (Time * rotationSpeed + Projectile.whoAmI * 100);

                local = new Vector2(MathF.Cos(angle + Main.rand.NextFloat(-0.1f, 0.1f)), MathF.Sin(angle));
                local *= new Vector2(ovalX, ovalY);

                world = local.RotatedBy(Projectile.rotation);
                Balls[i] = world;

                continue;
            }

            var speedMulti = calculateSpeed();

            //todo: mutliply this by time so that when its initially thrown its not immediately spread out.
            var BallOffset = i * (360f / Balls.Count) * Math.Clamp(Time / 20, 0, 1);
            var value = Projectile.direction * MathHelper.ToRadians(Time * 16 * Charge / 100 + BallOffset + Projectile.whoAmI * 100);

            local = new Vector2(MathF.Cos(value), MathF.Sin(value));
            xScale = 30f * speedMulti * (1 + 1.25f * Math.Abs(2 - speedMulti));
            yScale = 30f * speedMulti * (1 - 0.15f * Math.Abs(2 - speedMulti));

            local *= new Vector2(xScale, yScale);

            // Now rotate oval into world space
            world = local.RotatedBy(Projectile.rotation);

            Balls[i] = world;
        }
    }

    private void UpdateRope()
    {
        if (bolaRope == null || bolaRope.Count == 0)
        {
            return;
        }

        for (var i = 0; i < bolaRope.Count; i++)
        {
            var start = bolaRope[i];
            start.Start = Balls[i];
            bolaRope[i] = start;

            var tie = bolaRope[i];
            tie.End = tieCenter;
            bolaRope[i] = tie;

            if (bolaRope[i].String == null)
            {
                bolaRope.Add(new Ties(Balls[i], tieCenter));
            }

            bolaRope[i].String.segments[0].position = bolaRope[i].Start;
            bolaRope[i].String.segments[0].oldPosition = bolaRope[i].Start;
            bolaRope[i].String.segments[0].pinned = true;

            bolaRope[i].String.segments[^1].position = bolaRope[i].End;
            bolaRope[i].String.segments[^1].oldPosition = bolaRope[i].End;
            bolaRope[i].String.segments[^1].pinned = true;

            bolaRope[i].String.Update();
        }
    }

    #endregion

    #region DrawCode

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        if (CurrentState == BolaState.Windup)
        {
            overPlayers.Add(index);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        DrawTies();
        DrawBalls();

        return false;
    }

    private void DrawBalls()
    {
        var ball = AssetDirectory.Textures.Items.Weapons.Rogue.BolaBall.Value;
        var Origin = ball.Size() * 0.5f;
        Vector2 DrawPos;

        if (Balls != null && Balls.Count > 0)
        {
            for (var i = 0; i < Balls.Count; i++)
            {
                var Rot = tieCenter.AngleTo(Balls[i]);
                DrawPos = Balls[i] + Projectile.Center - Main.screenPosition;

                Main.EntitySpriteDraw
                (
                    ball,
                    DrawPos,
                    null,
                    Color.AntiqueWhite with
                    {
                        A = 0
                    },
                    Rot,
                    Origin,
                    0.25f,
                    0
                );
            }
        }
    }

    private void DrawTies()
    {
        var thing = Color.AliceBlue;

        if (bolaRope != null)
        {
            if (CurrentState == BolaState.Windup)
            {
                for (var i = 0; i < bolaRope.Count; i++)
                {
                    Utils.DrawLine(Main.spriteBatch, bolaRope[i].Start + Projectile.Center, bolaRope[i].End + Projectile.Center, Color.AntiqueWhite, Color.AntiqueWhite, 2);
                }
            }
            else

            {
                foreach (var t in bolaRope)
                {
                    // Get rope points
                    var points = t.String.GetPoints();

                    Texture2D pixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

                    for (var i = 0; i < points.Length - 1; i++)
                    {
                        var start = points[i] + Projectile.Center - Main.screenPosition;
                        var end = points[i + 1] + Projectile.Center - Main.screenPosition;

                        var edge = end - start;
                        var length = edge.Length();
                        var rotation = edge.ToRotation();

                        Main.EntitySpriteDraw(pixel, start, null, thing, rotation, pixel.Size() * 0.5f, new Vector2(length, 2f), 0);
                    }
                }
            }
        }
    }

    #endregion
}