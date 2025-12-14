using System.Collections.Generic;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.Temp;

public class DeadUniverse_Rift : ModProjectile
{
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindNPCsAndTiles.Add(index);
        overPlayers.Add(index);
    }

    public override void AI()
    {
        OpenInterpolant = float.Lerp(OpenInterpolant, 1, 0.2f);
    }

    #region DrawCode

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.PrepareForShaders();

        var squish = 0.5f;
        var scaleFactor = OpenInterpolant;
        var color = new Color(77, 0, 2);
        var edgeColor = new Color(1f, 0.08f, 0.08f);
        var glow = AssetDirectory.Textures.BigGlowball.Value;

        var innerRiftTexture = GennedAssets.Textures.FirstPhaseForm.RiftInnerTexture.Value;
        var textureArea = Projectile.Size * new Vector2(1f - squish, 1f) / innerRiftTexture.Size() * 5 * scaleFactor * 1.6f;

        Main.EntitySpriteDraw
        (
            glow,
            Projectile.Center - Main.screenPosition,
            glow.Frame(),
            Color.Red with
            {
                A = 200
            },
            Projectile.rotation,
            glow.Size() * 0.5f,
            new Vector2(0.12f, 0.25f) * 0.325f * OpenInterpolant,
            0
        );

        var riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
        riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.1f);
        riftShader.TrySetParameter("baseCutoffRadius", 0.1f);
        riftShader.TrySetParameter("swirlOutwardnessExponent", 0.151f);
        riftShader.TrySetParameter("swirlOutwardnessFactor", 3.67f);
        riftShader.TrySetParameter("vanishInterpolant", Utilities.InverseLerp(1f, 0f, Projectile.scale - Projectile.identity / 13f % 0.2f));
        riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
        riftShader.TrySetParameter("edgeColorBias", 0f);
        riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
        riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 2, SamplerState.AnisotropicWrap);
        riftShader.Apply();

        Main.spriteBatch.Draw(innerRiftTexture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(color), Projectile.rotation, innerRiftTexture.Size() * 0.5f, textureArea, 0, 0f);
        Main.spriteBatch.ResetToDefault();

        return false;
    }

    #endregion

    #region Setup and Values

    public ref Player Owner => ref Main.player[Projectile.owner];

    public BrazilPlayer goToBrazil => Owner.GetModPlayer<BrazilPlayer>();

    public float RiftSize => Projectile.Size.Length() * Projectile.scale * OpenInterpolant;

    public float OpenInterpolant { get; set; }

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.friendly = true;
        Projectile.hostile = false;

        Projectile.timeLeft = 180;
        Projectile.Size = new Vector2(20, 20);
        Projectile.penetrate = -1;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 30;
    }

    public override void OnSpawn(IEntitySource source) { }

    #endregion

    #region HitNPC code

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        //todo: if its hitbox is overlapping target hitbox, return true. 

        return base.Colliding(projHitbox, targetHitbox);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (!target.GetGlobalNPC<BrazilVictim>().Active)
        {
            target.GetGlobalNPC<BrazilVictim>().BrazilTimer = 180;
            target.GetGlobalNPC<BrazilVictim>().Active = true;
        }

        hit.HideCombatText = true;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.FinalDamage = StatModifier.Default * 0;
    }

    #endregion

    #region Helper code

    /// <summary>
    ///     computes the damage the NPC would take from entering the rift based on the size of the npc.
    /// </summary>
    public static int CalculateSizeDamage(NPC npc, DeadUniverse_Rift rift)
    {
        var BaseDamage = 10_000;
        int ModifiedDamage;

        var sizeDifference = CalculateNPCsizeDifference(npc, rift.RiftSize);

        ModifiedDamage = (int)(BaseDamage * sizeDifference);

        return ModifiedDamage;
    }

    private static float CalculateNPCsizeDifference(NPC npc, float RiftSize)
    {
        var scale = npc.scale * npc.Hitbox.Size().Length();
        ;
        var DebugString = "";
        DebugString = scale.ToString();
        Main.NewText(npc.FullName + " scale: " + DebugString);

        return scale;
    }

    #endregion
}