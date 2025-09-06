using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.GlobalInstances;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.Rarities
{
    public class BloodMoonRarity : ModRarity
    {
        public override Color RarityColor => new Color(220, 20, 70);


    }

    public class BloodRarityTooltip : GlobalItem
    {
        // simple droplet state type
        private struct Droplet
        {
            public Vector2 anchor;
            public float size;   // x anchor (relative to name left), initial
            public float VerticalOffset;             // vertical offset (starts negative, increases downward)
            public float speed;         // falling speed
                   
            public float phase;
            public float alpha;     
            
            public float FallInterp;

            /*
             * 
            public Vector2 anchor;// x anchor (relative to name left), initial
            public Vector2 Scale;
          
            public float VerticalOffset;
            public float CurrentSpeed;           // falling speed      
            public float FallInterp;
            public float alpha;       
             */
            // opacity
        }
        public float Time => Main.GlobalTimeWrappedHourly;

        // map by item type -> droplet list (persist across frames)
        private static readonly Dictionary<int, List<Droplet>> dropletMap = new();

        public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
        {

            if (item.rare == ModContent.RarityType<BloodMoonRarity>() && line.Mod == "Terraria" && line.Name == "ItemName")
            {
                string text = item.Name;
                DynamicSpriteFont font = FontAssets.MouseText.Value;

                Vector2 basePos = new Vector2(line.X, line.Y); // where the tooltip system wants to draw the name
                float time = Main.GlobalTimeWrappedHourly;

                // texture used to draw droplets. Replace with a round droplet texture if you have one.
                Texture2D dropletTex = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

                // ensure droplets exist and are attached to each letter
                EnsureDroplets(item.type, text, font);

                // Draw the item name ourselves (so the name remains legible)
                // Draw with a border so the name stays readable over the effect
                Vector2 measured = font.MeasureString(text);
                Vector2 nameOrigin = measured * 0.5f;
                Vector2 namePos = basePos;

                // Draw the name with a simple border for legibility (colors are example)
                //Utils.DrawBorderStringFourWay(Main.spriteBatch, font, text, namePos, Color.Red, Color.Black, Vector2.Zero, 1f);
                Utils.DrawBorderString(Main.spriteBatch, text, namePos, Color.Red, 1f);
                // Update & draw droplets
                List<Droplet> list = dropletMap[item.type];
                Vector2 nameLeft = namePos - nameOrigin;

                for (int i = 0; i < list.Count; i++)
                {
                    Droplet d = list[i];

                    // update
                    // tweak these multipliers to taste
                    float fallMultiplier = 0.5f;    // base falling speed multiplier
                    float dt = 1f; // per-tick increment; tooltip updates once per frame/tick in Terraria
                    d.VerticalOffset += d.speed * fallMultiplier * dt;

                    // small horizontal sway so drops feel organic
                    float sway = (float)System.Math.Sin(time * 3f + d.phase) * 2f;

                    // reset when droplet falls past a threshold under the name
                    float resetThreshold = measured.Y + 20f + d.size * 6f;
                    if (d.VerticalOffset > resetThreshold)
                    {
                        // reset above the name with a small random offset
                        d.VerticalOffset = Main.rand.NextFloat(-10f, -2f);
                        d.speed = Main.rand.NextFloat(10f, 28f); // randomize fall speed
                        d.phase = Main.rand.NextFloat(MathHelper.TwoPi);
                        d.size = Main.rand.NextFloat(4f, 9f);
                        d.alpha = Main.rand.NextFloat(0.6f, 1f);
                    }

                    // compute draw position: nameLeft + anchor + (sway, VerticalOffset)
                    Vector2 drawPos = nameLeft + new Vector2(d.anchor.X + 60 + sway, d.VerticalOffset + measured.Y *1f);

                    // draw the droplet: dropletTex is 1x1 so we scale width/height
                    // we'll make the droplet a vertical oval pointing down
                    float dropletWidth = d.size * 0.45f;
                    float dropletHeight = d.size * 1.1f;
                    Vector2 drawScale = new Vector2(dropletWidth, dropletHeight);

                    // draw with origin at texture center horizontally, top vertically so it looks like hanging
                    Vector2 origin = new Vector2(0.5f, 0f);
                    Color color = Color.Lerp(Color.DarkRed, Color.Red, 0.4f) * d.alpha;

                    Main.spriteBatch.Draw(
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

                    // optional: draw a tiny trail (a thin line) from the char baseline to the drop
                    // you can comment this out if you don't want the trail
                    Vector2 charBaseline = nameLeft + new Vector2(d.anchor.X + 60, measured.Y * 0.0f);
                    Vector2 TrailOrigin = new Vector2(0.5f, -0.115f);

                    float trailLength = d.VerticalOffset;
                    if (trailLength > 0.5f)
                    {
                        Vector2 trailPos = charBaseline + new Vector2(100f, 0f);
                        Vector2 trailScale = new Vector2(0.6f, trailLength);
                        Main.spriteBatch.Draw(
                            dropletTex,
                            trailPos,
                            null,
                            Color.DarkRed * (d.alpha * 0.6f),
                            0f,
                            TrailOrigin,
                            trailScale,
                            SpriteEffects.None,
                            0f
                        );
                    }
                }

                Texture2D glow = GennedAssets.Textures.GreyscaleTextures.BloomLine;
                
                Color e = Color.Red with { A = 0 };

                float Value = 1;// (float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly)/5);

                Vector2 Scale = new Vector2(0.4f, 0.009f * text.Length);
                Vector2 BarOrigin = new Vector2(glow.Width/2, glow.Width / 2);
                //new Vector2(0, 0);
                Vector2 BarDrawPos = namePos + new Vector2(1, 10);

                float rot = MathHelper.ToRadians(-90);
                Main.EntitySpriteDraw(glow, BarDrawPos, null, e, rot, BarOrigin, Scale, SpriteEffects.None);


                float textScaleInterp = 0;//(float)Math.Abs(Math.Sin(time));
                for(int u = 0; u < 12; u++)
                {

                    Vector2 DrawOffset = new Vector2(0, 1).RotatedBy(u);
                    
                    Utils.DrawBorderString(Main.spriteBatch, item.Name, namePos + DrawOffset, Color.DarkRed * 0.5f, 1 * (1+textScaleInterp), 0, 0);
                }
                Utils.DrawBorderString(Main.spriteBatch, item.Name,namePos, Color.Red);
                return false;
            }

            return base.PreDrawTooltipLine(item, line, ref yOffset);
        }

        // create droplets for this item type if not present or if text length changed
        private void EnsureDroplets(int itemType, string text, DynamicSpriteFont font)
        {
            // If we already have a list and its length matches number of letters (or is greater), do nothing.
            if (dropletMap.TryGetValue(itemType, out var existing))
            {
                // allow reusing if same length; otherwise recreate
                if (existing.Count >= text.Length)
                    return;
            }

            // create a new list
            var list = new List<Droplet>();

            // We'll create at least one droplet per character, and sometimes add extra random droplets
            for (int i = 0; i < text.Length; i++)
            {
                // measure the substring up to this character to get a horizontal anchor
                Vector2 measureBefore = font.MeasureString(text.Substring(0, i));
                Vector2 measureChar = font.MeasureString(text.Substring(i, 1));
                float charCenterX = measureBefore.X + measureChar.X * 0.5f;

                // primary droplet attached to this char
                var d = new Droplet()
                {
                    anchor = new Vector2(charCenterX, 0f),
                    VerticalOffset = Main.rand.NextFloat(-20f, -2f),
                    speed = Main.rand.NextFloat(12f, 24f),
                    size = Main.rand.NextFloat(4f, 8.5f),
                    phase = Main.rand.NextFloat(MathHelper.TwoPi),
                    alpha = Main.rand.NextFloat(0.5f, 1f)
                };
                list.Add(d);

                // randomly add an occasional extra droplet near this char
                if (Main.rand.NextBool(6))
                {
                    var d2 = new Droplet()
                    {
                        anchor = new Vector2(charCenterX + Main.rand.NextFloat(-4f, 6f), 0f),
                        VerticalOffset = Main.rand.NextFloat(-30f, -4f),
                        speed = Main.rand.NextFloat(8f, 30f),
                        size = Main.rand.NextFloat(3f, 7f),
                        phase = Main.rand.NextFloat(MathHelper.TwoPi),
                        alpha = Main.rand.NextFloat(0.35f, 0.95f)
                    };
                    list.Add(d2);
                }
            }

            dropletMap[itemType] = list;
        }
    }
}