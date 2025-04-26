using HeavenlyArsenal.Content.Subworlds;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Idol;

public partial class IdolSummoningRitualSystem : ModSystem
{
    private static float MaxDarknessFactor => 1.33f;

    private void Perform_OpenStatueEye()
    {
        int eyeOpenTime = 150;
        float animationCompletion = Timer / (float)eyeOpenTime;
        ForgottenShrineDarknessSystem.Darkness = MathHelper.Lerp(ForgottenShrineDarknessSystem.StandardDarkness, MaxDarknessFactor, animationCompletion);

        if (animationCompletion >= 1f)
            SwitchState(IdolSummoningRitualState.BatheWorldInCrimson);
    }
}
