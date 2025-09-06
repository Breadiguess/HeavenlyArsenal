using HeavenlyArsenal.Common;
using HeavenlyArsenal.Common.Utilities;
using HeavenlyArsenal.Content.Particles.Metaballs;
using HeavenlyArsenal.Core.Physics.ClothManagement;
using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;
using static Terraria.ModLoader.PlayerDrawLayer;
using Player = Terraria.Player;

namespace HeavenlyArsenal.Content.Items.Armor.ShintoArmor
{
    internal class CloakWingSystem : ModPlayer
    {
        public bool Active;

        public float WingRotation = 0f;
        public float WingFlapProgress = 0;
        public float WingActivationProgress = 0;
        public static float WingCycleTime = 57;
        
        public int Time = 0;

        private PiecewiseCurve _flapCurve;
        private float _cachedStartRot = float.NaN;

        public bool WingsActive
        {
            get;
            set;
        }
        public override void PostUpdateEquips()
        {
            Active = false;
        }

        private void FlapWings(float flapCompletion, float startingRotation)
        {

            if (_flapCurve == null || !startingRotation.Equals(_cachedStartRot))
            {
                _cachedStartRot = startingRotation;

                _flapCurve = new PiecewiseCurve()
                .Add(EasingCurves.Exp, EasingType.In, startingRotation + 2.3f, 0.5f, startingRotation)
                .Add(EasingCurves.Quadratic, EasingType.InOut, startingRotation + 1.86f, 0.6f)
                .Add(EasingCurves.Quadratic, EasingType.Out, startingRotation, 1f);
            }
            float previousWingRotation = WingRotation;
            float t = flapCompletion % 1f;
            WingRotation = _flapCurve.Evaluate(t);
            float wingSpeed = Math.Abs(previousWingRotation - WingRotation);

            if (wingSpeed >= 0.2f)
            {
                AntishadowBlob Blob = ModContent.GetInstance<AntishadowBlob>();
                for (int i = 0; i < 1; i++)

                {
                    float randomoffset = Main.rand.Next(-4, 4);
                    Vector2 bloodSpawnPosition = Player.Center + Main.rand.NextVector2CircularEdge(120 + randomoffset, 50);

                    //var dust = Dust.NewDustPerfect(bloodSpawnPosition, DustID.AncientLight, Vector2.Zero, default, Color.Red);
                    //dust.noGravity = true;
                    Blob.player = Player;

                    Blob.CreateParticle(bloodSpawnPosition, Vector2.Zero, 0, 0);
                }
            }
            Main.NewText($"{WingRotation}");
        }
        public override void FrameEffects()
        {
            //If the wing logic isnt the same as the visual wings that means its vanity. Vanilla does the same to check if the wings are real or not
           
            if (Player.GetModPlayer<ShintoArmorPlayer>().ShadowVeil)
            {
                
                if (WingsActive)
                {
                    WingActivationProgress = float.Lerp(WingActivationProgress, 1, 0.5f);
                    float baseRotation = Math.Abs(Player.velocity.Y)*-0.02f;
                    if (Player.controlJump)
                    {
                        
                        FlapWings((float)Time / WingCycleTime, baseRotation);
                        WingFlapProgress = (float)Math.Sin(Time / 8f) * 1.15f - 0.75f;
                        Time++;
                        if (Time > WingCycleTime + 1)
                            Time = 0;
                    }

                    else
                    {
                        Time = (int)float.Lerp(Time, 0, 0.12f);
                        FlapWings(Time/WingCycleTime, baseRotation);
                    }


                }
                else
                {
                    
                    Time = 0;
                    WingActivationProgress = float.Lerp(WingActivationProgress, 0, 0.15f);
                    WingFlapProgress = float.Lerp(WingFlapProgress, -4, 0.25f);
                }
                    
                
            }
        }

        public override void PreUpdateMovement()
        {

            // Start flying if wings are triggered
            //if (!IsOnGround(Player))
            //{
             //   WingsActive = true;
            //}
            //else
            //    WingsActive = false;

           // if (Player.velocity.Y == 0f)
            //{
           //     WingsActive = false;
         //   }
        }
        bool IsOnGround(Player player)
        {
            // Only check when the player isn't moving vertically
           

            // Check the tile directly below the player
            int tileX = (int)(player.Center.X / 16f);
            int tileY = (int)((player.position.Y + player.height + 1f) / 16f);

            Tile tileBelow = Framing.GetTileSafely(tileX, tileY);

            // Solid tiles or solid-top tiles (like platforms)
            return tileBelow.HasUnactuatedTile && Main.tileSolid[tileBelow.TileType] && !Main.tileSolidTop[tileBelow.TileType];
        }
    }

    public class AntishadowWing : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.body == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorBreastplate), EquipType.Body);

        public override bool IsHeadLayer => false;

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            CloakWingSystem cloakWingSystem = drawInfo.drawPlayer.GetModPlayer<CloakWingSystem>();
            Player player = drawInfo.drawPlayer;



            if (!cloakWingSystem.WingsActive && cloakWingSystem.WingActivationProgress <=0.1f)
                return;
            float Rot = cloakWingSystem.WingRotation;//WingFlapProgress * MathHelper.Pi/3 * (drawInfo.drawPlayer.gravDir < 0 ? 1 : -1);

            Utils.DrawBorderString(Main.spriteBatch, "Rotation: " + Rot.ToString() + ", WingProgress: " + cloakWingSystem.WingFlapProgress.ToString(), player.Center - Main.screenPosition - Vector2.UnitY * -100, Color.AntiqueWhite);

            Utils.DrawBorderString(Main.spriteBatch, "Time: " + cloakWingSystem.Time.ToString(), player.Center - Main.screenPosition - Vector2.UnitY * -120, Color.AntiqueWhite);

            Vector2 Scale = new Vector2(0.75f * cloakWingSystem.WingActivationProgress, 0.75f* cloakWingSystem.WingActivationProgress);

            Texture2D wing = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Items/WingTexture").Value;
            Vector2 DrawPosL = drawInfo.BodyPosition() + new Vector2(10, 10);
            
            Vector2 LeftOrigin = new Vector2(0, wing.Height);

            SpriteEffects ef = player.direction == 1 ? SpriteEffects.None : SpriteEffects.None;

            DrawData leftwing = new DrawData(wing, DrawPosL, null, Color.AntiqueWhite, Rot, LeftOrigin, Scale, ef);
            
            drawInfo.DrawDataCache.Add(leftwing);

            Vector2 DrawPosR = drawInfo.BodyPosition() + new Vector2(-7.5f, 10);

            Vector2 RightOrigin = new Vector2(wing.Width, wing.Height);
            DrawData rightwing = new DrawData(wing, DrawPosR, null, Color.AntiqueWhite, -Rot, RightOrigin, Scale, SpriteEffects.FlipHorizontally);
            drawInfo.DrawDataCache.Add(rightwing);
        }
    }
    class ShintoArmorCapePlayer : ModPlayer
    {
        private float ExistenceTimer;

        public ClothSimulation Robe
        {
            get;
            set;
        } = new ClothSimulation(Vector3.Zero, 10, 8, 3f, 50f, 0.1f);


        public override void Load()
        {
            On_Main.CheckMonoliths += DrawAllTargets;
        }
        private void DrawAllTargets(On_Main.orig_CheckMonoliths orig)
        {
            drawToTarget?.Invoke(Main.spriteBatch);

            Main.spriteBatch.GraphicsDevice.SetRenderTarget(null);
            Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);

            orig();
        }

        private RenderTarget2D RobeTarget;
        private RenderTarget2D RobeMapTarget;

        private static event Action<SpriteBatch> drawToTarget;
        private const int frontSize = 200;
        private const int backSize = 600;

        public override void Initialize()
        {
            Main.QueueMainThreadAction(() =>
            {
                drawToTarget += DrawRobeToTarget;
                RobeMapTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, backSize, backSize);
                RobeTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, backSize, backSize);
            });
        }

        public void DrawRobeToTarget(SpriteBatch spritebatch)
        {
            if (Player != null)
            {
                if (!IsReady() || !ShaderManager.HasFinishedLoading) // God damn Luminance you slowpoke
                    return;

                Main.spriteBatch.GraphicsDevice.SetRenderTarget(RobeMapTarget);
                Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);

                Vector2 robePosition = Player.Center + new Vector2(4 * Player.direction, -50f * Player.gravDir).RotatedBy(Player.fullRotation);

                Matrix world = Matrix.CreateTranslation(-robePosition.X + backSize / 2, -robePosition.Y + backSize / 2, 0f);

                Matrix projection = Matrix.CreateOrthographicOffCenter(0, backSize, 600, 0, -1000, 1000);
                Matrix matrix = world * projection;

                ManagedShader clothShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssasinRobeShader");
                clothShader.TrySetParameter("opacity", LumUtils.InverseLerp(60f, 120f, ExistenceTimer));
                clothShader.TrySetParameter("transform", matrix);
                clothShader.Apply();
                Robe.Render();

                Main.spriteBatch.End();

                Main.spriteBatch.GraphicsDevice.SetRenderTarget(RobeTarget);
                Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);

                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);
                Vector3[] palette =
                [
                    new Vector3(1.5f),
                    new Vector3(0f, 1f, 1.2f),
                    new Vector3(1f, 0f, 0f),
                ];
                ManagedShader overlayShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssassinColorProcessingShader");
                overlayShader.TrySetParameter("eyeScale", 1f);
                overlayShader.TrySetParameter("gradient", palette);
                overlayShader.TrySetParameter("gradientCount", palette.Length);
                overlayShader.TrySetParameter("textureSize", RobeMapTarget.Size() * 2);
                overlayShader.TrySetParameter("edgeColor", Color.Red.ToVector4());
                overlayShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
                overlayShader.Apply();

                Main.spriteBatch.Draw(RobeMapTarget, new Vector2(backSize / 2), null, Color.Black, 0f, RobeMapTarget.Size() * 0.5f, 2f, 0, 0);

                Main.spriteBatch.End();
            }
        }

        public bool IsReady() => RobeTarget != null;
        public DrawData GetRobeTarget() => new DrawData(RobeTarget, Vector2.Zero + new Vector2(0, Player.gfxOffY), null, Color.White, -Player.fullRotation, RobeTarget.Size() * 0.5f, 1f, 0);


        public override void PostUpdateMiscEffects()
        {

            UpdateCloth();
            ExistenceTimer++;

        }

        private void UpdateCloth()
        {
            Robe.DampeningCoefficient = 0.17f;

            int steps = 15;
            float windSpeed = Math.Clamp(Main.WindForVisuals * 8f, -1.3f, 0f);
            Vector2 robePosition = Player.Center + new Vector2(0, -50f * Player.gravDir).RotatedBy(Player.fullRotation);
            robePosition += Main.OffsetsPlayerHeadgear[(int)(Player.bodyFrame.Y / Player.bodyFrame.Height)] + Player.velocity;
            Vector3 wind = Vector3.UnitX * (LumUtils.AperiodicSin(ExistenceTimer * 0.029f) * 0.67f + windSpeed) * 1.74f;
            for (int i = 0; i < steps; i++)
            {
                for (int x = 0; x < Robe.Width; x++)
                {
                    for (int y = 0; y < 2; y++)
                        ConstrainParticle(robePosition + new Vector2((6 - x) * Player.direction, 0), Robe.particleGrid[x, y], 0f);
                }

                Robe.Simulate(0.06f, false, Vector3.UnitY * (5f * Player.gravDir) + wind * Player.direction);
            }
        }

        private void ConstrainParticle(Vector2 anchor, ClothPoint? point, float angleOffset)
        {
            if (point is null)
                return;

            point.Position = new Vector3(anchor, 0f);
            point.IsFixed = false;
        }
    }
    public class AntiShadowCloak_DrawLayer : PlayerDrawLayer
    {

        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BackAcc);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        =>
           drawInfo.drawPlayer.body == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorBreastplate), EquipType.Body);
        public override bool IsHeadLayer => false;


        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            ShintoArmorCapePlayer capePlayer = drawInfo.drawPlayer.GetModPlayer<ShintoArmorCapePlayer>();
            CloakWingSystem cloakWing = drawInfo.drawPlayer.GetModPlayer<CloakWingSystem>();


            if (!capePlayer.IsReady() || drawInfo.shadow > 0f)
                return;

            drawInfo.drawPlayer.GetModPlayer<ShintoArmorPlayer>().ShadowVeil = true;

            DrawData data = capePlayer.GetRobeTarget();
            data.position = drawInfo.BodyPosition() + new Vector2(2 * drawInfo.drawPlayer.direction, ((drawInfo.drawPlayer.gravDir < 0 ? 11 : 0) + -8) * drawInfo.drawPlayer.gravDir);
            data.color = Color.White *  (1-cloakWing.WingActivationProgress);
            data.effect = Main.GameViewMatrix.Effects;
            data.shader = drawInfo.cBody;
            
            //Main.NewText($"Position: {data.position}", Color.AntiqueWhite);
            drawInfo.DrawDataCache.Add(data);


        }
    }
}
