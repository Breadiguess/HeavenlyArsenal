using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.FormPresets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.Waters;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using NoxusBoss.Assets;
using Luminance.Assets;

namespace HeavenlyArsenal.Content.Projectiles.Misc
{
    class CosmicWatergun_proj :ModProjectile, IPixelatedPrimitiveRenderer
    {
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(358);

            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            
        }


        public override bool PreDraw(ref Color lightColor)
        {
            
            return false;//base.PreDraw(ref lightColor);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            Texture2D BubblyNoise = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/Noise/BubblyNoise").Value;
            Texture2D DendriticNoiseZoomedOut = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/Noise/DendriticNoiseZoomedOut").Value;



            Rectangle viewBox = Projectile.Hitbox;
            Rectangle screenBox = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
            viewBox.Inflate(540, 540);
            if (!viewBox.Intersects(screenBox))
                return;

            float lifetimeRatio = Time / 240f;
            float dissolveThreshold = InverseLerp(0.67f, 1f, lifetimeRatio) * 0.5f;

            ManagedShader BloodShader = ShaderManager.GetShader("HeavenlyArsenal.BloodBlobShader");
            BloodShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly + Projectile.identity * 72.113f);
            BloodShader.TrySetParameter("dissolveThreshold", dissolveThreshold);
            BloodShader.TrySetParameter("accentColor", new Vector4(0.6f, 0.02f, -0.1f, 0f));
            BloodShader.SetTexture(BubblyNoise, 1, SamplerState.LinearWrap);
            BloodShader.SetTexture(DendriticNoiseZoomedOut, 2, SamplerState.LinearWrap);









           
            Texture2D CosmicTexture = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            CosmicTexture = NoxusBoss.Assets.LazyAsset<Texture2D>.FromPath($"{ModContent.GetInstance<EternalGardenWater>().Texture}Cosmos");
            // = LazyAsset<Texture2D>.FromPath($"{ModContent.GetInstance<EternalGardenWater>().Texture}Cosmos");
            ManagedScreenFilter cosmicShader = ShaderManager.GetFilter("NoxusBoss.CosmicWaterShader");

            float brightnessFactor = 1f;
            Vector4 generalColor = Vector4.One;


            cosmicShader.TrySetParameter("targetSize", Main.ScreenSize.ToVector2());
            cosmicShader.TrySetParameter("generalColor", generalColor);
            cosmicShader.TrySetParameter("brightnessFactor", brightnessFactor);
            cosmicShader.TrySetParameter("oldScreenPosition", Main.screenPosition);
            cosmicShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
            cosmicShader.SetTexture(CosmicTexture.Value, 1, SamplerState.LinearWrap);
            cosmicShader.SetTexture(SmudgeNoise, 2, SamplerState.LinearWrap);
            cosmicShader.SetTexture(TileTargetManagers.LiquidTarget, 3);
            cosmicShader.SetTexture(TileTargetManagers.LiquidSlopesTarget, 4);

            PrimitiveSettings settings = new PrimitiveSettings(3, generalColor, _ => Projectile.Size * 0.5f + Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.56f, Pixelate: true, Shader: BloodShader);
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 9);


        }
    }
}
