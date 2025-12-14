using System.IO;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;
using NoxusBoss.Content.Particles.Metaballs;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.FleshlingCultist;

internal partial class FleshlingCultist : BloodMoonBaseNPC
{
    public static float BaseKnockback = 0.4f;

    public bool isWorshipping;

    private int spawnFrameStart = 0;

    private int spawnFrameEnd = 11;

    private readonly int IdleFrame = 10;

    private readonly int walkFrameStart = 11;

    private readonly int walkFrameEnd = 17;

    private readonly int worshipStartFrameStart = 18;

    private readonly int worshipStartFrameEnd = 22;

    private readonly int worshipLoopFrameStart = 23;

    private readonly int worshipLoopFrameEnd = 27;

    public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/FleshlingCultist/FleshlingCultist";

    public override float SacrificePrio => 1;

    public override int bloodBankMax => 30;

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(isWorshipping);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        isWorshipping = reader.ReadBoolean();
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange
        (
            [
                // Sets the preferred biomes of this town NPC listed in the bestiary.
                // With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
                // Sets your NPC's flavor text in the bestiary. (use localization keys)
                new FlavorTextBestiaryInfoElement("Mods.HeavenlyArsenal.Bestiary.FleshlingCultist1")

                //new FlavorTextBestiaryInfoElement("Mods.HeavenlyArsenal.Bestiary.ArtilleryCrab2")
            ]
        );
    }

    public override void SetStaticDefaults()
    {
        NPCID.Sets.ReflectStarShotsInForTheWorthy[Type] = true;

        Main.npcFrameCount[Type] = 28;

        ContentSamples.NpcBestiaryRarityStars[Type] = 4;
        NPCID.Sets.DoesntDespawnToInactivityAndCountsNPCSlots[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        NPC.lifeMax = 30_000;
        NPC.damage = 70;
        NPC.defense = 27;
        NPC.knockBackResist = 0.6f;
        NPC.Size = new Vector2(32, 50);
        NPC.HitSound = SoundID.NPCHit1;

        SpawnModBiomes =
        [
            ModContent.GetInstance<RiftEclipseBloodMoon>().Type
        ];
    }

    public override void OnSpawn(IEntitySource source)
    {
        blood = 0;
        CheckForEmptyCults();

        if (CultistCoordinator.GetCultOfNPC(NPC) != null)
        {
            CurrentState = Behaviors.Worship;
        }
        else
        {
            CurrentState = Behaviors.BlindRush;
        }
    }

    public override void AI()
    {
        StateMachine();

        //face towards the altar
        if (CurrentState != Behaviors.Worship)
        {
            NPC.direction = NPC.velocity.X != 0 ? Math.Sign(NPC.velocity.X) : 1;
        }
        else
        {
            var a = CultistCoordinator.GetCultOfNPC(NPC);

            if (a != null)
            {
                NPC.direction = Math.Sign(NPC.DirectionTo(a.Leader.Center).X);
            }
        }

        NPC.spriteDirection = -NPC.direction;

        if (Time % 120 == 0)
        {
            CheckForEmptyCults();
        }

        Time++;
    }

    private void CheckForEmptyCults()
    {
        if (CultistCoordinator.Cults.Count > 0)
        {
            foreach (var kvp in CultistCoordinator.Cults)
            {
                var cult = kvp.Value;

                if (cult.Leader.Center.Distance(NPC.Center) > 300)
                {
                    continue;
                }

                if (cult.Cultists.Count < cult.MaxCultists)
                {
                    CultistCoordinator.AttachToCult(cult.CultID, NPC);

                    break;
                }
            }
        }
    }

    public override void OnKill()
    {
        var RandomAbove = new Vector2(NPC.Center.X + Main.rand.NextFloat(-20, 20), NPC.Center.Y - 30);

        NPC.NewProjectileBetter(NPC.GetSource_Death(), NPC.Center, RandomAbove.AngleFrom(NPC.Center).ToRotationVector2() * 15, ModContent.ProjectileType<MaskProj>(), 0, 0);

        for (var i = 0; i < 20; i++)
        {
            var d = Dust.NewDustPerfect(NPC.Center + new Vector2(Main.rand.NextFloat(-20, 20), Main.rand.NextFloat(-20, 20)), DustID.Blood, Vector2.Zero, newColor: Color.Red);
            d.noGravity = true;
            d.scale = 1.5f;
        }

        var metaball = ModContent.GetInstance<BloodMetaball>();

        for (var i = 0; i < 4; i++)
        {
            var bloodSpawnPosition = NPC.Center;
            var bloodVelocity = (Main.rand.NextVector2Circular(30f, 30f) - NPC.velocity) * Main.rand.NextFloat(0.2f, 1.2f);
            metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(40f, 80f), 40);
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit)
    {
        blood += 2;
    }

    public override void FindFrame(int frameHeight)
    {
        if (isWorshipping)
        {
            if (NPC.localAI[0] < 1)
            {
                NPC.frameCounter += 0.2; // animation speed
                var totalStartFrames = worshipStartFrameEnd - worshipStartFrameStart + 1;

                if (NPC.frameCounter >= totalStartFrames)
                {
                    NPC.frameCounter = 0;
                    NPC.localAI[0] = 1; // mark start sequence as done
                }

                var frame = worshipStartFrameStart + (int)NPC.frameCounter;
                NPC.frame.Y = frame * frameHeight;
            }
            else
            {
                // Worship loop animation
                NPC.frameCounter += 0.2;
                var totalLoopFrames = worshipLoopFrameEnd - worshipLoopFrameStart + 1;

                if (NPC.frameCounter >= totalLoopFrames)
                {
                    NPC.frameCounter = 0;
                }

                var frame = worshipLoopFrameStart + (int)NPC.frameCounter;
                NPC.frame.Y = frame * frameHeight;
            }

            return; // Don't fall through to walking/idle logic
        }

        // Not worshipping — walking or idle
        if (Math.Abs(NPC.velocity.X) > 0.1f)
        {
            NPC.frameCounter += 0.2;
            var totalWalkFrames = walkFrameEnd - walkFrameStart + 1;

            if (NPC.frameCounter >= totalWalkFrames)
            {
                NPC.frameCounter = 0;
            }

            var frame = walkFrameStart + (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }
        else
        {
            NPC.frameCounter = 0;
            NPC.frame.Y = IdleFrame * frameHeight;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        /*
        string a = "";
        a += $"{CurrentState.ToString()}\n";
        if (CultistCoordinator.GetCultOfNPC(NPC) != null)
            a += $"CultID: {CultistCoordinator.GetCultOfNPC(NPC).CultID}\n";
        a += $"worshipping: {isWorshipping}\n";
        a += $"CanBeSacrificed: {this.canBeSacrificed}\n";
         if (!NPC.IsABestiaryIconDummy)
            Utils.DrawBorderString(spriteBatch, a, NPC.Center - screenPos, Color.AntiqueWhite, anchory:-1);
        */

        return base.PreDraw(spriteBatch, screenPos, drawColor);

        var TextureString = "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/FleshlingCultist/FleshlingCultist";
        var tex = ModContent.Request<Texture2D>(TextureString).Value;

        var DrawPos = NPC.Center - screenPos;

        var frame = tex.Frame();
        var Origin = frame.Size() * 0.5f + new Vector2(0, -1);

        var flip = NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        Main.EntitySpriteDraw(tex, DrawPos, null, drawColor, NPC.rotation, Origin, NPC.scale, flip);
        //string debug = "";
        //if (CultistCoordinator.GetCultOfNPC(NPC) != null)
        // {
        //     debug += $"CultID: {CultistCoordinator.GetCultOfNPC(NPC).CultID}\n";

        // }

        //debug += CurrentState.ToString() + $"\n";
        // Utils.DrawBorderString(spriteBatch, debug, NPC.Center - screenPos, Color.AntiqueWhite);

        return base.PreDraw(spriteBatch, screenPos, drawColor);
    }
}