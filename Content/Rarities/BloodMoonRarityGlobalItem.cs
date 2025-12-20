using System.Collections.Generic;
using HeavenlyArsenal.Core.Graphics;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Utilities;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Rarities;

[Autoload(Side = ModSide.Client)]
public sealed class BloodMoonRarityGlobalItem : GlobalItem
{
    private sealed class BloodMoonDroplet
    {
        /// <summary>
        ///     Gets or sets the position of the droplet, in screen coordinates.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        ///     Gets or sets the velocity of the droplet, in pixels per update.
        /// </summary>
        public Vector2 Velocity;

        private float opacity = 1f;

        /// <summary>
        ///     Gets or sets the opacity of the droplet.
        /// </summary>
        /// <value>
        ///     A value between <c>0f</c> and <c>1f</c>, where <c>0f</c> represents fully transparent and
        ///     <c>1f</c> represents fully opaque.
        /// </value>
        public float Opacity
        {
            get => opacity;
            set => opacity = MathHelper.Clamp(value, 0f, 1f);
        }
    }

    /// <summary>
    ///     The name of the rift shader.
    /// </summary>
    private const string RIFT_SHADER_NAME = "NoxusBoss.DarkPortalShader";

    private static readonly List<BloodMoonDroplet> Droplets = [];

    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return lateInstantiation && entity.rare == ModContent.RarityType<BloodMoonRarity>();
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (line.Mod != "Terraria" || line.Name != "ItemName")
        {
            return true;
        }

        var text = item.AffixName();
        var position = new Vector2(line.X, line.Y);

        SpawnDroplets(in position, text);
        UpdateDroplets();
        DrawDroplets();

        var font = FontAssets.MouseText.Value;
        var size = font.MeasureString(text);

        var offset = size / 2f;

        var center = position + offset;

        DrawEclipse(in center);
        DrawText(in position, text);

        return false;
    }

    private static void SpawnDroplets(in Vector2 position, string text)
    {
        if (!Main.rand.NextBool(2))
        {
            return;
        }

        var font = FontAssets.MouseText.Value;

        var offset = new Vector2(Main.rand.NextFloat(font.MeasureString(text).X), 0f);
        var velocity = new Vector2(-1f, 4f);

        var droplet = new BloodMoonDroplet
        {
            Position = position + offset,
            Velocity = velocity
        };

        Droplets.Add(droplet);
    }

    private static void UpdateDroplets()
    {
        for (var i = 0; i < Droplets.Count; i++)
        {
            var droplet = Droplets[i];

            droplet.Position += droplet.Velocity;

            droplet.Opacity -= 0.1f;

            if (droplet.Opacity > 0f)
            {
                continue;
            }

            Droplets.RemoveAt(i);

            i--;
        }
    }

    private static void DrawDroplets()
    {
        var bloom = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint.Value;
        var texture = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

        var batch = Main.spriteBatch;

        foreach (var droplet in Droplets)
        {
            var color = Color.Crimson * 0.5f * droplet.Opacity;

            color.A = 0;

            batch.Draw
            (
                bloom,
                droplet.Position,
                null,
                color,
                MathHelper.ToRadians(15f),
                bloom.Size() / 2f,
                0.25f,
                SpriteEffects.None,
                0f
            );

            color = Color.Crimson * droplet.Opacity;

            batch.Draw
            (
                texture,
                droplet.Position,
                null,
                color,
                MathHelper.ToRadians(15f),
                texture.Size() / 2f,
                new Vector2(2f, 20f),
                SpriteEffects.None,
                0f
            );
        }
    }

    private static void DrawText(in Vector2 position, string text)
    {
        var font = FontAssets.MouseText.Value;
        var cursor = position;

        var bloom = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint.Value;

        var batch = Main.spriteBatch;

        for (var i = 0; i < text.Length; i++)
        {
            var letter = text[i].ToString();

            var color = Color.Crimson * 0.5f;

            color.A = 0;

            var offset = font.MeasureString(letter) / 2f;

            var empty = string.IsNullOrEmpty(letter) || string.IsNullOrWhiteSpace(letter);

            if (!empty)
            {
                batch.Draw
                (
                    bloom,
                    cursor + offset,
                    null,
                    color,
                    0f,
                    bloom.Size() / 2f,
                    0.25f,
                    SpriteEffects.None,
                    0f
                );
            }

            var wave = MathF.Sin(Main.GlobalTimeWrappedHourly*10.1f/2 + i);

            offset = new Vector2(0f, wave);

            color = Color.Black;
            color.A = 255;

            var scale = 1f + wave * 0.01f;
            Utils.DrawBorderString(Main.spriteBatch, letter, cursor + offset, color , scale*1.2f);

            Utils.DrawBorderString(Main.spriteBatch, letter, cursor + offset, Color.Crimson, scale);

            cursor.X += font.MeasureString(letter).X * scale;
        }
    }

    private static void DrawEclipse(in Vector2 position)
    {
        var batch = Main.spriteBatch;

        var bloom = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint.Value;
        var origin = bloom.Size() / 2f;

        var color = Color.Crimson * 0.5f;

        color.A = 0;

        batch.Draw
        (
            bloom,
            position,
            null,
            color,
            0f,
            origin,
            1f,
            SpriteEffects.None,
            0f
        );

        var parameters = batch.Capture();

        batch.End();
        batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

        var shader = ShaderManager.GetShader(RIFT_SHADER_NAME);

        color = new Color(1f, 0.06f, 0.06f);

        shader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.1f);
        shader.TrySetParameter("baseCutoffRadius", 0.24f);
        shader.TrySetParameter("swirlOutwardnessExponent", 0.2f);
        shader.TrySetParameter("swirlOutwardnessFactor", 3f);
        shader.TrySetParameter("vanishInterpolant", 0.01f);
        shader.TrySetParameter("edgeColor", color.ToVector4());
        shader.TrySetParameter("edgeColorBias", 0.1f);

        shader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
        shader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap);

        shader.Apply();

        var rift = AssetDirectory.Textures.VoidLake.Value;

        origin = rift.Size() / 2f;

        batch.Draw(rift, position, null, Color.White, MathHelper.Pi, origin, 0.1f, SpriteEffects.None, 0f);

        batch.End();
        batch.Begin(in parameters);
    }
}