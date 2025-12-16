using System.Collections.Generic;
using HeavenlyArsenal.Content.Biomes;
using Luminance.Assets;
using NoxusBoss.Assets;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;

public class Umbralarva : BloodMoonBaseNPC
{
    public NPC Mother;

    public Player Target;

    public override bool ResistantToTrueMelee => false;

    public override int bloodBankMax => 2000;

    public override bool canBeSacrificed => segmentNum == 0;

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange
        (
            new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
                new FlavorTextBestiaryInfoElement("Mods.HeavenlyArsenal.Bestiary.Umbralarva")
            }
        );
    }

    private void StateMachine()
    {
        switch (CurrentState)
        {
            case LarvaAi.TrackPlayer:
                HandleTrackPlayer();

                break;
            case LarvaAi.Lunge:
                HandleLungeAtPlayer();

                break;
            case LarvaAi.Spit:
                HandleSpit();

                break;
        }
    }

    private void HandleTrackPlayer()
    {
        if (Target != null && Target.dead)
        {
            Target = null;

            return;
        }

        //todo: if target is null and not dead, find closest player and set it to target.
        // then skip over that step.
        if (Target == null || !Target.active)
        {
            var closestDist = float.MaxValue;
            Player closestPlayer = null;

            for (var i = 0; i < Main.maxPlayers; i++)
            {
                var p = Main.player[i];

                if (p != null && p.active && !p.dead)
                {
                    var d = Vector2.Distance(NPC.Center, p.Center);

                    if (d < closestDist)
                    {
                        closestDist = d;
                        closestPlayer = p;
                    }
                }
            }

            if (closestPlayer != null)
            {
                Target = closestPlayer;
            }
            else
            {
                return;
            }
        }

        var toPlayer = Target.Center - NPC.Center;
        var dist = toPlayer.Length();

        if (dist > 8f)
        {
            toPlayer.Normalize();

            var desiredVel = toPlayer * HEAD_SPEED;

            var WiggleOffset = new Vector2(0, (float)Math.Sin(Time / 4 + Main.rand.NextFloat(0, 0.4f)) * 6).RotatedBy(NPC.rotation);
            NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVel + WiggleOffset, HEAD_ACCEL);
        }

        if (SpitCooldown <= 0 && dist < 480f && Main.rand.NextBool(120))
        {
            CurrentState = LarvaAi.Spit;
            Time = 0;

            return;
        }

        if (Main.rand.NextBool(76) && dist < 320f && LungeCooldown <= 0)
        {
            CurrentState = LarvaAi.Lunge;
            Time = 0;

            SoundEngine.PlaySound
            (
                AssetDirectory.Sounds.NPCs.Hostile.BloodMoon.UmbralLeech.Bash with
                {
                    PitchVariance = 0.2f,
                    MaxInstances = 0
                },
                NPC.Center
            );
        }
    }

    private void HandleLungeAtPlayer()
    {
        if (Target == null || !Target.active || Target.dead)
        {
            CurrentState = LarvaAi.TrackPlayer;

            return;
        }

        var LungeDirection = NPC.Center.AngleTo(Target.Center);

        var desiredVel = new Vector2((float)Math.Cos(LungeDirection), (float)Math.Sin(LungeDirection)) * HEAD_SPEED * 2.5f;
        var WiggleOffset = new Vector2(0, (float)Math.Sin(Time / 4 + Main.rand.NextFloat(0, 0.4f)) * 6).RotatedBy(NPC.rotation) * 2;
        NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVel + WiggleOffset, 0.1f);

        if (Time > LungeDurationMax)
        {
            CurrentState = LarvaAi.TrackPlayer;
            Time = 0;
            LungeCooldown = 360;
        }
    }

    private void HandleSpit()
    {
        var ShootDirection = NPC.Center.AngleTo(Target.Center).ToRotationVector2();

        var a = Projectile.NewProjectileDirect
        (
            Projectile.GetSource_NaturalSpawn(),
            NPC.Center,
            ShootDirection,
            ModContent.ProjectileType<BloodSpat>(),
            NPC.damage / 4,
            0
        );

        //NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, NPCID.VileSpitEaterOfWorlds, ai0: NPC.target, ai1: NPC.whoAmI);
        a.velocity = ShootDirection * 8f;

        SpitCooldown = 70;
        CurrentState = LarvaAi.TrackPlayer;
    }

    public override void AI()
    {
        var isHead = (int)segmentNum == 0;

        if (isHead)
        {
            HeadAI();

            if (LungeCooldown > 0)
            {
                LungeCooldown--;
            }

            if (SpitCooldown > 0)
            {
                SpitCooldown--;
            }
        }

        NPC.rotation = NPC.velocity.ToRotation();
    }

    public override void PostAI()
    {
        var isHead = (int)segmentNum == 0;

        // Keep the position history length reasonable: cap based on segmentCount
        if (isHead)
        {
            var headId = NPC.whoAmI;
            var needed = (int)(segmentCount * HISTORY_PER_SEGMENT) + 10;
            var hist = _headPositionHistory[headId];

            // push current position at front
            hist.Insert(0, NPC.Center);

            if (hist.Count > needed)
            {
                hist.RemoveRange(needed, hist.Count - needed);
            }
        }
        else
        {
            BodyAI();
        }
    }

    private void HeadAI()
    {
        StateMachine();
        Time++;
    }

    private void BodyAI()
    {
        var headId = NPC.realLife;

        // If realLife is invalid, fallback to following predecessor chain stored in ai[0]
        if (headId < 0 || headId >= Main.maxNPCs || !Main.npc[headId].active)
        {
            // fallback - if predecessor invalid, die gracefully
            var pred = (int)NPC.ai[0];

            if (pred < 0 || pred >= Main.maxNPCs || !Main.npc[pred].active)
            {
                NPC.active = false;
                NPC.netUpdate = true;

                return;
            }

            FollowPredecessor(pred);

            return;
        }

        // If there's no recorded history for the head yet, fallback to predecessor-follow
        if (!_headPositionHistory.ContainsKey(headId) || _headPositionHistory[headId].Count < 2)
        {
            var pred = (int)NPC.ai[0];
            FollowPredecessor(pred);

            return;
        }

        var hist = _headPositionHistory[headId];

        // distance behind the head this segment should be
        var distanceBehind = (int)segmentNum * SEGMENT_DISTANCE;

        // sample a point along the history at cumulative distance = distanceBehind
        Vector2 samplePoint;
        var found = SamplePointAlongHistory(hist, distanceBehind, out samplePoint);

        if (!found)
        {
            // not enough history length to find that far back -> use predecessor snap/follow
            var pred = (int)NPC.ai[0];
            FollowPredecessor(pred);

            return;
        }

        // set position & smooth velocity
        var old = NPC.Center;
        NPC.Center = samplePoint;
        NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.Center - old, 0.8f);
    }

    public override bool CheckActive()
    {
        return false;
    }

    public override void OnKill()
    {
        if ((int)segmentNum == 0)
        {
            var headId = NPC.whoAmI;

            if (_headPositionHistory.ContainsKey(headId))
            {
                _headPositionHistory.Remove(headId);
            }
        }
    }

    private void DrawLine(List<Vector2> list)
    {
        Texture2D texture = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
        var frame = texture.Frame();
        var origin = new Vector2(0f, 0.5f);

        var pos = list[0];

        for (var i = 0; i < list.Count - 1; i++)
        {
            var element = list[i];
            var diff = list[i + 1] - element;

            var rotation = diff.ToRotation();
            var color = Color.Crimson;
            var scale = new Vector2(diff.Length() + 2f, 2f);

            if (i == list.Count - 2)
            {
                scale.X -= 5f;
            }

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None);
            Utils.DrawBorderString(Main.spriteBatch, i.ToString(), pos - Main.screenPosition, Color.AntiqueWhite, 0.35f);
            pos += diff;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
        {
            return false;
        }

        var larva = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/Umbralarva").Value;
        var DrawPos = NPC.Center - Main.screenPosition;
        var value = segmentNum == 0 ? 0 : segmentNum == segmentCount ? 3 : 1;

        var larvaFrame = larva.Frame(4, 1, value);

        var Orig = new Vector2(larvaFrame.Width / 2, larvaFrame.Height / 2);

        Main.EntitySpriteDraw(larva, DrawPos, larvaFrame, drawColor, NPC.rotation, Orig, 1, SpriteEffects.FlipHorizontally);

        //Utils.DrawBorderString(Main.spriteBatch, segmentNum.ToString(), DrawPos, Color.AntiqueWhite);
        return false;
    }

    //pain in my ass 
    // Build debug info by scanning NPC list for segments that reference this head via realLife
    public (int headID, List<int> segmentNpcIds, List<Vector2> positions) GetWormDebugInfo()
    {
        var headID = NPC.whoAmI;
        var ids = new List<int>();
        var poss = new List<Vector2>();

        for (var i = 0; i < Main.maxNPCs; i++)
        {
            var n = Main.npc[i];

            if (n != null && n.active && n.realLife == headID)
            {
                ids.Add(i);
                poss.Add(n.Center);
            }
        }

        return (headID, ids, poss);
    }

    #region setup

    public enum LarvaAi
    {
        TrackPlayer,

        Lunge,

        Spit,

        StayNearMother
    }

    public LarvaAi CurrentState;

    private int LungeCooldown;

    private readonly int LungeDurationMax = 120;

    private int SpitCooldown;

    private readonly int MAX_SEGMENT_COUNT = 8;

    private int DEFAULT_SEGMENT_COUNT = 12;

    public float SEGMENT_DISTANCE = 9f;

    private readonly float HEAD_SPEED = 6.6f;

    private readonly float HEAD_ACCEL = 0.18f;

    private readonly int HISTORY_PER_SEGMENT = 16;

    /// <summary>
    ///     Maintains a mapping of head identifiers to their respective lists of recorded head positions.
    /// </summary>
    /// <remarks>
    ///     Each entry in the dictionary associates a unique head identifier (<see cref="int" />)
    ///     with a list of recorded positions (<see cref="Vector2" />), where the newest position is stored
    ///     at index 0.
    ///     This static storage is used to track runtime history of head positions without relying on
    ///     instance-specific
    ///     storage.
    /// </remarks>
    private static readonly Dictionary<int, List<Vector2>> _headPositionHistory = new();

    /// <summary>
    ///     A static flag to prevent the recursive spawning of child segments..
    /// </summary>
    private static bool _suppressOnSpawnSpawning;

    public ref float Time => ref NPC.ai[0];

    public ref float segmentCount => ref NPC.ai[1];

    public ref float segmentNum => ref NPC.ai[2];

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 1;
    }

    public override void SetDefaults()
    {
        NPC.width = 30;
        NPC.height = 30;
        NPC.lifeMax = 30000;
        NPC.damage = 160;
        NPC.defense = 201;
        NPC.npcSlots = 0.1f;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = AssetDirectory.Sounds.NPCs.Hostile.BloodMoon.UmbralLeech.DyingNoise;
        NPC.Size = new Vector2(20, 20);

        SpawnModBiomes =
        [
            ModContent.GetInstance<RiftEclipseBiome>().Type
        ];
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
    {
        if (segmentNum == 0)
        {
            return base.DrawHealthBar(hbPosition, ref scale, ref position);
        }

        return false;
    }

    public override void OnSpawn(IEntitySource source)
    {
        //todo: if this npc was spawned by another npc that isn't an umbral larva, set that npc as the mother.
        if (source is EntitySource_Parent parentSource && parentSource.Entity is NPC parentNpc && parentNpc.type != NPC.type)
        {
            Mother = parentNpc;
        }

        if (_suppressOnSpawnSpawning)
        {
            return;
        }

        // Only the head should spawn the chain stop it STOP IT
        if ((int)segmentNum != 0)
        {
            return;
        }

        if (segmentCount <= 0f)
        {
            segmentCount = Main.rand.Next(5, MAX_SEGMENT_COUNT + 1);
        }

        var segCount = (int)segmentCount;

        if (!_headPositionHistory.ContainsKey(NPC.whoAmI))
        {
            _headPositionHistory[NPC.whoAmI] = new List<Vector2>();
        }

        _suppressOnSpawnSpawning = true;

        var lastIndex = NPC.whoAmI;

        for (var i = 1; i <= segCount; i++)
        {
            var newIndex = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X + i * 4, (int)NPC.Center.Y + i * 4, NPC.type);

            if (newIndex < 0 || newIndex >= Main.maxNPCs)
            {
                continue;
            }

            var child = Main.npc[newIndex];

            child.ai[2] = i; // Segment num
            child.ai[1] = segCount; // segmentCount
            child.ai[0] = lastIndex; // predecessor whoAmI
            child.realLife = NPC.whoAmI; // reference to head
            child.netUpdate = true;

            lastIndex = newIndex;
        }

        _suppressOnSpawnSpawning = false;

        // sync to make sure supress doesn't break
        NPC.netUpdate = true;
    }

    #endregion

    #region Helpers

    // follow predecessor by pulling this NPC towards desired distance behind predecessor
    private void FollowPredecessor(int predIndex)
    {
        if (predIndex < 0 || predIndex >= Main.maxNPCs || !Main.npc[predIndex].active)
        {
            NPC.active = false;
            NPC.netUpdate = true;

            return;
        }

        var pred = Main.npc[predIndex];
        var desiredDist = SEGMENT_DISTANCE;
        var dir = NPC.Center - pred.Center;
        var curDist = dir.Length();

        if (curDist == 0f)
        {
            NPC.Center += new Vector2(0.01f, 0.01f);
            dir = NPC.Center - pred.Center;
            curDist = dir.Length();
        }

        var diff = curDist - desiredDist;

        if (Math.Abs(diff) > 0.01f)
        {
            dir /= curDist;
            NPC.Center -= dir * diff;
        }

        NPC.rotation = (NPC.Center - pred.Center).ToRotation() + MathHelper.PiOver2;
        NPC.velocity = Vector2.Lerp(NPC.velocity, pred.velocity, 0.9f);
    }

    // Walk the head position history (newest->oldest) looking for a point at exactly `distance` pixels
    // behind the head along the recorded polyline. Returns true if found and writes to `outPoint`.
    /// <summary>
    ///     attempts to sample a point along the provided history of positions at a specified cumulative
    ///     distance.
    /// </summary>
    /// <param name="historyNewestFirst"></param>
    /// <param name="distance"></param>
    /// <param name="outPoint"></param>
    /// <returns></returns>
    private bool SamplePointAlongHistory(List<Vector2> historyNewestFirst, float distance, out Vector2 outPoint)
    {
        outPoint = Vector2.Zero;

        if (historyNewestFirst == null || historyNewestFirst.Count < 2)
        {
            return false;
        }

        var accum = 0f;

        for (var i = 0; i < historyNewestFirst.Count - 1; i++)
        {
            var p0 = historyNewestFirst[i];
            var p1 = historyNewestFirst[i + 1];
            var segLen = Vector2.Distance(p0, p1);

            if (accum + segLen >= distance)
            {
                var need = distance - accum;

                if (segLen <= 0.0001f)
                {
                    outPoint = p0;
                }
                else
                {
                    var t = need / segLen; // 0..1
                    outPoint = Vector2.Lerp(p0, p1, t);
                }

                return true;
            }

            accum += segLen;
        }

        // If we exit loop, we don't have enough recorded distance. Use the oldest point as fallback.
        // Return false so caller can fallback to predecessor-follow (or place at end of history).
        return false;
    }

    #endregion
}

public class BloodSpat : ModProjectile
{
    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.hostile = true;
        Projectile.friendly = false;

        Projectile.width = Projectile.height = 14;
        Projectile.timeLeft = 300;

        Projectile.penetrate = 1;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = -1;
    }

    public override void AI()
    {
        var b = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Rain_BloodMoon, 0, 0, 0, Color.Crimson);

        var a = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Blood, 0, 0, 0, Color.Purple);

        a.velocity = Projectile.velocity;
        b.velocity = Projectile.velocity;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
}