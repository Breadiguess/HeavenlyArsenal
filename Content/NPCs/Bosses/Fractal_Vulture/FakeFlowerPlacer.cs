using System.Collections.Generic;
using HeavenlyArsenal.Content.NPCs.Bosses.FractalVulture;
using Luminance.Assets;
using NoxusBoss.Content.Tiles.GenesisComponents;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture
{
    namespace HeavenlyArsenal.Systems
    {
        public class FakeFlowerPlacementSystem : ModSystem
        {
            private const int SearchRadius = 80;

            public static void TryPlaceFlowerAroundGenesis()
            {
                var genesisType = ModContent.TileType<GenesisTile>();
                var fakeFlowerType = ModContent.TileType<FakeFlowerTile>();

                // Convert top-left to origin, matching TileObjectData.Origin

                // STEP 1 — Find every Genesis origin tile
                var genesisOrigins = FindGenesisOrigins(genesisType);

                if (genesisOrigins.Count == 0)
                {
                    Main.NewText("No Genesis tiles found.");

                    return;
                }

                foreach (var genesis in genesisOrigins)
                {
                    Main.NewText($"Scanning around Genesis at {genesis.X}, {genesis.Y}...");

                    var spots = FindAllValidPlacementsAround(genesis);

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
                        var dist = ManhattanDistance(spot, genesis);

                        if (dist >= MinPreferredDistance)
                        {
                            preferred.Add(spot);
                        }
                        else
                        {
                            tooClose.Add(spot);
                        }
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
                var fakeFlowerType = ModContent.TileType<FakeFlowerTile>();

                // Convert top-left to origin, matching TileObjectData.Origin
                var originX = topLeft.X + FakeFlowerTile.Width / 2;
                var originY = topLeft.Y + FakeFlowerTile.Height - 1;

                // Debug
                var worldPos = new Vector2(originX, originY).ToWorldCoordinates();
                Main.NewText($"Placing Fake Flower at origin (tiles): {originX}, {originY}  (world: {worldPos})");

                var success = WorldGen.PlaceObject(originX, originY, fakeFlowerType, style: 0, mute: true);

                if (success)
                {
                    Main.NewText("Fake Flower successfully placed!");
                }
                else
                {
                    Main.NewText("Fake Flower FAILED to place.");
                }
            }

            private static List<Point16> FindGenesisOrigins(int genesisType)
            {
                List<Point16> list = new();

                for (var x = 0; x < Main.maxTilesX; x++)
                {
                    for (var y = 0; y < Main.maxTilesY; y++)
                    {
                        var t = Main.tile[x, y];

                        if (!t.HasTile || t.TileType != genesisType)
                        {
                            continue;
                        }

                        // Check if this is the ORIGIN tile
                        if (t.TileFrameX == 0 && t.TileFrameY == 0)
                        {
                            list.Add(new Point16(x, y));
                        }
                    }
                }

                return list;
            }

            private static Point16? FindPlacementAround(Point16 genesis)
            {
                var fw = FakeFlowerTile.Width;
                var fh = FakeFlowerTile.Height;

                for (var dx = -SearchRadius; dx <= SearchRadius; dx++)
                {
                    for (var dy = -SearchRadius; dy <= SearchRadius; dy++)
                    {
                        if (dx * dx + dy * dy > SearchRadius * SearchRadius)
                        {
                            continue;
                        }

                        var topLeftX = genesis.X + dx;
                        var topLeftY = genesis.Y + dy;

                        if (IsRegionSuitableForFakeFlower(topLeftX, topLeftY))
                        {
                            return new Point16(topLeftX, topLeftY);
                        }
                    }
                }

                return null;
            }

            private static List<Point16> FindAllValidPlacementsAround(Point16 genesis)
            {
                List<Point16> results = new();
                var w = FakeFlowerTile.Width;
                var h = FakeFlowerTile.Height;

                for (var dx = -SearchRadius; dx <= SearchRadius; dx++)
                {
                    for (var dy = -SearchRadius; dy <= SearchRadius; dy++)
                    {
                        if (dx * dx + dy * dy > SearchRadius * SearchRadius)
                        {
                            continue; // circle mask
                        }

                        var topLeftX = genesis.X + dx;
                        var topLeftY = genesis.Y + dy;

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
                var w = FakeFlowerTile.Width;
                var h = FakeFlowerTile.Height;

                // 1. Check footprint is clear OR cuttable (grass, plants, vines, etc)
                for (var i = 0; i < w; i++)
                {
                    for (var j = 0; j < h; j++)
                    {
                        var tx = x + i;
                        var ty = y + j;

                        if (!WorldGen.InWorld(tx, ty))
                        {
                            return false;
                        }

                        var t = Main.tile[tx, ty];

                        if (t.HasTile)
                        {
                            // Allow cuttable tiles (grass, plants, vines)
                            if (Main.tileCut[t.TileType])
                            {
                                continue;
                            }

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
                var bottomY = y + h; // the row of tiles directly below footprint

                for (var i = 0; i < w; i++)
                {
                    var tx = x + i;

                    if (!WorldGen.InWorld(tx, bottomY))
                    {
                        return false;
                    }

                    var below = Main.tile[tx, bottomY];

                    if (!below.HasTile || !Main.tileSolid[below.TileType])
                    {
                        return false;
                    }
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

                return true;
            }
        }
    }
}