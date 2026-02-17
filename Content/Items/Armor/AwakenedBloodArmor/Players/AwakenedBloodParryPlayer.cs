using HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Projectiles;
using NoxusBoss.Assets;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players;

public sealed class AwakenedBloodParryPlayer : ModPlayer
{
    public const int BLOOD_THORN_PROJECTILE_COUNT = 6;
    
    public const int BLOOD_THORN_PARRY_TIME = 30;
    
    /// <summary>
    ///     Gets the remaining parry duration, in frames.
    /// </summary>
    /// <remarks>
    ///     This value is decremented once per frame in <see cref="PostUpdateMiscEffects"/> until it reaches <c>0</c>.
    /// </remarks>
    public int Time { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the player is currently parrying.
    /// </summary>
    /// <remarks>
    ///     Equivalent to checking whether <see cref="Time"/> is greater than <c>0</c>.
    /// </remarks>
    public bool Parrying => Time > 0;

    public override void PostUpdateMiscEffects()
    {
        base.PostUpdateMiscEffects();
        
        if (Time <= 0)
        {
            return;
        }
        
        Time--;
    }

    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
    {
        base.OnHitByProjectile(proj, hurtInfo);
        
        if (!Parrying)
        {
            return;
        }
        
        Parry(BLOOD_THORN_PROJECTILE_COUNT);
    }

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
    {
        base.OnHitByNPC(npc, hurtInfo);
        
        if (!Parrying)
        {
            return;
        }
        
        Parry(BLOOD_THORN_PROJECTILE_COUNT);
    }

    public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
    {
        base.ModifyHitByNPC(npc, ref modifiers);
        
        if (!Parrying)
        {
            return;
        }
        
        modifiers.FinalDamage *= 0f;
    }

    public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
    {
        base.ModifyHitByProjectile(proj, ref modifiers);
        
        if (!Parrying)
        {
            return;
        }
        
        modifiers.FinalDamage *= 0f;
    }

    public void Parry(int amount)
    {
        for (var i = 0; i < amount; i++)
        {
            var Velocity = new Vector2(1f, 0).RotatedBy(i / 6f * MathHelper.TwoPi).RotatedByRandom(MathHelper.ToRadians(12f));
            
            Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.Center, Velocity, ModContent.ProjectileType<AwakenedBlood_ParryThorn>(), 100, 0f, Player.whoAmI);
        }
        
        SoundEngine.PlaySound(GennedAssets.Sounds.Common.MediumBloodSpill, Player.Center);
    }
}