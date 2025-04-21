using Microsoft.Xna.Framework;
using Terraria;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public class SetPlayerSpawnPointPass : GenPass
{
    public SetPlayerSpawnPointPass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Setting the player's spawn position.";

        Main.spawnTileX = 50;
        Main.spawnTileY = LumUtils.FindGroundVertical(new Point(Main.spawnTileX, Main.maxTilesY - 10)).Y;
    }
}
