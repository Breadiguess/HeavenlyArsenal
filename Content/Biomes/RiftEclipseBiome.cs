using Terraria.Localization;

namespace HeavenlyArsenal.Content.Biomes;

public sealed class RiftEclipseBiome : ModBiome
{
    public override Color? BackgroundColor { get; } = Color.Black;

    public override string BestiaryIcon { get; } = $"{nameof(HeavenlyArsenal)}/Content/Biomes/RiftEclipseBiome";
    
    public override float GetWeight(Player player)
    {
        return 0f;
    }
    
    public override bool IsBiomeActive(Player player)
    {
        return false;
    }
}