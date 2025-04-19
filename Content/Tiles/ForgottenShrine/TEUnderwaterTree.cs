using HeavenlyArsenal.Content.Subworlds.Generation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class TEUnderwaterTree : ModTileEntity
{
    private static readonly Asset<Texture2D>[] treeTextures =
    [
        ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/UnderwaterTree1"),
        ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/UnderwaterTree2"),
        ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/UnderwaterTree3")
    ];

    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<UnderwaterTree>();
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
    {
        // If in multiplayer, tell the server to place the tile entity and DO NOT place it yourself. That would mismatch IDs.
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendTileSquare(Main.myPlayer, i, j, 1, 1);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            return -1;
        }
        return Place(i, j);
    }

    /// <summary>
    /// Renders this tree.
    /// </summary>
    public void Render()
    {
        if (!Position.ToWorldCoordinates().WithinRange(WotGUtils.ViewportArea.Center() + Main.screenPosition, 3000f))
            return;

        int treeVariant = (Position.X * 4 + Position.Y * 17) % treeTextures.Length;
        Texture2D texture = treeTextures[treeVariant].Value;

        UnifiedRandom rng = new UnifiedRandom(Position.X + 74 + Position.Y * 113);
        float baseLength = ForgottenShrineGenerationConstants.WaterDepth * 16f;
        float treeLength = baseLength + rng.NextFloat(100f, 193f);
        float rotation = rng.NextFloatDirection() * 0.4f + MathHelper.PiOver2;

        // This ensures that rotations do not interfere with the rule that the tree should reach treeLength pixels upwards.
        float treeLengthAccountingForRotation = treeLength / MathF.Sin(rotation);
        float treeScale = treeLengthAccountingForRotation / texture.Height;

        Vector2 drawOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

        Vector2 drawPosition = new Vector2(Position.X * 16 - Main.screenPosition.X, Position.Y * 16 - Main.screenPosition.Y + 18f);
        SpriteEffects direction = rng.NextBool() ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        Color color = new Color(43, 11, 27);
        Main.spriteBatch.Draw(texture, drawPosition, null, color, rotation - MathHelper.PiOver2, new Vector2(0.5f, 1f) * texture.Size(), treeScale, direction, 0f);
    }

    // Sync the tile entity the moment it is place on the server.
    // This is done to cause it to register among all clients.
    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
}
