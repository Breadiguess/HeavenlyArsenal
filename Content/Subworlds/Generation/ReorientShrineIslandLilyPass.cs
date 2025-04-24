using HeavenlyArsenal.Content.Subworlds.Generation.Bridges;
using HeavenlyArsenal.Content.Tiles.ForgottenShrine;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public class ReorientShrineIslandLilyPass : GenPass
{
    public ReorientShrineIslandLilyPass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Moving lilies.";

        int grassID = ModContent.TileType<SacredGrass>();
        int left = BaseBridgePass.BridgeGenerator.Right + ForgottenShrineGenerationHelpers.LakeWidth + BaseBridgePass.GenerationSettings.DockWidth;
        int right = left + ForgottenShrineGenerationHelpers.ShrineIslandWidth;
        foreach (SpiderLilyData lily in ModContent.GetInstance<SpiderLilyManager>().TileObjects)
        {
            if (lily.Position.X < left * 16 || lily.Position.X > right * 16)
                continue;

            int idealMoveDownLevels = 0;
            int totalMoveDownLevels = 0;
            if (WorldGen.genRand.NextBool())
                idealMoveDownLevels = WorldGen.genRand.Next(4);

            for (int i = 0; i < idealMoveDownLevels; i++)
            {
                int previousLilyY = lily.Position.Y;
                ShoveDownLily(lily, grassID);

                if (lily.Position.Y == previousLilyY)
                {
                    totalMoveDownLevels = i;
                    break;
                }
            }

            lily.ZPosition = (4f - totalMoveDownLevels);
        }
    }

    private static void ShoveDownLily(SpiderLilyData lily, int grassID)
    {
        for (int i = 1; i < 10; i++)
        {
            Tile below = Framing.GetTileSafely(lily.Position.X / 16, lily.Position.Y / 16 + i);
            bool topFrames = below.TileFrameY <= 18 && below.TileFrameX >= 18 && below.TileFrameX <= 54;
            if (below.HasTile && below.TileType == grassID && topFrames)
            {
                lily.Position.Y += i * 16;
                return;
            }
        }
    }
}
