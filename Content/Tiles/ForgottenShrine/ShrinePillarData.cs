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

    /// <summary>
    /// The rotation of this pillar.
    /// </summary>
    public float Rotation
    {
        get;
        set;
    }

    /// <summary>
    /// The scale of this pillar.
    /// </summary>
    public float Scale
    {
        get;
        set;
    }

    public ShrinePillarData() { }

    public ShrinePillarData(Point position, float rotation, float scale) : base(position)
    {
        Rotation = rotation;
        Scale = scale;
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
        Main.spriteBatch.Draw(pillar, bottom, null, Color.White, Rotation, pillar.Size() * new Vector2(0.5f, 1f), Scale, 0, 0f);

        Vector2 pillarBottom = bottom - Vector2.UnitY.RotatedBy(Rotation) * pillar.Height * Scale;
        Main.spriteBatch.Draw(pillarTop, pillarBottom, null, Color.White, Rotation, pillarTop.Size() * new Vector2(0.5f, 1f), Scale, 0, 0f);
    }

    public override TagCompound Serialize()
    {
        return new TagCompound()
        {
            ["Start"] = Position,
            ["Rotation"] = Rotation,
            ["Scale"] = Scale
        };
    }

    public override ShrinePillarData Deserialize(TagCompound tag)
    {
        ShrinePillarData shrine = new ShrinePillarData(tag.Get<Point>("Start"), tag.GetFloat("Rotation"), tag.GetFloat("Scale"));
        return shrine;
    }
}
