using Daybreak.Common.Assets;
using HeavenlyArsenal.Content.NPCs.Hostile;
using Luminance.Core.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using NoxusBoss.Core.Graphics.SwagRain;
using NoxusBoss.Core.World.GameScenes.AvatarAppearances;
using ReLogic.Content;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;

namespace HeavenlyArsenal.Common
{

    internal class BloodMoonNPCBestiary : GlobalNPC
    {
        public static InstancedRequestableTarget BestiaryTarget
        {
            get;
            set;
        } = new InstancedRequestableTarget();

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.ContentThatNeedsRenderTargets.Add(BestiaryTarget);
        }

     
        public override void Load()
        {

            base.Load();
            
        }

        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return (entity.ModNPC is not null ? entity.ModNPC is BaseBloodMoonNPC : false);
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 drawPosition = npc.Center - screenPos;
            Vector2 scale = Vector2.One / new Vector2(Main.GameViewMatrix.TransformationMatrix.M11, Main.GameViewMatrix.TransformationMatrix.M22);

            Texture2D texture = InvisiblePixel;

            Matrix TransformPerspective = Main.UIScaleMatrix;
            if (npc.IsABestiaryIconDummy)
            {
                BestiaryTarget.Request(512, 512, 0, () =>
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

                   
                    npc.position -= npc.Size * 0.5f;

                    float backglowScale = npc.scale * (0.74f);
                    float backglowOpacity =  1f;
                    Vector2 drawPosition = npc.Center - screenPos;

                  

                    if (npc.IsABestiaryIconDummy)
                    {
                        if ((RiftEclipseBloodMoonRainSystem.EffectActive || RiftEclipseBloodMoonRainSystem.MonolithEffectActive))
                        {
                            Texture2D flare = Luminance.Assets.MiscTexturesRegistry.ShineFlareTexture.Value;
                            Vector2 lensFlareScale = new Vector2(1.5f, 0.48f) * backglowScale * 2f;
                            Vector2 lensFlarePosition = npc.Center - screenPos;

                            Main.spriteBatch.Draw(flare, lensFlarePosition, null, npc.GetAlpha(Color.Red) with { A = 0 } * backglowOpacity, 0f, flare.Size() * 0.5f, lensFlareScale, 0, 0f);
                            Main.spriteBatch.Draw(flare, lensFlarePosition, null, npc.GetAlpha(Color.White) with { A = 0 } * backglowOpacity * 0.6f, 0f, flare.Size() * 0.5f, lensFlareScale * 0.8f, 0, 0f);

                            Main.spriteBatch.Draw(BloomCircleSmall, lensFlarePosition, null, npc.GetAlpha(Color.Red) with { A = 0 } * backglowOpacity, 0f, BloomCircleSmall.Size() * 0.5f, backglowScale * 6f, 0, 0f);
                            Main.spriteBatch.Draw(BloomCircleSmall, lensFlarePosition, null, npc.GetAlpha(Color.Red) with { A = 0 } * backglowOpacity * 0.15f, 0f, BloomCircleSmall.Size() * 0.5f, backglowScale * 36f, 0, 0f);
                        }

                        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, npc.GetAlpha(Color.Red) with { A = 0 } * backglowOpacity * 0.25f, 0f, BloomCircleSmall.Size() * 0.5f, backglowScale * 12.5f, 0, 0f);
                        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, npc.GetAlpha(Color.Red) with { A = 0 } * backglowOpacity * 0.4f, 0f, BloomCircleSmall.Size() * 0.5f, backglowScale * 6f, 0, 0f);
                        Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, npc.GetAlpha(Color.Crimson) with { A = 0 } * backglowOpacity * 0.12f, 0f, BloomCircleSmall.Size() * 0.5f, backglowScale * 12f, 0, 0f);
                    }

                 
                    Texture2D riftTexture = WavyBlotchNoise;
                    Vector2 riftScale = npc.Size / riftTexture.Size() * 2f;
                    float vanishInterpolant = 0f;
                    float time = 0f;
                    float baseCutoffRadius = 0.1f;
                    ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.AvatarRiftShapeShader");
                    riftShader.TrySetParameter("time", time);
                    riftShader.TrySetParameter("baseCutoffRadius", baseCutoffRadius);
                    riftShader.TrySetParameter("swirlOutwardnessExponent", 0.151f);
                    riftShader.TrySetParameter("swirlOutwardnessFactor", 3.67f);
                    riftShader.TrySetParameter("vanishInterpolant", vanishInterpolant);
                    riftShader.SetTexture(WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
                    riftShader.SetTexture(WavyBlotchNoise, 2, SamplerState.AnisotropicWrap);
                    riftShader.Apply();

                    // Draw the rift.
                    Vector2 riftDrawPosition = npc.Center - screenPos;
                    Main.spriteBatch.Draw(riftTexture, riftDrawPosition, null, Color.White * npc.Opacity, 0f, riftTexture.Size() * 0.5f, riftScale, 0, 0f);

                    // Draw the rift.
                    //AvatarRiftDrawRiftWithShader(npc, screenPos, TransformPerspective, BackgroundProp, SuckOpacity, 0f, TargetIdentifierOverride);
                    Main.spriteBatch.End();
                    // Draw the eyes.

                });

                if (BestiaryTarget.TryGetTarget(0, out RenderTarget2D? target) && target is not null)
                    texture = target;
            }
            spriteBatch.Draw(texture, Vector2.Zero, null, Color.White * npc.Opacity, npc.rotation, Vector2.Zero, 1, 0, 0f);

            if (npc.IsABestiaryIconDummy)
                Main.spriteBatch.ResetToDefaultUI();


            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
    }
}
