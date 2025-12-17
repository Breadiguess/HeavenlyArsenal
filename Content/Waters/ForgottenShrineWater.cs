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

public sealed class ForgottenShrineWater : ModWaterStyle
{
    public override void Load()
    {
        base.Load();
        
        IL_LiquidRenderer.DrawNormalLiquids += LiquidRenderer_DrawNormalLiquids_Edit;
    }

    public override int GetSplashDust()
    {
        return DustID.BloodWater;
    }

    public override int GetDropletGore()
    {
        return GoreID.WaterDripBlood;
    }

    public override Color BiomeHairColor()
    {
        return new Color(137, 18, 32);
    }
    
    public override int ChooseWaterfallStyle()
    {
        return ModContent.GetInstance<ForgottenShrineWaterfall>().Slot;
    }
    
    public override void LightColorMultiplier(ref float r, ref float g, ref float b)
    {
        base.LightColorMultiplier(ref r, ref g, ref b);
        
        const float brightness = 1.1f;
        
        r = brightness;
        g = brightness;
        b = brightness;
    }
    
    private static void LiquidRenderer_DrawNormalLiquids_Edit(ILContext il)
    {
        try
        {
            var cursor = new ILCursor(il);

            const string drawTileInWaterActionName = "DrawTileInWater";
            
            if (!cursor.TryGotoNext(MoveType.Before, static c => c.MatchLdarg2(), static c => c.MatchLdloc3(), static c => c.MatchLdloc(4), static c => c.MatchCall<Main>(drawTileInWaterActionName)))
            {
                throw new InvalidOperationException($"Could not match {drawTileInWaterActionName}.");
            }

            const string liquidDrawCacheTypeName = "LiquidDrawCache";
            const string typeFieldName = "Type";

            var typeField = typeof(LiquidRenderer).GetNestedType(liquidDrawCacheTypeName, BindingFlags.NonPublic).GetRuntimeField(typeFieldName);

            if (typeField == null)
            {
                throw new MissingFieldException(liquidDrawCacheTypeName, typeFieldName);
            }

            cursor.Emit(OpCodes.Ldloc_3);
            cursor.Emit(OpCodes.Ldloc, 4);
            cursor.Emit(OpCodes.Ldloc_2);
            cursor.Emit(OpCodes.Ldfld, typeField);
            cursor.Emit(OpCodes.Ldloca, 9);

            cursor.EmitDelegate(InterpolateLiquidColor);
        }
        catch (Exception exception)
        {
            HeavenlyArsenal.Instance.Logger.Error($"Failed to apply IL patch to {nameof(LiquidRenderer.DrawNormalLiquids)}", exception);
        }
    }

    private static void InterpolateLiquidColor(int x, int y, int liquidType, ref VertexColors liquidColor)
    {
        var slot = ModContent.GetInstance<ForgottenShrineWater>().Slot;
        
        if (liquidType != LiquidID.Water || Main.liquidAlpha[slot] <= 0f)
        {
            return;
        }
        
        var progress = Main.liquidAlpha[slot] * 0.85f;
        
        // TODO: Isn't this just Color.White?
        var color = new Color(255, 255, 255);

        liquidColor.TopLeftColor = Color.Lerp(liquidColor.TopLeftColor, color, progress);
        liquidColor.TopRightColor = Color.Lerp(liquidColor.TopRightColor, color, progress);
        liquidColor.BottomLeftColor = Color.Lerp(liquidColor.BottomLeftColor, color, progress);
        liquidColor.BottomRightColor = Color.Lerp(liquidColor.BottomRightColor, color, progress);
    }
}
