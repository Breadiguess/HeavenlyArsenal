using NoxusBoss.Core.Graphics.RenderTargets;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;

public class AntishadowAssassinDyeRenderer : ModSystem
{
    /// <summary>
    ///     The render target responsible for rendering antishadow assassins and their particles.
    /// </summary>
    public static InstancedRequestableTarget Target { get; private set; }

    public override void OnModLoad()
    {
        if (!Main.dedServ)
        {
            Target = new InstancedRequestableTarget();
            Main.ContentThatNeedsRenderTargets.Add(Target);
            On_Main.DrawProjectiles += RenderWrapper;
        }
    }

    private static void RenderWrapper(On_Main.orig_DrawProjectiles orig, Main self)
    {
        if (Main.dedServ)
        {
            return;
        }

        orig(self);

        Main.spriteBatch.Begin
            (SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        foreach (var player in Main.ActivePlayers)
        {
            Render(player.whoAmI);
        }

        Main.spriteBatch.End();
    }

    private static void Render(int playerIndex)
    {
        if (Main.dedServ)
        {
            return;
        }

        var assassinID = ModContent.ProjectileType<AntishadowAssassin>();
        var slashID = ModContent.ProjectileType<AntishadowAssassinSlash>();
        var unidirectionalSlashID = ModContent.ProjectileType<AntishadowUnidirectionalAssassinSlash>();
        var identifier = playerIndex + Main.maxPlayers;

        Target.Request
        (
            Main.screenWidth,
            Main.screenHeight,
            identifier,
            () =>
            {
                var backFireParticlesExist = AntishadowFireParticleSystemManager.BackParticleSystem.TryGetValue(playerIndex, out var backFireParticleSystem);
                var frontFireParticlesExist = AntishadowFireParticleSystemManager.ParticleSystem.TryGetValue(playerIndex, out var frontFireParticleSystem);

                var assassinExists = Main.player[playerIndex].ownedProjectileCounts[assassinID] >= 1 ||
                                     Main.player[playerIndex].ownedProjectileCounts[slashID] >= 1 ||
                                     Main.player[playerIndex].ownedProjectileCounts[unidirectionalSlashID] >= 1;

                if (!backFireParticlesExist && !frontFireParticlesExist && !assassinExists)
                {
                    return;
                }

                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                if (backFireParticlesExist)
                {
                    backFireParticleSystem.RenderAll();
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                foreach (var projectile in Main.ActiveProjectiles)
                {
                    var validType = projectile.type == assassinID || projectile.type == slashID || projectile.type == unidirectionalSlashID;

                    if (validType && projectile.owner == playerIndex)
                    {
                        Color _ = default;
                        projectile.ModProjectile.PreDraw(ref _);
                    }
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                if (frontFireParticlesExist)
                {
                    frontFireParticleSystem.RenderAll();
                }

                Main.spriteBatch.End();
            }
        );

        if (Target.TryGetTarget(identifier, out var target) && target is not null)
        {
            var dyeShader = Main.player[playerIndex].cMinion;
            GameShaders.Armor.Apply(dyeShader, null, new DrawData(Main.screenTarget, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White));
            Main.spriteBatch.Draw(target, Main.screenLastPosition - Main.screenPosition, null, Color.White, 0f, Vector2.Zero, 1f, 0, 0f);
        }
    }
}