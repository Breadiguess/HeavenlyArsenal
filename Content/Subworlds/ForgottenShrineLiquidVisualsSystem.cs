using HeavenlyArsenal.Content.Waters;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.LightingMask;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using NoxusBoss.Core.Utilities;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Subworlds;

public class ForgottenShrineLiquidVisualsSystem : ModSystem
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct VertexPositionVectorTexture : IVertexType
    {
        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return new VertexDeclaration(
                    [
                        new VertexElement(
                            0,
                            VertexElementFormat.Vector3,
                            VertexElementUsage.Position,
                            0
                        ),
                        new VertexElement(
                            sizeof(float) * 3,
                            VertexElementFormat.Vector4,
                            VertexElementUsage.Color,
                            0
                        ),
                        new VertexElement(
                            sizeof(float) * 7,
                            VertexElementFormat.Vector2,
                            VertexElementUsage.TextureCoordinate,
                            0
                        )
                    ]);
            }
        }

        public Vector3 Position;
        public Vector4 Color;
        public Vector2 TextureCoordinate;

        public VertexPositionVectorTexture(Vector3 position, Vector4 color, Vector2 textureCoordinate)
        {
            Position = position;
            Color = color;
            TextureCoordinate = textureCoordinate;
        }
    }

    private static bool prepareLiquidDistanceTarget;

    internal static ManagedRenderTarget liquidDistanceTarget;

    /// <summary>
    /// A general purpose timer used for water perturbations that increments based on wind speed and direction.
    /// </summary>
    public static float WindTimer
    {
        get;
        private set;
    }

    /// <summary>
    /// A queue of points that determines where points in space should be converted to ripples, in world coordinates.
    /// </summary>
    public static readonly Queue<Vector2> PointsToAddRipplesAt = new Queue<Vector2>(32);

    /// <summary>
    /// The render target responsible for temporarily storing information to be swapped over back into the original target.
    /// </summary>
    public static ManagedRenderTarget UpdateTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// The render target responsible for water ripple effects.
    /// </summary>
    public static ManagedRenderTarget WaterStepRippleTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// Whether water effects for this system are active or not.
    /// </summary>
    public static bool WaterEffectsActive => ForgottenShrineSystem.WasInSubworldLastFrame;

    /// <summary>
    /// The sound played when players walk on water and create ripples.
    /// </summary>
    public static readonly SoundStyle RippleStepSound = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Environment/WaterRipple", 3);

    /// <summary>
    /// The render target that holds vertical liquid distance information.
    /// </summary>
    public static ManagedRenderTarget LiquidDistanceTarget
    {
        get
        {
            prepareLiquidDistanceTarget = true;
            return liquidDistanceTarget;
        }
    }

    public override void OnModLoad()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            UpdateTarget = new ManagedRenderTarget(true, (width, height) =>
            {
                return new RenderTarget2D(Main.instance.GraphicsDevice, width / 2, height / 2, true, SurfaceFormat.Vector4, DepthFormat.Depth24);
            });
            WaterStepRippleTarget = new ManagedRenderTarget(true, (width, height) =>
            {
                return new RenderTarget2D(Main.instance.GraphicsDevice, width / 2, height / 2, true, SurfaceFormat.Vector4, DepthFormat.Depth24);
            });
            liquidDistanceTarget = new ManagedRenderTarget(true, (width, height) =>
            {
                return new RenderTarget2D(Main.instance.GraphicsDevice, width, height, true, SurfaceFormat.Vector4, DepthFormat.Depth24);
            });
        }

        On_Main.CalculateWaterStyle += ForceShrineWater;
        On_WaterShaderData.Apply += DisableIdleLiquidDistortion;
        RenderTargetManager.RenderTargetUpdateLoopEvent += UpdateTargets;
    }

    // Not doing this results in beach water somehow having priority over shrine water in the outer parts of the subworld.
    private static int ForceShrineWater(On_Main.orig_CalculateWaterStyle orig, bool ignoreFountains)
    {
        if (SubworldSystem.IsActive<ForgottenShrineSubworld>())
            return ModContent.GetInstance<ForgottenShrineWater>().Slot;

        return orig(ignoreFountains);
    }

    private static void DisableIdleLiquidDistortion(On_WaterShaderData.orig_Apply orig, WaterShaderData self)
    {
        // Ensure that orig is still called, so as to not mess up any detours to this method made by other mods.
        orig(self);

        // However, at the same time, if the subworld is active, apply a separate water distortion shader, so that the water can be rendered completely still by default.
        if (SubworldSystem.IsActive<ForgottenShrineSubworld>())
        {
            float perturbationStrength = LumUtils.InverseLerp(0f, 1f, MathF.Abs(Main.windSpeedCurrent)) * 0.023f;
            Vector2 perturbationScroll = WindTimer * new Vector2(2.3f, 0.98f);
            Vector2 screenSize = Main.ScreenSize.ToVector2();
            RenderTarget2D distortionTarget = (RenderTarget2D)typeof(WaterShaderData).GetField("_distortionTarget", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self);
            ManagedShader waterShader = ShaderManager.GetShader("HeavenlyArsenal.ShrineWaterShader");
            waterShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
            waterShader.TrySetParameter("screenOffset", (Main.screenPosition - Main.screenLastPosition) / screenSize);
            waterShader.TrySetParameter("targetSize", screenSize);
            waterShader.TrySetParameter("perturbationStrength", perturbationStrength);
            waterShader.TrySetParameter("perturbationScroll", perturbationScroll);
            waterShader.TrySetParameter("screenPosition", Main.screenPosition);
            waterShader.SetTexture(distortionTarget, 1);
            waterShader.SetTexture(TileTargetManagers.LiquidTarget, 2, SamplerState.LinearClamp);
            waterShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 3, SamplerState.LinearWrap);
            waterShader.Apply();

            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        }
    }

    private static void UpdateTargets()
    {
        if (prepareLiquidDistanceTarget)
        {
            PrepareLiquidDistanceTarget();
            prepareLiquidDistanceTarget = false;
        }

        if (!WaterEffectsActive || Main.gamePaused)
            return;

        // R = Pressure.
        // G = Pressure speed.
        // B = Gradient X.
        // A = Gradient Y.
        GraphicsDevice gd = Main.instance.GraphicsDevice;

        gd.SetRenderTarget(WaterStepRippleTarget);
        gd.Clear(Color.Transparent);
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
        RenderTargetWithUpdateLoop(UpdateTarget);
        Main.spriteBatch.End();

        gd.SetRenderTarget(UpdateTarget);
        gd.Clear(Color.Transparent);
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
        RenderTargetWithUpdateLoop(WaterStepRippleTarget);
        Main.spriteBatch.End();

        gd.SetRenderTarget(null);
    }

    private static void PrepareLiquidDistanceTarget()
    {
        GraphicsDevice gd = Main.instance.GraphicsDevice;
        gd.SetRenderTarget(liquidDistanceTarget);
        gd.Clear(Color.Transparent);

        int padding = 0;
        int left = (int)(Main.screenPosition.X / 16f - padding);
        int top = (int)(Main.screenPosition.Y / 16f - padding);
        int right = (int)(left + gd.Viewport.Width / 16f + padding);
        int bottom = (int)(top + gd.Viewport.Height / 16f + padding);
        Rectangle tileArea = new Rectangle(left, top, right - left, bottom - top);

        Vector3 screenPosition3 = new Vector3(Main.screenPosition, 0f);

        int horizontalSamples = tileArea.Width / 2 + 1;
        int verticalSamples = tileArea.Height / 2 + 1;
        int meshWidth = tileArea.Width + 1;
        int meshHeight = tileArea.Height + 1;
        int vertexCount = meshWidth * meshHeight;
        int indexCount = tileArea.Width * tileArea.Height * 6;
        short[] indices = new short[indexCount];
        VertexPositionVectorTexture[] vertices = new VertexPositionVectorTexture[vertexCount];
        float[] waterLines = new float[horizontalSamples * 2];
        float[] tileLines = new float[horizontalSamples * 2];

        // Calculate samples.
        Parallel.For(0, horizontalSamples * 2, i =>
        {
            int x = tileArea.X + i;
            int waterLineY = 0;
            waterLines[i] = 1f;
            for (float y = Main.screenPosition.Y; y < Main.screenPosition.Y + Main.screenHeight + 32f; y += 16f)
            {
                int tileY = (int)(y / 16f);
                Tile t = Framing.GetTileSafely(x, tileY);
                bool solidTile = t.HasTile && Main.tileSolid[t.TileType];
                if (t.LiquidAmount >= 100 && !solidTile)
                {
                    waterLineY = tileY;
                    waterLines[i] = LumUtils.InverseLerp(Main.screenPosition.Y, Main.screenPosition.Y + Main.screenHeight, y);
                    break;
                }
            }

            if (waterLineY >= 1)
            {
                for (int dy = 0; dy < tileArea.Height; dy++)
                {
                    int y = waterLineY + dy;
                    Tile t = Framing.GetTileSafely(x, y);
                    bool solidTile = t.HasTile && Main.tileSolid[t.TileType];
                    if (solidTile)
                    {
                        tileLines[i] = LumUtils.InverseLerp(0f, Main.screenHeight / 16f, dy);
                        break;
                    }
                }
            }
            else
                tileLines[i] = 0f;
        });

        for (int j = 0; j < verticalSamples; j++)
        {
            float yInterpolant = j / (float)(verticalSamples - 1f);
            float nextYInterpolant = (j + 0.5f) / (float)(verticalSamples - 1f);
            for (int i = 0; i < horizontalSamples; i++)
            {
                float leftLine = waterLines[i * 2];
                float rightLine = waterLines[i * 2 + 1];
                float topLeftDistance = leftLine - yInterpolant;
                float topRightDistance = rightLine - yInterpolant;
                float bottomLeftDistance = leftLine - nextYInterpolant;
                float bottomRightDistance = rightLine - nextYInterpolant;

                float depthToGroundLeft = tileLines[i * 2];
                float depthToGroundRight = tileLines[i * 2 + 1];

                Vector4 topLeftColor = new Vector4(topLeftDistance, depthToGroundLeft, leftLine, 1f);
                Vector4 topRightColor = new Vector4(topRightDistance, depthToGroundRight, rightLine, 1f);
                Vector4 bottomLeftColor = new Vector4(bottomLeftDistance, depthToGroundLeft, leftLine, 1f);
                Vector4 bottomRightColor = new Vector4(bottomRightDistance, depthToGroundRight, rightLine, 1f);

                bool rightEdge = i * 2 == tileArea.Width;
                bool bottomEdge = j * 2 == tileArea.Height;

                Vector2 topLeftUv = new Vector2(i * 2f / (meshWidth - 1), j * 2f / (meshHeight - 1));
                Vector2 bottomRightUv = new Vector2((i * 2f + 1) / (meshWidth - 1), (j * 2f + 1) / (meshHeight - 1));

                vertices[i * 2 + j * 2 * meshWidth] = new VertexPositionVectorTexture(new Vector3(tileArea.X + i * 2, tileArea.Y + j * 2, 0f) * 16f - screenPosition3, topLeftColor, topLeftUv);
                if (!rightEdge)
                    vertices[i * 2 + 1 + j * 2 * meshWidth] = new VertexPositionVectorTexture(new Vector3(tileArea.X + i * 2 + 1, tileArea.Y + j * 2, 0f) * 16f - screenPosition3, topRightColor, new Vector2(bottomRightUv.X, topLeftUv.Y));
                if (!bottomEdge)
                    vertices[i * 2 + (j * 2 + 1) * meshWidth] = new VertexPositionVectorTexture(new Vector3(tileArea.X + i * 2, tileArea.Y + j * 2 + 1, 0f) * 16f - screenPosition3, bottomLeftColor, new Vector2(topLeftUv.X, bottomRightUv.Y));
                if (!bottomEdge && !rightEdge)
                    vertices[i * 2 + 1 + (j * 2 + 1) * meshWidth] = new VertexPositionVectorTexture(new Vector3(tileArea.X + i * 2 + 1, tileArea.Y + j * 2 + 1, 0f) * 16f - screenPosition3, bottomRightColor, bottomRightUv);
            }
        }
        int currentIndex = 0;
        for (int j = 0; j < meshHeight - 1; j++)
        {
            for (int i = 0; i < meshWidth - 1; i++)
            {
                indices[currentIndex] = (short)(i + j * meshWidth);
                indices[currentIndex + 1] = (short)(i + 1 + j * meshWidth);
                indices[currentIndex + 2] = (short)(i + (j + 1) * meshWidth);
                indices[currentIndex + 3] = (short)(i + 1 + j * meshWidth);
                indices[currentIndex + 4] = (short)(i + 1 + (j + 1) * meshWidth);
                indices[currentIndex + 5] = (short)(i + (j + 1) * meshWidth);
                currentIndex += 6;
            }
        }

        ManagedShader shader = ShaderManager.GetShader("Luminance.StandardPrimitiveShader");
        shader.TrySetParameter("uWorldViewProjection", Matrix.CreateOrthographicOffCenter(0f, WotGUtils.ViewportSize.X, WotGUtils.ViewportSize.Y, 0f, 0f, 1f));
        shader.Apply();

        gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, currentIndex / 3);
        gd.SetRenderTarget(null);
    }

    private static void RenderTargetWithUpdateLoop(Texture2D texture)
    {
        int rippleIndex = 0;
        Vector2[] ripplePositions = [.. Enumerable.Repeat(Vector2.One * -99999f, 10)];
        while (PointsToAddRipplesAt.TryDequeue(out Vector2 rippleWorldPosition))
        {
            ripplePositions[rippleIndex] = (rippleWorldPosition - Main.screenPosition) / texture.Size() * 0.5f;

            rippleIndex++;
            if (rippleIndex >= ripplePositions.Length)
                break;
        }

        ManagedShader rippleShader = ShaderManager.GetShader("HeavenlyArsenal.ShrineWaterRippleUpdateShader");
        rippleShader.TrySetParameter("ripplePoints", ripplePositions);
        rippleShader.TrySetParameter("stepSize", Vector2.One / texture.Size());
        rippleShader.TrySetParameter("decayFactor", 0.996f);
        rippleShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
        rippleShader.Apply();

        Main.spriteBatch.Draw(texture, (Main.screenLastPosition - Main.screenPosition) * 0.25f, Color.White);
    }

    public override void PostUpdatePlayers()
    {
        foreach (Player p in Main.ActivePlayers)
        {
            bool headIsDry = !Collision.WetCollision(p.TopLeft, p.width, 16);
            bool waterAtFeet = Collision.WetCollision(p.TopLeft, p.width, p.height + 16);
            if (headIsDry && waterAtFeet && p.velocity.Length() >= 2f && Main.rand.NextBool(3))
            {
                SoundEngine.PlaySound(RippleStepSound with { MaxInstances = 1, Volume = 0.2f, PitchVariance = 0.15f, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew }, p.Bottom);
                PointsToAddRipplesAt.Enqueue(p.Bottom + Vector2.UnitY * 5f + Main.rand.NextVector2Circular(4f, 0f));
            }
        }
        UpdateWaterShaders();
    }

    public override void PostDrawTiles()
    {
        ManagedScreenFilter mistShader = ShaderManager.GetFilter("HeavenlyArsenal.ForgottenShrineMistShader");
        ManagedScreenFilter reflectionShader = ShaderManager.GetFilter("HeavenlyArsenal.ForgottenShrineWaterReflectionShader");
        if (!WaterEffectsActive)
        {
            for (int i = 0; i < 30; i++)
            {
                mistShader.Update();
                reflectionShader.Update();
            }

            return;
        }

        UpdateWaterShaders();

        WindTimer += Main.windSpeedCurrent / 60f;
        if (MathF.Abs(WindTimer) >= 1000f)
            WindTimer = 0f;
    }

    private static void UpdateWaterShaders()
    {
        ManagedScreenFilter mistShader = ShaderManager.GetFilter("HeavenlyArsenal.ForgottenShrineMistShader");
        ManagedScreenFilter reflectionShader = ShaderManager.GetFilter("HeavenlyArsenal.ForgottenShrineWaterReflectionShader");
        mistShader.TrySetParameter("targetSize", Main.ScreenSize.ToVector2());
        mistShader.TrySetParameter("oldScreenPosition", Main.screenLastPosition);
        mistShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        mistShader.TrySetParameter("mistColor", new Color(84, 74, 154).ToVector4());
        mistShader.TrySetParameter("noiseAppearanceThreshold", 0.3f);
        mistShader.TrySetParameter("noiseAppearanceSmoothness", 0.59f);
        mistShader.TrySetParameter("mistCoordinatesZoom", new Vector2(1f, 0.4f));
        mistShader.TrySetParameter("mistHeight", 160f);
        mistShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
        mistShader.SetTexture(TileTargetManagers.LiquidTarget, 2, SamplerState.LinearClamp);
        mistShader.SetTexture(LightingMaskTargetManager.LightTarget, 3, SamplerState.LinearClamp);
        mistShader.SetTexture(LiquidDistanceTarget, 4, SamplerState.LinearClamp);
        mistShader.SetTexture(TileTargetManagers.TileTarget, 5, SamplerState.LinearClamp);
        mistShader.Activate();

        float perturbationStrength = LumUtils.InverseLerp(0f, 1.2f, MathF.Abs(Main.windSpeedCurrent)) * 0.015f;
        reflectionShader.TrySetParameter("targetSize", Main.ScreenSize.ToVector2());
        reflectionShader.TrySetParameter("oldScreenPosition", Main.screenLastPosition);
        reflectionShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        reflectionShader.TrySetParameter("reflectionStrength", 0.47f);
        reflectionShader.TrySetParameter("reflectionMaxDepth", 276f);
        reflectionShader.TrySetParameter("reflectionWaviness", 0.0023f);
        reflectionShader.TrySetParameter("ripplePerspectiveSquishFactor", 2.36f);
        reflectionShader.TrySetParameter("perturbationStrength", perturbationStrength);
        reflectionShader.TrySetParameter("perturbationScroll", Main.GlobalTimeWrappedHourly * new Vector2(Main.windSpeedCurrent.NonZeroSign() * 2f, 0.2f));
        reflectionShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
        reflectionShader.SetTexture(TileTargetManagers.LiquidTarget, 2, SamplerState.LinearClamp);
        reflectionShader.SetTexture(WaterStepRippleTarget, 3, SamplerState.LinearClamp);
        reflectionShader.SetTexture(TileTargetManagers.TileTarget, 4, SamplerState.LinearClamp);
        reflectionShader.SetTexture(LiquidDistanceTarget, 5, SamplerState.LinearClamp);
        reflectionShader.Activate();
    }
}
