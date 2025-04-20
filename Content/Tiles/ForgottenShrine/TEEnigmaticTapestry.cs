using HeavenlyArsenal.Core.Physics.ClothManagement;
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

public class TEEnigmaticTapestry : ModTileEntity, IClientSideTileEntityUpdater
{
    private ClothSimulation cloth;

    private static float ClothPointSpacing => 13f;

    private static readonly Asset<Texture2D>[] tapestryTextures =
    [
        ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/EnigmaticTapestry1"),
        ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/EnigmaticTapestry2")
    ];

    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        return tile.HasTile && tile.TileType == ModContent.TileType<EnigmaticTapestry>();
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

    public void ClientSideUpdate()
    {
        Vector3 anchorPosition = new Vector3(Position.ToWorldCoordinates(0f, 0f), 0f);
        if (!Main.LocalPlayer.WithinRange(Position.ToWorldCoordinates(), 3000f))
            return;

        cloth ??= new ClothSimulation(anchorPosition, 21, 13, ClothPointSpacing, 50f, 0.018f);

        for (int i = 0; i < 10; i++)
        {
            for (int x = 0; x < cloth.Width; x++)
            {
                for (int y = 0; y < 2; y++)
                    ConstrainParticle(anchorPosition, cloth.particleGrid[x, y]);
            }

            Vector3 playerPosition3 = new Vector3(Main.LocalPlayer.Center, 0f);
            for (int y = 0; y < cloth.Height; y++)
            {
                for (int x = 0; x < cloth.Width; x++)
                {
                    float pushInterpolant = LumUtils.InverseLerp(36f, 19f, Vector3.Distance(playerPosition3, cloth.particleGrid[x, y].Position));
                    Vector3 pushForce = new Vector3(Main.LocalPlayer.velocity * pushInterpolant * 0.75f, 0f);
                    cloth.particleGrid[x, y].AddForce(pushForce);
                }
            }

            cloth.Simulate(0.051f, false, Vector3.UnitY * 3.5f);
        }
    }

    private void ConstrainParticle(Vector3 anchor, ClothPoint? point)
    {
        if (point is null)
            return;

        float width = cloth.Width * ClothPointSpacing;
        float xInterpolant = point.X / (float)cloth.Width;
        point.Position = anchor + new Vector3((xInterpolant - 0.5f) * width, 0f, LumUtils.Convert01To010(xInterpolant) * 25f);
        point.IsFixed = true;
    }

    /// <summary>
    /// Renders this ofuda.
    /// </summary>
    public void Render()
    {
        if (!Position.ToWorldCoordinates().WithinRange(WotGUtils.ViewportArea.Center() + Main.screenPosition, 3000f))
            return;
        if (cloth is null)
            return;

        int ofudaVariant = ID % tapestryTextures.Length;
        Texture2D texture = tapestryTextures[ofudaVariant].Value;
        RenderTapestry(texture);
    }

    private void RenderTapestry(Texture2D texture)
    {
        EnigmaticTapestryRenderer.TapestryTarget.Request(400, 400, ID, () =>
        {
            Vector2 drawOffset = -Position.ToWorldCoordinates(0f, 0f) + WotGUtils.ViewportSize * 0.5f;
            Matrix world = Matrix.CreateTranslation(drawOffset.X, drawOffset.Y, 0f);
            Matrix projection = Matrix.CreateOrthographicOffCenter(0f, WotGUtils.ViewportSize.X, WotGUtils.ViewportSize.Y, 0f, -1000f, 1000f);

            ManagedShader clothShader = ShaderManager.GetShader("HeavenlyArsenal.ShrineTapestryShader");
            clothShader.SetTexture(texture, 1, SamplerState.LinearWrap);
            clothShader.TrySetParameter("transform", world * projection);
            clothShader.Apply();

            cloth.Render();
        });
        if (EnigmaticTapestryRenderer.TapestryTarget.TryGetTarget(ID, out RenderTarget2D? target) && target is not null)
        {
            ManagedShader pixelationShader = ShaderManager.GetShader("HeavenlyArsenal.ShrineTapestryPostProcessingShader");
            pixelationShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
            pixelationShader.TrySetParameter("screenSize", WotGUtils.ViewportSize);
            pixelationShader.TrySetParameter("pixelationFactor", Vector2.One * 2f / target.Size());
            pixelationShader.SetTexture(LightingMaskTargetManager.LightTarget, 1);
            pixelationShader.Apply();

            Vector2 drawPosition = Position.ToWorldCoordinates() - Main.screenPosition + Vector2.UnitX * 4f;
            Main.spriteBatch.Draw(target, drawPosition, null, Color.White, 0f, target.Size() * 0.5f, 1f, 0, 0f);
        }
    }

    // Sync the tile entity the moment it is place on the server.
    // This is done to cause it to register among all clients.
    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
}
