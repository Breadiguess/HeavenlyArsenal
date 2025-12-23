using System.Linq;
using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture;
using Luminance.Assets;
using NoxusBoss.Assets;
using NoxusBoss.Core.SoundSystems;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace HeavenlyArsenal.Content.NPCs.Bosses.FractalVulture;

public class FakeFlowerTile : ModTile, ICustomPlacementSound
{
    /// <summary>
    ///     The tiled width of the seedling.
    /// </summary>
    public const int Width = 9;

    /// <summary>
    ///     The tiled height of the seedling.
    /// </summary>
    public const int Height = 4;

    public SoundStyle PlaceSound => GennedAssets.Sounds.Common.TwinkleMuffled;

    /// <summary>
    ///     The actual texture of the seedling.
    /// </summary>
    public static NoxusBoss.Assets.LazyAsset<Texture2D> ActualTexture { get; private set; }

    public override string Texture => MiscTexturesRegistry.PixelPath; //GetAssetPath("Content/Tiles/GenesisComponents", Name);

    public override void SetStaticDefaults()
    {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.Width = Width;
        TileObjectData.newTile.Height = Height;
        TileObjectData.newTile.Origin = new Point16(Width / 2, Height - 1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, TileObjectData.newTile.Height).ToArray();
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        HitSound = null;
        AddMapEntry(new Color(99, 87, 142));
    }

    public override bool CanDrop(int i, int j)
    {
        return false;
    }

    public override bool CreateDust(int i, int j, ref int type)
    {
        return false;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        var centerTileX = i + Width / 2;
        var centerTileY = j + Height / 2;

        // Convert the center tile to world position
        var centerWorld = new Vector2(centerTileX, centerTileY).ToWorldCoordinates();
        centerWorld.Y -= 16f;
        if(voidVulture.Myself is null)
        NPC.NewNPCDirect(null, centerWorld, ModContent.NPCType<voidVulture>());
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.6f;
        g = 0.6f;
        b = 0.85f;
    }

    public override void NearbyEffects(int i, int j, bool closer)
    {
        var t = Main.tile[i, j];

        if (t.TileFrameX == (int)(Width * 0.5f) * 18 && t.TileFrameY == (Height - 1) * 18)
        {
            ModContent.GetInstance<FakeFlowerTileRender>().AddPoint(new Point(i, j));
        }
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        return false;
    }
}