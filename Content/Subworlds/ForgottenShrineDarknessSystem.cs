using HeavenlyArsenal.Content.Subworlds.Generation;
using HeavenlyArsenal.Content.Subworlds.Generation.Bridges;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Graphics.RenderTargets;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Subworlds;

public class ForgottenShrineDarknessSystem : ModSystem
{
    /// <summary>
    /// A render target that contains all glow information in contrast to the darkness.
    /// </summary>
    public static InstancedRequestableTarget GlowTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// A queue that represents the set of draw actions that should be render into the glow target.
    /// </summary>
    public static Queue<Action> GlowActionsQueue
    {
        get;
        private set;
    } = new Queue<Action>(256);

    /// <summary>
    /// Whether the darkening effect should be applied.
    /// </summary>
    public static bool EffectShouldBeActive => ForgottenShrineSystem.WasInSubworldLastFrame;

    public override void OnModLoad()
    {
        RenderTargetManager.RenderTargetUpdateLoopEvent += ConsumeGlowActions;
        GlowTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(GlowTarget);
    }

    private void ConsumeGlowActions()
    {
        GlowTarget.Request((int)WotGUtils.ViewportSize.X, (int)WotGUtils.ViewportSize.Y, 0, () =>
        {
            if (GlowActionsQueue.Count <= 0)
                return;

            Main.spriteBatch.ResetToDefault(false);
            while (GlowActionsQueue.TryDequeue(out Action action))
                action();

            Main.spriteBatch.End();
        });
    }

    private static void UpdateDarknessOverlay()
    {
        if (Main.netMode == NetmodeID.Server || !EffectShouldBeActive)
            return;

        if (GlowTarget.TryGetTarget(0, out RenderTarget2D? glowTarget) && glowTarget is not null)
        {
            int left = BaseBridgePass.BridgeGenerator.Right + ForgottenShrineGenerationHelpers.LakeWidth + BaseBridgePass.GenerationSettings.DockWidth;
            int right = left + ForgottenShrineGenerationHelpers.ShrineIslandWidth;

            Matrix worldToUV = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f) *
                Main.GameViewMatrix.TransformationMatrix *
                Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);
            Matrix uvToWorld = Matrix.Invert(worldToUV);

            ManagedScreenFilter darknessShader = ShaderManager.GetFilter("HeavenlyArsenal.ForgottenShrineStillDarknessShader");
            darknessShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
            darknessShader.TrySetParameter("screenOffset", (Main.screenPosition - Main.screenLastPosition) / glowTarget.Size());
            darknessShader.TrySetParameter("targetSize", glowTarget.Size());
            darknessShader.TrySetParameter("baseDarkness", 0.6f);
            darknessShader.TrySetParameter("islandLeft", left * 16f);
            darknessShader.TrySetParameter("islandRight", right * 16f);
            darknessShader.TrySetParameter("uvToWorld", uvToWorld);
            darknessShader.TrySetParameter("darknessTaperDistance", 3300f);
            darknessShader.SetTexture(glowTarget, 1, SamplerState.LinearClamp);
            darknessShader.Activate();
        }
    }

    public override void PostUpdatePlayers() => UpdateDarknessOverlay();

    public override void PostDrawTiles() => UpdateDarknessOverlay();
}
