using HeavenlyArsenal.Content.Tiles.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class SpiderLilyData : WorldOrientedTileObject
{
    private static readonly Asset<Texture2D> lilyTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/SpiderLily");

    /// <summary>
    /// A general-purpose timer used for wind movement of this lily.
    /// </summary>
    public float WindTime
    {
        get;
        set;
    }

    /// <summary>
    /// The scale of this lily.
    /// </summary>
    public float Scale
    {
        get;
        set;
    }

    /// <summary>
    /// The Z position of this lily.
    /// </summary>
    public float ZPosition
    {
        get;
        set;
    }

    /// <summary>
    /// The direction of this lily.
    /// </summary>
    public SpriteEffects Direction
    {
        get;
        set;
    }

    public SpiderLilyData() { }

    public SpiderLilyData(Point position) : base(position)
    {
        Scale = MathHelper.Lerp(0.8f, 1f, MathF.Pow(Main.rand.NextFloat(), 4f));
        Direction = Main.rand.NextFromList(SpriteEffects.None, SpriteEffects.FlipHorizontally);
    }

    /// <summary>
    /// Updates this lily.
    /// </summary>
    public override void Update()
    {
        WindTime = (WindTime + MathF.Abs(Main.windSpeedCurrent) * 0.11f) % (MathHelper.TwoPi * 5000f);
    }

    /// <summary>
    /// Renders this lily.
    /// </summary>
    public override void Render()
    {
        if (!Main.LocalPlayer.WithinRange(Position.ToVector2(), 2350f))
            return;

        int frameY = (Position.X + Position.Y * 13) % 3;
        float brightness = 1.5f - ZPosition / 5.1f;
        Color color = new Color(brightness, brightness, brightness);

        int windPushTime = 30;
        Main.instance.TilesRenderer.Wind.GetWindTime(Position.X / 16, Position.Y / 16, windPushTime, out int windTimeLeft, out int direction, out _);
        float windGrindPush = LumUtils.Convert01To010(windTimeLeft / (float)windPushTime);
        float rotation = LumUtils.AperiodicSin(WindTime + Position.X * 10f + Position.Y * 20f) * 0.3f + direction * windGrindPush * 0.45f;

        Texture2D texture = lilyTexture.Value;
        Rectangle frame = texture.Frame(1, 3, 0, frameY);
        Vector2 lilyDrawPosition = Position.ToVector2() - Main.screenPosition + Vector2.UnitY * 4f;
        Main.spriteBatch.Draw(texture, lilyDrawPosition, frame, color, rotation, frame.Size() * new Vector2(0.5f, 1f), Scale, 0, 0f);
    }

    /// <summary>
    /// Serializes this lily data as a tag compound for world saving.
    /// </summary>
    public override TagCompound Serialize()
    {
        return new TagCompound()
        {
            ["Start"] = Position,
            ["ZPosition"] = ZPosition,
        };
    }

    /// <summary>
    /// Deserializes a tag compound containing data for a lily back into said lily.
    /// </summary>
    public override SpiderLilyData Deserialize(TagCompound tag)
    {
        SpiderLilyData lily = new SpiderLilyData(tag.Get<Point>("Start"))
        {
            ZPosition = tag.GetFloat("ZPosition")
        };
        return lily;
    }
}
