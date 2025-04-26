using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Idol;

public partial class IdolSummoningRitualSystem : ModSystem
{
    /// <summary>
    /// The intensity of rumbles created by this ritual.
    /// </summary>
    public float RumbleInterpolant
    {
        get;
        set;
    }

    private void Perform_WorldRumble()
    {
        int rumbleBuildupTime = 180;
        RumbleInterpolant = MathHelper.SmoothStep(0f, 1f, Timer / (float)rumbleBuildupTime).Cubed();

        if (Timer >= rumbleBuildupTime)
            SwitchState(IdolSummoningRitualState.OpenStatueEye);
    }
}
