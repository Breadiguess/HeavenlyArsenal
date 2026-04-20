namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight;

public sealed class TwistedBloodBlightPlayer : ModPlayer
{
    /// <summary>
    ///     The maximum duration of the saturation gain buffer, in ticks. Equivalent to 5 seconds.
    /// </summary>
    /// <remarks>
    ///     Used as the upper bound when clamping <see cref="SaturationGainBuffer" />.
    /// </remarks>
    public const int MAX_SATURATION_GAIN_BUFFER = 5 * 60;

    /// <summary>
    ///     The maximum duration of the saturation decay buffer, in ticks. Equivalent to 5 seconds.
    /// </summary>
    /// <remarks>
    ///     Used as the upper bound when clamping <see cref="SaturationDecayBuffer" />.
    /// </remarks>
    public const int MAX_SATURATION_DECAY_BUFFER = 5 * 60;

    /// <summary>
    ///     The maximum saturation value the player can reach.
    /// </summary>
    /// <remarks>
    ///     Used as the upper bound when clamping <see cref="Saturation" />.
    /// </remarks>
    public const float MAX_SATURATION = 100f;

    private float saturation;

    private int saturationGainBuffer;

    private int saturationDecayBuffer;

    /// <summary>
    ///     Gets or sets the player's current saturation value.
    /// </summary>
    /// <value>
    ///     A value clamped between <c>0</c> and <see cref="MAX_SATURATION" />.
    /// </value>
    public float Saturation
    {
        get => saturation;
        set => saturation = Math.Clamp(value, 0f, MAX_SATURATION);
    }

    /// <summary>
    ///     Tracks how long the player has been continuously gaining saturation.
    /// </summary>
    /// <value>
    ///     A value clamped between <c>0</c> and <see cref="MAX_SATURATION_GAIN_BUFFER" />.
    /// </value>
    /// <remarks>
    ///     Typically incremented while saturation gain occurs.
    /// </remarks>
    public int SaturationGainBuffer
    {
        get => saturationGainBuffer;
        set => saturationGainBuffer = Math.Clamp(value, 0, MAX_SATURATION_GAIN_BUFFER);
    }

    /// <summary>
    ///     Tracks how long the player has gone without gaining saturation.
    /// </summary>
    /// <value>
    ///     A value clamped between <c>0</c> and <see cref="MAX_SATURATION_DECAY_BUFFER" />.
    /// </value>
    /// <remarks>
    ///     Typically incremented while no saturation gain occurs.
    /// </remarks>
    public int SaturationDecayBuffer
    {
        get => saturationDecayBuffer;
        set => saturationDecayBuffer = Math.Clamp(value, 0, MAX_SATURATION_DECAY_BUFFER);
    }

    /// <summary>
    ///     Gets or sets the rate at which saturation decays over time.
    /// </summary>
    /// <value>
    ///     Defaults to <c>0.5f</c>.
    /// </value>
    /// <remarks>
    ///     Applied to <see cref="Saturation" /> through <see cref="UpdateSaturationDecayBuffer" /> once
    ///     <see cref="SaturationDecayBuffer" /> reaches <see cref="MAX_SATURATION_DECAY_BUFFER" />.
    /// </remarks>
    public float SaturationDecayRate { get; set; } = 0.5f;

    /// <summary>
    ///     Gets or sets the multiplier applied to damage when calculating saturation gain.
    /// </summary>
    /// <value>
    ///     Defaults to <c>0.1f</c>.
    /// </value>
    /// <remarks>
    ///     Applied to damage from hitting or being hit through <see cref="OnHitNPC" />,
    ///     <see cref="OnHitByNPC" />, and <see cref="OnHitByProjectile" />.
    /// </remarks>
    public float SaturationGainMultiplier { get; set; } = 0.1f;

    public override void PostUpdateMiscEffects()
    {
        base.PostUpdateMiscEffects();

        UpdateSaturationGainBuffer();
        UpdateSaturationDecayBuffer();
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitNPC(target, hit, damageDone);

        var progress = saturationGainBuffer / (float)MAX_SATURATION_GAIN_BUFFER;
        var multiplier = 1f + progress * progress;

        Saturation += damageDone * SaturationGainMultiplier * multiplier;

        SaturationGainBuffer = MAX_SATURATION_GAIN_BUFFER;
        SaturationDecayBuffer = 0;
    }

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
    {
        base.OnHitByNPC(npc, hurtInfo);

        var progress = saturationGainBuffer / (float)MAX_SATURATION_GAIN_BUFFER;
        var multiplier = 1f + progress * progress;

        Saturation += hurtInfo.Damage * SaturationGainMultiplier * multiplier;

        SaturationGainBuffer = MAX_SATURATION_GAIN_BUFFER;
        SaturationDecayBuffer = 0;
    }

    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
    {
        base.OnHitByProjectile(proj, hurtInfo);

        var progress = saturationGainBuffer / (float)MAX_SATURATION_GAIN_BUFFER;
        var multiplier = 1f + progress * progress;

        Saturation += hurtInfo.Damage * SaturationGainMultiplier * multiplier;

        SaturationGainBuffer = MAX_SATURATION_GAIN_BUFFER;
        SaturationDecayBuffer = 0;
    }

    private void UpdateSaturationGainBuffer()
    {
        if (SaturationGainBuffer <= 0)
        {
            return;
        }

        SaturationGainBuffer--;
        SaturationDecayBuffer = 0;
    }

    private void UpdateSaturationDecayBuffer()
    {
        if (SaturationGainBuffer > 0)
        {
            return;
        }

        SaturationDecayBuffer++;

        if (SaturationDecayBuffer < MAX_SATURATION_DECAY_BUFFER)
        {
            return;
        }

        Saturation -= SaturationDecayRate;
    }
}