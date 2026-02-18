using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;
using Terraria;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Waters;


public class ForgottenShrineWater : ModWaterStyle
{
    public override int ChooseWaterfallStyle() => ModContent.Find<ModWaterfallStyle>("HeavenlyArsenal/ForgottenShrineWaterfall").Slot;

    public override int GetSplashDust() => DustID.BloodWater;

    public override int GetDropletGore() => GoreID.WaterDripBlood;

    public override Color BiomeHairColor() => new Color(137, 18, 32);

    public override void Load() => IL_LiquidRenderer.DrawNormalLiquids += ModifyWaterOpacity;

    private void ModifyWaterOpacity(ILContext il)
    {
        return;
        ILCursor cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(MoveType.Before, c => c.MatchLdarg2(), c => c.MatchLdloc3(), c => c.MatchLdloc(4), c => c.MatchCall<Main>("DrawTileInWater")))
        {
            Mod.Logger.Error("Could not locate the liquid vertex colors for drawing.");
            return;
        }

        cursor.Emit(OpCodes.Ldloc_3);
        cursor.Emit(OpCodes.Ldloc, 4);
        cursor.Emit(OpCodes.Ldloc_2);
        cursor.Emit(OpCodes.Ldfld, typeof(LiquidRenderer).GetNestedType("LiquidDrawCache", BindingFlags.NonPublic).GetRuntimeField("Type"));
        cursor.Emit(OpCodes.Ldloca, 9);

        cursor.EmitDelegate((int x, int y, int liquidType, ref VertexColors liquidColor) =>
        {
            if (liquidType == LiquidID.Water && Main.liquidAlpha[Slot] > 0f)
            {
                float colorFade = Main.liquidAlpha[Slot] * 0.85f;
                Color idealColor = new Color(255, 255, 255);

                liquidColor.TopLeftColor = Color.Lerp(liquidColor.TopLeftColor, idealColor, colorFade);
                liquidColor.TopRightColor = Color.Lerp(liquidColor.TopRightColor, idealColor, colorFade);
                liquidColor.BottomLeftColor = Color.Lerp(liquidColor.BottomLeftColor, idealColor, colorFade);
                liquidColor.BottomRightColor = Color.Lerp(liquidColor.BottomRightColor, idealColor, colorFade);
            }
        });
    }

    public override void LightColorMultiplier(ref float r, ref float g, ref float b)
    {
        float brightness = 1.1f;
        r = brightness;
        g = brightness;
        b = brightness;
    }
}
