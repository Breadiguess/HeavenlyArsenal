using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Tiles.Generic;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.DataStructures;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class OrnamentalShrineRopeData : WorldOrientedTileObject
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
            ClampToMaxLength(ref endVector);

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
    public static float Gravity => 0.65f;

    /// <summary>
    /// The asset for the paper lantern texture used by this rope.
    /// </summary>
    public static readonly Asset<Texture2D> PaperLanternTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/PaperLantern");

    public OrnamentalShrineRopeData() { }

    public OrnamentalShrineRopeData(Point start, Point end, float sag)
    {
        Vector2 startVector = start.ToVector2();
        Vector2 endVector = end.ToVector2();
        Sag = sag;

        Position = start;
        this.end = end;

        MaxLength = Rope.CalculateSegmentLength(Vector2.Distance(Start.ToVector2(), End.ToVector2()), Sag);

        int segmentCount = 30;
        VerletRope = new Rope(startVector, endVector, segmentCount, MaxLength / segmentCount, Vector2.UnitY * Gravity, 15)
        {
            tileCollide = true
        };
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
    public override void Update()
    {
        bool startHasNoTile = !Framing.GetTileSafely(Start.ToVector2().ToTileCoordinates()).HasTile;
        bool endHasNoTile = !Framing.GetTileSafely(End.ToVector2().ToTileCoordinates()).HasTile;
        if (startHasNoTile || endHasNoTile)
        {
            ModContent.GetInstance<OrnamentalShrineRopeSystem>().Remove(this);
            return;
        }

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
                ropeSegment.position += player.velocity * playerProximityInterpolant * 0.4f;
            }
        }

        WindTime = (WindTime + MathF.Abs(Main.windSpeedCurrent) * 0.11f) % (MathHelper.TwoPi * 5000f);
        VerletRope.Update();
    }

    private void DrawProjectionButItActuallyWorks(Texture2D projection, Vector2 drawOffset, bool flipHorizontally, Func<float, Color> colorFunction, int? projectionWidth = null, int? projectionHeight = null, float widthFactor = 1f, bool unscaledMatrix = false)
    {
        ManagedShader shader = ShaderManager.GetShader("NoxusBoss.PrimitiveProjection");
        Main.instance.GraphicsDevice.Textures[1] = projection;
        Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.AnisotropicClamp;
        Main.instance.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
        shader.TrySetParameter("horizontalFlip", flipHorizontally);
        shader.TrySetParameter("heightRatio", (float)projection.Height / projection.Width);
        shader.TrySetParameter("lengthRatio", 1f);
        List<Vector2> positions = [.. VerletRope.segments.Select((Rope.RopeSegment r) => r.position)];
        positions.Add(End.ToVector2());

        PrimitiveSettings settings = new PrimitiveSettings((float _) => projection.Width * widthFactor, colorFunction.Invoke, (float _) => drawOffset + Main.screenPosition, Smoothen: true, Pixelate: false, shader, projectionWidth, projectionHeight, unscaledMatrix);
        PrimitiveRenderer.RenderTrail(positions, settings, 36);
    }

    /// <summary>
    /// Renders this rope.
    /// </summary>
    public override void Render()
    {
        Color ropeColorFunction(float completionRatio) => new Color(63, 22, 32);
        DrawProjectionButItActuallyWorks(MiscTexturesRegistry.Pixel.Value, -Main.screenPosition, false, ropeColorFunction, widthFactor: 2f);

        Vector2[] curveControlPoints = new Vector2[VerletRope.segments.Length];
        for (int i = 0; i < curveControlPoints.Length; i++)
            curveControlPoints[i] = VerletRope.segments[i].position;

        DeCasteljauCurve positionCurve = new DeCasteljauCurve(curveControlPoints);

        Main.instance.LoadProjectile(ProjectileID.ReleaseLantern);

        int ornamentCount = 7;
        Texture2D glowTexture = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;
        for (int i = 0; i < ornamentCount; i++)
        {
            float sampleInterpolant = MathHelper.Lerp(0.06f, 0.8f, i / (float)(ornamentCount - 1f));
            Vector2 ornamentWorldPosition = positionCurve.Evaluate(sampleInterpolant);

            Lighting.AddLight(ornamentWorldPosition, Color.Wheat.ToVector3() * 0.4f);

            int windGridTime = 33;
            Point ornamentTilePosition = ornamentWorldPosition.ToTileCoordinates();
            Main.instance.TilesRenderer.Wind.GetWindTime(ornamentTilePosition.X, ornamentTilePosition.Y, windGridTime, out int windTimeLeft, out int direction, out _);
            float windGridInterpolant = windTimeLeft / (float)windGridTime;
            float windGridRotation = Utils.GetLerpValue(0f, 0.5f, windGridInterpolant, true) * Utils.GetLerpValue(1f, 0.5f, windGridInterpolant, true) * direction * -0.93f;

            // Draw ornamental spirals.
            float windForceWave = LumUtils.AperiodicSin(WindTime * 0.4f + ornamentWorldPosition.X * 0.095f);
            float windForce = windForceWave * LumUtils.InverseLerp(0f, 0.75f, MathF.Abs(Main.windSpeedCurrent)) * 0.4f;
            float spiralRotation = WindTime + ornamentWorldPosition.X * 0.02f;
            Vector2 spiralDrawPosition = ornamentWorldPosition - Main.screenPosition + Vector2.UnitY * 3f;
            Main.spriteBatch.Draw(spiralTexture.Value, spiralDrawPosition, null, Color.White, spiralRotation, spiralTexture.Size() * 0.5f, 1f, 0, 0f);

            // Draw lanterns.
            Texture2D lanternTexture = TextureAssets.Projectile[ProjectileID.ReleaseLantern].Value;
            sampleInterpolant = MathHelper.Lerp(0.06f, 0.8f, (i + 0.5f) / (float)(ornamentCount - 1f));
            float lanternRotation = LumUtils.AperiodicSin(WindTime * 0.23f) * 0.45f + windGridRotation + (positionCurve.Evaluate(sampleInterpolant + 0.001f) - positionCurve.Evaluate(sampleInterpolant)).ToRotation();
            Vector2 lanternWorldPosition = positionCurve.Evaluate(sampleInterpolant);
            Vector2 lanternDrawPosition = lanternWorldPosition - Main.screenPosition;
            Vector2 lanternGlowDrawPosition = lanternDrawPosition + Vector2.UnitY.RotatedBy(lanternRotation) * 8f;
            Rectangle lanternFrame = lanternTexture.Frame(1, 4, 0, i % 4);
            Color lanternGlowColor = new Color(1f, 1f, 0.4f, 0f);
            float lanternGlowOpacity = 0.36f;
            float lanternGlowScaleFactor = 1f;
            float lanternScale = 0.8f;
            if (i == ornamentCount / 2)
            {
                lanternTexture = PaperLanternTexture.Value;
                lanternFrame = lanternTexture.Frame();
                lanternGlowColor = new Color(1f, 0.44f, 0.15f, 0f);
                lanternGlowOpacity = 0.5f;
                lanternRotation *= 0.33f;
                lanternGlowDrawPosition = lanternDrawPosition + Vector2.UnitY.RotatedBy(lanternRotation) * 26f;
                lanternGlowScaleFactor = 1.6f;
                lanternScale = 0.6f;
            }

            Main.spriteBatch.Draw(lanternTexture, lanternDrawPosition, lanternFrame, Color.White, lanternRotation, lanternFrame.Size() * new Vector2(0.5f, 0f), lanternScale, 0, 0f);
            Main.spriteBatch.Draw(glowTexture, lanternGlowDrawPosition, null, lanternGlowColor * lanternGlowOpacity, 0f, glowTexture.Size() * 0.5f, lanternGlowScaleFactor * 0.5f, 0, 0f);
            Main.spriteBatch.Draw(glowTexture, lanternGlowDrawPosition, null, lanternGlowColor * lanternGlowOpacity * 0.6f, 0f, glowTexture.Size() * 0.5f, lanternGlowScaleFactor * 1.1f, 0, 0f);
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
    public override OrnamentalShrineRopeData Deserialize(TagCompound tag)
    {
        OrnamentalShrineRopeData rope = new OrnamentalShrineRopeData(tag.Get<Point>("Start"), tag.Get<Point>("End"), tag.GetFloat("Sag"))
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
