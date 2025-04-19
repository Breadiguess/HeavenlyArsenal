using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.Physics.VerletIntergration;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class ShrineRopeData
{
    private Point end;

    private static readonly Asset<Texture2D> spiralTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/RopeSpiral");

    /// <summary>
    /// A general-purpose timer used for wind movement on the baubles attached to this rope.
    /// </summary>
    public float WindTime
    {
        get;
        set;
    }

    /// <summary>
    /// The starting position of the rope.
    /// </summary>
    public readonly Point Start;

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
            ClampToMaxLength(ref endVector);

            VerletRope.Rope[^1].Position = endVector;
            VerletRope.Rope[^1].OldPosition = endVector;
        }
    }

    /// <summary>
    /// The verlet segments associated with this rope.
    /// </summary>
    public readonly VerletSimulatedRope VerletRope;

    /// <summary>
    /// The maximum length of ropes.
    /// </summary>
    public static float MaxLength => 1400f;

    /// <summary>
    /// The amount of gravity imposed on this rope.
    /// </summary>
    public static float Gravity => 0.65f;

    public ShrineRopeData(Point start, Point end)
    {
        Vector2 startVector = start.ToVector2();
        Vector2 endVector = end.ToVector2();

        VerletRope = new VerletSimulatedRope(startVector, Vector2.Zero, 35, MaxLength * 0.22f);
        Start = start;
        End = end;

        VerletRope.Rope[0].Position = startVector;
        VerletRope.Rope[0].OldPosition = startVector;
        VerletRope.Rope[0].Locked = true;

        VerletRope.Rope[^1].Position = endVector;
        VerletRope.Rope[^1].OldPosition = endVector;
        VerletRope.Rope[^1].Locked = true;
    }

    private void ClampToMaxLength(ref Vector2 end)
    {
        Vector2 startVector = Start.ToVector2();
        if (!end.WithinRange(startVector, MaxLength))
            end = startVector + (end - startVector).SafeNormalize(Vector2.Zero) * MaxLength;
    }

    /// <summary>
    /// Updates this rope.
    /// </summary>
    public void Update_Standard()
    {
        bool startHasNoTile = !Framing.GetTileSafely(Start.ToVector2().ToTileCoordinates()).HasTile;
        bool endHasNoTile = !Framing.GetTileSafely(End.ToVector2().ToTileCoordinates()).HasTile;
        if (startHasNoTile || endHasNoTile)
        {
            ShrineRopeSystem.Remove(this);
            return;
        }

        for (int i = 0; i < VerletRope.Rope.Count; i++)
        {
            VerletSimulatedSegment ropeSegment = VerletRope.Rope[i];
            if (ropeSegment.Locked)
                continue;

            float playerProximityInterpolant = LumUtils.InverseLerp(50f, 10f, Main.LocalPlayer.Distance(ropeSegment.Position));
            ropeSegment.Velocity += Main.LocalPlayer.velocity * playerProximityInterpolant;
        }

        WindTime = (WindTime + MathF.Abs(Main.windSpeedCurrent) * 0.11f) % (MathHelper.TwoPi * 5000f);
        VerletSimulations.TileCollisionVerletSimulation(VerletRope.Rope, 0f, Vector2.UnitX * 0.9f, 20, Gravity);
    }

    private void DrawProjectionButItActuallyWorks(VerletSimulatedRope rope, Texture2D projection, Vector2 drawOffset, bool flipHorizontally, Func<float, Color> colorFunction, int? projectionWidth = null, int? projectionHeight = null, float widthFactor = 1f, bool unscaledMatrix = false)
    {
        ManagedShader shader = ShaderManager.GetShader("NoxusBoss.PrimitiveProjection");
        Main.instance.GraphicsDevice.Textures[1] = projection;
        Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.AnisotropicClamp;
        Main.instance.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
        shader.TrySetParameter("horizontalFlip", flipHorizontally);
        shader.TrySetParameter("heightRatio", (float)projection.Height / projection.Width);
        shader.TrySetParameter("lengthRatio", 1f);
        List<Vector2> positions = [.. rope.Rope.Select((VerletSimulatedSegment r) => r.Position)];
        positions.Add(End.ToVector2());

        PrimitiveSettings settings = new PrimitiveSettings((float _) => projection.Width * widthFactor, colorFunction.Invoke, (float _) => drawOffset + Main.screenPosition, Smoothen: true, Pixelate: false, shader, projectionWidth, projectionHeight, unscaledMatrix);
        PrimitiveRenderer.RenderTrail(positions, settings, 36);
    }

    /// <summary>
    /// Renders this rope.
    /// </summary>
    public void Render(bool emitLight, Color colorModifier)
    {
        Color ropeColorFunction(float completionRatio) => new Color(63, 22, 32).MultiplyRGBA(colorModifier);
        DrawProjectionButItActuallyWorks(VerletRope, MiscTexturesRegistry.Pixel.Value, -Main.screenPosition, false, ropeColorFunction, widthFactor: 2f);

        Vector2[] curveControlPoints = new Vector2[VerletRope.Rope.Count];
        Vector2[] curveVelocities = new Vector2[VerletRope.Rope.Count];
        for (int i = 0; i < curveControlPoints.Length; i++)
        {
            curveControlPoints[i] = VerletRope.Rope[i].Position;
            curveVelocities[i] = VerletRope.Rope[i].Velocity;
        }

        DeCasteljauCurve positionCurve = new DeCasteljauCurve(curveControlPoints);
        DeCasteljauCurve velocityCurve = new DeCasteljauCurve(curveVelocities);

        int ornamentCount = 7;
        Texture2D pinTexture = GennedAssets.Textures.SolynCampsite.SolynTentOrnamentPin.Value;
        for (int i = 0; i < ornamentCount; i++)
        {
            float sampleInterpolant = MathHelper.Lerp(0.1f, 0.8f, i / (float)(ornamentCount - 1f));
            Vector2 ornamentWorldPosition = positionCurve.Evaluate(sampleInterpolant);
            Vector2 velocity = velocityCurve.Evaluate(sampleInterpolant) * 0.3f;

            // Emit light at the point of the ornament.
            if (emitLight)
                Lighting.AddLight(ornamentWorldPosition, Color.Wheat.MultiplyRGBA(colorModifier).ToVector3() * 0.3f);

            int windGridTime = 20;
            Point ornamentTilePosition = ornamentWorldPosition.ToTileCoordinates();
            Main.instance.TilesRenderer.Wind.GetWindTime(ornamentTilePosition.X, ornamentTilePosition.Y, windGridTime, out int windTimeLeft, out int direction, out _);
            float windGridInterpolant = windTimeLeft / (float)windGridTime;
            float windGridRotation = Utils.GetLerpValue(0f, 0.5f, windGridInterpolant, true) * Utils.GetLerpValue(1f, 0.5f, windGridInterpolant, true) * direction * -0.93f;

            float windForceWave = LumUtils.AperiodicSin(WindTime + ornamentWorldPosition.X * 0.025f);
            float windForce = windForceWave * LumUtils.InverseLerp(0f, 0.75f, MathF.Abs(Main.windSpeedCurrent)) * 0.4f;
            float spiralRotation = WindTime + ornamentWorldPosition.X * 0.02f;
            Vector2 spiralDrawPosition = ornamentWorldPosition - Main.screenPosition + Vector2.UnitY * 3f;
            Main.spriteBatch.Draw(spiralTexture.Value, spiralDrawPosition, null, colorModifier, spiralRotation, spiralTexture.Size() * 0.5f, 0.5f, 0, 0f);

            // Draw golden pins.
            sampleInterpolant = MathHelper.Lerp(0.1f, 0.8f, (i + 0.5f) / (float)(ornamentCount - 1f));
            Vector2 pinWorldPosition = positionCurve.Evaluate(sampleInterpolant);
            Vector2 pinDrawPosition = pinWorldPosition - Main.screenPosition;
            float pinRotation = (positionCurve.Evaluate(sampleInterpolant + 0.001f) - pinWorldPosition).ToRotation();
            if (MathF.Cos(pinRotation) < 0f)
                pinRotation += MathHelper.Pi;

            Main.spriteBatch.Draw(pinTexture, pinDrawPosition, null, colorModifier, pinRotation, pinTexture.Size() * new Vector2(0.5f, 0f), 0.8f, 0, 0f);
        }
    }

    /// <summary>
    /// Serializes this rope data as a tag compound for world saving.
    /// </summary>
    public TagCompound Serialize()
    {
        return new TagCompound()
        {
            ["Start"] = Start,
            ["End"] = End,
            ["RopePositions"] = VerletRope.Rope.Select(p => p.Position.ToPoint()).ToList()
        };
    }

    /// <summary>
    /// Deserializes a tag compound containing data for a rope back into said rope.
    /// </summary>
    public static ShrineRopeData Deserialize(TagCompound tag)
    {
        ShrineRopeData rope = new ShrineRopeData(tag.Get<Point>("Start"), tag.Get<Point>("End"));
        Vector2[] ropePositions = [.. tag.Get<Point[]>("RopePositions").Select(p => p.ToVector2())];

        rope.VerletRope.Rope = new List<VerletSimulatedSegment>();
        for (int i = 0; i < ropePositions.Length; i++)
        {
            bool locked = i == 0 || i == ropePositions.Length - 1;
            rope.VerletRope.Rope.Add(new VerletSimulatedSegment(ropePositions[i], Vector2.Zero, locked));
        }

        return rope;
    }
}
