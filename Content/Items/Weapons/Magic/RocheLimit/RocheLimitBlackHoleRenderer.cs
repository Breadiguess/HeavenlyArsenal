using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Graphics.RenderTargets;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.RocheLimit;

public class RocheLimitBlackHoleRenderer : ModSystem
{
    /// <summary>
    /// The render target that holds all black holes.
    /// </summary>
    private static InstancedRequestableTarget blackHoleTarget;

    public override void OnModLoad()
    {
        Main.ContentThatNeedsRenderTargets.Add(blackHoleTarget = new InstancedRequestableTarget());
        On_Main.DrawProjectiles += RenderBlackHolesWrapper;
    }

    private static void RenderBlackHoles()
    {
        int blackHoleID = ModContent.ProjectileType<RocheLimitBlackHole>();
        foreach (Projectile blackHole in Main.ActiveProjectiles)
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (blackHole.type == blackHoleID)
                blackHole.As<RocheLimitBlackHole>().RenderBlackHole();

            Main.spriteBatch.End();
        }
    }

    private static void RenderBlackHolesWrapper(On_Main.orig_DrawProjectiles orig, Main self)
    {
        orig(self);

        int blackHoleID = ModContent.ProjectileType<RocheLimitBlackHole>();
        if (!LumUtils.AnyProjectiles(blackHoleID))
            return;

        blackHoleTarget.Request(Main.screenWidth, Main.screenHeight, 0, RenderBlackHoles);
        if (blackHoleTarget.TryGetTarget(0, out RenderTarget2D target) && target is not null)
        {
            int index = 0;
            float[] blackHoleRadii = new float[5];
            Vector2 aspectRatioCorrectionFactor = new Vector2(WotGUtils.ViewportSize.X / WotGUtils.ViewportSize.Y, 1f);
            Vector2[] blackHolePositions = new Vector2[5];
            foreach (Projectile blackHole in Main.ActiveProjectiles)
            {
                if (blackHole.type == blackHoleID)
                {
                    if (index < blackHoleRadii.Length - 1)
                    {
                        blackHoleRadii[index] = blackHole.As<RocheLimitBlackHole>().BlackHoleDiameter / WotGUtils.ViewportSize.X * Main.GameViewMatrix.Zoom.X * 0.275f;

                        Vector2 positionCoords = (blackHole.Center - Main.screenPosition) / WotGUtils.ViewportSize;
                        blackHolePositions[index] = (positionCoords - Vector2.One * 0.5f) * aspectRatioCorrectionFactor * Main.GameViewMatrix.Zoom + Vector2.One * 0.5f;
                    }
                    index++;
                }
            }

            ManagedScreenFilter distortionShader = ShaderManager.GetFilter("HeavenlyArsenal.BlackHoleDistortionShader");
            distortionShader.TrySetParameter("maxLensingAngle", 73.1f);
            distortionShader.TrySetParameter("aspectRatioCorrectionFactor", aspectRatioCorrectionFactor);
            distortionShader.TrySetParameter("sourceRadii", blackHoleRadii);
            distortionShader.TrySetParameter("sourcePositions", blackHolePositions);
            distortionShader.SetTexture(target, 1);
            distortionShader.Activate();
        }
    }
}
