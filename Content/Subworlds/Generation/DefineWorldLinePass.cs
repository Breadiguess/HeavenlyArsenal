using Terraria;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public class DefineWorldLinePass : GenPass
{
    public DefineWorldLinePass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Defining world lines.";

        // Define the position of the world lines.
        Main.worldSurface = Main.maxTilesY - 8;
        Main.rockLayer = Main.maxTilesY - 9;
    }
}
