using HeavenlyArsenal.Content.Subworlds;
using HeavenlyArsenal.Content.Tiles.ForgottenShrine;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Idol;

public partial class IdolSummoningRitualSystem : ModSystem
{
    private static float MaxLanternSpeedup => 25f;

    private void Perform_BatheWorldInCrimson()
    {
        float animationCompletion = LumUtils.InverseLerp(0f, 240f, Timer);
        ForgottenShrineDarknessSystem.Darkness = MaxDarknessFactor;
        ShiftSkyPalette(animationCompletion);

        float backglowFadeIn = MathF.Pow(LumUtils.InverseLerp(0f, 0.732f, animationCompletion), 1.65f);
        ForgottenShrineBackground.LanternSpeed = MathHelper.Lerp(1f, MaxLanternSpeedup, animationCompletion);
        ForgottenShrineBackground.MoonBackglow = MathHelper.SmoothStep(0f, 1.5f, backglowFadeIn);
        ForgottenShrineBackground.LanternsCanSpawn = animationCompletion < 0.8f;

        HarshWindSoundVolume = LumUtils.InverseLerp(0f, 0.6f, animationCompletion);
        BaseWindSoundVolume = LumUtils.Saturate(1.2f - HarshWindSoundVolume);

        IdolStatueManager.WaterFlowCutoffInterpolant = 1f;
        IdolStatueManager.ExtraDrawAction = p => DrawGlowOnStatueEye(1f, p);
    }

    private static void ShiftSkyPalette(float shiftInterpolant)
    {
        ForgottenShrineBackground.AltSkyGradient =
        [
            new Color(255, 0, 31),
            new Color(142, 20, 32),
            new Color(53, 21, 33),
            new Color(4, 0, 0),
        ];
        ForgottenShrineBackground.AltSkyGradientInterpolant = shiftInterpolant;

        Main.windSpeedCurrent = (shiftInterpolant + 0.001f) * 1.75f;

        float swirlVariance = MathHelper.Lerp(0.2f, 0.3f, LumUtils.Cos01(Main.GlobalTimeWrappedHourly * 4f));
        ManagedScreenFilter tintShader = ShaderManager.GetFilter("HeavenlyArsenal.SwirlyScreenTintShader");
        tintShader.TrySetParameter("additiveLight", new Vector4(0.45f, 0.03f, 0f, 0f) * shiftInterpolant);
        tintShader.TrySetParameter("swirlVariance", swirlVariance);
        tintShader.TrySetParameter("windDirection", Main.windSpeedCurrent.NonZeroSign());
        tintShader.TrySetParameter("moonPosition", ForgottenShrineBackground.MoonPosition / WotGUtils.ViewportSize);
        tintShader.TrySetParameter("moonGlowDistance", ForgottenShrineBackground.MoonBackglow * 0.4f);
        tintShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
        tintShader.Activate();
    }
}
