using System.Collections.Generic;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using ReLogic.Graphics;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Rarities;

public sealed class BloodMoonRarityGlobalItem : GlobalItem
{
    private struct Droplet
    {
        public Vector2 Anchor;

        public float Size; // x anchor (relative to name left), initial

        public float Offset; // vertical offset (starts negative, increases downward)

        public float Speed; // falling speed

        public float Phase;

        public float Alpha;
    }

    // map by item type -> droplet list (persist across frames)
    private static readonly Dictionary<int, List<Droplet>> dropletMap = new();

    public float Time => Main.GlobalTimeWrappedHourly;
    
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.rare == ModContent.RarityType<BloodMoonRarity>();
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (line.Mod != "Terraria" || line.Name != "ItemName")
        {
            return true;
        }
        
        var text = item.AffixName();
        var font = FontAssets.MouseText.Value;

        var basePos = new Vector2(line.X, line.Y);
        var time = Main.GlobalTimeWrappedHourly;

        Texture2D dropletTex = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

        EnsureDroplets(item.type, text, font);

        var measured = font.MeasureString(text);
        var nameOrigin = measured * 0.5f;
        var namePos = basePos;

        // Draw the name with a simple border for legibility (colors are example)
        //Utils.DrawBorderStringFourWay(Main.spriteBatch, font, text, namePos, Color.Red, Color.Black, Vector2.Zero, 1f);
        Utils.DrawBorderString(Main.spriteBatch, text, namePos, Color.Red);
        // Update & draw droplets
        var list = dropletMap[item.type];
        var nameLeft = namePos - nameOrigin;
        DrawEclipse(namePos + nameOrigin);

        for (var i = 0; i < list.Count; i++)
        {
            var d = list[i];

            // update
            // tweak these multipliers to taste
            var fallMultiplier = 0.5f; // base falling speed multiplier
            var dt = 1f; // per-tick increment; tooltip updates once per frame/tick in Terraria
            d.Offset += d.Speed * fallMultiplier * dt;

            var sway = (float)Math.Sin(time * 3f + d.Phase) * 2f;

            var resetThreshold = measured.Y + 20f + d.Size * 6f;

            if (d.Offset > resetThreshold)
            {
                d.Offset = Main.rand.NextFloat(-10f, -2f);
                d.Speed = Main.rand.NextFloat(10f, 28f); // randomize fall speed
                d.Phase = Main.rand.NextFloat(MathHelper.TwoPi);
                d.Size = Main.rand.NextFloat(4f, 9f);
                d.Alpha = Main.rand.NextFloat(0.6f, 1f);
            }

            var drawPos = nameLeft + new Vector2(d.Anchor.X + 60 + sway, d.Offset + measured.Y * 1f);

            // draw the droplet: dropletTex is 1x1 so we scale width/height
            // we'll make the droplet a vertical oval pointing down
            var dropletWidth = d.Size * 0.45f;
            var dropletHeight = d.Size * 1.1f;
            var drawScale = new Vector2(dropletWidth, dropletHeight);

            // draw with origin at texture center horizontally, top vertically so it looks like hanging
            var origin = new Vector2(0.5f, 0f);
            var color = Color.Lerp(Color.DarkRed, Color.Red, 0.4f) * d.Alpha;

            Main.spriteBatch.Draw
            (
                dropletTex,
                drawPos,
                null,
                color,
                0f,
                origin,
                drawScale,
                SpriteEffects.None,
                0f
            );

            var charBaseline = nameLeft + new Vector2(d.Anchor.X + 60, measured.Y * 0.0f);
            var TrailOrigin = new Vector2(0.5f, -0.115f);

            var trailLength = d.Offset;

            if (trailLength > 0.5f)
            {
                var trailPos = charBaseline + new Vector2(100f, 0f);
                var trailScale = new Vector2(0.6f, trailLength);

                Main.spriteBatch.Draw
                (
                    dropletTex,
                    trailPos,
                    null,
                    Color.DarkRed * (d.Alpha * 0.6f),
                    0f,
                    TrailOrigin,
                    trailScale,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        Texture2D glow = GennedAssets.Textures.GreyscaleTextures.BloomLine;

        var e = Color.Red with
        {
            A = 0
        };

        float Value = 1; // (float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly)/5);

        var Scale = new Vector2(0.4f, 0.009f * text.Length);
        var BarOrigin = new Vector2(glow.Width / 2, glow.Width / 2);
        //new Vector2(0, 0);
        var BarDrawPos = namePos + new Vector2(1, 10);

        var rot = MathHelper.ToRadians(-90);
        Main.EntitySpriteDraw(glow, BarDrawPos, null, e, rot, BarOrigin, Scale, SpriteEffects.None);

        float textScaleInterp = 0; //(float)Math.Abs(Math.Sin(time));

        var A = Color.Lerp(new Color(220, 20, 70), Color.Red, (float)Math.Sin(Main.GlobalTimeWrappedHourly));

        Utils.DrawBorderString(Main.spriteBatch, text, namePos, A);

        return false;
    }
    
    public void DrawEclipse(Vector2 NamePos)
    {
        Texture2D placeholder = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;
        var ModifiedPos = NamePos + new Vector2(0, 0);
        var EclipseOrigin = placeholder.Size() * 0.5f;

        var t = new Vector2(1, 0.9f);

        Main.EntitySpriteDraw
        (
            placeholder,
            ModifiedPos,
            null,
            Color.Crimson with
            {
                A = 0
            },
            0,
            EclipseOrigin,
            t,
            SpriteEffects.None
        );

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

        var particleDrawCenter = ModifiedPos + new Vector2(0f, 0f);
        var glow = AssetDirectory.Textures.BigGlowball.Value;

        var Scale = new Vector2(0.1f);

        Main.EntitySpriteDraw
        (
            glow,
            particleDrawCenter - Main.screenPosition,
            glow.Frame(),
            Color.Red with
            {
                A = 200
            },
            0,
            glow.Size() * 0.5f,
            new Vector2(0.12f, 0.25f),
            0
        );

        var innerRiftTexture = AssetDirectory.Textures.VoidLake.Value;
        var edgeColor = new Color(1f, 0.06f, 0.06f);

        var riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
        riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.1f);
        riftShader.TrySetParameter("baseCutoffRadius", 0.24f);
        riftShader.TrySetParameter("swirlOutwardnessExponent", 0.2f);
        riftShader.TrySetParameter("swirlOutwardnessFactor", 3f);
        riftShader.TrySetParameter("vanishInterpolant", 0.01f);
        riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
        riftShader.TrySetParameter("edgeColorBias", 0.1f);
        riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
        riftShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap);
        riftShader.Apply();

        Main.spriteBatch.Draw(innerRiftTexture, particleDrawCenter, null, Color.White, 0 + MathHelper.Pi, innerRiftTexture.Size() * 0.5f, Scale, 0, 0);

        Main.spriteBatch.End();

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
    }

    private void EnsureDroplets(int itemType, string text, DynamicSpriteFont font)
    {
        // If we already have a list and its length matches number of letters (or is greater), do nothing.
        if (dropletMap.TryGetValue(itemType, out var existing))
        {
            // allow reusing if same length; otherwise recreate
            if (existing.Count >= text.Length)
            {
                return;
            }
        }

        // create a new list
        var list = new List<Droplet>();

        // We'll create at least one droplet per character, and sometimes add extra random droplets
        for (var i = 0; i < text.Length; i++)
        {
            // measure the substring up to this character to get a horizontal anchor
            var measureBefore = font.MeasureString(text.Substring(0, i));
            var measureChar = font.MeasureString(text.Substring(i, 1));
            var charCenterX = measureBefore.X + measureChar.X * 0.5f;

            // primary droplet attached to this char
            var d = new Droplet
            {
                Anchor = new Vector2(charCenterX, 0f),
                Offset = Main.rand.NextFloat(-20f, -2f),
                Speed = Main.rand.NextFloat(12f, 24f),
                Size = Main.rand.NextFloat(4f, 8.5f),
                Phase = Main.rand.NextFloat(MathHelper.TwoPi),
                Alpha = Main.rand.NextFloat(0.5f, 1f)
            };

            list.Add(d);

            // randomly add an occasional extra droplet near this char
            if (Main.rand.NextBool(6))
            {
                var d2 = new Droplet
                {
                    Anchor = new Vector2(charCenterX + Main.rand.NextFloat(-4f, 6f), 0f),
                    Offset = Main.rand.NextFloat(-30f, -4f),
                    Speed = Main.rand.NextFloat(8f, 30f),
                    Size = Main.rand.NextFloat(3f, 7f),
                    Phase = Main.rand.NextFloat(MathHelper.TwoPi),
                    Alpha = Main.rand.NextFloat(0.35f, 0.95f)
                };

                list.Add(d2);
            }
        }

        dropletMap[itemType] = list;
    }
}