using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.FuneralDirge;

public class AvatarRifleSuperBullet : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    public bool hasEmpowerment;
    public int empowerment;

    private Vector2[] oldPos;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }
    public override void SetDefaults(Projectile entity)
    {
        // TODO -- Applying penetrate = -1 to every projectile ever seems a bit funny? I'm guessing this is just code that's unfinished.
        // entity.penetrate = -1;
        if (hasEmpowerment)
        {
            entity.damage = (int)(entity.damage * 1+empowerment/7);
        }
    }
    public override bool PreAI(Projectile projectile)
    {
        if (hasEmpowerment)
        {
            if (oldPos == null)
                oldPos = Enumerable.Repeat(projectile.Center, 20).ToArray();

            for (int i = oldPos.Length - 2; i > 0; i--)
            {
                oldPos[i] = oldPos[i - 1];
            }

            oldPos[0] = projectile.Center + projectile.velocity * 2;
        }

        return base.PreAI(projectile);
    }

    public override void PostDraw(Projectile projectile, Color lightColor)
    {
        if (hasEmpowerment && oldPos != null)
        {
            float WidthFunction(float p) => 50f * MathF.Pow(p, 0.66f) * (1f - p * 0.5f);
            Color ColorFunction(float p) => new Color(215, 30, 35, 200);

            ManagedShader trailShader = ShaderManager.GetShader("HeavenlyArsenal.AvatarRifleBulletAuroraEffect");
            trailShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * projectile.velocity.Length() / 8f + projectile.identity * 72.113f);
            trailShader.TrySetParameter("spin", 2f * Math.Sign(projectile.velocity.X));
            trailShader.TrySetParameter("brightness", empowerment / 1.5f);
            trailShader.SetTexture(GennedAssets.Textures.Noise.DendriticNoiseZoomedOut, 0, SamplerState.LinearWrap);
            trailShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoiseDetailed, 1, SamplerState.LinearWrap);
            trailShader.SetTexture(GennedAssets.Textures.Noise.DendriticNoiseZoomedOut, 2, SamplerState.LinearWrap);

            PrimitiveRenderer.RenderTrail(oldPos, new PrimitiveSettings(WidthFunction, ColorFunction, _ => Vector2.Zero, Shader: trailShader, Smoothen: false), oldPos.Length);
            //Utils.DrawBorderString(Main.spriteBatch, "Empowerment: " + empowerment.ToString(), projectile.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.White);
            //Utils.DrawBorderString(Main.spriteBatch, "Empowerment: " + hasEmpowerment.ToString(), projectile.Center - Vector2.UnitY * 140 - Main.screenPosition, Color.White);

        }
    }
}
