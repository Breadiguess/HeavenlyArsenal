using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class EnigmaticTapestry : ModTile
{
    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        AddMapEntry(new Color(204, 204, 204));
    }

    public override bool CanExplode(int i, int j) => false;

    public override bool CanReplace(int i, int j, int tileTypeBeingPlaced) => false;

    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;
}
