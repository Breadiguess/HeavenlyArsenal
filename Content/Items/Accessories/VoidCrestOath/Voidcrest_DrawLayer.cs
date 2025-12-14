using HeavenlyArsenal.Common.Utilities;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.RenderTargets;
using Terraria.DataStructures;
using static NoxusBoss.Core.Utilities.Utilities;

namespace HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath;

public class Voidcrest_DrawLayer : PlayerDrawLayer
{
    /// <summary>
    ///     The render target responsible for rendering the Halo for players.
    /// </summary>
    /// <remarks> directly stolen from portal skirt. thanks, Lucille!!</remarks>
    public static InstancedRequestableTarget HaloTarget { get; private set; }

    public override bool IsHeadLayer => true;

    public override void Load()
    {
        On_PlayerDrawLayers.DrawPlayer_08_Backpacks += What;
        //On_Player.UpdateItemDye += FindSkirtItemDyeShader;

        HaloTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(HaloTarget);
    }

    /// <summary>
    ///     I love code!
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="drawinfo"></param>
    private void What(On_PlayerDrawLayers.orig_DrawPlayer_08_Backpacks orig, ref PlayerDrawSet drawinfo)
    {
        if (drawinfo.drawPlayer.GetValueRef<bool>(VoidCrestOath.HaloEquippedVariableName))
        {
            Draw(ref drawinfo);

            return;
        }

        orig(ref drawinfo);
    }

    public override Position GetDefaultPosition()
    {
        return new AfterParent(PlayerDrawLayers.Backpacks);
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        var Owner = drawInfo.drawPlayer;
        var Void = Owner.GetModPlayer<VoidCrestOathPlayer>();

        if (Void.Hide)
        {
            return false;
        }

        if (Void.Vanity || Void.voidCrestOathEquipped)
        {
            return true;
        }

        return false;
    }

    private static void RenderIntoTarget()
    {
        if (GennedAssets.Textures.Noise.WavyBlotchNoise.Uninitialized || !GennedAssets.Textures.Noise.WavyBlotchNoise.Asset.IsLoaded)
        {
            return;
        }

        if (GennedAssets.Textures.Noise.BurnNoise.Uninitialized || !GennedAssets.Textures.Noise.BurnNoise.Asset.IsLoaded)
        {
            return;
        }

        var Rot = MathHelper.ToRadians(45);
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null);
        var glow = AssetDirectory.Textures.BigGlowball.Value;

        Main.EntitySpriteDraw
        (
            glow,
            ViewportSize * 0.5f,
            glow.Frame(),
            Color.Red with
            {
                A = 200
            },
            Rot,
            glow.Size() * 0.5f,
            new Vector2(0.1f, 0.05f) * 0.275f,
            0
        );

        var innerRiftTexture = GennedAssets.Textures.FirstPhaseForm.RiftInnerTexture.Value;
        var riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
        riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.1f);
        riftShader.TrySetParameter("baseCutoffRadius", 0.24f);
        riftShader.TrySetParameter("swirlOutwardnessExponent", 0.151f);
        riftShader.TrySetParameter("swirlOutwardnessFactor", 6f);
        riftShader.TrySetParameter("vanishInterpolant", 0.001f);
        riftShader.TrySetParameter("edgeColor", new Vector4(1f, 0.08f, 0.08f, 1f));
        riftShader.TrySetParameter("edgeColorBias", 0.15f);
        riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
        riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 2, SamplerState.AnisotropicWrap);
        riftShader.Apply();

        Main.spriteBatch.Draw
            (innerRiftTexture, ViewportSize * 0.5f, null, new Color(77, 0, 2), 0f, innerRiftTexture.Size() * 0.5f, ViewportSize / innerRiftTexture.Size() * new Vector2(0.1f, 0.05f), 0, 0f);

        Main.spriteBatch.End();
    }

    public void drawRift(ref PlayerDrawSet drawInfo)
    {
        var interp = drawInfo.drawPlayer.GetModPlayer<VoidCrestOathPlayer>().ResourceInterp;
        var val = (float)Math.Sin(Main.GlobalTimeWrappedHourly / 2) * 2 + 1;
        var Rot = drawInfo.drawPlayer.fullRotation + MathHelper.ToRadians(drawInfo.drawPlayer.direction * -45);
        var position = drawInfo.HeadPosition() + new Vector2(0, -20f + val / 2).RotatedBy(Rot) + new Vector2(0, 1 * val);
        var particleDrawCenter = position + new Vector2(0f, 0f);
        var glow = AssetDirectory.Textures.BigGlowball.Value;

        var RiftScale = new Vector2(0.1f, 0.05f) * interp;
        var GlowScale = RiftScale * 0.6f;

        //maybe i'll try again later.
        //Texture2D a = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Accessories/VoidCrestOath/CleanedUpOmega").Value;
        //Main.EntitySpriteDraw(a, position, null, Color.Crimson, MathHelper.ToRadians(drawInfo.drawPlayer.direction * 20), a.Size() * 0.5f, new Vector2(0.05f, 0.1f), 0);

        Main.EntitySpriteDraw
        (
            glow,
            particleDrawCenter,
            glow.Frame(),
            Color.Red with
            {
                A = 200
            },
            Rot,
            glow.Size() * 0.5f,
            GlowScale,
            0
        );

        var innerRiftTexture = AssetDirectory.Textures.VoidLake.Value;
        var edgeColor = new Color(1f, 0.06f, 0.06f);
        var timeOffset = Main.myPlayer * 2.5552343f;

        var riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
        riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.2f + timeOffset);
        riftShader.TrySetParameter("baseCutoffRadius", 0.24f * interp);
        riftShader.TrySetParameter("swirlOutwardnessExponent", 0.2f);
        riftShader.TrySetParameter("swirlOutwardnessFactor", 3f);
        riftShader.TrySetParameter("vanishInterpolant", 0.01f * (1 - interp));
        riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
        riftShader.TrySetParameter("edgeColorBias", 0.1f);
        riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
        riftShader.SetTexture(GennedAssets.Textures.Noise.FireNoiseA, 2, SamplerState.AnisotropicWrap);
        riftShader.Apply();

        var flip = drawInfo.drawPlayer.direction == 1 ? SpriteEffects.FlipHorizontally : 0;
        Main.spriteBatch.Draw(innerRiftTexture, particleDrawCenter, null, Color.White, Rot + MathHelper.Pi, innerRiftTexture.Size() * 0.5f, RiftScale, flip, 0);

        Main.spriteBatch.ResetToDefault();
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (drawInfo.shadow != 0f || drawInfo.drawPlayer.dead)
        {
            return;
        }

        HaloTarget.Request(208, 100, drawInfo.drawPlayer.whoAmI, RenderIntoTarget);

        if (!HaloTarget.TryGetTarget(drawInfo.drawPlayer.whoAmI, out var portalTexture) || portalTexture is null)
        {
            return;
        }

        var val = (float)Math.Sin(Main.GlobalTimeWrappedHourly / 2) * 2;
        var Rot = drawInfo.drawPlayer.fullRotation + MathHelper.ToRadians(drawInfo.drawPlayer.direction * -45);
        var position = drawInfo.HeadPosition() + new Vector2(0, -20f + val).RotatedBy(Rot);

        var rift = new DrawData(portalTexture, position, null, Color.White, Rot + MathHelper.Pi, portalTexture.Size() * 0.5f, 1f, 0);
        //drawInfo.DrawDataCache.Add(rift);
        drawRift(ref drawInfo);
    }
}