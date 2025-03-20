using CalamityMod;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics;
using NoxusBoss.Core.Graphics.LightingMask;
using NoxusBoss.Core.Graphics.Meshes;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.Utilities;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.BrutalForgiveness;

public class BrutalVine : ModProjectile
{
    internal record AppendangeData(Asset<Texture2D> Asset, Vector2 Origin);

    private readonly List<Vector3> oldPositions = new List<Vector3>(Lifetime);

    private readonly List<VineAppendage> appendages = new List<VineAppendage>(64);

    private static Asset<Texture2D> normalMapTexture;

    private static AppendangeData[] appendageTextures;

    private static InstancedRequestableTarget target;

    /// <summary>
    /// The owner of this vine.
    /// </summary>
    public ref Player Owner => ref Main.player[Projectile.owner];

    /// <summary>
    /// The current Z position of this vine.
    /// </summary>
    public ref float Z => ref Projectile.ai[0];

    /// <summary>
    /// How long this vine has existed for.
    /// </summary>
    public ref float Time => ref Projectile.ai[1];

    /// <summary>
    /// The twist offset angle of this vine.
    /// </summary>
    public ref float VineTwistAngle => ref Projectile.localAI[0];

    /// <summary>
    /// How long this vine should exist for.
    /// </summary>
    public static int Lifetime => 210;

    public override void SetStaticDefaults()
    {
        normalMapTexture = ModContent.Request<Texture2D>($"{Texture}NormalMap");
        appendageTextures =
        [
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage1"), new Vector2(0f, 203f)),
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage2"), new Vector2(12f, 520f)),
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage3"), new Vector2(18f, 1f)),
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage4"), new Vector2(42f, 174f)),
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage5"), new Vector2(84f, 118f)),
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage6"), new Vector2(72f, 216f)),
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage7"), new Vector2(4f, 354f)),
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage8"), new Vector2(30f, 36f)),
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage9"), new Vector2(90f, 419f)),
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage10"), new Vector2(78f, 334f)),
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage11"), new Vector2(62f, 326f)),
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage12"), new Vector2(12f, 85f)),
        ];

        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = Lifetime;
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 5000;

        target = new();
        Main.ContentThatNeedsRenderTargets.Add(target);
        On_Main.DrawProjectiles += DrawVinesSeparately;
    }

    public override void SetDefaults()
    {
        float vineSize = Main.rand?.NextFloat().Cubed() ?? 0f;
        Projectile.width = (int)MathHelper.Lerp(16f, 54f, vineSize);
        Projectile.height = Projectile.width;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = Lifetime;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.MaxUpdates = 3;
        Projectile.hide = true;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 1;
        Projectile.DamageType = DamageClass.Magic;
        ProjectileID.Sets.TrailCacheLength[Type] = Lifetime;
    }

    public override void AI()
    {
        if (appendages.Count < 30 && Main.rand.NextBool(3) && Time >= 4f)
            GenerateAppendage();

        // Grow!
        Projectile.scale = MathF.Pow(LumUtils.InverseLerpBump(0f, 15f, Lifetime - 54f, Lifetime, Time), 0.75f);

        NPC? target = Projectile.FindTargetWithinRange(800f);
        if (target is not null)
            AttackTarget(target);
        SwirlAround();

        Z = (1f - LumUtils.Cos01(MathHelper.TwoPi * Time / 60f)) * 100f + 600f;

        // Twist around while appearing.
        VineTwistAngle += LumUtils.InverseLerp(56f, 16f, Time) * 0.075f + 0.002f;

        oldPositions.Add(new Vector3(Projectile.Center, Z));
        Time++;
    }

    /// <summary>
    /// Makes this vine twist around at its front, giving winding shapes as it travels.
    /// </summary>
    private void SwirlAround()
    {
        float swirlTime = MathHelper.TwoPi * Time / 35f + Projectile.identity * 1.1f;
        float swirlAngle = LumUtils.AperiodicSin(swirlTime) * 0.8f + MathF.Cos(swirlTime) * 0.9f;
        Vector2 swirl = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(swirlAngle);
        Projectile.Center += swirl * Projectile.scale * 15f;
    }

    /// <summary>
    /// Makes this vine impale a given NPC target.
    /// </summary>
    private void AttackTarget(NPC target)
    {
        float speedFactor = LumUtils.InverseLerp(0f, 30f, Time);
        Vector2 directionToTarget = Projectile.SafeDirectionTo(target.Center);
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, directionToTarget * speedFactor * 20f, 0.033f / Projectile.MaxUpdates);
        Projectile.velocity += directionToTarget * speedFactor * 2f;

        if (Vector2.Dot(Projectile.velocity, directionToTarget) < 0f)
            Projectile.velocity *= 0.98f;

        // Check if the target has a mercy ticker over their head or not.
        // If they don't, add one.
        int mercyID = ModContent.ProjectileType<Mercy>();
        bool hasTicker = false;
        foreach (Projectile mercy in Main.ActiveProjectiles)
        {
            if (mercy.type == mercyID && mercy.owner == Projectile.owner && mercy.As<Mercy>().TargetIndex == target.whoAmI)
            {
                hasTicker = true;
                break;
            }
        }
        if (!hasTicker)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.FeatherPreDisappear with { MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew }, target.Center).WithVolumeBoost(1.35f);
            if (Main.myPlayer == Projectile.owner)
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Top, Vector2.Zero, mercyID, 0, 0f, Projectile.owner, target.whoAmI);
        }
    }

    private void GenerateAppendage()
    {
        AppendangeData data = Main.rand.Next(appendageTextures);
        appendages.Add(new VineAppendage()
        {
            Angle = Main.rand.NextFloatDirection() * 0.51f + Projectile.velocity.ToRotation() + Main.rand.NextFromList(-MathHelper.PiOver2, MathHelper.PiOver2),
            Origin = data.Origin,
            Texture = data.Asset,
            VinePositionInterpolant = (oldPositions.Count / (float)Lifetime + Main.rand.NextFloat(0.11f)) % 0.93f,
            MaxScale = Main.rand.NextFloat(0.135f, 0.23f)
        });
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        ScreenShakeSystem.StartShakeAtPoint(target.Center, 3f, shakeStrengthDissipationIncrement: 0.6f);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        foreach (Vector3 position in oldPositions)
        {
            if (Utils.CenteredRectangle(new Vector2(position.X, position.Y), Projectile.Size * Projectile.scale).Intersects(targetHitbox))
                return true;
        }

        return false;
    }

    private void RenderAppendages()
    {
        float unwrapInterpolant = oldPositions.Count / (float)Lifetime;
        for (int i = 0; i < appendages.Count; i++)
        {
            Texture2D texture = appendages[i].Texture.Value;
            Vector2 origin = appendages[i].Origin;
            float vinePositionInterpolant = appendages[i].VinePositionInterpolant;

            int index = (int)(vinePositionInterpolant * (Lifetime - 2f));
            if (index >= oldPositions.Count - 1)
                continue;

            // Perform a bunch of math to calculate the scale of the appendage at the given position, making it appear and disappear based on how .
            float positionInterpolant = vinePositionInterpolant * (Lifetime - 1f) % 1f;
            float growthInterpolant = LumUtils.InverseLerp(-0.15f, 0.05f, positionInterpolant - (1f - unwrapInterpolant));
            float easedGrowthInterpolant = EasingCurves.Quadratic.Evaluate(EasingType.In, growthInterpolant);
            float scale = easedGrowthInterpolant * appendages[i].MaxScale * CalculateScaleAtVineInterpolant(vinePositionInterpolant);

            Vector3 position3D = Vector3.Lerp(oldPositions[index], oldPositions[index + 1], positionInterpolant);
            Vector2 drawPosition = new Vector2(position3D.X, position3D.Y) - Main.screenPosition;

            float wind = LumUtils.AperiodicSin(Main.GlobalTimeWrappedHourly * 0.7f + position3D.X * 0.03f) * 0.5f + Main.WindForVisuals;
            float rotation = appendages[i].Angle + wind * 0.18f;

            Main.spriteBatch.Draw(texture, drawPosition, null, Color.White, rotation, origin, scale, 0, 0f);
        }
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.PrepareForShaders(BlendState.NonPremultiplied);

        ManagedShader lightShader = ShaderManager.GetShader("HeavenlyArsenal.BrutalForgivenessAppendageShader");
        lightShader.SetTexture(LightingMaskTargetManager.LightTarget, 1);
        lightShader.TrySetParameter("screenSize", WotGUtils.ViewportSize);
        lightShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        lightShader.Apply();

        RenderAppendages();
        Main.spriteBatch.ResetToDefault();

        return false;
    }

    private float CalculateScaleAtVineInterpolant(float vineInterpolant)
    {
        return MathHelper.SmoothStep(0f, 1f, LumUtils.InverseLerp(-0.5f, 0f, vineInterpolant - (1f - Projectile.scale)));
    }

    // https://en.wikipedia.org/wiki/Rodrigues%27_rotation_formula
    private static Vector3 RodriguesRotation(Vector3 v, Vector3 axis, float angle)
    {
        float cosine = MathF.Cos(angle);
        float sine = MathF.Sin(angle);
        return v * cosine + Vector3.Cross(v, axis) * sine + axis * Vector3.Dot(axis, v) * (1 - cosine);
    }

    private void RenderVine()
    {
        int cylinderWidth = 8;
        int cylinderHeight = oldPositions.Count - 1;
        float unwrapInterpolant = oldPositions.Count / (float)Lifetime;
        if (cylinderHeight <= 0)
            return;

        VertexPositionColorNormalTexture[] vertices = new VertexPositionColorNormalTexture[(cylinderWidth + 1) * (cylinderHeight + 1)];
        short[] indices = new short[cylinderWidth * cylinderHeight * 6];

        for (int i = 0; i <= cylinderWidth; i++)
        {
            for (int j = 0; j < cylinderHeight; j++)
            {
                float vineInterpolant = j / (float)(cylinderHeight - 1f);

                float frontInterpolant = MathF.Pow(LumUtils.InverseLerp(0f, 0.06f / unwrapInterpolant, vineInterpolant), 0.7f);
                float tipInterpolant = LumUtils.InverseLerp(0.75f, 1f, vineInterpolant) + 0.0001f;
                float width = frontInterpolant * MathHelper.SmoothStep(1f, 0f, tipInterpolant) * Projectile.width * CalculateScaleAtVineInterpolant(vineInterpolant) * 0.5f;

                // MATH!
                float angle = MathHelper.TwoPi * i / cylinderWidth - VineTwistAngle;
                Vector3 start = oldPositions[j];
                Vector3 end = oldPositions[j + 1];
                Vector3 direction = Vector3.Normalize(end - start);
                Vector3 normal = RodriguesRotation(Vector3.UnitZ, direction, angle);
                Vector3 position = start + normal * width;
                Vector2 uv = new Vector2(i / (float)cylinderWidth, vineInterpolant * unwrapInterpolant);

                vertices[i + (cylinderWidth + 1) * j] = new VertexPositionColorNormalTexture(position, new Color(255, 255, 255), uv, normal);
            }
        }

        int index = 0;
        for (short y = 0; y < cylinderHeight - 1; y++)
        {
            for (short x = 0; x < cylinderWidth; x++)
            {
                short topLeft = (short)(y * (cylinderWidth + 1) + x);
                short topRight = (short)(topLeft + 1);
                short bottomLeft = (short)((y + 1) * (cylinderWidth + 1) + x);
                short bottomRight = (short)(bottomLeft + 1);

                indices[index++] = topLeft;
                indices[index++] = bottomRight;
                indices[index++] = bottomLeft;

                indices[index++] = topLeft;
                indices[index++] = topRight;
                indices[index++] = bottomRight;
            }
        }

        bool orthographic = true;
        Vector3 cameraPosition = new Vector3(Main.screenPosition + WotGUtils.ViewportSize * 0.5f, 0f);
        Matrix view = Matrix.CreateLookAt(cameraPosition, cameraPosition + Vector3.UnitZ, Vector3.UnitY * -Main.LocalPlayer.gravDir);
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2 / Main.GameViewMatrix.Zoom.X, Main.instance.GraphicsDevice.Viewport.AspectRatio, 0.01f, 6400f);
        if (orthographic)
        {
            view = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f) * Main.GameViewMatrix.TransformationMatrix;
            projection = Matrix.CreateOrthographicOffCenter(0f, WotGUtils.ViewportSize.X, WotGUtils.ViewportSize.Y, 0f, -2000f, 2000f);
        }

        Matrix matrix = view * projection;
        Vector3 lightPosition = new Vector3(SunMoonPositionRecorder.SunPosition / Main.ScreenSize.ToVector2(), -0.51f);

        ManagedShader vineShader = ShaderManager.GetShader("HeavenlyArsenal.BrutalForgivenessVineShader");
        vineShader.TrySetParameter("uWorldViewProjection", matrix);
        vineShader.TrySetParameter("screenSize", WotGUtils.ViewportSize);
        vineShader.TrySetParameter("gameZoom", Main.GameViewMatrix.Zoom);
        vineShader.TrySetParameter("textureLookupZoom", new Vector2(0.3f, 6f));
        vineShader.TrySetParameter("diffuseLightExponent", 2.85f);
        vineShader.TrySetParameter("ambientLight", Vector3.One);
        vineShader.TrySetParameter("lightPosition", lightPosition);
        vineShader.SetTexture(TextureAssets.Projectile[Type].Value, 1, SamplerState.LinearWrap);
        vineShader.SetTexture(normalMapTexture.Value, 2, SamplerState.LinearWrap);
        vineShader.SetTexture(LightingMaskTargetManager.LightTarget, 3);
        vineShader.Apply();

        Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);
    }

    private void DrawVinesSeparately(On_Main.orig_DrawProjectiles orig, Main self)
    {
        // Not doing this results in frustrating layering artifacts on the vines, with back vertices rendering over front vertices.
        if (LumUtils.AnyProjectiles(Type))
        {
            target.Request(Main.screenWidth, Main.screenHeight, 0, () =>
            {
                Main.spriteBatch.ResetToDefault(false);

                foreach (Projectile vine in Main.ActiveProjectiles)
                {
                    if (vine.type == Type)
                        vine.As<BrutalVine>().RenderVine();
                }

                Main.spriteBatch.End();
            });
            if (target.TryGetTarget(0, out RenderTarget2D? rt) && rt is not null)
            {
                Main.spriteBatch.Begin();
                Main.spriteBatch.Draw(rt, Main.screenLastPosition - Main.screenPosition, Color.White);
                Main.spriteBatch.End();
            }
        }
        orig(self);
    }
}
