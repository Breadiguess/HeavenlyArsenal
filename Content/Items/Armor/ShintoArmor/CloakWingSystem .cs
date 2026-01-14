using HeavenlyArsenal.Common.Utilities;
using HeavenlyArsenal.Core.Physics.ClothManagement;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.DataStructures;
using static Luminance.Common.Utilities.Utilities;
using Player = Terraria.Player;

namespace HeavenlyArsenal.Content.Items.Armor.ShintoArmor;

internal class ShintoArmorCapePlayer : ModPlayer
{
    private const int frontSize = 200;

    private const int backSize = 600;

    private float ExistenceTimer;

    private RenderTarget2D RobeTarget;

    private RenderTarget2D RobeMapTarget;

    public ClothSimulation Robe { get; set; } = new(Vector3.Zero, 10, 8, 3f, 50f, 0.1f);

    private static event Action<SpriteBatch> drawToTarget;

    public override void Load()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            On_Main.CheckMonoliths += DrawAllTargets;

            On_Player.UpdateTouchingTiles += UpdateCloak;
        }
    }

    private void UpdateCloak(On_Player.orig_UpdateTouchingTiles orig, Player self)
    {
        orig(self);
        UpdateCloth(self);
        ExistenceTimer++;
    }

    private void DrawAllTargets(On_Main.orig_CheckMonoliths orig)
    {
        if (Main.netMode != NetmodeID.Server)
        {
            drawToTarget?.Invoke(Main.spriteBatch);

            Main.spriteBatch.GraphicsDevice.SetRenderTarget(null);
            Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);
        }

        orig();
    }

    public override void Initialize()
    {
        Main.QueueMainThreadAction
        (
            () =>
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    drawToTarget += DrawRobeToTarget;
                    RobeMapTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, backSize, backSize);
                    RobeTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, backSize, backSize);
                }
            }
        );
    }

    public void DrawRobeToTarget(SpriteBatch spritebatch)
    {
        if (Player != null && Main.netMode != NetmodeID.Server)
        {
            if (!IsReady() || !ShaderManager.HasFinishedLoading) // God damn Luminance you slowpoke
            {
                return;
            }

            Main.spriteBatch.GraphicsDevice.SetRenderTarget(RobeMapTarget);
            Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);

            var robePosition = Player.Center + new Vector2(4 * Player.direction, -50f * Player.gravDir).RotatedBy(Player.fullRotation);

            var world = Matrix.CreateTranslation(-robePosition.X + backSize / 2, -robePosition.Y + backSize / 2, 0f);

            var projection = Matrix.CreateOrthographicOffCenter(0, backSize, 600, 0, -1000, 1000);
            var matrix = world * projection;

            var clothShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssasinRobeShader");
            clothShader.TrySetParameter("opacity", InverseLerp(60f, 120f, ExistenceTimer));
            clothShader.TrySetParameter("transform", matrix);
            clothShader.Apply();
            Robe.Render();

            Main.spriteBatch.End();

            Main.spriteBatch.GraphicsDevice.SetRenderTarget(RobeTarget);
            Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);

            Vector3[] palette =
            [
                new(1.5f),
                new(0f, 1f, 1.2f),
                new(1f, 0f, 0f)
            ];

            var overlayShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssassinColorProcessingShader");
            overlayShader.TrySetParameter("eyeScale", 1f);
            overlayShader.TrySetParameter("gradient", palette);
            overlayShader.TrySetParameter("gradientCount", palette.Length);
            overlayShader.TrySetParameter("textureSize", RobeMapTarget.Size() * 2);
            overlayShader.TrySetParameter("edgeColor", Color.Red.ToVector4());
            overlayShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
            overlayShader.Apply();

            Main.spriteBatch.Draw(RobeMapTarget, new Vector2(backSize / 2), null, Color.Black, 0f, RobeMapTarget.Size() * 0.5f, 2f, 0, 0);

            Main.spriteBatch.End();
        }
    }

    public bool IsReady()
    {
        return RobeTarget != null;
    }

    public DrawData GetRobeTarget()
    {
        return new DrawData(RobeTarget, Vector2.Zero + new Vector2(0, Player.gfxOffY), null, Color.White, -Player.fullRotation, RobeTarget.Size() * 0.5f, 1f, 0);
    }

    public override void PostUpdateMiscEffects()
    {
        ExistenceTimer++;
    }

    private static void UpdateCloth(Player player)
    {
        if (player == null)
        {
            return;
        }

        var capePlayer = player.GetModPlayer<ShintoArmorCapePlayer>();

        if (capePlayer == null)
        {
            return;
        }

        capePlayer.Robe.DampeningCoefficient = 0.17f;

        var steps = 10;
        var windSpeed = Math.Clamp(Main.WindForVisuals * 8f, -1.3f, 0f);
        var robePosition = player.Center + new Vector2(0, -50 * player.gravDir).RotatedBy(player.fullRotation);
        robePosition += Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];
        var wind = Vector3.UnitX * (AperiodicSin(capePlayer.ExistenceTimer * 0.029f) * 0.67f + windSpeed) * 1.74f;

        for (var i = 0; i < steps; i++)
        {
            for (var x = 0; x < capePlayer.Robe.Width; x++)
            {
                for (var y = 0; y < 2; y++)
                {
                    capePlayer.ConstrainParticle(robePosition + new Vector2((6 - x) * player.direction, 0), capePlayer.Robe.particleGrid[x, y], 0f);
                }
            }

            capePlayer.Robe.Simulate(0.06f, false, Vector3.UnitY * (5f * player.gravDir) + wind * player.direction);
        }
    }

    private void ConstrainParticle(Vector2 anchor, ClothPoint? point, float angleOffset)
    {
        if (point is null)
        {
            return;
        }

        point.Position = new Vector3(anchor, 0f);
        point.IsFixed = false;
    }
}

public class AntiShadowCloak_DrawLayer : PlayerDrawLayer
{
    public override bool IsHeadLayer => false;

    public override Position GetDefaultPosition()
    {
        return new AfterParent(PlayerDrawLayers.BackAcc);
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return drawInfo.drawPlayer.body == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorBreastplate), EquipType.Body);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        var capePlayer = drawInfo.drawPlayer.GetModPlayer<ShintoArmorCapePlayer>();

        if (!capePlayer.IsReady() || drawInfo.shadow > 0f)
        {
            return;
        }

        drawInfo.drawPlayer.GetModPlayer<ShintoArmorPlayer>().ShadowVeil = true;

        var data = capePlayer.GetRobeTarget();
        data.position = drawInfo.BodyPosition() + new Vector2(2 * drawInfo.drawPlayer.direction, ((drawInfo.drawPlayer.gravDir < 0 ? 11 : 0) + -8) * drawInfo.drawPlayer.gravDir);
        data.color = Color.White;
        data.effect = Main.GameViewMatrix.Effects;
        data.shader = drawInfo.cBody;

        //Main.NewText($"Position: {data.position}", Color.AntiqueWhite);
        drawInfo.DrawDataCache.Add(data);
    }
}