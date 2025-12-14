using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture;

// certainly! here's a ready-made ai class for your void vulture!
// lmao.
partial class voidVulture //_AI
{
    private Vector2[] LegPos = new[]
    {
        new Vector2(40, 60),
        new Vector2(-40, 60)
    };

    public override bool CheckDead()
    {
        if (!HasSecondPhaseTriggered)
        {
            currentState = Behavior.PhaseTransition;
            Time = 0;
            NPC.life = 1;
            NPC.dontTakeDamage = true;
            HasSecondPhaseTriggered = true;
            NPC.noGravity = true;

            return false;
        }

        return HasSecondPhaseTriggered && HasDoneCutscene;
    }

    public override bool PreAI()
    {
        Myself = NPC;

        if (HasSecondPhaseTriggered)
        {
            DecideSolynBehavior();
        }

        foreach (var player in Main.ActivePlayers)
        {
            CalamityCompatibility.GrantInfiniteCalFlight(player);
        }

        manageHead();

        return base.PreAI();
    }

    public override void AI()
    {
        if (currentTarget != null)
        {
            foreach (var a in Main.ActivePlayers)
            {
                if (a.dead)
                {
                    NPC.active = false;
                }
            }
        }

        if (NPC.Opacity < 0.4f)
        {
            NPC.ShowNameOnHover = false;
        }
        else
        {
            NPC.ShowNameOnHover = true;
        }

        if (currentState == Behavior.EjectCoreAndStalk)
        {
            NPC.dontTakeDamage = CoreDeployed;
        }

            if (currentState == Behavior.reveal)
            {
                if(Time == 1)
                HeadPos = NPC.Center + new Vector2(0, 100);
                //wings[0].Time = 4;
                //voidVultureWing.UpdateWings(wings[0], NPC);
            }
            else
            {

        NPC.noGravity = true;

        StateMachine();

        if (currentState != Behavior.EjectCoreAndStalk)
        {
            NPC.Center = Vector2.Lerp(NPC.Center, TargetPosition, targetInterpolant);
        }

        Time++;
    }

    public override void PostAI()
    {
        //BattleSolynBird.SummonSolynForBattle(NPC.GetSource_FromThis(), currentTarget.Center, BattleSolynBird.SolynAIType.FightBird);
        if (currentState != Behavior.RiseSpin)
        {
            if (RiserForSpin != null)
            {
                if (RiserForSpin.LoopIsBeingPlayed)
                {
                    RiserForSpin.Stop();
                }
            }
        }

        if (currentState != Behavior.VomitCone)
        {
            NPC.direction = NPC.velocity != Vector2.Zero ? Math.Sign(NPC.velocity.X) : currentTarget != null ? Math.Sign(NPC.Center.X - currentTarget.Center.X) : 1;
            NPC.direction = currentState == Behavior.RiseSpin ? 0 : NPC.direction;
        }

        Staggered = StaggerTimer > 0;
        StaggerTimer -= StaggerTimer > 0 ? 1 : 0;
        ManageIK();
        ManageNeck();
        ManageTail();

        // Clear all debuffs when faded below 50% opacity
        if (NPC.Opacity < 0.5f)
        {
            // Loop backwards so removing buffs does not shift the remaining indices
            for (var i = NPC.buffType.Length - 1; i >= 0; i--)
            {
                if (NPC.buffType[i] > 0) // has a buff in this slot
                {
                    NPC.DelBuff(i);
                }
            }
        }

        if (currentState != Behavior.reveal)
        {
            foreach (var wing in wings)
            {
                voidVultureWing.UpdateWings(wing, NPC);
            }
            //if(!HasSecondPhaseTriggered)
            //TileDisablingSystem.TilesAreUninteractable = true;
        }

        if (HitTimer > 0)
        {
            HitTimer--;
        }
        //NPC.velocity = Vector2.Zero;
    }

        };
        public void ManageIK()
        {

            Vector2[] LegPos = new Vector2[]
            {
                new Vector2(40, 40),
                new Vector2(-40, 40)

        var offset = MathF.Sin(Time / 10.1f);

        var offset2 = MathF.Sin(Time / 10.1f + 40);
        Neck2.TargetPosition = HeadPos;
        UpdateLimbState(ref Neck2, NPC.Center, 0.6f, 30);

        if (currentState != Behavior.RiseSpin)
        {
            _LeftLeg.TargetPosition = NPC.Center + new Vector2(30, 190);
            _rightLeg.TargetPosition = NPC.Center + new Vector2(-30, 190);
        }
        else
        {
            _LeftLeg.TargetPosition = NPC.Center + new Vector2(0, 150);
            _rightLeg.TargetPosition = NPC.Center + new Vector2(0, 150);
        }

            //Dust a = Dust.NewDustPerfect(_LeftLeg.EndPosition, DustID.Cloud, Vector2.Zero);
            //a.velocity = Vector2.Zero;
            //a.noGravity = true;
            //CreateLegs();
            UpdateLegState(ref _LeftLeg, NPC.Center + LegPos[0] + NPC.velocity, 0.12f, 0);
            UpdateLegState(ref _rightLeg, NPC.Center + LegPos[1] + NPC.velocity, 0.12f, 0);
            //Main.NewText(_LeftLeg.Skeleton.JointCount);
            //TODO: set constraints based on the direction to the currentTarget (if that target exists).
            //these constraints should be set up so that they roughly cause the bend between skeleton 0 and 1 to be pointing towards the player
            if (NPC.direction != 0)
            { 
            }
        }
        void manageHead()
        {

            if (currentTarget == null)
                currentTarget = Main.LocalPlayer;
            if (currentState == Behavior.VomitCone || (currentState == Behavior.CollidingCommet && Time > 60) || currentState == Behavior.reveal)
            {
                return;
            }

        var maxStretch = Neck2.Skeleton._maxDistance * 0.86f;
        var minDistance = 38f;
        var lerpStrength = 0.6f;

        // Vector from NPC body to player
        var toPlayer = currentTarget.Center - NPC.Center;
        var distance = toPlayer.Length();

        if (distance < minDistance)
        {
            // Normalize safely, even if distance is tiny
            var safeDir = toPlayer.SafeNormalize(Vector2.UnitY);
            var pushedTarget = NPC.Center + safeDir * minDistance;

            // Just move head toward that pushed target
            HeadPos = Vector2.Lerp(HeadPos, pushedTarget, lerpStrength);
        }
        else
        {
            // Regular logic
            var dir = toPlayer / distance; // faster normalize
            var idealHeadPos = NPC.Center + dir * maxStretch;
            HeadPos = Vector2.Lerp(HeadPos, idealHeadPos, lerpStrength);
        }
    }

    private void manageWings()
    {
        //NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.AngleTo(TargetPosition).ToRotationVector2(),0.1f);
        if (TargetPosition.Y > NPC.Center.Y && NPC.Distance(TargetPosition) > 140)
        {
            isFalling = true;
        }
        else
        {
            isFalling = false;
        }

        var thing = wings[0].Time / voidVultureWing.WingCycleTime; //wings[0].WingCycleTime;

        if (!isFalling)
        {
            if (thing > 0.3f && thing < 0.5f)
            {
                NPC.velocity.Y -= 1;
            }

            if (thing > 0.5f || thing < 0.3f)
            {
                if (Math.Sign(NPC.velocity.Y) > 0)
                {
                    NPC.velocity.Y *= 0.929f;
                }
            }
        }
    }
}