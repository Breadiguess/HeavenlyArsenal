using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Subworlds;

public class ForgottenShrineSystem : ModSystem
{
    public override void PreUpdateEntities()
    {
        if (!SubworldSystem.IsActive<ForgottenShrineSubworld>())
            return;

        ManagedScreenFilter mistShader = ShaderManager.GetFilter("HeavenlyArsenal.ForgottenShrineMistShader");
        mistShader.TrySetParameter("targetSize", Main.ScreenSize.ToVector2());
        mistShader.TrySetParameter("oldScreenPosition", Main.screenPosition);
        mistShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        mistShader.TrySetParameter("mistColor", new Color(84, 74, 154).ToVector4());
        mistShader.TrySetParameter("noiseAppearanceThreshold", 0.3f);
        mistShader.TrySetParameter("mistCoordinatesZoom", new Vector2(1f, 0.4f));
        mistShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
        mistShader.SetTexture(TileTargetManagers.LiquidTarget, 2, SamplerState.LinearClamp);
        mistShader.Activate();

        ModContent.GetInstance<ForgottenShrineBackground>().ShouldBeActive = true;
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (!SubworldSystem.IsActive<ForgottenShrineSubworld>())
            return;

        tileColor = new Color(0.6f, 0.4f, 0.4f);
    }
}
