using System.Reflection;
using CalamityMod.BiomeManagers;
using CalamityMod.Systems;
using SubworldLibrary;

namespace HeavenlyArsenal.Common.Compatibility;

public sealed class CalamitySubworldCompatibilitySystem : ModSystem
{
    public delegate void orig_WorldMiscUpdateSystem_HandleTileGrowth();

    public delegate bool orig_SulphurousSeaBiome_IsBiomeActive(SulphurousSeaBiome self, Player player);

    public override void Load()
    {
        base.Load();

        var handleTileGrowthMethod = typeof(WorldMiscUpdateSystem).GetMethod("HandleTileGrowth", BindingFlags.Public | BindingFlags.Static);

        if (handleTileGrowthMethod == null)
        {
            throw new MissingMethodException(nameof(WorldMiscUpdateSystem), "HandleTileGrowth");
        }

        var isBiomeActiveMethod = typeof(SulphurousSeaBiome).GetMethod("IsBiomeActive", BindingFlags.Public | BindingFlags.Instance);

        if (isBiomeActiveMethod == null)
        {
            throw new MissingMethodException(nameof(SulphurousSeaBiome), nameof(SulphurousSeaBiome.IsBiomeActive));
        }

        MonoModHooks.Add(handleTileGrowthMethod, WorldMiscUpdateSystem_HandleTileGrowth_Hook);
        MonoModHooks.Add(isBiomeActiveMethod, SulphurousSeaBiome_IsBiomeActive_Hook);
    }

    private static void WorldMiscUpdateSystem_HandleTileGrowth_Hook(orig_WorldMiscUpdateSystem_HandleTileGrowth orig)
    {
        if (SubworldSystem.IsActive<ForgottenShrineSubworld>())
        {
            return;
        }

        orig();
    }

    private static bool SulphurousSeaBiome_IsBiomeActive_Hook(orig_SulphurousSeaBiome_IsBiomeActive orig, SulphurousSeaBiome self, Player player)
    {
        return SubworldSystem.IsActive<ForgottenShrineSubworld>() ? false : orig(self, player);
    }
}