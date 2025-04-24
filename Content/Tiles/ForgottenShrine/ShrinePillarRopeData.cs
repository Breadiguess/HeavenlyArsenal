using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Tiles.Generic;
using Luminance.Assets;
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
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class ShrinePillarRopeData : WorldOrientedTileObject
{
    private Point end;

    private static readonly Asset<Texture2D> beadsTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/ShrineRopeBeads");

    /// <summary>
    /// The amount of beads this rope should have.
    /// </summary>
    public int BeadCount
    {
        get;
        set;
    }

    /// <summary>
    /// A general purpose identifier number used for this rope for RNG determinations.
    /// </summary>
    public int ID
    {
        get;
        set;
    }

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
    /// The amount of gravity imposed on this rope.
    /// </summary>
    public static float Gravity => 0.5f;

    public ShrinePillarRopeData() { }

    public ShrinePillarRopeData(Point start, Point end, int beadCount, float sag)
    {
        Vector2 startVector = start.ToVector2();
        Vector2 endVector = end.ToVector2();
        BeadCount = beadCount;
        Sag = sag;
        ID = Main.rand.Next();

        Position = start;
        this.end = end;

        MaxLength = Rope.CalculateSegmentLength(Vector2.Distance(Start.ToVector2(), End.ToVector2()), Sag);

        int segmentCount = 30;
        VerletRope = new Rope(startVector, endVector, segmentCount, MaxLength / segmentCount, Vector2.UnitY * Gravity, 12)
        {
            tileCollide = true
        };
    }

    /// <summary>
    /// Updates this rope.
    /// </summary>
    public override void Update()
    {
        for (int i = 0; i < VerletRope.segments.Length; i++)
        {
            Rope.RopeSegment ropeSegment = VerletRope.segments[i];
            if (ropeSegment.pinned)
                continue;

            foreach (Player player in Main.ActivePlayers)
            {
                float playerProximityInterpolant = LumUtils.InverseLerp(50f, 10f, player.Distance(ropeSegment.position));
                ropeSegment.position += player.velocity * playerProximityInterpolant * 0.4f;
            }
        }

        VerletRope.Update();
    }

    private void DrawProjectionButItActuallyWorks(Texture2D projection, Vector2 drawOffset, bool flipHorizontally, Func<float, Color> colorFunction, int? projectionWidth = null, int? projectionHeight = null, float widthFactor = 1f, bool unscaledMatrix = false)
    {
        List<Vector2> positions = [.. VerletRope.segments.Select((Rope.RopeSegment r) => r.position)];
        positions.Add(End.ToVector2());
        ManagedShader overlayShader = ShaderManager.GetShader("HeavenlyArsenal.LitPrimitiveOverlayShader");
        overlayShader.TrySetParameter("exposure", 1f);
        overlayShader.TrySetParameter("screenSize", WotGUtils.ViewportSize);
        overlayShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        overlayShader.SetTexture(MiscTexturesRegistry.Pixel.Value, 1, SamplerState.LinearClamp);
        overlayShader.SetTexture(LightingMaskTargetManager.LightTarget, 2);
        overlayShader.Apply();

        PrimitiveSettings settings = new PrimitiveSettings((float _) => projection.Width * widthFactor, colorFunction.Invoke, (float _) => drawOffset + Main.screenPosition, Smoothen: true, Pixelate: false, overlayShader, projectionWidth, projectionHeight, unscaledMatrix);
        PrimitiveRenderer.RenderTrail(positions, settings, 36);
    }

    /// <summary>
    /// Renders this rope.
    /// </summary>
    public override void Render()
    {
        static Color ropeColorFunction(float completionRatio) => new Color(255, 28, 58);
        DrawProjectionButItActuallyWorks(MiscTexturesRegistry.Pixel.Value, -Main.screenPosition, false, ropeColorFunction, widthFactor: 2f);

        if (BeadCount >= 1)
        {
            UnifiedRandom rng = new UnifiedRandom(ID);
            DeCasteljauCurve positionCurve = new DeCasteljauCurve(VerletRope.GetPoints());
            Texture2D beadTexture = beadsTexture.Value;
            for (int i = 0; i < BeadCount; i++)
            {
                float positionInterpolant = MathHelper.SmoothStep(0.25f, 0.75f, i / (float)(BeadCount - 1f));
                if (BeadCount == 1)
                    positionInterpolant = 0.5f;

                int frameY = rng.Next(3);
                Rectangle frame = beadsTexture.Frame(1, 3, 0, frameY);
                Vector2 beadWorldPosition = positionCurve.Evaluate(positionInterpolant);
                Vector2 drawPosition = beadWorldPosition - Main.screenPosition;
                float beadRotation = beadWorldPosition.AngleTo(positionCurve.Evaluate(positionInterpolant + 0.001f));
                Main.spriteBatch.Draw(beadTexture, drawPosition, frame, Lighting.GetColor(beadWorldPosition.ToTileCoordinates()), beadRotation, frame.Size() * 0.5f, 0.5f, 0, 0f);
            }
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
            ["BeadCount"] = BeadCount,
            ["MaxLength"] = MaxLength,
            ["ID"] = ID,
            ["RopePositions"] = VerletRope.segments.Select(p => p.position.ToPoint()).ToList()
        };
    }

    /// <summary>
    /// Deserializes a tag compound containing data for a rope back into said rope.
    /// </summary>
    public override ShrinePillarRopeData Deserialize(TagCompound tag)
    {
        ShrinePillarRopeData rope = new ShrinePillarRopeData(tag.Get<Point>("Start"), tag.Get<Point>("End"), tag.GetInt("BeadCount"), tag.GetFloat("Sag"))
        {
            MaxLength = tag.GetFloat("MaxLength"),
            ID = tag.GetInt("ID")
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
