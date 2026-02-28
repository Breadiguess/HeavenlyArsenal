using CalamityMod;
using CalamityMod.Items.Materials;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using HeavenlyArsenal.Core.Systems;
using Luminance.Common.Utilities;
using NoxusBoss.Content.Biomes;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;

internal partial class RitualAltar : BaseBloodMoonNPC
{
    public override BloodMoonBalanceStrength Strength => new(0.2f,0.2f,0.2f);
    public enum AltarAI
    {
        LookingForSacrifice,

        Sacrificing,

        lookForBuffTargets,

        Buffing,

        WalkTowardsPlayer,

        DeathAnimation
    }

    public int SacrificeCooldown;

    public bool isSacrificing;

    public AltarAI currentAIState = AltarAI.LookingForSacrifice;

    private float SpeedMulti = 1;

    private int Variant;
    private Entity currentTarget;
    private Player playerTarget;

    public NPC NPCTarget { get; set; }
    public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/RitualAltarNPC/RitualAltarConcept";



    public override int MaxBlood => 100;

    private Vector2 MotionIntent;

    public override void SetStaticDefaults2()
    {
        Main.npcFrameCount[NPC.type] = 1;
        NPCID.Sets.CantTakeLunchMoney[NPC.type] = true;
        NPCID.Sets.DoesntDespawnToInactivityAndCountsNPCSlots[Type] = true;

        ContentSamples.NpcBestiaryRarityStars[Type] = 0;

        var value = new NPCID.Sets.NPCBestiaryDrawModifiers
        {
            Scale = 0.15f,
            PortraitScale = 0.3f
        };
    }

    public override void SendExtraAI2(BinaryWriter writer)
    {
        base.SendExtraAI(writer);

    }

    public override void ReceiveExtraAI2(BinaryReader reader)
    {
        base.ReceiveExtraAI(reader);

       
    }

    public override void OnSpawn(IEntitySource source)
    {
        NPC.rotation = -MathHelper.PiOver2;
        CreateLimbs();
        CultistCoordinator.CreateNewCult(NPC, Main.rand.Next(5, 8));

        for (var i = 0; i < Main.rand.Next(2, 5); i++)
        {
            float thing = 1;

            if (i % 2 == 0)
            {
                thing = -1;
            }

            var offset = new Vector2(10 * thing * i, 0);
            //NPC.NewNPCDirect(NPC.GetSource_FromThis(), NPC.Center, ModContent.NPCType<FleshlingCultist.FleshlingCultist>());
        }
    }


    public override bool CheckDead()
    {
        if (NPC.Opacity > 0)
        {
            Time = -1;
            NPC.life = 1;
            NPC.dontTakeDamage = true;
            currentAIState = AltarAI.DeathAnimation;
        }

        return false;
    }
    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<PenumbralMembrane>(), 4, 1));
        npcLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<RitualAltar>()));

        npcLoot.Add(ModContent.ItemType<BloodOrb>(), 1, 10, 18);
    }

    protected override void SetDefaults2()
    {
        Variant = Main.rand.Next(1, 5);
        NPC.width = 80;
        NPC.height = 80;
        NPC.lifeMax = 350_000;
        NPC.damage = 300;
        NPC.defense = 120;
        NPC.npcSlots = 4f;
        NPC.knockBackResist = 0f;
        NPC.noGravity = false;
        NPC.noTileCollide = false;

        CanBeSacrificed = false;
       
    }

    public override void AI()
    {
        // NPC.Center = Main.MouseWorld;
        //NPC.velocity.X = NPC.AngleTo(Main.LocalPlayer.Calamity().mouseWorld).ToRotationVector2().X * 10;//* NPC.Distance(Main.MouseWorld) ;

        NPC.velocity.X = 0;
        //return;
        locateSacrifices();
        UpdateList();
        StateMachine();

        SpeedMulti = Math.Abs(MathF.Sin(Time + NPC.whoAmI) * 2);
        NPC.direction = Math.Sign(NPC.velocity.X.NonZeroSign()) != 0 ? Math.Sign(NPC.velocity.X) : 1;

        if (isSacrificing && (NPCTarget == null || !NPCTarget.active))
        {
            isSacrificing = false;
        }

        if (NPCTarget == null || !NPCTarget.active)
        {
            NPCTarget = default;
        }

        //NPC.Center = Main.LocalPlayer.Calamity().mouseWorld;
        Time++;
    }

    private void balanceHead(float interp = 0.2f)
    {
        var d = Vector2.Zero;

        EstimateSurfaceFrame(NPC.Center, out normal, out tangent);


        // Tilt based on horizontal velocity only:
        // - referenceSpeed defines how fast it must move to reach full tilt.
        // - maxTilt limits the tilt angle (in radians).
        var referenceSpeed = 1f;
        var maxTilt = MathHelper.ToRadians(20f);
        var normalized = MathHelper.Clamp(NPC.velocity.X / referenceSpeed, -1f, 1f);
        var targetRotation = normalized * maxTilt + tangent.ToRotation() - MathHelper.PiOver2;

        // Slightly lerp rotation toward the horizontal-velocity-based target.
        NPC.rotation = NPC.rotation.AngleLerp(targetRotation, 0.1f);
    }

    public override void PostAI()
    {


        balanceHead(0.025f);

        Levitate();
        for(int i = 0; i< LimbCount; i++)
        {
            var limb = _limbs[i];

            UpdateLimbState(ref limb, NPC.Center+ _limbBaseOffsets[i], i);
        }
        
        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

        if (NPCTarget != null)
        {
            currentTarget = NPCTarget;
        }
        else if (playerTarget != null)
        {
            currentTarget = playerTarget;
        }
    }
    private void Levitate()
    {

        float maxCheck = 170f;
        int hitCount = 0;
        float accumulatedHeight = 0f;

        for (int i = 0; i < 3; i++)
        {
            Vector2 start = NPC.Center;
            Vector2 end = start + Vector2.UnitY.RotatedBy(MathHelper.PiOver2 * i / 3f - MathHelper.PiOver2 / 3f - NPC.rotation - MathHelper.PiOver2) * maxCheck;

            Point? hit = LineAlgorithm.RaycastTo(start, end, debug: false);

            if (!hit.HasValue)
                continue;

            float height =
                hit.Value.ToWorldCoordinates().Y - NPC.Center.Y;

            accumulatedHeight += height;
            hitCount++;
        }

        if (hitCount < 2)
        {
            NPC.noGravity = false;
            return;
        }

        float actualHeight = accumulatedHeight / hitCount;
        float desiredHeight = 90f;
        float tolerance = 1.5f;

        float error = desiredHeight - actualHeight;

        if (MathF.Abs(error) < tolerance)
        {
            NPC.velocity.Y = 0f;
            NPC.noGravity = true;
            return;
        }

        float correctionStrength = 0.08f;

        float moveAmount = error * correctionStrength;
        moveAmount = MathHelper.Clamp(moveAmount, -2f, 2f);

        NPC.position.Y -= moveAmount;
        NPC.noGravity = true;
        NPC.velocity.Y = 0f;
    }



    public Vector2 normal;
    public Vector2 tangent;
    private Vector2 _lastBodyPos;

    public static bool EstimateSurfaceFrame(Vector2 origin, out Vector2 normal, out Vector2 tangent)
    {
        const int samples = 5;       // must be odd
        const float spacing = 63 * 3f;
        const float depth = 300f;

        int half = samples / 2;

        float sumX = 0f;
        float sumY = 0f;
        float sumXX = 0f;
        float sumXY = 0f;

        int valid = 0;

        for (int i = 0; i < samples; i++)
        {
            float x = (i - half) * spacing;

            Vector2 start = origin + new Vector2(x, -120);
            Vector2 end = start + Vector2.UnitY * depth;

            Point? hit = LineAlgorithm.RaycastTo(
                start,
                end,
                ShouldCountWater: false,
                debug: false);

            if (!hit.HasValue)
                continue;

            Vector2 world = hit.Value.ToWorldCoordinates();

            float y = world.Y;

            sumX += x;
            sumY += y;
            sumXX += x * x;
            sumXY += x * y;

            valid++;
        }

        if (valid < 2)
        {
            tangent = Vector2.UnitX;
            normal = Vector2.UnitY;
            return false;
        }

        float denom = valid * sumXX - sumX * sumX;

        if (Math.Abs(denom) < 0.001f)
        {
            tangent = Vector2.UnitX;
            normal = Vector2.UnitY;
            return false;
        }

        float slope = (valid * sumXY - sumX * sumY) / denom;

        tangent = new Vector2(1f, slope).SafeNormalize(Vector2.UnitX);
        normal = new Vector2(-tangent.Y, tangent.X);

        if (normal.Y < 0)
            normal *= -1f;

        return true;
    }
}