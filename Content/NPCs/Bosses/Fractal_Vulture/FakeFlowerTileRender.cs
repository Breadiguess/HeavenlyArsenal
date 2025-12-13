using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Items.GenesisComponents;
using NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles;
using NoxusBoss.Content.Tiles.GenesisComponents;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Content.Tiles.GenesisComponents.SyntheticSeedlingTile;

namespace HeavenlyArsenal.Content.NPCs.Bosses.FractalVulture;


public class FakeFlowerTileRender : FakeFlowerRender
{
    public override int ItemID => ModContent.ItemType<FakeFlowerDebugItem>();

    public override int TileID => ModContent.TileType<FakeFlowerTile>();

    public override void UpdatePoint(Point p)
    {
       
    }

    public override void InstaceRenderFunction(bool disappearing, float growthInterpolant, float growthInterpolantModified, int i, int j, SpriteBatch spriteBatch)
    {

        // Draw the flower.
        Vector2 drawPosition = new Vector2((i + 0.5f) * 16f, j * 16f + 18f) - Main.screenPosition;
        Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Bosses/Fractal_Vulture/PlantForm").Value;
        Vector2 scale = new Vector2(MathF.Pow(growthInterpolantModified, 1.7f), growthInterpolantModified);

        Rectangle frame = texture.Frame(1, 2);
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(default, BlendState.Opaque, SamplerState.PointClamp, default, default);
        Main.spriteBatch.Draw(texture, drawPosition, null, Color.White, 0f, texture.Size() * new Vector2(0.5f, 1f), scale, 0, 0f);
        Main.spriteBatch.ResetToDefault();
    }
}
