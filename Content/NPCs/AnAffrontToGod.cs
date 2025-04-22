using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.World.GameScenes.AvatarAppearances;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.NPCs
{
    public partial class AnAffrontToGod : ModNPC
    {
        public bool DrawnFromTelescope
        {
            get;
            set;
        }

        public bool BackgroundProp
        {
            get;
            set;
        }
        public int? TargetIdentifierOverride
        {
            get;
            set;
        }
        public Matrix TransformPerspective
        {
            get
            {
                if (DrawnFromTelescope)
                    return Matrix.Identity;

                if (BackgroundProp)
                    return Main.GameViewMatrix.EffectMatrix;

                if (NPC.IsABestiaryIconDummy)
                    return Main.UIScaleMatrix;

                return Main.GameViewMatrix.TransformationMatrix;
            }
        }



        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.KingSlime);
            NPC.damage = 104384;
            NPC.lifeMax = 3349340;
            NPC.defense = 603;
            NPC.value = 302933;
            NPC.knockBackResist = 0f;
            NPC.width = 50;
            NPC.height = 30;
            //NPC.aiStyle = -1;
        }



        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.AstrumAureus")
            });
        }
        public enum AATGState
        {
            Idle,
            Walking,
            Attacking,
            Dead,


            Scream
        }


        public override bool PreAI()
        {
            return true;
        }
        public override void AI()
        {

            //AstrumAureusAI.VanillaAstrumAureusAI(NPC, Mod);
        }



        public void DrawSelf(Vector2 screenPos)
        {
            // Draw the backglow.
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, TransformPerspective);

            // NPC.position -= NPC.Size * 0.5f;

            float backglowScale = NPC.scale * (DrawnFromTelescope ? 0.3f : 0.74f);
            float backglowOpacity = BackgroundProp ? RiftEclipseSky.RiftScaleFactor : 1f;
            Vector2 drawPosition = NPC.Center - screenPos + new Vector2(24f, -120f) * backglowScale;

            if (!DrawnFromTelescope)
            {
                float growInterpolant = RiftEclipseSky.RiftScaleFactor / RiftEclipseSky.ScaleWhenOverSun;
                float growPulse = Convert01To010(growInterpolant.Squared()).Cubed();
                backglowScale += growPulse.Cubed() * Cos01(Main.GlobalTimeWrappedHourly * 56f) * 0.6f + growPulse * 1.3f;
            }



            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, TransformPerspective);

            ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
            riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.2f);
            riftShader.TrySetParameter("baseCutoffRadius", 0.1f);
            riftShader.TrySetParameter("swirlOutwardnessExponent", 0.42f);
            riftShader.TrySetParameter("swirlOutwardnessFactor", 5f);
            riftShader.TrySetParameter("vanishInterpolant", 1f);
            riftShader.TrySetParameter("edgeColor", Color.Crimson);
            riftShader.TrySetParameter("edgeColorBias", 0.15f);
            riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
            riftShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap);
            riftShader.Apply();



        }

        public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
        {
            return base.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Main.NewText("PreDraw is running!", Color.LimeGreen);
            DrawSelf(screenPos);
            Texture2D bodyTexture = TextureAssets.Npc[NPC.type].Value;
            Texture2D Glow = GennedAssets.Textures.FirstPhaseForm.AvatarRift;
            Vector2 bodyOrigin = new Vector2(bodyTexture.Width / 2f, bodyTexture.Height / 2f);
            float scale = 0.4f;


            Main.spriteBatch.Draw(Glow, NPC.Center - screenPos, null, drawColor, NPC.rotation, bodyOrigin, scale, SpriteEffects.None, 0f);

            // Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, TransformPerspective);

            Main.spriteBatch.Draw(bodyTexture, NPC.Center - screenPos, null, drawColor, NPC.rotation, bodyOrigin, scale, SpriteEffects.None, 0f);

            float wind = AperiodicSin(Main.GlobalTimeWrappedHourly * 0.56f + NPC.Center.X + NPC.Center.Y) *
                //clamps rotation kinda
                0.033f
                + Main.windSpeedCurrent * 0.17f;

            SpriteEffects spriteEffects = SpriteEffects.None;
            Texture2D LillyTexture = GennedAssets.Textures.SecondPhaseForm.SpiderLily;
            Rectangle Lillyframe = LillyTexture.Frame(1, 3, 0, (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3);
            Vector2 Lorigin = new Vector2(Lillyframe.Width / 2, Lillyframe.Height + 54 * Math.Sign(NPC.gravity));
            float LillySquish = MathF.Cos(Main.GlobalTimeWrappedHourly * 10.5f + NPC.Center.X + NPC.Center.Y) * 1f;
            float LillyScale = 0.1f;
            Vector2 LillyPos = NPC.Center;
            Color glowmaskColor = new Color(2, 0, 156);
            //Main.NewText($"{LillyPos - Main.screenPosition}");
            Main.EntitySpriteDraw(LillyTexture, LillyPos - Main.screenPosition, Lillyframe, drawColor, wind, Lorigin, LillyScale, spriteEffects, 0f);
            
            return false;
        }
    }

   
}
