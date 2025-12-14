using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Armor.ShintoArmor;

internal class ShadeTeleportDrawlayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition()
    {
        return new AfterParent(PlayerDrawLayers.FaceAcc);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (drawInfo.drawPlayer.GetModPlayer<ShintoArmorPlayer>().isShadeTeleporting)
        {
            // drawInfo.DrawDataCache.Clear();
        }
    }
}

public class ShadeDrawSystem : ModSystem
{
    public static RenderTarget2D ShadeLayer { get; set; }

    public override void Load()
    {
        On_Main.CheckMonoliths += DrawShade;
    }

    private void ComposePlayer(Player player)
    {
        // Create fresh caches for this snapshot
        List<DrawData> drawData = new();
        List<int> dust = new();
        List<int> gore = new();

        var drawInfo = new PlayerDrawSet();

        drawInfo.BoringSetup
        (
            player,
            drawData,
            dust,
            gore,
            player.Center - new Vector2(0, Main.GlobalTimeWrappedHourly) - Main.screenPosition,
            0f,
            0f,
            Vector2.Zero
        );

        PlayerLoader.ModifyDrawInfo(ref drawInfo);

        // Run all player layers (vanilla + modded)
        foreach (var layer in PlayerDrawLayerLoader.Layers)
        {
            if (layer.GetDefaultVisibility(drawInfo))
            {
                layer.DrawWithTransformationAndChildren(ref drawInfo);
            }
        }

        // Finally draw everything from the cache
        foreach (var data in drawInfo.DrawDataCache)
        {
            data.Draw(Main.spriteBatch);
        }
    }

    private void DrawShade(On_Main.orig_CheckMonoliths orig)
    {
        if (ShadeLayer == null || ShadeLayer.IsDisposed)
        {
            ShadeLayer = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
        }
        else if (ShadeLayer.Size() != new Vector2(Main.screenWidth, Main.screenHeight))
        {
            Main.QueueMainThreadAction
            (
                () =>
                {
                    ShadeLayer.Dispose();
                    ShadeLayer = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                }
            );

            return;
        }

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, default, null);

        Main.graphics.GraphicsDevice.SetRenderTarget(ShadeLayer);
        Main.graphics.GraphicsDevice.Clear(Color.Transparent);

        foreach (var player in Main.player.Where(n => n.active && !n.dead && n.GetModPlayer<ShintoArmorPlayer>().SetActive && n.GetModPlayer<ShintoArmorPlayer>().isShadeTeleporting))
        {
            ComposePlayer(player);
        }

        Main.graphics.GraphicsDevice.SetRenderTarget(null);

        Main.spriteBatch.End();

        orig();
    }
}