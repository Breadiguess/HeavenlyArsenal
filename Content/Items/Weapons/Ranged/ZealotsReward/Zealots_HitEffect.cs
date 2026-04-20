using Luminance.Core.Graphics;
using NoxusBoss.Assets;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ZealotsReward;

public class Zealots_HitEffect : MetaballType
{
    public override string MetaballAtlasTextureToUse => "NoxusBoss.BasicMetaballCircle.png";

    public override Color EdgeColor => Color.CadetBlue;

    public override bool ShouldRender => ActiveParticleCount >= 1; 

    public override Func<Texture2D>[] LayerTextures => [() => GennedAssets.Textures.GreyscaleTextures.WhitePixel];

    public override void PrepareShaderForTarget(int layerIndex)
    {
        // Store the shader in an easy to use local variable.
        var metaballShader = ShaderManager.GetShader("HeavenlyArsenal.FusionrifleHit");

        // Fetch the layer texture. This is the texture that will be overlaid over the greyscale contents on the screen.
        var layerTexture = LayerTextures[layerIndex]();

        // Calculate the layer scroll offset. This is used to ensure that the texture contents of the given metaball have parallax, rather than being static over the screen
        // regardless of world position.
        // This may be toggled off optionally by the metaball.
        var screenSize = Main.ScreenSize.ToVector2();
        var layerScrollOffset = Main.screenPosition / screenSize + CalculateManualOffsetForLayer(layerIndex);

        if (LayerIsFixedToScreen(layerIndex))
        {
            layerScrollOffset = Vector2.Zero;
        }

        // Supply shader parameter values.
        metaballShader.TrySetParameter("layerSize", layerTexture.Size());
        metaballShader.TrySetParameter("screenSize", screenSize);
        metaballShader.TrySetParameter("layerOffset", layerScrollOffset);
        metaballShader.TrySetParameter("edgeColor", EdgeColor.ToVector4());
        metaballShader.TrySetParameter("singleFrameScreenOffset", (Main.screenLastPosition - Main.screenPosition) / screenSize);

        // Supply the metaball's layer texture to the graphics device so that the shader can read it.
        metaballShader.SetTexture(layerTexture, 1, SamplerState.PointWrap);

        // Apply the metaball shader.
        metaballShader.Apply();
    }

    public override void UpdateParticle(MetaballInstance particle)
    {
        particle.Velocity = Collision.TileCollision(particle.Center, particle.Velocity, 1, 1);

       

        particle.Size *= 0.94f;
    }

    public override bool ShouldKillParticle(MetaballInstance particle)
    {
        return particle.Size <= 1f;
    }

    public override void ExtraDrawing() { }
}