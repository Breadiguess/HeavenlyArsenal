using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Core.Graphics.RenderTargets;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;

public class AntishadowAssassinDyeRenderer : ModSystem
{
    /// <summary>
    /// The render target responsible for rendering antishadow assassins and their particles.
    /// </summary>
    public static InstancedRequestableTarget Target
    {
        get;
        private set;
    }

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
            return;
        orig(self);
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        foreach (Player player in Main.ActivePlayers)
            Render(player.whoAmI);

        Main.spriteBatch.End();
    }

    private static void Render(int playerIndex)
    {
        if (Main.dedServ)
            return;
        int assassinID = ModContent.ProjectileType<AntishadowAssassin>();
        int slashID = ModContent.ProjectileType<AntishadowAssassinSlash>();
        int unidirectionalSlashID = ModContent.ProjectileType<AntishadowUnidirectionalAssassinSlash>();
        int identifier = playerIndex + Main.maxPlayers;
        Target.Request(Main.screenWidth, Main.screenHeight, identifier, () =>
        {
            bool backFireParticlesExist = AntishadowFireParticleSystemManager.BackParticleSystem.TryGetValue(playerIndex, out FireParticleSystem? backFireParticleSystem);
            bool frontFireParticlesExist = AntishadowFireParticleSystemManager.ParticleSystem.TryGetValue(playerIndex, out FireParticleSystem? frontFireParticleSystem);
            bool assassinExists = Main.player[playerIndex].ownedProjectileCounts[assassinID] >= 1 ||
                                  Main.player[playerIndex].ownedProjectileCounts[slashID] >= 1 ||
                                  Main.player[playerIndex].ownedProjectileCounts[unidirectionalSlashID] >= 1;
            if (!backFireParticlesExist && !frontFireParticlesExist && !assassinExists)
                return;

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            if (backFireParticlesExist)
                backFireParticleSystem.RenderAll();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                bool validType = projectile.type == assassinID || projectile.type == slashID || projectile.type == unidirectionalSlashID;
                if (validType && projectile.owner == playerIndex)
                {
                    Color _ = default;
                    projectile.ModProjectile.PreDraw(ref _);
                }
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            if (frontFireParticlesExist)
                frontFireParticleSystem.RenderAll();

            Main.spriteBatch.End();
        });

        if (Target.TryGetTarget(identifier, out RenderTarget2D? target) && target is not null)
        {
            int dyeShader = Main.player[playerIndex].cMinion;
            GameShaders.Armor.Apply(dyeShader, null, new DrawData(Main.screenTarget, Vector2.Zero, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White));
            Main.spriteBatch.Draw(target, Main.screenLastPosition - Main.screenPosition, null, Color.White, 0f, Vector2.Zero, 1f, 0, 0f);
        }
    }
}
