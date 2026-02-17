using HeavenlyArsenal.Common.Ui;
using NoxusBoss.Assets;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players;

public sealed class AwakenedBloodPlayer : ModPlayer
{
    /// <summary>
    ///     The default maximum amount of blood and clot resources the player can have.
    /// </summary>
    public const int DEFAULT_MAX_RESOURCE = 100;

    private int blood;

    private int clot;

    /// <summary>
    ///     Gets or sets whether the Awakened Blood armor set bonus is currently active for the player.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     Gets or sets whether the Blood Boost is currently active for the player.
    /// </summary>
    public bool BloodBoostActive { get; set; }

    /// <summary>
    ///     Gets or sets the duration, in frames, that the Blood Boost remains active before it starts to
    ///     rapidly drain the player's blood resource.
    /// </summary>
    /// <remarks>
    ///     Defaults to 6 seconds (<c>360</c> frames).
    /// </remarks>
    public int BloodBoostSink { get; set; } = 6 * 60;

    /// <summary>
    ///     Gets or sets the total time, in frames, that the Blood Boost has been active for the player.
    /// </summary>
    public int BloodBoostTotalTime { get; set; }

    /// <summary>
    ///     Gets or sets the timer, in frames, used to control the rate at which the player loses blood
    ///     while the Blood Boost is active.
    /// </summary>
    public int BloodBoostDrainTimer { get; set; }

    /// <summary>
    ///     Gets or sets the multiplier applied to incoming damage to determine how much blood the player
    ///     loses.
    /// </summary>
    public float BloodLossMultiplier { get; set; } = 0.1f;

    /// <summary>
    ///     Gets or sets the player's active form of the Awakened Blood Armor set bonus.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="AwakenedBloodForm.Offense" />.
    /// </remarks>
    public AwakenedBloodForm Form { get; set; } = AwakenedBloodForm.Offense;

    /// <summary>
    ///     Gets or sets the timer, in frames, used to control the rate at which the player loses clot.
    /// </summary>
    public int ClotDecayTimer { get; set; }

    /// <summary>
    ///     Gets or sets the timer, in frames, used to control the rate at which the player gains blood.
    /// </summary>
    public int BloodGainTimer { get; set; }

    /// <summary>
    ///     Gets or sets the player's current blood resource amount.
    /// </summary>
    /// <remarks>
    ///     The value is clamped between <c>0</c> and <see cref="MaxBlood" />.
    /// </remarks>
    public int Blood
    {
        get => blood;
        set => blood = (int)MathHelper.Clamp(value, 0, MaxBlood);
    }

    /// <summary>
    ///     Gets or sets the maximum amount of blood the player can have.
    /// </summary>
    public int MaxBlood { get; set; } = DEFAULT_MAX_RESOURCE;

    /// <summary>
    ///     Gets or sets the player's current clot resource amount.
    /// </summary>
    /// <remarks>
    ///     The value is clamped between <c>0</c> and <see cref="MaxClot" />.
    /// </remarks>
    public int Clot
    {
        get => clot;
        set => clot = (int)MathHelper.Clamp(value, 0, MaxClot);
    }

    /// <summary>
    ///     Gets or sets the maximum amount of clot the player can have.
    /// </summary>
    public int MaxClot { get; set; } = DEFAULT_MAX_RESOURCE;

    /// <summary>
    ///     Gets the total amount of blood and clot resources the player currently has.
    /// </summary>
    /// <remarks>
    ///     Equivalent of <see cref="Blood" /> + <see cref="Clot" />.
    /// </remarks>
    public int Combined => Blood + Clot;

    public override void ResetEffects()
    {
        base.ResetEffects();

        Enabled = false;
    }

    public override void PreUpdate()
    {
        base.PreUpdate();

        UpdateGainTimer();

        if (!Enabled)
        {
            return;
        }

        UpdateDisplayBars();
    }

    public override void PostUpdate()
    {
        base.PostUpdate();

        if (!Enabled)
        {
            return;
        }

        UpdateForm();

        UpdateBloodBoost();
        UpdateBloodConversion();
    }

    public override void ArmorSetBonusActivated()
    {
        base.ArmorSetBonusActivated();

        if (!Enabled)
        {
            return;
        }

        switch (Form)
        {
            case AwakenedBloodForm.Offense:
                ActivateOffense();
                break;
            case AwakenedBloodForm.Defense:
                ActivateDefense();
                break;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitNPC(target, hit, damageDone);

        if (!Enabled || BloodGainTimer > 0)
        {
            return;
        }

        ApplyBloodGain(5);
    }

    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (!Enabled || BloodGainTimer > 0)
        {
            return;
        }

        ApplyBloodGain(5);
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (!Enabled || BloodGainTimer > 0)
        {
            return;
        }

        ApplyBloodGain(5);
    }

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
    {
        base.OnHitByNPC(npc, hurtInfo);

        if (!Enabled)
        {
            return;
        }

        ApplyBloodLoss(in hurtInfo);
    }

    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
    {
        base.OnHitByProjectile(proj, hurtInfo);

        if (!Enabled)
        {
            return;
        }

        ApplyBloodLoss(in hurtInfo);
    }

    /// <summary>
    ///     Reduces the player's blood based on incoming damage.
    /// </summary>
    /// <param name="hurtInfo">The <see cref="Player.HurtInfo" /> used to determine blood loss.</param>
    /// <remarks>
    ///     The amount of blood lost is calculated as <c>hurtInfo.Damage * BloodLossMultiplier</c>. The
    ///     resulting value is applied through <see cref="Blood" />, which enforces clamping.
    /// </remarks>
    public void ApplyBloodLoss(in Player.HurtInfo hurtInfo)
    {
        Blood -= (int)(hurtInfo.Damage * BloodLossMultiplier);
    }

    /// <summary>
    ///     Attempts to grant blood to the player.
    /// </summary>
    /// <param name="amount"> The amount of blood to add.</param>
    /// <remarks>
    ///     Blood gain is prevented while <see cref="BloodGainTimer" /> is greater than <c>0</c> or if the
    ///     player's <see cref="Combined" /> resources have reached <see cref="DEFAULT_MAX_RESOURCE" />.
    ///     When successful, blood is increased and <see cref="ClotDecayTimer" /> is incremented.
    /// </remarks>
    public void ApplyBloodGain(int amount)
    {
        BloodGainTimer = 20;

        if (BloodGainTimer > 0 || Combined >= DEFAULT_MAX_RESOURCE)
        {
            return;
        }

        Blood += amount;

        ClotDecayTimer++;
    }
    
    private void ActivateOffense()
    {
        var value = 76;

        if (value > 75 && !BloodBoostActive)
        {
            BloodBoostDrainTimer = 0;
            BloodBoostActive = true;

            SoundEngine.PlaySound(in GennedAssets.Sounds.Avatar.BloodCry);
        }
    }
    
    private void ActivateDefense()
    {
        Clot = 0;

        Player.Heal(Clot);
    }

    private void UpdateGainTimer()
    {
        if (BloodGainTimer <= 0)
        {
            return;
        }

        BloodGainTimer--;
    }

    private void UpdateDisplayBars()
    {
        var bloodPercent = Blood / (float)MaxBlood;
        var clotPercent = Clot / (float)MaxClot;
        var bloodClotPercent = (Blood + Clot) / (float)(MaxBlood + MaxClot);

        WeaponBar.DisplayBar(Color.AntiqueWhite, Color.Crimson, bloodPercent, 150, 0, new Vector2(0, -20));
        WeaponBar.DisplayBar(Color.Crimson, Color.AntiqueWhite, clotPercent, 150, 0, new Vector2(0, -30));
        WeaponBar.DisplayBar(Color.HotPink, Color.Silver, bloodClotPercent, 150, 1, new Vector2(0, -40));
    }

    private void UpdateForm()
    {
        switch (Form)
        {
            case AwakenedBloodForm.Offense:
                UpdateOffense();
                break;
            case AwakenedBloodForm.Defense:
                UpdateDefense();
                break;
        }
    }

    private void UpdateOffense()
    {
        const int defenseBonus = 75;

        Player.statDefense -= defenseBonus;

        const float knockback = 1f;

        const int count = 2;
        const int damage = 600;

        var type = ModContent.ProjectileType<BloodNeedle>();

        if (Player.ownedProjectileCounts[type] >= count)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            var projectile = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, type, damage, knockback);

            projectile.localAI[0] = i + 1f;
        }
    }

    private void UpdateDefense()
    {
        const int defenseBonus = 25;

        Player.statDefense += defenseBonus;

        if (Clot <= 0 || Player.statLife >= Player.statLifeMax2)
        {
            return;
        }

        Clot = 0;

        var amount = Clot * 4;

        Player.Heal(amount);
    }

    private void UpdateBloodBoost()
    {
        if (!BloodBoostActive)
        {
            return;
        }

        var gate = BloodBoostTotalTime < BloodBoostSink ? 4 : 2;

        Player.GetDamage(DamageClass.Generic) += 0.55f;
        Player.GetArmorPenetration(DamageClass.Generic) += 15;
        Player.GetCritChance(DamageClass.Generic) += 10;

        if (Blood > 0 && BloodBoostDrainTimer > gate)
        {
            Blood--;
            BloodBoostDrainTimer = 0;
        }

        BloodBoostDrainTimer++;
        BloodBoostTotalTime++;

        if (Blood > 0)
        {
            return;
        }

        BloodBoostActive = false;
    }

    private void UpdateBloodConversion()
    {
        ClotDecayTimer++;

        var max = Form == AwakenedBloodForm.Defense ? 60 : 180;

        if (ClotDecayTimer < max || Combined >= DEFAULT_MAX_RESOURCE)
        {
            return;
        }

        var amount = (int)Math.Round(DEFAULT_MAX_RESOURCE * (Blood / (float)DEFAULT_MAX_RESOURCE)) / 4;

        Blood -= amount;
        Clot += amount;

        ClotDecayTimer = 0;
    }
}