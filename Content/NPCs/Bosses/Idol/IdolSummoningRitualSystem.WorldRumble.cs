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

        if (Timer >= rumbleBuildupTime)
            SwitchState(IdolSummoningRitualState.OpenStatueEye);
    }
}
