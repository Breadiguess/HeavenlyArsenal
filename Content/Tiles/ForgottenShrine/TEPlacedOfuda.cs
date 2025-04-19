using HeavenlyArsenal.Common.utils;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Tiles.TileEntities;
using NoxusBoss.Core.Graphics.LightingMask;
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

    private static float WidthFunction(float completionRatio) => LumUtils.InverseLerp(0f, 0.28f, completionRatio) * 10f;

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

            // TODO -- You know, I've just been using LocalPlayer for these sorts of things, but really such processes should loop over every player so they can apply forces individually.
            float proximityInterpolant = LumUtils.InverseLerp(40f, 12f, Main.LocalPlayer.Distance(segment.position));
            segment.position.X += Main.LocalPlayer.velocity.X * proximityInterpolant * 0.06f;
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
        RenderTrail(texture, false);
    }

    private void RenderTrail(Texture2D texture, bool pixelated)
    {
        ManagedShader overlayShader = ShaderManager.GetShader("HeavenlyArsenal.LitPrimitiveTrailOverlayShader");
        overlayShader.TrySetParameter("exposure", 1f);
        overlayShader.TrySetParameter("screenSize", WotGUtils.ViewportSize);
        overlayShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        overlayShader.TrySetParameter("pixelationLevels", new Vector2(75f, 37.5f));
        overlayShader.SetTexture(texture, 1, SamplerState.LinearClamp);
        overlayShader.SetTexture(LightingMaskTargetManager.LightTarget, 2);

        PrimitiveSettings settings = new PrimitiveSettings(WidthFunction, ColorFunction, Shader: overlayShader, Pixelate: pixelated);
        PrimitiveRenderer.RenderTrail(rope.GetPoints(), settings, 30);
    }

    // Sync the tile entity the moment it is place on the server.
    // This is done to cause it to register among all clients.
    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
}
