using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.sipho;

internal partial class CryonophoreLimb : BloodMoonBaseNPC
{
    public int OwnerIndex;

    public CryonophoreZooid self;

    private Vector2[] LimbSegmentPos;

    private Vector2[] LimbSegmentVels;

    public override bool canBeSacrificed => false;

    public override bool canBebuffed => false;

    public NPC Owner => Main.npc[OwnerIndex] != null ? Main.npc[OwnerIndex] : default;

    public override void SetDefaults()
    {
        NPC.lifeMax = 64_000;
        NPC.aiStyle = -1;
        NPC.Size = new Vector2(30, 30);
        NPC.noGravity = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        LimbSegmentPos = new Vector2[8];
        LimbSegmentVels = new Vector2[8];

        for (var i = 0; i < LimbSegmentPos.Length; i++)
        {
            LimbSegmentPos[i] = NPC.Center;
        }
    }

    public override void AI()
    {
        //Main.NewText(NPC.Center);

        NPC.rotation = NPC.velocity.ToRotation();

        if (currentTarget == null)
        {
            var d = Owner.ModNPC as Cryonophore;
            currentTarget = d.currentTarget;
            NPC.Center = Owner.Center;
        }
        else
        {
            StateMachine();
        }

        Time++;
    }

    public override void PostAI()
    {
        ManageLimb();
        float pushRadius = 30;

        for (var i = 0; i < Main.maxNPCs; i++)
        {
            var other = Main.npc[i];

            if (other.active && other.whoAmI != NPC.whoAmI && other.type == NPC.type)
            {
                var dist = Vector2.Distance(NPC.Center, other.Center);

                if (dist < pushRadius && dist > 0f)
                {
                    // Compute a small push vector away from the other NPC
                    var pushDir = (NPC.Center - other.Center).SafeNormalize(Vector2.Zero);
                    var pushAmount = (pushRadius - dist) / pushRadius; // stronger when closer
                    NPC.velocity += pushDir * 1.2f * pushAmount;
                }
            }
        }
    }

    private void ManageLimb()
    {
        float segmentLength = 16;
        var BodyRot = -NPC.rotation.ToRotationVector2();
        LimbSegmentPos[0] = NPC.Center;

        for (var i = 1; i < LimbSegmentPos.Length; i++)
        {
            var targetPos = LimbSegmentPos[i - 1] + BodyRot * segmentLength;

            var alignVel = (targetPos - LimbSegmentPos[i]) * 0.5f;
            LimbSegmentVels[i] = Vector2.Lerp(LimbSegmentVels[i], alignVel, 0.5f);
            LimbSegmentPos[i] += LimbSegmentVels[i];

            if (LimbSegmentPos[i] == Vector2.Zero)
            {
                LimbSegmentPos[i] = NPC.Center;
            }
        }
    }

    private void StateMachine()
    {
        switch (self.type)
        {
            case ZooidType.basic:
                NPC.velocity = NPC.AngleTo(currentTarget.Center).ToRotationVector2() * 10;

                break;

            case ZooidType.Ranged:
                //ManageRanged();
                break;
        }
    }

    private void ManageRanged()
    {
        NPC.Center = currentTarget.Center + Main.rand.NextVector2CircularEdge(70, 70);
    }
}