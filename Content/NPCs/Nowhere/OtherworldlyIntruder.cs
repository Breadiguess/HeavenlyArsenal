using System.Collections.Generic;
using System.Linq;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.DataStructures;

//ignore that the usings are fucked up, I literally am writing this blind :sob:
//no, ignore that EVERYTHING is fucked up. AAAAAAAAAAAAAAAAAAA
namespace HeavenlyArsenal.Content.NPCs.Nowhere;

public class OtherworldlyIntruder : ModNPC
{
    #region Setup

    public enum IntruderState
    {
        EmergeFromPortal,

        Idle,

        LookAtInterest, //go, my abstract autistic son, pursue your fixation!

        ScareOff,

        Leave
    }

    public IntruderState CurrentState { get; set; }

    public float LeaveTime { get; private set; }

    public enum InterestType
    {
        None,

        Player,

        NPC,

        Projectile,

        Tile
    }

    public struct InterestData
    {
        public InterestType Type;

        public int Index; // index in corresponding array or tile ID in list

        public Point TilePos;

        public Vector2 Position =>
            Type == InterestType.Tile ? new Vector2(TilePos.X * 16, TilePos.Y * 16) :
            Type == InterestType.Player ? Main.player[Index].Center :
            Type == InterestType.NPC ? Main.npc[Index].Center :
            Type == InterestType.Projectile ? Main.projectile[Index].Center :
            Vector2.Zero;
    }

    public ref float Time => ref NPC.ai[0];

    public InterestData Interest;

    private static readonly List<ushort> ValidTileTypes = new()
    {
        TileID.WorkBenches // i don't currently remember the method of getting modded tiles, but in short the only ones its really gonna be interested in are the genesis and its assorted flowers.
    };

    /// <summary>
    ///     the position of the rift this intruder is hanging out of
    /// </summary>
    public Vector2 RiftPos { get; private set; }

    /// <summary>
    ///     Gets the anchor point for the tentacle's position in 2D space.
    /// </summary>
    public Vector2 TentacleAnchor { get; private set; }

    /// <summary>
    ///     used to dictate the angle the intruder should be looking towards.
    /// </summary>
    public float IntAngle;

    /// <summary>
    ///     The current alpha interpolant of the intruder.
    /// </summary>
    public float AlphaInterp;

    /// <summary>
    ///     how open the portal of this intruder is.
    /// </summary>
    public float PortalInterp;

    /// <summary>
    ///     How far out of the portal this intruder is hanging.
    /// </summary>
    public float HangoutInterp;

    /// <summary>
    ///     The intention of this float is to determine how quickly the intruder runs away, as well as
    ///     their tolerance of other npcs entities getting close to them.
    ///     Its on a scale of 0-1, where 0 is they're not scared at all (you can almost walk right up to
    ///     it), and 1 is the biggest coward you've ever met.
    /// </summary>
    public float Cowardice { get; private set; }

    private const int TendrilCount = 6;

    private TreeLimb rootLimb;

    private List<Mandible> mandibles;

    public override void SetStaticDefaults()
    {
        //DisplayName.SetDefault("???");
    }

    public override void OnSpawn(IEntitySource source)
    {
        base.OnSpawn(source);
    }

    public override void SetDefaults()
    {
        IntAngle = 90;
        NPC.width = 34;
        NPC.height = 48;
        NPC.damage = -1;
        NPC.defense = 999;
        NPC.lifeMax = 5;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.friendly = false;
        NPC.dontTakeDamageFromHostiles = false;
        NPC.immortal = true;
        NPC.knockBackResist = 0;
        // //=======\\
        LeaveTime = Main.rand.Next(600, 1200);
        Cowardice = Main.rand.NextFloat(0, 1);
        CurrentState = IntruderState.EmergeFromPortal;
        // \\=======//

        RiftPos = new Vector2(NPC.Center.X, NPC.Center.Y);

        // Create a root tree limb with depth 3, branch factor 2
        rootLimb = new TreeLimb(NPC, 40f, MathHelper.PiOver2, 3, 0.5f); // Initialize mandibles

        mandibles = new List<Mandible>
        {
            new(NPC, 30f, MathHelper.PiOver2 - MathHelper.ToRadians(30), 0),
            new(NPC, 30f, MathHelper.PiOver2 + MathHelper.ToRadians(30), 1)
        };
    }

    #endregion

    #region AI

    public override void AI()
    {
        NPC.direction = 1;

        if (Interest.Type == InterestType.None)
        {
            FindInterest();
        }

        StateMachine();
        HandleFear();

        rootLimb.Update(NPC.Center, IntAngle);

        foreach (var m in mandibles)
        {
            m.Update(IntAngle);
        }

        RiftPos = IntAngle.ToRotationVector2() * -10;
        TentacleAnchor = IntAngle.ToRotationVector2() * 10;

        Time++;
    }

    private void StateMachine()
    {
        switch (CurrentState)
        {
            case IntruderState.EmergeFromPortal:
                HandleEmerge();

                break;
            case IntruderState.Idle:
                HandleIdle();

                break;
            case IntruderState.LookAtInterest:
                HandleLookAtInterest();

                break;
            case IntruderState.ScareOff:
                HandleScareOff();

                break;
            case IntruderState.Leave:
                HandleLeave();

                break;
        }
    }

    private void HandleEmerge()
    {
        PortalInterp = float.Lerp(PortalInterp, 1, 0.125f);
        AlphaInterp = float.Lerp(AlphaInterp, 1, 0.125f);

        if (Time > 60)
        {
            Time = 0;
            CurrentState = IntruderState.Idle;
        }
    }

    private void HandleIdle()
    {
        if (Main.rand.NextBool(100))
        {
            CurrentState = IntruderState.LookAtInterest;
        }

        if (Time > LeaveTime)
        {
            Time = 0;
            //CurrentState = IntruderState.Leave;
        }
    }

    private void HandleLookAtInterest()
    {
        Time--;

        if (Interest.Position != Vector2.Zero)
            // Fix: Removed the null-coalescing operator '??' since Vector2 is a value type and cannot be null.
        {
            var toInterest = Interest.Position - NPC.Center;
            var angleToLook = toInterest.ToRotation();
            IntAngle = angleToLook;
        }
    }

    /// <summary>
    ///     called when my poor, precious child either gets too scared or gets hurt :(
    ///     how could you do this to my child?
    /// </summary>
    private void HandleScareOff()
    {
        if (Time > 10)
        {
            CurrentState = IntruderState.Leave;
            Time = 0;
        }
    }

    private void HandleLeave()
    {
        //todo: Lerp Back into portal
        HangoutInterp = float.Lerp(HangoutInterp, 0, 0.4f);
        AlphaInterp = float.Lerp(AlphaInterp, 0, 0.125f);

        if (HangoutInterp <= 0.4f)
        {
            PortalInterp = float.Lerp(PortalInterp, 0, 0.3f);
        }

        if (Time > 10)
        {
            NPC.active = false;
        }
    }

    private void FindInterest()
    {
        // 0: Player, 1: NPC, 2: Projectile, 3: Tile
        var choice = Main.rand.Next(4);

        switch (choice)
        {
            case 0:
                var activePlayers = Main.player.Where(p => p.active && !p.dead).ToArray();

                if (activePlayers.Length > 0)
                {
                    var idx = Main.rand.Next(activePlayers.Length);

                    Interest = new InterestData
                    {
                        Type = InterestType.Player,
                        Index = activePlayers[idx].whoAmI
                    };
                }

                break;
            case 1:
                var hostiles = Main.npc.Where(n => n.active && n.CanBeChasedBy() && !n.friendly).ToArray();

                if (hostiles.Length > 0)
                {
                    var idx = Main.rand.Next(hostiles.Length);

                    Interest = new InterestData
                    {
                        Type = InterestType.NPC,
                        Index = hostiles[idx].whoAmI
                    };
                }

                break;
            case 2:
                var projs = Main.projectile.Where(pr => pr.active && !pr.hostile).ToArray();

                if (projs.Length > 0)
                {
                    var idx = Main.rand.Next(projs.Length);

                    Interest = new InterestData
                    {
                        Type = InterestType.Projectile,
                        Index = projs[idx].whoAmI
                    };
                }

                break;
            case 3:
                // Find valid tiles within 100 tiles of the NPC
                var foundTiles = new List<Point>();
                var centerTile = NPC.Center.ToTileCoordinates();
                var radius = 100;

                for (var x = centerTile.X - radius; x <= centerTile.X + radius; x++)
                {
                    if (x < 0 || x >= Main.maxTilesX)
                    {
                        continue;
                    }

                    for (var y = centerTile.Y - radius; y <= centerTile.Y + radius; y++)
                    {
                        if (y < 0 || y >= Main.maxTilesY)
                        {
                            continue;
                        }

                        var tile = Main.tile[x, y];

                        if (tile != null && tile.HasTile && ValidTileTypes.Contains(tile.TileType))
                        {
                            foundTiles.Add(new Point(x, y));
                        }
                    }
                }

                if (foundTiles.Count > 0)
                {
                    var chosen = foundTiles[Main.rand.Next(foundTiles.Count)];

                    Interest = new InterestData
                    {
                        Type = InterestType.Tile,
                        TilePos = chosen
                    };
                }

                break;
        }
    }

    /// <summary>
    ///     Runs every so often to check if the intruder should run away.
    ///     takes cowardice into account. if the object they're interested in gets too close, be afraid!
    /// </summary>
    private void HandleFear()
    {
        if (CurrentState == IntruderState.ScareOff)
        {
            return;
        }

        var fearScore = 0f;

        // Count hostile NPCs
        var hostileCount = Main.npc.Count(n => n.active && n.CanBeChasedBy() && !n.friendly);
        fearScore += hostileCount * 0.05f;

        // Distance to all players
        foreach (var player in Main.player)
        {
            if (player != null && player.active && !player.dead)
            {
                var playerDist = Vector2.Distance(NPC.Center, player.Center);
                fearScore += MathHelper.Clamp(1f - playerDist / 600f, 0f, 1f) * 0.4f;
            }
        }

        // Distance to interest
        if (Interest.Type != 0)
        {
            var interestDist = Vector2.Distance(NPC.Center, Interest.Position);
            fearScore += MathHelper.Clamp(1f - interestDist / 400f, 0f, 1f) * 0.3f;
        }

        // Check for summoner projectiles nearby
        foreach (var proj in Main.projectile)
        {
            if (proj.active && (proj.minion || proj.sentry) && proj.timeLeft > 5 && Vector2.Distance(NPC.Center, proj.Center) < 500f)
            {
                fearScore += 0.3f;

                break;
            }
        }

        // Time of day adjustment
        fearScore += Main.dayTime ? 0.2f : -0.2f;

        // Adjust by cowardice
        fearScore *= Cowardice;

        if (fearScore > 1f)
        {
            Time = 0;
            CurrentState = IntruderState.ScareOff;
        }
        //Main.NewText($"Fear score: {fearScore}");
    }

    #endregion

    #region DrawCode From hell

    private void DrawRift(SpriteBatch spriteBatch)
    {
        var DrawPos = NPC.Center - RiftPos - Main.screenPosition;
        var Rotation = float.Lerp(MathHelper.ToRadians(-90), IntAngle, 0f); //IntAngle;//(NPC.Center - RiftPos).ToRotation()*10;
        var glow = AssetDirectory.Textures.BigGlowball.Value;

        var offset = NPC.velocity.SafeNormalize(Vector2.Zero);
        Main.spriteBatch.End();

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        Main.EntitySpriteDraw
        (
            glow,
            DrawPos,
            glow.Frame(),
            Color.MediumPurple with
            {
                A = 200
            },
            Rotation,
            glow.Size() * 0.5f,
            new Vector2(0.12f, 0.25f) / 2 * PortalInterp,
            0
        );

        Texture2D innerRiftTexture = GennedAssets.Textures.Noise.TechyNoise; //AssetDirectory.Textures.VoidLake.Value;
        var edgeColor = Color.MediumPurple; // new Color(0.4f, 0.06f, 0.06f);
        var timeOffset = Main.myPlayer * 2.5552343f;

        var riftShader = ShaderManager.GetShader("HeavenlyArsenal.DarkPortalShader");
        riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.08f + timeOffset);
        riftShader.TrySetParameter("baseCutoffRadius", 0.3f);
        riftShader.TrySetParameter("swirlOutwardnessExponent", 0.2f);
        riftShader.TrySetParameter("swirlOutwardnessFactor", 3f);
        riftShader.TrySetParameter("vanishInterpolant", 0.01f);
        riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
        riftShader.TrySetParameter("edgeColorBias", 0.01f);
        riftShader.SetTexture(GennedAssets.Textures.Noise.DendriticNoiseZoomedOut, 1, SamplerState.AnisotropicWrap);
        riftShader.SetTexture(GennedAssets.Textures.Noise.FireNoiseA, 2, SamplerState.AnisotropicWrap);
        riftShader.Apply();

        Main.spriteBatch.Draw
        (
            innerRiftTexture,
            DrawPos,
            null,
            Color.Aquamarine with
            {
                A = 200
            },
            Rotation,
            innerRiftTexture.Size() * 0.5f,
            new Vector2(0.2f, 0.4f) / 2 * PortalInterp,
            0,
            0
        );

        Main.spriteBatch.End();

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
    }

    private void DrawTalisman() { }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        DrawRift(spriteBatch);
        var DrawPos = NPC.Center - Main.screenPosition;

        rootLimb.Draw(spriteBatch, screenPos);

        foreach (var m in mandibles)
        {
            m.Draw(spriteBatch, screenPos);
        }

        Utils.DrawBorderString(Main.spriteBatch, "time: " + Time, DrawPos - Vector2.UnitY * 100, Color.AntiqueWhite);

        Utils.DrawBorderString(Main.spriteBatch, "Interest: " + Interest.Type + ", Cowardice: " + Cowardice, DrawPos - Vector2.UnitY * 120, Color.AntiqueWhite);
        Utils.DrawBorderString(Main.spriteBatch, "State: " + CurrentState + ", IntruderAngle: " + IntAngle, DrawPos - Vector2.UnitY * 140, Color.AntiqueWhite);

        return false;
    }

    #endregion

    #region Hit Responses

    public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
    {
        Time = 0;

        CurrentState = IntruderState.ScareOff;
    }

    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        Time = 0;
        CurrentState = IntruderState.ScareOff;
    }

    public override void PartyHatPosition(ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        // maybe implement later for the funnies
    }

    public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        modifiers.SetMaxDamage(1);
        modifiers.HideCombatText();
    }

    public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        modifiers.SetMaxDamage(1);
        modifiers.HideCombatText();
    }

    #endregion
}

public class TreeLimb
{
    private readonly float length;

    private float baseAngle;

    private readonly float angleOffset;

    private float branchAngle;

    private readonly List<TreeLimb> children;

    public List<(Vector2 Start, Vector2 End)> Segments { get; } = new();

    /// <summary>
    ///     Constructs a TreeLimb node. angleOffset offsets branch relative to target angle.
    /// </summary>
    public TreeLimb(NPC parent, float length, float initialAngle, int depth, float branchAngle, float angleOffset = 0f)
    {
        this.length = length;
        baseAngle = initialAngle;
        this.branchAngle = branchAngle;
        this.angleOffset = angleOffset;
        children = new List<TreeLimb>();

        if (depth > 0)
        {
            // branches: offset from parent's targetAngle
            children.Add(new TreeLimb(parent, length * 0.7f, initialAngle + branchAngle, depth - 1, branchAngle, branchAngle));
            children.Add(new TreeLimb(parent, length * 0.7f, initialAngle - branchAngle, depth - 1, branchAngle, -branchAngle));
        }
    }

    public void Update(Vector2 origin, float targetAngle)
    {
        // desired angle includes branch offset
        var desired = targetAngle + angleOffset;
        // smoothly interpolate toward desired
        baseAngle = MathHelper.Lerp(baseAngle, desired, 0.5f);
        // compute endpoint
        var endPoint = origin + baseAngle.ToRotationVector2() * length;
        Segments.Clear();
        Segments.Add((origin, endPoint));

        // update children
        foreach (var child in children)
        {
            child.Update(endPoint, targetAngle);
            Segments.AddRange(child.Segments);
        }
    }

    public void Draw(SpriteBatch sb, Vector2 screenPos)
    {
        foreach (var seg in Segments)
        {
            Utils.DrawLine
            (
                sb,
                seg.Start,
                seg.End,
                Color.Black,
                Color.White,
                4f
            );
        }
    }
}

public class Mandible
{
    private readonly NPC parent;

    private readonly float length;

    private readonly float baseAngle;

    private float addedRotation;

    private readonly int Index;

    public Vector2 End { get; private set; }

    public Mandible(NPC parent, float length, float baseAngle, int index)
    {
        this.parent = parent;
        this.length = length;
        this.baseAngle = baseAngle;
        Index = index;
        addedRotation = 90f;
    }

    public void AddRotation(float delta)
    {
        addedRotation += delta;
    }

    public void ResetRotation()
    {
        addedRotation = 0f;
    }

    public void Update(float targetAngle)
    {
        // Only 10% influence toward target (stiffer)
        var angle = baseAngle + (targetAngle - baseAngle) * 0.1f + addedRotation;
        addedRotation *= 0.7f;

        End = parent.Center + angle.ToRotationVector2() * length;
    }

    public void Draw(SpriteBatch sb, Vector2 screenPos)
    {
        var tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Nowhere/IntruderMandible").Value;

        var drawPos = parent.Center - screenPos;
        var origin = new Vector2(tex.Width / 2, tex.Height);

        var Mandibles = Index == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        var rot = parent.Center.AngleTo(End);
        Main.EntitySpriteDraw(tex, drawPos, null, Color.White, rot + addedRotation, origin, 1f, Mandibles);
        //Utils.DrawLine(sb,parent.Center, End, Color.CornflowerBlue);
    }
}