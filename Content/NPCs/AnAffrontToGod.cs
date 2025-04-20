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

        public static Texture2D[] leftLegTextures = new Texture2D[]
        {
                GennedAssets.Textures.Hands.FrontHandLeftFinger1Digit1.Value,
                GennedAssets.Textures.Hands.FrontHandLeftFinger1Digit1.Value,
                GennedAssets.Textures.Hands.FrontHandLeftFinger1Digit1.Value
        };
        private Vector2[] leftLegOrigins = new Vector2[]
        {
                new Vector2(0.5f, 04f),
                new Vector2(0.5f, 03f),
                new Vector2(0.5f, 02f)
        };
        private Vector2[] leftLegOffsetConfig = new Vector2[]
        {
                new Vector2(0f, leftLegTextures[0].Height * 0.2f),
                new Vector2(0f, leftLegTextures[1].Height * 0.3f),
                new Vector2(0f, leftLegTextures[2].Height * 0.2f)
        };
        private float[] leftLegRotationOffsets = new float[]
        {
                0f, 0f, 0f
        };

        public static Texture2D[] rightLegTextures = new Texture2D[]
        {
                GennedAssets.Textures.Hands.FrontHandRightFinger1Digit1.Value,
                GennedAssets.Textures.Hands.FrontHandRightFinger1Digit2.Value,
                GennedAssets.Textures.Hands.FrontHandRightFinger2Digit2.Value
        };
        private Vector2[] rightLegOrigins = new Vector2[]
        {
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f)
        };
        private Vector2[] rightLegOffsetConfig = new Vector2[]
        {
                new Vector2(0f, rightLegTextures[0].Height * 0.2f),
                new Vector2(0f, rightLegTextures[1].Height * 0.3f),
                new Vector2(0f, rightLegTextures[2].Height * 0.2f)
        };
        private float[] rightLegRotationOffsets = new float[]
        {
                0f, 0f, 0f
        };

        private float leftLegAnimationPhase = 0f;
        private float rightLegAnimationPhase = MathHelper.Pi;

        private void UpdateLegAnimationPhases()
        {
            float speed = 0.05f;
            leftLegAnimationPhase += speed;
            rightLegAnimationPhase += speed;
            if (leftLegAnimationPhase > MathHelper.TwoPi)
                leftLegAnimationPhase -= MathHelper.TwoPi;
            if (rightLegAnimationPhase > MathHelper.TwoPi)
                rightLegAnimationPhase -= MathHelper.TwoPi;
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

        public void RenderLegs(SpriteBatch spriteBatch)
        {
            //Main.NewText("RenderLegs is being called!", Color.Red);

            UpdateLegAnimationPhases();

            Vector2 leftLegStart = NPC.position + new Vector2(-1f, 10f);
            Vector2 rightLegStart = NPC.position + new Vector2(1f, 10f);

            Vector2 baseLeftFootTarget = FindGroundVertical(leftLegStart.ToTileCoordinates()).ToWorldCoordinates(0f, 0f);
            Vector2 baseRightFootTarget = FindGroundVertical(rightLegStart.ToTileCoordinates()).ToWorldCoordinates(0f, 0f);

            Vector2 leftCadenceOffset = new Vector2((float)Math.Sin(leftLegAnimationPhase) * 20f,
                                                    (float)Math.Abs(Math.Sin(leftLegAnimationPhase)) * -10f);
            Vector2 rightCadenceOffset = new Vector2((float)Math.Sin(rightLegAnimationPhase) * 20f,
                                                    (float)Math.Abs(Math.Sin(rightLegAnimationPhase)) * -10f);

            Vector2 leftFootTarget = baseLeftFootTarget + leftCadenceOffset;
            Vector2 rightFootTarget = baseRightFootTarget + rightCadenceOffset;

            float legScale = NPC.scale * 1f;

            Vector2[] leftJoints = new Vector2[leftLegTextures.Length + 1];
            leftJoints[0] = leftLegStart;

            for (int i = 0; i < leftLegTextures.Length; i++)
            {
                leftJoints[i + 1] = leftJoints[i] + leftLegOffsetConfig[i];
            }

            Vector2[] rightJoints = new Vector2[rightLegTextures.Length + 1];
            rightJoints[0] = rightLegStart;

            for (int i = 0; i < rightLegTextures.Length; i++)
            {
                rightJoints[i + 1] = rightJoints[i] + rightLegOffsetConfig[i];
            }

            ForwardKinematics(legScale, Color.White, leftLegStart - Main.screenPosition,
                leftLegOrigins, leftLegTextures, leftJoints, leftLegRotationOffsets,
                spriteBatch);

            ForwardKinematics(legScale, Color.White, rightLegStart - Main.screenPosition,
                rightLegOrigins, rightLegTextures, rightJoints, rightLegRotationOffsets,
                spriteBatch);
        }

        public void ForwardKinematics(float scale, Color color, Vector2 start, Vector2[] origins, Texture2D[] textures, Vector2[] offsets, float[] rotationOffsets, SpriteBatch spriteBatch)
        {
            SpriteEffects direction = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 currentDrawPosition = start;
            //Main.NewText($"Drawing segment at: {currentDrawPosition}", Color.Green);
            for (int i = 0; i < textures.Length; i++)
            {
                float rotation = offsets[i].ToRotation() - rotationOffsets[i] - MathHelper.PiOver2;
                Texture2D texture = textures[i];
                Vector2 origin = origins[i];

                if (direction == SpriteEffects.FlipHorizontally)
                    origin.X = texture.Width - origin.X;

                spriteBatch.Draw(texture, currentDrawPosition, null, color, rotation, origin, scale, direction, 0f);

                currentDrawPosition += offsets[i];
            }
        }

        private Vector2 FindGroundVertical(Point tileCoordinates)
        {
            return new Vector2(tileCoordinates.X * 16, tileCoordinates.Y * 16);
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
            RenderLegs(spriteBatch);
            return false;
        }
    }

    // TODO -- Uhhhhh why is there an extensions method static class just hiding in this NPC file? It should be moved somewhere else and made into a separate .cs file.
    public static class ExtensionMethods
    {
        public static Vector2 ToWorldCoordinates(this Vector2 tilePosition, float offsetX = 0f, float offsetY = 0f)
        {
            return new Vector2(tilePosition.X * 16 + offsetX, tilePosition.Y * 16 + offsetY);
        }

        public static Point ToTileCoordinates(this Vector2 worldPosition)
        {
            return new Point((int)(worldPosition.X / 16), (int)(worldPosition.Y / 16));
        }

        public static float Size(this Texture2D texture)
        {
            return (texture.Width + texture.Height) / 2f;
        }
    }

}
