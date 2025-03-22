using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.Graphics.RenderTargets;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.RocheLimit;

public class RocheLimitGlobalNPC : GlobalNPC
{
    private static int timeSinceLastRTAccess;

    /// <summary>
    /// Whether this NPC is currently being shredded by a black hole.
    /// </summary>
    public bool BeingShredded
    {
        get;
        set;
    }

    /// <summary>
    /// The amount by which this NPC should be downscaled due to entering a black hole.
    /// </summary>
    public float DownscaleFactor
    {
        get;
        set;
    } = 1f;

    /// <summary>
    /// The disintegration render target for NPCs.
    /// </summary>
    public static InstancedRequestableTarget DisintegrationTarget
    {
        get;
        set;
    }

    public override bool InstancePerEntity => true;

    public override void SetStaticDefaults()
    {
        Main.ContentThatNeedsRenderTargets.Add(DisintegrationTarget = new InstancedRequestableTarget());
        On_Main.DrawNPC += DecreaseTargetScale;
        On_Main.DrawNPCs += ApplyDisintegrationEffect;
    }

    private static void ApplyDisintegrationEffect(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
    {
        int blackHoleID = ModContent.ProjectileType<RocheLimitBlackHole>();
        int targetIdentifier = behindTiles.ToInt();
        if (LumUtils.AnyProjectiles(blackHoleID))
        {
            // Not doing this causes one-frame visual bugs in which the old contents of the RT flicker on the screen.
            timeSinceLastRTAccess++;

            DisintegrationTarget.Request(Main.screenWidth, Main.screenHeight, targetIdentifier, () =>
            {
                Main.spriteBatch.ResetToDefault(false);
                orig(self, behindTiles);
                Main.spriteBatch.End();
            });

            if (RocheLimitBlackHoleRenderer.blackHoleTarget.TryGetTarget(0, out RenderTarget2D blackHoleTarget) && blackHoleTarget is not null &&
                DisintegrationTarget.TryGetTarget(targetIdentifier, out RenderTarget2D npcTarget) && npcTarget is not null && timeSinceLastRTAccess >= 3)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

                Vector2 aspectRatioCorrectionFactor = new Vector2(WotGUtils.ViewportSize.X / WotGUtils.ViewportSize.Y, 1f);
                RocheLimitBlackHoleRenderer.GetBlackHoleData(aspectRatioCorrectionFactor, out float[] blackHoleRadii, out Vector2[] blackHolePositions);

                ManagedShader spaghettificationShader = ShaderManager.GetShader("HeavenlyArsenal.RocheLimitSpaghettificationShader");
                spaghettificationShader.TrySetParameter("sourceRadii", blackHoleRadii);
                spaghettificationShader.TrySetParameter("sourcePositions", blackHolePositions);
                spaghettificationShader.TrySetParameter("aspectRatioCorrectionFactor", aspectRatioCorrectionFactor);
                spaghettificationShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
                spaghettificationShader.SetTexture(blackHoleTarget, 1);
                spaghettificationShader.Apply();

                Main.spriteBatch.Draw(npcTarget, Main.screenLastPosition - Main.screenPosition, Color.White);

                Main.spriteBatch.ResetToDefault();
            }
            else
                orig(self, behindTiles);

            return;
        }
        else
            timeSinceLastRTAccess = 0;

        orig(self, behindTiles);
    }

    private static void DecreaseTargetScale(On_Main.orig_DrawNPC orig, Main self, int index, bool behindTiles)
    {
        NPC npc = Main.npc[index];
        if (!npc.active || !npc.TryGetGlobalNPC(out RocheLimitGlobalNPC globalNPC))
            return;

        float originalScale = npc.scale;

        try
        {
            npc.scale *= globalNPC.DownscaleFactor;
            orig(self, index, behindTiles);
        }
        finally
        {
            if (npc is not null)
                npc.scale = originalScale;
        }
    }

    // Lobotomize targets that are preoccupied with being fucking shredded to pieces by a black hole.
    public override bool PreAI(NPC npc) => !BeingShredded;

    public override void PostAI(NPC npc)
    {
        bool wasShreddedBefore = BeingShredded;
        BeingShredded = false;
        DownscaleFactor = MathHelper.Lerp(DownscaleFactor, 1f, 0.12f);
        if (npc.realLife >= 0 && Main.npc[npc.realLife].TryGetGlobalNPC(out RocheLimitGlobalNPC ownerGlobalNPC))
            DownscaleFactor = ownerGlobalNPC.DownscaleFactor;

        if (!npc.CanBeChasedBy() && !wasShreddedBefore)
            return;

        float minDistance = 9999999f;
        int blackHoleID = ModContent.ProjectileType<RocheLimitBlackHole>();
        var blackHoles = LumUtils.AllProjectilesByID(blackHoleID);
        Projectile? closestBlackHole = null;
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            if (projectile.type == blackHoleID)
            {
                float distanceToBlackHole = projectile.Distance(npc.Center);
                if (distanceToBlackHole < minDistance)
                {
                    closestBlackHole = projectile;
                    minDistance = distanceToBlackHole;
                }
            }
        }

        if (closestBlackHole is not null && npc.WithinRange(closestBlackHole.Center, 900f))
        {
            Vector2 suctionOrigin = closestBlackHole.Center - Vector2.UnitY * 2f;

            float suctionInterpolant = closestBlackHole.As<RocheLimitBlackHole>().BlackHoleDiameter / RocheLimitBlackHole.MaxBlackHoleDiameter;
            float suctionAcceleration = suctionInterpolant * 0.09f;
            npc.velocity = Vector2.Lerp(npc.velocity, npc.SafeDirectionTo(suctionOrigin) * suctionInterpolant * 80f, suctionAcceleration);

            if (npc.realLife == -1)
            {
                float idealDownscaling = EasingCurves.Exp.Evaluate(EasingType.Out, LumUtils.InverseLerp(150f, 700f, npc.Distance(suctionOrigin)));
                DownscaleFactor = MathHelper.Lerp(1f, idealDownscaling, suctionInterpolant);
            }

            // It's time to die.
            if (npc.WithinRange(suctionOrigin, 175f) && suctionInterpolant >= 0.85f)
            {
                BeingShredded = true;

                npc.velocity = Vector2.Zero;
                npc.Center = suctionOrigin;
            }

            // Hits are inputted manually to ensure maximum control over the NPC's death, which needs to be more interesting than just splaying a bunch of gore and loot.
            if (closestBlackHole.Colliding(closestBlackHole.Hitbox, npc.Hitbox) || BeingShredded)
            {
                int damage = closestBlackHole.damage;
                bool willDie = npc.life - damage <= 0; // This calculation doesn't care about defense and DR but honestly who cares?
                if (willDie)
                {
                    npc.active = false;
                    npc.NPCLoot();
                }
                else
                    npc.SimpleStrikeNPC(damage, 0);
            }
        }
    }
}
