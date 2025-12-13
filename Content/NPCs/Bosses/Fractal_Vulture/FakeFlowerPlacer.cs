using System.Collections.Generic;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture
{
    using CalamityMod;
    using global::HeavenlyArsenal.Content.NPCs.Bosses.FractalVulture;
    using Luminance.Assets;
    using Microsoft.Xna.Framework;
    using NoxusBoss.Content.Tiles.GenesisComponents;
    using System;
    using Terraria;
    using Terraria.DataStructures;
    using Terraria.ID;
    using Terraria.ModLoader;

    namespace HeavenlyArsenal.Systems
    {
        public class FakeFlowerPlacementSystem : ModSystem
        {


            public static void TryPlaceFlowerAroundGenesis()
            {
                int genesisType = ModContent.TileType<GenesisTile>();
                int fakeFlowerType = ModContent.TileType<FakeFlowerTile>();


                // Convert top-left to origin, matching TileObjectData.Origin

                // STEP 1 — Find every Genesis origin tile
                List<Point16> genesisOrigins = FindGenesisOrigins(genesisType);

                if (genesisOrigins.Count == 0)
                {
                    Main.NewText("No Genesis tiles found.");
                    return;
                }

                foreach (Point16 genesis in genesisOrigins)
                {
                    Main.NewText($"Scanning around Genesis at {genesis.X}, {genesis.Y}...");

                    List<Point16> spots = FindAllValidPlacementsAround(genesis);

                    if (spots.Count == 0)
                    {
                        Main.NewText("→ No suitable placement locations found.");
                        continue;
                    }

                    Main.NewText($"→ Found {spots.Count} possible flower placements.");

                    // REQUIREMENT: avoid placing within 2 tiles of the Genesis unless no other option exists
                    const int MinPreferredDistance = 3;

                    // Split placements: far (preferred) and close (fallback)
                    List<Point16> preferred = new();
                    List<Point16> tooClose = new();

                    foreach (var spot in spots)
                    {
                        // Check distance between *origins*
                        int dist = ManhattanDistance(spot, genesis);

                        if (dist >= MinPreferredDistance)
                            preferred.Add(spot);
                        else
                            tooClose.Add(spot);
                    }

                    Point16 chosen;

                    if (preferred.Count > 0)
                    {
                        // Use safe-distance placements first
                        chosen = preferred[Main.rand.Next(preferred.Count)];
                        Main.NewText($"→ Choosing a placement NOT near Genesis ({preferred.Count} valid).");
                    }
                    else
                    {
                        // If absolutely necessary, place close
                        chosen = tooClose[Main.rand.Next(tooClose.Count)];
                        Main.NewText($"→ Only close placements available ({tooClose.Count}). Using fallback.");
                    }


                    Main.NewText($"→ Chosen placement: {chosen.X}, {chosen.Y}");

                    TryPlaceFakeFlower(chosen);
                    
                    return; // stop after one genesis
                }
            }
            private static int ManhattanDistance(Point16 a, Point16 b)
            {
                return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
            }

            private static void TryPlaceFakeFlower(Point16 topLeft)
            {
                int fakeFlowerType = ModContent.TileType<FakeFlowerTile>();

                // Convert top-left to origin, matching TileObjectData.Origin
                int originX = topLeft.X + FakeFlowerTile.Width / 2;
                int originY = topLeft.Y + FakeFlowerTile.Height - 1;

                // Debug
                Vector2 worldPos = new Vector2(originX, originY).ToWorldCoordinates();
                Main.NewText($"Placing Fake Flower at origin (tiles): {originX}, {originY}  (world: {worldPos})");

                bool success = WorldGen.PlaceObject(originX, originY, fakeFlowerType, style: 0, mute: true);

                if (success)
                    Main.NewText("Fake Flower successfully placed!");
                else
                    Main.NewText("Fake Flower FAILED to place.");
            }

            private static List<Point16> FindGenesisOrigins(int genesisType)
            {
                List<Point16> list = new();

                for (int x = 0; x < Main.maxTilesX; x++)
                {
                    for (int y = 0; y < Main.maxTilesY; y++)
                    {
                        Tile t = Main.tile[x, y];
                        if (!t.HasTile || t.TileType != genesisType)
                            continue;

                        // Check if this is the ORIGIN tile
                        if (t.TileFrameX == 0 && t.TileFrameY == 0)
                            list.Add(new Point16(x, y));
                    }
                }

                return list;
            }


            private const int SearchRadius = 80;

            private static Point16? FindPlacementAround(Point16 genesis)
            {
                int fw = FakeFlowerTile.Width;
                int fh = FakeFlowerTile.Height;

                for (int dx = -SearchRadius; dx <= SearchRadius; dx++)
                {
                    for (int dy = -SearchRadius; dy <= SearchRadius; dy++)
                    {
                        if (dx * dx + dy * dy > SearchRadius * SearchRadius)
                            continue;

                        int topLeftX = genesis.X + dx;
                        int topLeftY = genesis.Y + dy;

                        if (IsRegionSuitableForFakeFlower(topLeftX, topLeftY))
                            return new Point16(topLeftX, topLeftY);
                    }
                }

                return null;
            }

            private static List<Point16> FindAllValidPlacementsAround(Point16 genesis)
            {
                List<Point16> results = new();
                int w = FakeFlowerTile.Width;
                int h = FakeFlowerTile.Height;

                for (int dx = -SearchRadius; dx <= SearchRadius; dx++)
                {
                    for (int dy = -SearchRadius; dy <= SearchRadius; dy++)
                    {
                        if (dx * dx + dy * dy > SearchRadius * SearchRadius)
                            continue; // circle mask

                        int topLeftX = genesis.X + dx;
                        int topLeftY = genesis.Y + dy;

                        if (IsRegionSuitableForFakeFlower(topLeftX, topLeftY))
                        {
                            results.Add(new Point16(topLeftX, topLeftY));
                        }
                    }
                }

                return results;
            }


            private static bool IsRegionSuitableForFakeFlower(int x, int y)
            {
                int w = FakeFlowerTile.Width;
                int h = FakeFlowerTile.Height;

                // 1. Check footprint is clear OR cuttable (grass, plants, vines, etc)
                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        int tx = x + i;
                        int ty = y + j;

                        if (!WorldGen.InWorld(tx, ty))
                            return false;

                        Tile t = Main.tile[tx, ty];

                        if (t.HasTile)
                        {
                            // Allow cuttable tiles (grass, plants, vines)
                            if (Main.tileCut[t.TileType])
                                continue;

                            // ALSO allow:
                            // - Moss
                            // - Jungle plants
                            // - Surface foliage
                            // - Pots
                            // (all of these are marked tileCut)

                            // If NOT cuttable = it's actually blocking placement.
                            return false;
                        }
                    }
                }

                // 2. Check bottom row is SOLID (anchor requirement)
                int bottomY = y + h; // the row of tiles directly below footprint

                for (int i = 0; i < w; i++)
                {
                    int tx = x + i;
                    if (!WorldGen.InWorld(tx, bottomY))
                        return false;

                    Tile below = Main.tile[tx, bottomY];

                    if (!below.HasTile || !Main.tileSolid[below.TileType])
                        return false;
                }

                return true;
            }


        }

        public class GenesisFlowerSeeder : ModItem
        {
            public override string Texture => MiscTexturesRegistry.PixelPath;
            // ^ Placeholder texture (Gold Coin). 
            // Replace with: $"{Mod.Name}/Content/Items/GenesisFlowerSeeder"



            public override void SetDefaults()
            {
                Item.width = 20;
                Item.height = 20;

                Item.useStyle = ItemUseStyleID.HoldUp;
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.UseSound = SoundID.Item4;

                Item.rare = ItemRarityID.Green;
                Item.maxStack = 1;
                Item.consumable = false;

                Item.value = Item.buyPrice(silver: 50);
            }

            public override bool? UseItem(Player player)
            {
                // Ensure this runs on the server so tile placement is synced.
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    FakeFlowerPlacementSystem.TryPlaceFlowerAroundGenesis();
                }
                else
                {

                }

                return true;
            }
        }


    }

}
