using HeavenlyArsenal.Content.Tiles.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class ShrinePillarData : WorldOrientedTileObject
{
    private static readonly Asset<Texture2D> pillarTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/ShrinePillar");

    private static readonly Asset<Texture2D> pillarTopTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/ShrinePillarTop");

    private static readonly Asset<Texture2D> ropeAnchorTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/ShrinePillarRopeAnchor");

    /// <summary>
    /// The relative Y interpolant of the rope anchor on this pillar.
    /// </summary>
    public float RopeAnchorYInterpolant
    {
        get;
        set;
    }

    /// <summary>
    /// The rotation of this pillar.
    /// </summary>
    public float Rotation
    {
        get;
        set;
    }

    /// <summary>
    /// The height of this pillar.
    /// </summary>
    public float Height
    {
        get;
        set;
    }

    /// <summary>
    /// Whether this pillar has a rope anchor or not.
    /// </summary>
    public bool HasRopeAnchor => RopeAnchorYInterpolant > 0f && RopeAnchorYInterpolant < 1f;

    /// <summary>
    /// The position of this rope's anchor in world coordinates, or null if there is no anchor.
    /// </summary>
    public Vector2? RopeAnchorPosition
    {
        get
        {
            if (!HasRopeAnchor)
                return null;

            return Position.ToVector2() - Vector2.UnitY.RotatedBy(Rotation) * (Height * RopeAnchorYInterpolant - 24f);
        }
    }

    public ShrinePillarData() { }

    public ShrinePillarData(Point position, float rotation, float height) : base(position)
    {
        Rotation = rotation;
        Height = height;
    }

    public override void Update()
    {
    }

    public override void Render()
    {
        if (!Main.LocalPlayer.WithinRange(Position.ToVector2(), 2350f))
            return;

        Texture2D pillar = pillarTexture.Value;
        Texture2D pillarTop = pillarTopTexture.Value;
        Vector2 bottom = Position.ToVector2() - Main.screenPosition;
        Main.spriteBatch.Draw(pillar, bottom, null, Color.White, Rotation, pillar.Size() * new Vector2(0.5f, 1f), new Vector2(1f, Height), 0, 0f);

        Vector2 pillarBottom = bottom - Vector2.UnitY.RotatedBy(Rotation) * pillar.Height * Height;
        Main.spriteBatch.Draw(pillarTop, pillarBottom, null, Color.White, Rotation, pillarTop.Size() * new Vector2(0.5f, 1f), 1f, 0, 0f);

        if (HasRopeAnchor)
        {
            Texture2D ropeAnchor = ropeAnchorTexture.Value;
            Vector2 ropePosition = RopeAnchorPosition.Value - Main.screenPosition;
            Main.spriteBatch.Draw(ropeAnchor, ropePosition, null, Color.White, Rotation, ropeAnchor.Size() * new Vector2(0.5f, 1f), 1f, 0, 0f);
        }
    }

    public override TagCompound Serialize()
    {
        return new TagCompound()
        {
            ["Start"] = Position,
            ["Rotation"] = Rotation,
            ["Height"] = Height,
            ["RopeAnchorYInterpolant"] = RopeAnchorYInterpolant
        };
    }

    public override ShrinePillarData Deserialize(TagCompound tag)
    {
        ShrinePillarData shrine = new ShrinePillarData(tag.Get<Point>("Start"), tag.GetFloat("Rotation"), tag.GetFloat("Height"))
        {
            RopeAnchorYInterpolant = tag.GetFloat("RopeAnchorYInterpolant")
        };
        return shrine;
    }
}
