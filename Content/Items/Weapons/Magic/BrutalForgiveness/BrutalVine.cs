using System.Collections.Generic;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics;
using NoxusBoss.Core.Graphics.LightingMask;
using NoxusBoss.Core.Graphics.Meshes;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.Utilities;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.BrutalForgiveness;

public class BrutalVine : ModProjectile
{
    internal record AppendangeData(Asset<Texture2D> Asset, Vector2 Origin);

    private static Asset<Texture2D> normalMapTexture;

    private static AppendangeData[] appendageTextures;

    private static InstancedRequestableTarget target;

    private readonly List<Vector3> oldPositions = new(Lifetime);

    private readonly List<VineAppendage> appendages = new(64);

    /// <summary>
    ///     The owner of this vine.
    /// </summary>
    public ref Player Owner => ref Main.player[Projectile.owner];

    /// <summary>
    ///     The current Z position of this vine.
    /// </summary>
    public ref float Z => ref Projectile.ai[0];

    /// <summary>
    ///     How long this vine has existed for.
    /// </summary>
    public ref float Time => ref Projectile.ai[1];

    /// <summary>
    ///     The twist offset angle of this vine.
    /// </summary>
    public ref float VineTwistAngle => ref Projectile.localAI[0];

    /// <summary>
    ///     How long this vine should exist for.
    /// </summary>
    public static int Lifetime => 210;

    public override void SetStaticDefaults()
    {
        normalMapTexture = ModContent.Request<Texture2D>($"{Texture}NormalMap");

        appendageTextures =
        [
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage1"), new Vector2(0f, 203f)), // Oak tree.
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage2"), new Vector2(12f, 520f)), // Sakura branch.
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage3"), new Vector2(18f, 1f)), // Biblical fruit branch.
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage4"), new Vector2(42f, 174f)), // Plumeria flower.
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage5"), new Vector2(84f, 118f)), // Sunflower.
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage6"), new Vector2(72f, 216f)), // Lotus.
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage7"), new Vector2(4f, 354f)), // Ivy vine.
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage8"), new Vector2(30f, 36f)), // Pine tree branch.
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage9"), new Vector2(90f, 419f)), // White rose bush.
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage10"), new Vector2(78f, 334f)), // Lavender vine.
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage11"), new Vector2(62f, 326f)), // Hibiscus flowers.
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage12"), new Vector2(12f, 85f)), // Camellia flower.
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage13"), new Vector2(2f, 100f)), // Peacock feather.
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage14"), new Vector2(3f, 300f)), // Oyster mushroom.
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage15"), new Vector2(3f, 93f)), // Pomegranate branch.
            new AppendangeData(ModContent.Request<Texture2D>($"{Texture}Appendage16"), new Vector2(8f, 416f)) // Willow tree.
        ];

        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = Lifetime;
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 5000;

        target = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(target);
        On_Main.DrawPlayers_AfterProjectiles += DrawVinesSeparately;
    }

    public override void SetDefaults()
    {
        var vineSize = Main.rand?.NextFloat().Cubed() ?? 0f;
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
        {
            GenerateAppendage();
        }

        // Grow!
        Projectile.scale = MathF.Pow(LumUtils.InverseLerpBump(0f, 15f, Lifetime - 54f, Lifetime, Time), 0.75f);

        var target = Projectile.FindTargetWithinRange(800f);

        if (target is not null)
        {
            AttackTarget(target);
        }

        SwirlAround();

        Z = (1f - LumUtils.Cos01(MathHelper.TwoPi * Time / 60f)) * 100f + 600f;

        // Twist around while appearing.
        VineTwistAngle += LumUtils.InverseLerp(56f, 16f, Time) * 0.075f + 0.002f;

        oldPositions.Add(new Vector3(Projectile.Center, Z));
        Time++;
    }

    /// <summary>
    ///     Makes this vine twist around at its front, giving winding shapes as it travels.
    /// </summary>
    private void SwirlAround()
    {
        var swirlTime = MathHelper.TwoPi * Time / 35f + Projectile.identity * 1.1f;
        var swirlAngle = LumUtils.AperiodicSin(swirlTime) * 0.8f + MathF.Cos(swirlTime) * 0.9f;
        var swirl = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(swirlAngle);
        Projectile.Center += swirl * Projectile.scale * 15f;
    }

    /// <summary>
    ///     Makes this vine impale a given NPC target.
    /// </summary>
    private void AttackTarget(NPC target)
    {
        var speedFactor = LumUtils.InverseLerp(0f, 30f, Time);
        var directionToTarget = Projectile.SafeDirectionTo(target.Center);
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, directionToTarget * speedFactor * 20f, 0.033f / Projectile.MaxUpdates);
        Projectile.velocity += directionToTarget * speedFactor * 2f;

        if (Vector2.Dot(Projectile.velocity, directionToTarget) < 0f)
        {
            Projectile.velocity *= 0.98f;
        }

        // Check if the target has a mercy ticker over their head or not.
        // If they don't, add one.
        var mercyID = ModContent.ProjectileType<Mercy>();
        var hasTicker = false;

        foreach (var mercy in Main.ActiveProjectiles)
        {
            if (mercy.type == mercyID && mercy.owner == Projectile.owner && mercy.As<Mercy>().TargetIndex == target.whoAmI)
            {
                hasTicker = true;

                break;
            }
        }

        if (!hasTicker)
        {
            SoundEngine.PlaySound
                (
                    GennedAssets.Sounds.NamelessDeity.FeatherPreDisappear with
                    {
                        MaxInstances = 1,
                        SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
                    },
                    target.Center
                )
                .WithVolumeBoost(1.35f);

            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Top, Vector2.Zero, mercyID, 0, 0f, Projectile.owner, target.whoAmI);
            }
        }
    }

    private void GenerateAppendage()
    {
        var data = Main.rand.Next(appendageTextures);

        appendages.Add
        (
            new VineAppendage
            {
                Angle = Main.rand.NextFloatDirection() * 0.51f + Projectile.velocity.ToRotation() + Main.rand.NextFromList(-MathHelper.PiOver2, MathHelper.PiOver2),
                Origin = data.Origin,
                Texture = data.Asset,
                VinePositionInterpolant = (oldPositions.Count / (float)Lifetime + Main.rand.NextFloat(0.11f)) % 0.93f,
                MaxScale = Main.rand.NextFloat(0.135f, 0.23f)
            }
        );
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (ScreenShakeSystem.OverallShakeIntensity < 5f)
        {
            ScreenShakeSystem.StartShakeAtPoint(target.Center, 2.5f, shakeStrengthDissipationIncrement: 0.6f);
        }
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        foreach (var position in oldPositions)
        {
            if (Utils.CenteredRectangle(new Vector2(position.X, position.Y), Projectile.Size * Projectile.scale).Intersects(targetHitbox))
            {
                return true;
            }
        }

        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.PrepareForShaders(BlendState.NonPremultiplied);

        var lightShader = ShaderManager.GetShader("HeavenlyArsenal.BrutalForgivenessAppendageShader");
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
        var cosine = MathF.Cos(angle);
        var sine = MathF.Sin(angle);

        return v * cosine + Vector3.Cross(v, axis) * sine + axis * Vector3.Dot(axis, v) * (1 - cosine);
    }

    private void RenderVine()
    {
        var cylinderWidth = 8;
        var cylinderHeight = oldPositions.Count - 1;
        var unwrapInterpolant = oldPositions.Count / (float)Lifetime;

        if (cylinderHeight <= 0)
        {
            return;
        }

        var vertices = new VertexPositionColorNormalTexture[(cylinderWidth + 1) * (cylinderHeight + 1)];
        var indices = new short[cylinderWidth * cylinderHeight * 6];

        for (var i = 0; i <= cylinderWidth; i++)
        {
            for (var j = 0; j < cylinderHeight; j++)
            {
                var vineInterpolant = j / (cylinderHeight - 1f);

                var frontInterpolant = MathF.Pow(LumUtils.InverseLerp(0f, 0.06f / unwrapInterpolant, vineInterpolant), 0.7f);
                var tipInterpolant = LumUtils.InverseLerp(0.75f, 1f, vineInterpolant) + 0.0001f;
                var width = frontInterpolant * MathHelper.SmoothStep(1f, 0f, tipInterpolant) * Projectile.width * CalculateScaleAtVineInterpolant(vineInterpolant) * 0.5f;

                // MATH!
                var angle = MathHelper.TwoPi * i / cylinderWidth - VineTwistAngle;
                var start = oldPositions[j];
                var end = oldPositions[j + 1];
                var direction = Vector3.Normalize(end - start);
                var normal = RodriguesRotation(Vector3.UnitZ, direction, angle);
                var position = start + normal * width;
                var uv = new Vector2(i / (float)cylinderWidth, vineInterpolant * unwrapInterpolant);

                vertices[i + (cylinderWidth + 1) * j] = new VertexPositionColorNormalTexture(position, new Color(255, 255, 255), uv, normal);
            }
        }

        var index = 0;

        for (short y = 0; y < cylinderHeight - 1; y++)
        {
            for (short x = 0; x < cylinderWidth; x++)
            {
                var topLeft = (short)(y * (cylinderWidth + 1) + x);
                var topRight = (short)(topLeft + 1);
                var bottomLeft = (short)((y + 1) * (cylinderWidth + 1) + x);
                var bottomRight = (short)(bottomLeft + 1);

                indices[index++] = topLeft;
                indices[index++] = bottomRight;
                indices[index++] = bottomLeft;

                indices[index++] = topLeft;
                indices[index++] = topRight;
                indices[index++] = bottomRight;
            }
        }

        var cameraPosition = new Vector3(Main.screenPosition + WotGUtils.ViewportSize * 0.5f, 0f);
        var view = Matrix.CreateTranslation(-Main.screenPosition.X, -Main.screenPosition.Y, 0f) * Main.GameViewMatrix.TransformationMatrix;
        var projection = Matrix.CreateOrthographicOffCenter(0f, WotGUtils.ViewportSize.X, WotGUtils.ViewportSize.Y, 0f, -2000f, 2000f);
        var matrix = view * projection;
        var lightPosition = new Vector3(SunMoonPositionRecorder.SunPosition / Main.ScreenSize.ToVector2(), -0.51f);

        var vineShader = ShaderManager.GetShader("HeavenlyArsenal.BrutalForgivenessVineShader");
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

    private void RenderAppendages()
    {
        var unwrapInterpolant = oldPositions.Count / (float)Lifetime;

        for (var i = 0; i < appendages.Count; i++)
        {
            var texture = appendages[i].Texture.Value;
            var origin = appendages[i].Origin;
            var vinePositionInterpolant = appendages[i].VinePositionInterpolant;

            var index = (int)(vinePositionInterpolant * (Lifetime - 2f));

            if (index >= oldPositions.Count - 1)
            {
                continue;
            }

            // Perform a bunch of math to calculate the scale of the appendage at the given position, making it appear and disappear based on how .
            var positionInterpolant = vinePositionInterpolant * (Lifetime - 1f) % 1f;
            var growthInterpolant = LumUtils.InverseLerp(-0.15f, 0.05f, positionInterpolant - (1f - unwrapInterpolant));
            var easedGrowthInterpolant = EasingCurves.Quadratic.Evaluate(EasingType.In, growthInterpolant);
            var scale = easedGrowthInterpolant * appendages[i].MaxScale * CalculateScaleAtVineInterpolant(vinePositionInterpolant);

            var position3D = Vector3.Lerp(oldPositions[index], oldPositions[index + 1], positionInterpolant);
            var drawPosition = new Vector2(position3D.X, position3D.Y) - Main.screenPosition;

            var wind = LumUtils.AperiodicSin(Main.GlobalTimeWrappedHourly * 0.7f + position3D.X * 0.03f) * 0.5f + Main.WindForVisuals;
            var rotation = appendages[i].Angle + wind * 0.18f;

            Main.spriteBatch.Draw(texture, drawPosition, null, Color.White, rotation, origin, scale, 0, 0f);
        }
    }

    private void DrawVinesSeparately(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self)
    {
        orig(self);

        // Not doing this results in frustrating layering artifacts on the vines, with back vertices rendering over front vertices.
        if (LumUtils.AnyProjectiles(Type))
        {
            target.Request
            (
                Main.screenWidth,
                Main.screenHeight,
                0,
                () =>
                {
                    Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

                    foreach (var vine in Main.ActiveProjectiles)
                    {
                        if (vine.type == Type)
                        {
                            vine.As<BrutalVine>().RenderVine();
                        }
                    }
                }
            );

            if (target.TryGetTarget(0, out var rt) && rt is not null)
            {
                Main.spriteBatch.Begin();
                Main.spriteBatch.Draw(rt, Main.screenLastPosition - Main.screenPosition, Color.White);
                Main.spriteBatch.End();
            }
        }
    }
}