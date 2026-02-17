using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;

namespace HeavenlyArsenal.Content.Biomes;

public sealed class RiftEclipseBestiaryBackgroundSystem : ModSystem
{
    public static RenderTarget2D Target { get; private set; }

    public override void Load()
    {
        base.Load();

        On_Main.CheckMonoliths += Main_CheckMonoliths_Hook;
        
        Main.QueueMainThreadAction
        (
            static () =>
            {
                var device = Main.graphics.GraphicsDevice;

                Target = new RenderTarget2D(device, Main.screenWidth, Main.screenHeight);
            }
        );
        
        Main.OnResolutionChanged += Main_OnResolutionChanged_Hook;
    }

    public override void Unload()
    {
        base.Unload();

        Main.QueueMainThreadAction
        (
            static () =>
            {
                Target?.Dispose();
                Target = null;
            }
        );
        
        Main.OnResolutionChanged -= Main_OnResolutionChanged_Hook;
    }
    
    private static void Main_OnResolutionChanged_Hook(Vector2 size)
    {
        Main.QueueMainThreadAction
        (
            () =>
            {
                var device = Main.graphics.GraphicsDevice;
                
                Target?.Dispose();
                Target = new RenderTarget2D(device, (int)size.X, (int)size.Y);
            }
        );
    }

    private static void Main_CheckMonoliths_Hook(On_Main.orig_CheckMonoliths orig)
    {
        if (Target == null || Target.IsDisposed == true)
        {
            return;
        }

        var batch = Main.spriteBatch;
        var device = Main.graphics.GraphicsDevice;

        batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null);

        device.SetRenderTarget(Target);
        device.Clear(Color.Transparent);

        DrawBloodMoon();

        device.SetRenderTarget(null);

        batch.End();

        orig();
    }

    private static void DrawBloodMoon()
    {
        var texture = GennedAssets.Textures.Noise.WavyBlotchNoise.Value;

        var shader = ShaderManager.GetShader("NoxusBoss.AvatarRiftShapeShader");

        shader.TrySetParameter("time", Main.GlobalTimeWrappedHourly);
        shader.TrySetParameter("baseCutoffRadius", 0.2f);
        shader.TrySetParameter("swirlOutwardnessExponent", 0.151f);
        shader.TrySetParameter("swirlOutwardnessFactor", 3.67f);
        shader.TrySetParameter("vanishInterpolant", 0.1f);

        shader.SetTexture(texture, 1, SamplerState.AnisotropicWrap);
        shader.SetTexture(texture, 2, SamplerState.AnisotropicWrap);

        shader.Apply();

        var flare = MiscTexturesRegistry.ShineFlareTexture.Value;

        var scale = new Vector2(1.5f, 0.5f) * 0.8f;
        var position = Main.screenPosition;

        var dummy = new NPC();

        dummy.SetDefaults(ModContent.NPCType<AvatarRift>());

        dummy.As<AvatarRift>().PreDraw(Main.spriteBatch, Vector2.Zero, Color.White);
    }
}