namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.ShieldGuy;

internal partial class PerversionOfFaith
{
    public enum Behavior
    {
        debug,

        FindShieldTarget,

        ProtectTarget
    }

    public Behavior CurrentState;
}