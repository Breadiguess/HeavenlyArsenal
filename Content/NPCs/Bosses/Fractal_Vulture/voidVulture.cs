using System.Collections.Generic;
using CalamityMod;
using HeavenlyArsenal.Common.IK;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Items.Weapons.Magic.RocheLimit;
using Luminance.Assets;
using Luminance.Common.Utilities;
using NoxusBoss.Assets;
using NoxusBoss.Content.Items.MiscOPTools;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria.Audio;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture;

[AutoloadBossHead]
partial class voidVulture : ModNPC
{
    public const NoxusBoss.Content.NPCs.Friendly.BattleSolyn.SolynAIType Funny = (NoxusBoss.Content.NPCs.Friendly.BattleSolyn.SolynAIType)(-174993);


    //CLUTTERR AAAA
    private static NPC? myself;

    private static readonly Vector2[] wingPos = new[]
    {
        new Vector2(40, 0),
        new Vector2(-40, 0)
    };

    public bool hideBar;

    public voidVultureNeck Neck2;

    public List<Rope> NeckStrands;

    public Rope Neck;

    public Vector2 HeadPos;

    public Entity currentTarget;

    public bool CoreDeployed = false;

    public bool HasSecondPhaseTriggered = false;

    public bool HasDoneCutscene = false;

    public bool isFalling;

    public Vector2 TargetPosition;

    public float targetInterpolant = 0.2f;

    public List<voidVultureWing> wings = new(2);

    private IKSkeleton _neckRightSkeleton;

    private IKSkeleton _neckLeftSkeleton;

    private voidVultureLeg _LeftLeg;

    private voidVultureLeg _rightLeg;

    private readonly int TailLength = 30;

    private Vector2[] TailPos;

    private Vector2[] TailVels;

    private List<(Vector2[], Vector2[])> neckStrandPositions;

    public static NPC Myself
    {
        get
        {
            if (Main.gameMenu)
            {
                return myself = null;
            }

            if (myself is null)
            {
                return null;
            }

            if (!myself.active)
            {
                return null;
            }

            if (myself.type != ModContent.NPCType<voidVulture>())
            {
                return null;
            }

            return myself;
        }
        private set => myself = value;
    }

    public int Time
    {
        get => (int)NPC.ai[0];
        set => NPC.ai[0] = value;
    }

    public int HitTimer
    {
        get => (int)NPC.localAI[0];
        set => NPC.localAI[0] = value;
    }

    public override string Texture => MiscTexturesRegistry.PixelPath;

    public override string BossHeadTexture => this.GetPath() + "_Head";

    public override string HeadTexture => this.GetPath() + "_Head";

    public override void BossHeadSlot(ref int index)
    {
        if (NPC.Opacity < 0.2f)
        {
            index = -1;
        }
        else
        {
            index = NPCID.Sets.BossHeadTextures[Type];
        }
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
    {
        if (currentState == Behavior.reveal || hideBar || NPC.Opacity < 0.2f || CoreDeployed)
        {
            return false;
        }

        return base.DrawHealthBar(hbPosition, ref scale, ref position);
    }

    public override void SetStaticDefaults()
    {
        NPCID.Sets.MPAllowedEnemies[Type] = true;

        EmptinessSprayer.NPCsToNotDelete[Type] = true;
        RocheLimitGlobalNPC.ImmuneToLobotomy[Type] = true;
        NPCID.Sets.DoesntDespawnToInactivityAndCountsNPCSlots[Type] = true;
        // NPCID.Sets.MustAlwaysDrawHealthBar[Type] = true
    }

    public override void OnSpawn(IEntitySource source)
    {
        NPC.Opacity = 0;
        NPC.dontTakeDamage = true;
        //NPC.Center = Main.LocalPlayer.Center;
        hideBar = true;
        CreateLimbs();
        CreateLegs();
        Neck = new Rope(HeadPos, NPC.Center, 40, 2f, Vector2.UnitY);
        HeadPos = NPC.Center;
        neckStrandPositions = new List<(Vector2[], Vector2[])>(2);

        for (var i = 0; i < 2; i++)
        {
            neckStrandPositions.Add((new Vector2[2], new Vector2[2]));
        }

        NeckStrands = new List<Rope>(2);
        NeckStrands.Add(new Rope(NPC.Center, NPC.Center, 30, 2, Vector2.UnitY));
        NeckStrands.Add(new Rope(NPC.Center, NPC.Center, 30, 2, Vector2.UnitY));
    }

    public override void SetDefaults()
    {
        LegVerticies = new List<VertexPositionColorTexture[]>(2);
        LegVerticies.Add(new VertexPositionColorTexture[256]);
        LegVerticies.Add(new VertexPositionColorTexture[256]);
        legsVertCount.Add(0);
        legsVertCount.Add(0);

        legEffects = new List<BasicEffect>(2);
        legEffects.Add(null);
        legEffects.Add(null);
        wings.Add(new voidVultureWing(0, 0, 1, 0, 0));
        wings.Add(new voidVultureWing(0, 0, 1, 0, 0));
        TailPos = new Vector2[TailLength];
        TailVels = new Vector2[TailLength];
        ResetTail();
        NPC.Calamity().VulnerableToHeat = false;
        NPC.Calamity().VulnerableToElectricity = false;
        NPC.Calamity().VulnerableToSickness = false;
        NPC.Calamity().VulnerableToWater = false;
        NPC.Calamity().VulnerableToCold = true;
        NPC.Size = new Vector2(120, 120);
        NPC.noGravity = true;
        NPC.boss = true;
        NPC.lifeMax = 1_300_000;
        NPC.defense = 140;
        NPC.noTileCollide = true;
        NPC.knockBackResist = 0;
        NPC.damage = 200;
        NPC.BossBar = ModContent.GetInstance<VoidVultureBar>();
        //placeholder music by "holecatofficial" on discord 
        Music = MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/BirdUp");

        if (Main.netMode != NetmodeID.Server)
        {
            NPCNameFontSystem.RegisterFontForNPCID(Type, DisplayName.Value, Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/WINDLISTENERGRAPHIC", AssetRequestMode.ImmediateLoad).Value);
        }
    }

    public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
    {
        boundingBox = NPC.Hitbox;
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot)
    {
        if (currentState == Behavior.reveal || currentState == Behavior.debug || currentState == Behavior.Idle)
        {
            return false;
        }

        return base.CanHitPlayer(target, ref cooldownSlot);
    }

    public override void OnKill()
    {
        ModContent.GetInstance<FractalBird_Event>().hasBirdBeenDefeated = true;
    }

    public void ManageNeck()
    {
        // these two define the curve we animate
        var neckBase = NPC.Center + new Vector2(30 * -NPC.direction, 0);
        var neckHead = HeadPos;

        // ensure skeleton is updated
        Neck2.Skeleton.Update(neckBase, neckHead);

        // We use strand root positions along curve **instead of rope**
        for (var i = 0; i < neckStrandPositions.Count; i++)
        {
            float tStart;
            float tEnd;

            // These are the equivalent of "pick 1/2 down the neck" or "1/3 down the neck"
            if (i == 0)
            {
                tStart = 0.8f; // halfway down
                tEnd = 0.15f; // near the base
            }
            else // i == 1
            {
                tStart = 0.63f;
                tEnd = 0.10f;
            }

            var neckStart = SampleNeckCurve(Neck2.Skeleton, tStart);
            var neckEnd = SampleNeckCurve(Neck2.Skeleton, tEnd);

            // write new pos into your tuple storage
            neckStrandPositions[i].Item1[0] = neckStart;
            neckStrandPositions[i].Item2[0] = neckEnd;

            // init if missing
            if (NeckStrands[i] == null)
            {
                NeckStrands[i] = new Rope(neckStart, neckEnd, 30, 2, Vector2.UnitY);
            }

            // pin both ends
            NeckStrands[i].segments[0].position = neckStart;
            NeckStrands[i].segments[^1].position = neckEnd;

            NeckStrands[i].segments[0].pinned = true;
            NeckStrands[i].segments[^1].pinned = true;

            for (var x = 0; x < NeckStrands[i].segments.Length; x++)
            {
                NeckStrands[i].segments[x].velocity += NPC.velocity;
            }

            NeckStrands[i].Update();
        }
    }

    public void ManageTail()
    {
        if (TailPos == null)
        {
            return;
        }

        // head
        TailPos[0] = NPC.Center + new Vector2(40 * NPC.direction, 0);

        var minLerp = 0.15f; // normal softness when close
        var maxLerp = 0.55f; // strong pull when stretched
        var desiredDist = 12f;
        var maxDist = 40f; // after this, apply maxLerp

        for (var i = 1; i < TailPos.Length; i++)
        {
            TailVels[i] = new Vector2
            (
                0,
                MathF.Sin(Time / 10.1f) * i / 10.1f
            ).RotatedBy(TailPos[i].AngleTo(TailPos[i - 1]));

            TailVels[i].Y += 4f * Math.Clamp(NPC.velocity.Length(), 0.4f, 1);
            TailVels[i] += NPC.velocity * 0.2f;

            var dist = Vector2.Distance(TailPos[i], TailPos[i - 1]);

            // 0 when close, 1 when near maxDist
            var stretch = MathHelper.Clamp((dist - desiredDist) / (maxDist - desiredDist), 0f, 1f);

            // so tail gets pulled harder the further away it is
            var lerpPower = MathHelper.Lerp(minLerp, maxLerp, stretch);

            var target = TailPos[i - 1];
            TailPos[i] = Vector2.Lerp(TailPos[i], target, lerpPower);

            // Add motion influences
            TailPos[i] += TailVels[i];
        }
    }

    private void ResetTail()
    {
        for (var i = 0; i < TailLength; i++)
        {
            TailPos[i] = NPC.Center;
            TailVels[i] *= 0;
        }
    }

    private Vector2 SampleNeckCurve(IKSkeleton skel, float t)
    {
        var P0 = skel.Position(0);
        var P1 = skel.Position(1);
        var P2 = skel.Position(2);

        var C0 = P0 + (P1 - P0) * 0.5f;
        var C1 = P2 + (P1 - P2) * 0.5f;

        var u = 1f - t;

        return
            u * u * u * P0 +
            3 * u * u * t * C0 +
            3 * u * t * t * C1 +
            t * t * t * P2;
    }

    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        if (Staggered)
        {
            SoundEngine.PlaySound
            (
                GennedAssets.Sounds.Avatar.Phase2IntroNeckSnap with
                {
                    PitchVariance = 0.4f,
                    MaxInstances = 0
                }
            );
        }

        if (currentState == Behavior.placeholder2 && Time > 40)
        {
            Main.NewText(letGOcount);

            if (HitTimer <= 0)
            {
                letGOcount++;
                HitTimer = 50;
            }

            if (letGOcount > 6 && !Staggered)
            {
                Time = -1;
                letGOcount = 0;
                StaggerTimer = 180;
                currentState = Behavior.Idle;
            }
        }
    }

    public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
    {
        if (currentState == Behavior.Medusa && !Staggered)
        {
            modifiers.FinalDamage *= 1.2f;
        }

        if (Staggered)
        {
            modifiers.ArmorPenetration += AddableFloat.Zero + 30;
            modifiers.FinalDamage *= 2.6f;
        }

        base.ModifyIncomingHit(ref modifiers);
    }
}