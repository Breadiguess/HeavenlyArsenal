using HeavenlyArsenal.Common.utils;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Tiles.TileEntities;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class TEPlacedOfuda : ModTileEntity, IClientSideTileEntityUpdater
{
    private Rope rope;

    private static readonly Asset<Texture2D>[] ofudaTextures =
    [
        ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/PlacedOfuta1"),
        ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/PlacedOfuta2"),
        ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/PlacedOfuta3"),
    ];

    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<PlacedOfuda>();
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

    private static float WidthFunction(float completionRatio) => LumUtils.InverseLerp(0f, 0.33f, completionRatio) * 10f;

    private static Color ColorFunction(float completionRatio) => Color.White;

    public void ClientSideUpdate()
    {
        Vector2 start = Position.ToWorldCoordinates(0f, 0f);
        Vector2 end = start + Vector2.UnitY * 93f;
        if (rope is null)
        {
            int segmentCount = 10;
            rope = new Rope(start, end, segmentCount, start.Distance(end) / segmentCount, Vector2.UnitY * 0.3f, 5);
            rope.segments[^1].pinned = false;
            rope.tileCollide = true;
            rope.Settle();
        }

        rope.segments[0].position = start;
        for (int i = 0; i < rope.segments.Length; i++)
        {
            Rope.RopeSegment segment = rope.segments[i];
            if (segment.pinned)
                continue;

            foreach (Player player in Main.ActivePlayers)
            {
                float playerProximityInterpolant = LumUtils.InverseLerp(40f, 12f, player.Distance(segment.position));
                segment.position.X += player.velocity.X * playerProximityInterpolant * 0.06f;
            }
        }
        rope.Update();
    }

    /// <summary>
    /// Renders this ofuda.
    /// </summary>
    public void Render()
    {
        if (!Position.ToWorldCoordinates().WithinRange(WotGUtils.ViewportArea.Center() + Main.screenPosition, 3000f))
            return;
        if (rope is null)
            return;

        int ofudaVariant = ID % ofudaTextures.Length;
        Texture2D texture = ofudaTextures[ofudaVariant].Value;
        RenderTrail(texture);
    }

    private void RenderTrail(Texture2D texture)
    {
        PlacedOfudaRenderer.OfudaTarget.Request(240, 240, ID, () =>
        {
            ManagedShader overlayShader = ShaderManager.GetShader("HeavenlyArsenal.ShrineOfudaShader");
            overlayShader.TrySetParameter("exposure", 1f);
            overlayShader.TrySetParameter("screenSize", WotGUtils.ViewportSize);
            overlayShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
            overlayShader.TrySetParameter("pixelationLevels", new Vector2(75f, 37.5f));
            overlayShader.SetTexture(texture, 1, SamplerState.PointClamp);

            Vector2 offsetToRTCenter = -rope.segments[0].position + WotGUtils.ViewportSize * 0.5f + Main.screenPosition;
            PrimitiveSettings settings = new PrimitiveSettings(WidthFunction, ColorFunction, Shader: overlayShader, OffsetFunction: _ => offsetToRTCenter, UseUnscaledMatrix: true, ProjectionAreaWidth: 240, ProjectionAreaHeight: 240);
            PrimitiveRenderer.RenderTrail(rope.GetPoints(), settings, 30);
        });
        if (PlacedOfudaRenderer.OfudaTarget.TryGetTarget(ID, out RenderTarget2D? target) && target is not null)
        {
            ManagedShader pixelationShader = ShaderManager.GetShader("Luminance.PixelationShader");
            pixelationShader.TrySetParameter("pixelationFactor", Vector2.One * 1.5f / target.Size());
            pixelationShader.Apply();

            Vector2 drawPosition = Position.ToWorldCoordinates() - Main.screenPosition - Vector2.UnitY * 8f;
            Main.spriteBatch.Draw(target, drawPosition, null, Color.White, 0f, target.Size() * 0.5f, 1f, 0, 0f);
        }
    }

    // Sync the tile entity the moment it is place on the server.
    // This is done to cause it to register among all clients.
    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
}
