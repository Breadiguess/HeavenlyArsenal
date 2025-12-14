using Luminance.Core.Graphics;

namespace HeavenlyArsenal.Content.Particles.Metaballs;

public class AntishadowBlob : MetaballType
{
    public Player player;

    public override string MetaballAtlasTextureToUse => "NoxusBoss.BasicMetaballCircle.png";

    public override Color EdgeColor => Color.Red;

    public override bool ShouldRender => ActiveParticleCount >= 1; //|| AnyProjectiles(ModContent.ProjectileType<DimensionTwistedComet>());

    public override Func<Texture2D>[] LayerTextures => [() => ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Extra/BlackPixel").Value];

    public override void PrepareShaderForTarget(int layerIndex)
    {
        // Store the shader in an easy to use local variable.
        var metaballShader = ShaderManager.GetShader("NoxusBoss.PaleAvatarBlobMetaballShader");

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
        metaballShader.SetTexture(layerTexture, 1, SamplerState.LinearWrap);
        //metaballShader.TrySetParameter("overlayTexture", GennedAssets.Textures.Noise.HarshCellNoise);

        // Apply the metaball shader.
        metaballShader.Apply();
    }

    public override void UpdateParticle(MetaballInstance particle)
    {
        if (particle.ExtraInfo[0] >= 0 && particle.ExtraInfo[0] < 40)
        {
            particle.Size = float.Lerp(particle.Size, Main.rand.NextFloat(5, 10), 0.4f);
        }
        else
        {
            particle.Size *= 0.46f;
        }

        if (player != null)
        {
            if (player.velocity == Vector2.Zero)
            {
                particle.Center = Vector2.Lerp(particle.Center, player.Center, 0.1f);
            }

            if (particle.Center.Distance(player.Center) <= 1)
            {
                particle.Center = player.Center;
            }
        }

        particle.ExtraInfo[0]++;
        //Main.NewText($"{particle.ExtraInfo[0]}");
    }

    public override bool ShouldKillParticle(MetaballInstance particle)
    {
        if (particle.ExtraInfo[0] > 10 && particle.Size < 1f)
        {
            return true;
        }

        return false;
    }

    public override void ExtraDrawing()
    {
        //Utils.DrawBorderString(Main.spriteBatch, , player.Center - Main.screenPosition, Color.AntiqueWhite);
    }
}