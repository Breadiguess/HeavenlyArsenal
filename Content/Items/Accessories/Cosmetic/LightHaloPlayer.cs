namespace HeavenlyArsenal.Content.Items.Accessories.Cosmetic;

public sealed class LightHaloPlayer : ModPlayer
{
    /// <summary>
    ///     Gets or sets whether the Light Halo effects are enabled.
    /// </summary>
    public bool Enabled { get; set; }

    public override void ResetEffects()
    {
        base.ResetEffects();
        
        Enabled = false;
    }
}