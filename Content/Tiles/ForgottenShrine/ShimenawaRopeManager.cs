using HeavenlyArsenal.Content.Tiles.Generic;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Graphics.RenderTargets;
using System.Linq;
using Terraria;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class ShimenawaRopeManager : WorldOrientedTileObjectManager<ShimenawaRopeData>
{
    /// <summary>
    /// Then render target responsible for holding draw contents of all shimenawa ropes.
    /// </summary>
    public static InstancedRequestableTarget RopeTarget
    {
        get;
        private set;
    }

    public override void OnModLoad()
    {
        RopeTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(RopeTarget);
    }

    /// <summary>
    /// Registers a new rope into the set of ropes maintained by the world.
    /// </summary>
    public override void Register(ShimenawaRopeData rope)
    {
        bool ropeAlreadyExists = TileObjects.Any(r => (r.Start == rope.Start && r.End == rope.End) ||
                                                      (r.Start == rope.End && r.End == rope.Start));
        if (ropeAlreadyExists)
            return;

        base.Register(rope);
    }

    public override void PostDrawTiles()
    {
        if (TileObjects.Count <= 0)
            return;

        RopeTarget.Request(Main.screenWidth, Main.screenHeight, 0, () =>
        {
            Main.spriteBatch.Begin();
            foreach (ShimenawaRopeData rope in TileObjects)
                rope.Render();

            Main.spriteBatch.End();
        });
        if (RopeTarget.TryGetTarget(0, out RenderTarget2D? target) && target is not null)
        {
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            ManagedShader pixelationShader = ShaderManager.GetShader("Luminance.PixelationShader");
            pixelationShader.TrySetParameter("pixelationFactor", Vector2.One / target.Size());
            pixelationShader.Apply();

            Main.spriteBatch.Draw(target, Main.screenLastPosition - Main.screenPosition, Color.White);
            Main.spriteBatch.End();
        }
    }
}
