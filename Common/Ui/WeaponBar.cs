using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace HeavenlyArsenal.Common.Ui;

public class WeaponBar : ModSystem
{
    private static int showTime;
    private static Color baseColor;   // Keep this as the default or most recent base color
   private static Color fillColor;   // Keep this as the default or most recent fill color
    private static Vector2 Baroffset; // Keep this as the default or most recent offset

    private static float fillPercent;
    private static int style;

    // Internal struct for tracking individual bars
    private class WeaponBarInfo
    {
        public Color BaseColor;
        public Color FillColor;
        public float FillPercent;
        public int TimeLeft;
        public int Style;
        public Vector2 Offset;

        public WeaponBarInfo(Color baseColor, Color fillColor, float fillPercent, int timeLeft, int style, Vector2 offset)
        {
            BaseColor = baseColor;
            FillColor = fillColor;
            FillPercent = fillPercent;
            TimeLeft = timeLeft;
            Style = style;
            Offset = offset;
        }
    }

    private static readonly List<WeaponBarInfo> ActiveBars = new();

    public static void DisplayBar(Color baseColor, Color fillColor, float percent, int showTime = 120, int style = 0, Vector2 BarOffset = default)
    {
        WeaponBar.showTime = showTime;
        WeaponBar.baseColor = baseColor;
        WeaponBar.fillColor = fillColor;
        fillPercent = percent;
        WeaponBar.style = style;
        Baroffset = BarOffset;

        ActiveBars.Add(new WeaponBarInfo(baseColor, fillColor, percent, showTime, style, BarOffset));
    }

    public override void UpdateUI(GameTime gameTime)
    {
        for (int i = ActiveBars.Count - 1; i >= 0; i--)
        {
            ActiveBars[i].TimeLeft--;
            if (ActiveBars[i].TimeLeft <= 0)
                ActiveBars.RemoveAt(i);
        }

        if (showTime > 0)
        {
            showTime--;
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (ActiveBars.Count > 0)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Entity Health Bars"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "HeavenlyArsenal: Weapon Charge Bars",
                    delegate
                    {
                        foreach (var barInfo in ActiveBars)
                        {
                            float fade = Utils.GetLerpValue(0, 30, barInfo.TimeLeft, true);

                            Texture2D bar = AssetDirectory.Textures.Bars.Bar[barInfo.Style].Value;
                            Texture2D barCharge = AssetDirectory.Textures.Bars.BarFill[barInfo.Style].Value;

                            int fillAmount = barInfo.FillPercent > 0.99f ? barCharge.Width : (int)(barCharge.Width * barInfo.FillPercent);
                            Rectangle fillFrame = new Rectangle(0, 0, fillAmount, barCharge.Height);

                            Vector2 position = Main.LocalPlayer.Center - Main.screenPosition + barInfo.Offset - new Vector2(barCharge.Width / 2f, 48f / Main.UIScale);

                            Main.spriteBatch.Draw(bar, position, bar.Frame(), barInfo.BaseColor * fade, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            Main.spriteBatch.Draw(barCharge, position, fillFrame, barInfo.FillColor * fade, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                        }

                        return true;
                    },
                    InterfaceScaleType.Game));
            }
        }
    }
}
