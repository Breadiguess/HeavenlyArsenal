using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Tiles.Generic;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.LightingMask;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class HangingLanternRopeData : WorldOrientedTileObject
{
    /// <summary>
    /// The amount by which this rope should sag when completely at rest.
    /// </summary>
    public float Sag
    {
        get;
        set;
    }

    /// <summary>
    /// The horizontal direction of this rope's lantern.
    /// </summary>
    public int Direction
    {
        get;
        set;
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
    public static float Gravity => 0.6f;

    /// <summary>
    /// The asset for the knot texture used by this rope.
    /// </summary>
    public static readonly Asset<Texture2D> KnotTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/HangingLanternRopeKnot");

    public HangingLanternRopeData() { }

    public HangingLanternRopeData(Point anchorPosition, float ropeLength)
    {
        Vector2 startVector = anchorPosition.ToVector2();
        Position = anchorPosition;

        int segmentCount = 24;
        VerletRope = new Rope(startVector, startVector + Vector2.UnitY * ropeLength, segmentCount, ropeLength / segmentCount, Vector2.UnitY * Gravity, 12)
        {
            tileCollide = true
        };
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

        VerletRope.segments[^1].pinned = false;

        for (int i = 0; i < VerletRope.segments.Length; i++)
        {
            Rope.RopeSegment ropeSegment = VerletRope.segments[i];
            if (ropeSegment.pinned)
                continue;

            foreach (Player player in Main.ActivePlayers)
            {
                float playerProximityInterpolant = LumUtils.InverseLerp(30f, 10f, player.Distance(ropeSegment.position));
                ropeSegment.position += player.velocity * playerProximityInterpolant * 0.21f;
            }
        }

        Lighting.AddLight(VerletRope.segments[^1].position, Color.Orange.ToVector3());

        VerletRope.damping = 0.01f;
        VerletRope.Update();
    }

    private void DrawProjectionButItActuallyWorks(Vector2 drawOffset, Func<float, Color> colorFunction, int? projectionWidth = null, int? projectionHeight = null, bool unscaledMatrix = false)
    {
        List<Vector2> positions = [.. VerletRope.segments.Select((Rope.RopeSegment r) => r.position)];
        ManagedShader overlayShader = ShaderManager.GetShader("HeavenlyArsenal.LitPrimitiveOverlayShader");
        overlayShader.TrySetParameter("exposure", 1f);
        overlayShader.TrySetParameter("screenSize", WotGUtils.ViewportSize);
        overlayShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        overlayShader.SetTexture(MiscTexturesRegistry.Pixel.Value, 1, SamplerState.LinearClamp);
        overlayShader.SetTexture(LightingMaskTargetManager.LightTarget, 2);
        overlayShader.Apply();

        PrimitiveSettings settings = new PrimitiveSettings((float _) => 2f, colorFunction.Invoke, (float _) => drawOffset + Main.screenPosition, Smoothen: true, Pixelate: false, overlayShader, projectionWidth, projectionHeight, unscaledMatrix);
        PrimitiveRenderer.RenderTrail(positions, settings, 36);

        // Draw the lantern at the bottom of the rope.
        Texture2D lantern = OrnamentalShrineRopeData.PaperLanternTexture.Value;
        Texture2D glowTexture = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;
        float flickerInterpolant = LumUtils.Cos01(Main.GlobalTimeWrappedHourly * 5f + VerletRope.segments[0].position.X * 0.1f);
        float flicker = MathHelper.Lerp(0.93f, 1.07f, flickerInterpolant);
        float lanternScale = 0.8f;
        float glowScale = lanternScale * flicker;
        float lanternRotation = VerletRope.segments[0].position.AngleTo(VerletRope.segments[^1].position);
        Vector2 lanternDrawPosition = VerletRope.segments[^1].position - Main.screenPosition;
        Color lanternGlowColor = new Color(1f, 0.44f, 0.15f, 0f) * 0.75f;
        Main.spriteBatch.Draw(lantern, lanternDrawPosition, null, Color.White, lanternRotation + MathHelper.PiOver2, lantern.Size() * 0.5f, lanternScale, Direction.ToSpriteDirection(), 0f);
        Main.spriteBatch.Draw(glowTexture, lanternDrawPosition, null, lanternGlowColor, 0f, glowTexture.Size() * 0.5f, glowScale * 1.05f, 0, 0f);
        Main.spriteBatch.Draw(glowTexture, lanternDrawPosition, null, lanternGlowColor * 0.6f, 0f, glowTexture.Size() * 0.5f, glowScale * 1.4f, 0, 0f);

        // Draw the knot above the rope.
        Texture2D knot = KnotTexture.Value;
        Vector2 knotBottom = VerletRope.segments[0].position;
        Color knotColor = Lighting.GetColor(knotBottom.ToTileCoordinates());
        Main.spriteBatch.Draw(knot, knotBottom - Main.screenPosition, null, knotColor, 0f, knot.Size() * new Vector2(0.5f, 1f), 1f, 0, 0f);
    }

    /// <summary>
    /// Renders this rope.
    /// </summary>
    public override void Render()
    {
        DrawProjectionButItActuallyWorks(-Main.screenPosition, _ => new Color(255, 28, 58));
    }

    /// <summary>
    /// Serializes this rope data as a tag compound for world saving.
    /// </summary>
    public override TagCompound Serialize()
    {
        return new TagCompound()
        {
            ["Position"] = Position,
            ["Sag"] = Sag,
            ["MaxLength"] = MaxLength,
            ["Direction"] = Direction,
            ["RopePositions"] = VerletRope.segments.Select(p => p.position.ToPoint()).ToList()
        };
    }

    /// <summary>
    /// Deserializes a tag compound containing data for a rope back into said rope.
    /// </summary>
    public override HangingLanternRopeData Deserialize(TagCompound tag)
    {
        HangingLanternRopeData rope = new HangingLanternRopeData(tag.Get<Point>("Position"), tag.GetFloat("Sag"))
        {
            MaxLength = tag.GetFloat("MaxLength"),
            Direction = tag.GetInt("Direction")
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
