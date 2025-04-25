using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Content.Tiles.Generic;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.Graphics.LightingMask;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class ShimenawaRopeData : WorldOrientedTileObject
{
    public class ShimenawaRopeOrnament
    {
        /// <summary>
        /// How many frames it's been since this ornament was last interacted with.
        /// </summary>
        public int InteractionTimer
        {
            get;
            set;
        }

        /// <summary>
        /// The position of this ornament.
        /// </summary>
        public Vector2 Position
        {
            get;
            internal set;
        }

        /// <summary>
        /// The texture associated with this orament.
        /// </summary>
        public Asset<Texture2D> TextureAsset
        {
            get;
            private set;
        }

        /// <summary>
        /// A 0-1 interpolant which dictates how far along the rope this ornament should be placed.
        /// </summary>
        public float PositionInterpolant
        {
            get;
            set;
        }

        /// <summary>
        /// The current rotation of this ornament.
        /// </summary>
        public float Rotation
        {
            get;
            set;
        }

        /// <summary>
        /// The angular velocity of this ornament.
        /// </summary>
        public float AngularVelocity
        {
            get;
            set;
        }

        /// <summary>
        /// The scale of this ornament.
        /// </summary>
        public float Scale
        {
            get;
            set;
        }

        /// <summary>
        /// An optional action that should happen when this ornament is interacted with.
        /// </summary>
        public Action<ShimenawaRopeOrnament, Player, float>? InteractionAction
        {
            get;
            set;
        }

        public ShimenawaRopeOrnament(Asset<Texture2D> texture, float positionInterpolant, float scale, Action<ShimenawaRopeOrnament, Player, float> interactionAction)
        {
            TextureAsset = texture;
            PositionInterpolant = positionInterpolant;
            InteractionAction = interactionAction;
            Scale = scale;
        }

        public void Update()
        {
            // An ODE of the form d^2u/dt^2 = -au (aka acceleration being determined by the negative of current angle) results in oscillating sinusoidal motion, which is
            // ideal for swaying rope ornaments that respond to the environment.
            AngularVelocity -= Rotation * 0.015f;

            AngularVelocity *= 0.96f - MathF.Abs(Rotation * 0.08f);
            Rotation += AngularVelocity;
            InteractionTimer++;
        }

        /// <summary>
        /// Renders this ornament on the rope.
        /// </summary>
        /// <param name="drawPosition">The draw position of the rope.</param>
        /// <param name="color">The color of rope.</param>
        public void Render(Vector2 drawPosition, Color color)
        {
            Texture2D texture = TextureAsset.Value;
            Main.spriteBatch.Draw(texture, drawPosition, null, color, Rotation, texture.Size() * new Vector2(0.5f, 0f), Scale, 0, 0f);
        }
    }

    private Point end;

    private static readonly Asset<Texture2D> bellTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/Bell");

    private static readonly Asset<Texture2D> ropeTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/Rope");

    private static readonly Asset<Texture2D> shideTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/Shide");

    /// <summary>
    /// The amount by which this rope should sag when completely at rest.
    /// </summary>
    public float Sag
    {
        get;
        set;
    }

    /// <summary>
    /// The starting position of the rope.
    /// </summary>
    public Point Start => Position;

    /// <summary>
    /// The end position of the rope.
    /// </summary>
    public Point End
    {
        get => end;
        set
        {
            end = value;
            Vector2 endVector = end.ToVector2();
            VerletRope.segments[^1].position = endVector;
            VerletRope.segments[^1].oldPosition = endVector;
        }
    }

    /// <summary>
    /// The verlet segments associated with this rope.
    /// </summary>
    public readonly Rope VerletRope;

    /// <summary>
    /// The maximum length of this rope.
    /// </summary>
    public float MaxLength
    {
        get;
        private set;
    }

    /// <summary>
    /// The set of ornaments on this rope.
    /// </summary>
    public ShimenawaRopeOrnament[] Ornaments =
    [
        new ShimenawaRopeOrnament(shideTexture, 0.15f, 0.6f, StandardOrnamentInteraction),
        new ShimenawaRopeOrnament(shideTexture, 0.33f, 0.6f, StandardOrnamentInteraction),
        new ShimenawaRopeOrnament(bellTexture, 0.5f, 0.76f, BellOrnamentInteraction),
        new ShimenawaRopeOrnament(shideTexture, 0.67f, 0.6f, StandardOrnamentInteraction),
        new ShimenawaRopeOrnament(shideTexture, 0.85f, 0.6f, StandardOrnamentInteraction),
    ];

    /// <summary>
    /// The amount of gravity imposed on this rope.
    /// </summary>
    public static float Gravity => 0.5f;

    public ShimenawaRopeData() { }

    public ShimenawaRopeData(Point start, Point end, float sag)
    {
        Vector2 startVector = start.ToVector2();
        Vector2 endVector = end.ToVector2();
        Sag = sag;

        Position = start;
        this.end = end;

        MaxLength = Rope.CalculateSegmentLength(Vector2.Distance(Start.ToVector2(), End.ToVector2()), Sag);

        int segmentCount = 30;
        VerletRope = new Rope(startVector, endVector, segmentCount, MaxLength / segmentCount, Vector2.UnitY * Gravity, 12)
        {
            tileCollide = true
        };
    }

    private static void StandardOrnamentInteraction(ShimenawaRopeOrnament ornament, Player player, float playerProximityInterpolant)
    {
        ornament.AngularVelocity -= player.velocity.X * playerProximityInterpolant * 0.0021f;
    }

    private static void BellOrnamentInteraction(ShimenawaRopeOrnament ornament, Player player, float playerProximityInterpolant)
    {
        StandardOrnamentInteraction(ornament, player, playerProximityInterpolant);

        if (ornament.InteractionTimer >= 45)
        {
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0f, MaxInstances = 3, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew }, ornament.Position);

            for (int i = -1; i <= 1; i += 2)
            {
                CartoonWaveParticle wave = CartoonWaveParticle.Pool.RequestParticle();
                wave.Prepare(ornament.Position + Vector2.UnitY * 12f, Vector2.UnitX * i * 6f, 54, new Color(255, 175, 50), Vector2.One * 0.65f);
                ParticleEngine.Particles.Add(wave);
            }
        }
    }

    /// <summary>
    /// Updates this rope.
    /// </summary>
    public override void Update()
    {
        // Only do tile collision checks if a player is close, to save on performance.
        Vector2 segmentCenter = VerletRope.segments[VerletRope.segments.Length / 2].position;
        bool playerNearby = Main.player[Player.FindClosest(segmentCenter, 1, 1)].WithinRange(segmentCenter, 1900f);
        VerletRope.tileCollide = !playerNearby;

        for (int i = 0; i < VerletRope.segments.Length; i++)
        {
            Rope.RopeSegment ropeSegment = VerletRope.segments[i];
            if (ropeSegment.pinned)
                continue;

            foreach (Player player in Main.ActivePlayers)
            {
                float playerProximityInterpolant = LumUtils.InverseLerp(30f, 10f, player.Distance(ropeSegment.position));
                ropeSegment.position += player.velocity * playerProximityInterpolant * 0.24f;
            }
        }

        DeCasteljauCurve ropeCurve = new DeCasteljauCurve(VerletRope.GetPoints());
        foreach (ShimenawaRopeOrnament ornament in Ornaments)
        {
            Vector2 ornamentPosition = ropeCurve.Evaluate(ornament.PositionInterpolant);
            ornament.Position = ornamentPosition;
            Vector2 top = ornamentPosition;
            Vector2 bottom = ornamentPosition + Vector2.UnitY.RotatedBy(ornament.Rotation) * ornament.TextureAsset.Height() * ornament.Scale;

            foreach (Player player in Main.ActivePlayers)
            {
                float playerProximityInterpolantTop = LumUtils.InverseLerp(45f, 10f, player.Distance(top));
                float playerProximityInterpolantBottom = LumUtils.InverseLerp(45f, 10f, player.Distance(bottom));
                float playerProximityInterpolant = MathF.Max(playerProximityInterpolantTop, playerProximityInterpolantBottom);
                if (playerProximityInterpolant > 0f && player.velocity.Length() >= 1f)
                {
                    ornament.InteractionAction(ornament, player, playerProximityInterpolant);
                    ornament.InteractionTimer = 0;
                }
            }
            ornament.Update();
        }

        VerletRope.Update();
    }

    private void DrawProjectionButItActuallyWorks(Texture2D projection, Vector2 drawOffset, bool flipHorizontally, Func<float, Color> colorFunction, int? projectionWidth = null, int? projectionHeight = null, float widthFactor = 1f, bool unscaledMatrix = false)
    {
        List<Vector2> positions = [.. VerletRope.segments.Select((Rope.RopeSegment r) => r.position)];
        positions.Add(End.ToVector2());
        ManagedShader overlayShader = ShaderManager.GetShader("HeavenlyArsenal.ShimenawaRopeShader");
        overlayShader.TrySetParameter("screenSize", WotGUtils.ViewportSize);
        overlayShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        overlayShader.SetTexture(projection, 1, SamplerState.PointWrap);
        overlayShader.SetTexture(LightingMaskTargetManager.LightTarget, 2);
        overlayShader.Apply();

        PrimitiveSettings settings = new PrimitiveSettings((float _) => projection.Width * widthFactor * 0.25f, colorFunction.Invoke, (float _) => drawOffset + Main.screenPosition, Smoothen: true, Pixelate: false, overlayShader, projectionWidth, projectionHeight, unscaledMatrix);
        PrimitiveRenderer.RenderTrail(positions, settings, 40);
    }

    /// <summary>
    /// Renders this rope.
    /// </summary>
    public override void Render()
    {
        DrawProjectionButItActuallyWorks(ropeTexture.Value, -Main.screenPosition, false, _ => Color.White, unscaledMatrix: true);
        DeCasteljauCurve ropeCurve = new DeCasteljauCurve(VerletRope.GetPoints());
        foreach (ShimenawaRopeOrnament ornament in Ornaments)
        {
            Vector2 ornamentPosition = ropeCurve.Evaluate(ornament.PositionInterpolant);
            Color color = Lighting.GetColor(ornamentPosition.ToTileCoordinates());
            ornament.Render(ornamentPosition - Main.screenPosition, color);
        }
    }

    /// <summary>
    /// Serializes this rope data as a tag compound for world saving.
    /// </summary>
    public override TagCompound Serialize()
    {
        return new TagCompound()
        {
            ["Start"] = Start,
            ["End"] = End,
            ["Sag"] = Sag,
            ["MaxLength"] = MaxLength,
            ["RopePositions"] = VerletRope.segments.Select(p => p.position.ToPoint()).ToList()
        };
    }

    /// <summary>
    /// Deserializes a tag compound containing data for a rope back into said rope.
    /// </summary>
    public override ShimenawaRopeData Deserialize(TagCompound tag)
    {
        ShimenawaRopeData rope = new ShimenawaRopeData(tag.Get<Point>("Start"), tag.Get<Point>("End"), tag.GetFloat("Sag"))
        {
            MaxLength = tag.GetFloat("MaxLength")
        };
        Vector2[] ropePositions = [.. tag.Get<Point[]>("RopePositions").Select(p => p.ToVector2())];

        rope.VerletRope.segments = new Rope.RopeSegment[ropePositions.Length];
        for (int i = 0; i < ropePositions.Length; i++)
        {
            bool locked = i == 0 || i == ropePositions.Length - 1;
            rope.VerletRope.segments[i] = new Rope.RopeSegment(ropePositions[i]);
            rope.VerletRope.segments[i].pinned = locked;
        }

        return rope;
    }
}
