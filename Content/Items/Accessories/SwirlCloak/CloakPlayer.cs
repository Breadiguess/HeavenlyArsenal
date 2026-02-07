using CalamityMod;
using NoxusBoss.Core.AdvancedProjectileOwnership;

namespace HeavenlyArsenal.Content.Items.Accessories.SwirlCloak;

internal class CloakPlayer : ModPlayer
{
    public int MaxTrappedProjectiles = 13;

    public bool Active;

    public override void PostUpdateMiscEffects() { }

    public void CreateSwirlVortex()
    {
        var stealth = Player.Calamity().rogueStealth;
        float MaxStealth = 1;
        Main.NewText($"Attempting to create vortex! Stealth: {stealth}");

        if (stealth > 0 && Player.ownedProjectileCounts[ModContent.ProjectileType<SwirlCloak_Veil>()] < 1)
        {
            Player.NewProjectileBetter(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<SwirlCloak_Veil>(), 600, 10);
        }
    }

    public void ApplyStealthBoost()
    {
        if (Active && Player.velocity.Length() < 0.01f)
        {
            Player.Calamity().accStealthGenBoost += 2;
        }
    }

    public override void ResetEffects()
    {
        Active = false;
    }
}