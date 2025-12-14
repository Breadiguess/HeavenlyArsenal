using System.Collections.Generic;
using CalamityMod;
using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.Meshes;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight;

//your stereotypical godsword.
public class SilentLight : ModProjectile
{
    public enum SwordState
    {
        Normal,

        Thrown
    }

    public bool Thrown;

    public PiecewiseCurve SwingCurve;

    private float WindupTimer = 0;

    private bool Swinging;

    private bool AltFire;

    public int stage
    {
        get => (int)Projectile.ai[2];
        set => Projectile.ai[2] = value;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.scale = 0;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        if (AltFire)
        {
            overPlayers.Add(index);
        }
    }

    public override void AI()
    {
        Projectile.damage = (int)Owner.GetTotalDamage(DamageClass.Melee).ApplyTo(Owner.HeldItem.damage * 1.4f);
        Projectile.ContinuouslyUpdateDamageStats = true;
        Owner.heldProj = Projectile.whoAmI;
        CheckDespawnConditions();

        if (stage != 3)
        {
            Projectile.Center = Owner.GetFrontHandPositionImproved(Owner.compositeFrontArm);
        }

        Projectile.timeLeft++;
        Timer++;

        if (AltFire)
        {
            Projectile.extraUpdates = 0;
            PlaceholderName();

            return;
        }

        if (Swinging)
        {
            ManageSwing();

            return;
        }

        if (Owner.altFunctionUse == 2)
        {
            ResetValues();
            AltFire = true;
        }

        if (Owner.controlUseItem && !Swinging && Owner.altFunctionUse != 2)
        {
            Swinging = true;
        }
    }

    public void CheckDespawnConditions()
    {
        if (!Glass.Empowered || Owner.HeldItem.type != ModContent.ItemType<Rapture>() || Owner.dead || !Owner.active || Owner.HasBuff(BuffID.Cursed))
        {
            Projectile.Kill();
        }
    }

    public void ManageSwing()
    {
        Owner.SetDummyItemTime(2);
        UpdateSlash();
        UpdateTrail();
        canHit = true;

        Projectile.extraUpdates = 5;

        if (stage == 0)
        {
            if (t == 0)
            {
                swingDirection = Owner.Center.AngleTo(Main.MouseWorld) + MathHelper.ToRadians(190) * Owner.direction;

                SoundEngine.PlaySound
                (
                    AssetDirectory.Sounds.Items.Weapons.Rapture.Swing with
                    {
                        PitchVariance = 0.2f,
                        Pitch = 0.4f
                    },
                    Projectile.Center
                );
            }

            if (t <= 0.2)
            {
                Projectile.scale = float.Lerp(Projectile.scale, 1.5f, t);
                Owner.direction = Math.Sign(Main.MouseWorld.X - Owner.Center.X);
            }

            if (t >= 0.4f && t <= 0.6f)
            {
                ScreenShakeSystem.StartShake(0.1f);
            }

            if (SwingCurve == null)
            {
                SwingCurve = new PiecewiseCurve()
                    .Add(EasingCurves.Sextic, EasingType.Out, 1, 1);
            }

            t = Math.Clamp(t + 0.005f, 0, 1);
            swingInterp = SwingCurve.Evaluate(t);

            Projectile.rotation = swingDirection + MathHelper.TwoPi * swingInterp * Owner.direction;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

            if (t >= 0.74)
            {
                Projectile.scale *= 0.88f;
            }

            if (t == 1)
            {
                ResetValues();

                return;
            }
        }

        if (stage == 1)
        {
            if (t == 0)
            {
                swingDirection = Owner.Center.AngleTo(Main.MouseWorld) + MathHelper.ToRadians(290) * Owner.direction;

                //Main.NewText($"{Projectile.rotation}");
                SoundEngine.PlaySound
                (
                    AssetDirectory.Sounds.Items.Weapons.Rapture.Swing with
                    {
                        PitchVariance = 0.2f,
                        Pitch = 0.6f
                    },
                    Projectile.Center
                );
            }

            if (t <= 0.2)
            {
                Projectile.scale = float.Lerp(Projectile.scale, 1.5f, t);
                Owner.direction = Math.Sign(Main.MouseWorld.X - Owner.Center.X);
            }

            if (t >= 0.4f && t <= 0.6f)
            {
                ScreenShakeSystem.StartShake(0.1f);
            }

            if (SwingCurve == null)
            {
                SwingCurve = new PiecewiseCurve()
                    .Add(EasingCurves.Sextic, EasingType.Out, 1, 1);
            }

            t = Math.Clamp(t + 0.005f, 0, 1);
            swingInterp = SwingCurve.Evaluate(t);
            Projectile.rotation = swingDirection + MathHelper.TwoPi * 1.2f * -swingInterp * Owner.direction;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

            if (t >= 0.74)
            {
                Projectile.scale *= 0.88f;
            }

            if (t == 1)
            {
                ResetValues();

                return;
            }
        }

        if (stage == 2)
        {
            Projectile.extraUpdates = 12;

            //Main.NewText($"{t}");
            if (t <= 0.2f)
            {
                swingDirection = Owner.Center.AngleTo(Main.MouseWorld);
                Projectile.rotation = swingDirection;
                Owner.direction = Math.Sign(swingDirection.ToRotationVector2().X);
            }

            if (SwingCurve == null)
            {
                SwingCurve = new PiecewiseCurve()
                    .Add(EasingCurves.Exp, EasingType.Out, 1, 0.75f)
                    .Add(EasingCurves.Circ, EasingType.Out, 0, 1.0f);
            }

            t = Math.Clamp(t + 0.005f, 0, 1);
            swingInterp = SwingCurve.Evaluate(t);
            Projectile.velocity = Projectile.rotation.ToRotationVector2() * swingInterp * 10;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

            if (t >= 0.74)
            {
                Projectile.scale *= 0.88f;
            }
            else
            {
                Projectile.scale = swingInterp * 1.5f;
            }

            if (t == 1)
            {
                ResetValues();

                return;
            }
        }

        if (stage == 3)
        {
            Projectile.localNPCHitCooldown = 25;

            if (t == 0)
            {
                Thrown = true;
                swingDirection = Owner.MountedCenter.AngleTo(Main.MouseWorld);
            }

            if (SwingCurve == null)
            {
                SwingCurve = new PiecewiseCurve()
                    .Add(EasingCurves.Sextic, EasingType.Out, 1f, 1f);
            }

            t = Math.Clamp(t + 0.005f, 0, 1);
            swingInterp = SwingCurve.Evaluate(t);

            Projectile.scale = float.Lerp(Projectile.scale, 1, 0.2f);
            Projectile.velocity = new Vector2(30 * (1 - swingInterp), 0).RotatedBy(swingDirection);
            // Rotate several 360s but then end facing the player.
            var totalSpins = 6f; // Number of full rotations before ending
            var spinProgress = MathHelper.Clamp(swingInterp, 0f, 1f);
            var spinRotation = swingDirection + MathHelper.TwoPi * totalSpins * spinProgress * Owner.direction;
            var endRotation = Projectile.Center.AngleTo(Owner.Center);

            Projectile.rotation = MathHelper.Lerp(spinRotation, endRotation, spinProgress);

            if (t > 0.8f)
            {
                swingDirection = Owner.MountedCenter.AngleTo(Main.MouseWorld);
                Projectile.Center = Vector2.Lerp(Projectile.Center, Owner.Center, 0.1f);
                Projectile.scale = float.Lerp(Projectile.scale, 0, 0.2f);
            }

            if (t == 1)
            {
                ResetValues();
            }
        }
    }

    public void ResetValues()
    {
        Thrown = false;
        t = 0;
        //Glass.EmpoweredAttackCount--;
        swingDirection = 0;
        swingInterp = 0;
        Projectile.scale = 0;
        Projectile.localNPCHitCooldown = 70;
        AltFire = false;
        AltFireInterpolant = 0;

        SwingCurve = null;
        Swinging = false;
        stage++;
        Projectile.ResetLocalNPCHitImmunity();
        ResetSlash();
        ResetTrail();

        if (stage > 3)
        {
            stage = 0;
        }

        Timer = 0;
    }

    private void PlaceholderName()
    {
        var Threshold = 120 * 6;
        var ToMouse = Owner.MountedCenter.AngleTo(Main.MouseWorld);

        swingDirection = swingDirection.AngleLerp(ToMouse, 0.06f);

        if (Timer == 1)
        {
            swingDirection = ToMouse;
        }

        Owner.direction = Math.Sign(Main.MouseWorld.X - Owner.Center.X);
        Projectile.velocity = swingDirection.ToRotationVector2() * 10;

        if (Timer < Threshold)
        {
            AltFireInterpolant = float.Lerp(AltFireInterpolant, 1, 0.05f);
        }
        else
        {
            if (Timer > Threshold)
            {
                AltFireInterpolant = float.Lerp(AltFireInterpolant, 0, 0.2f);

                if (AltFireInterpolant < 0.02f)
                {
                    ResetValues();

                    return;
                }
            }
        }

        if (AltFireInterpolant > 0.9f)
        {
            if (Timer % 2 == 0)
            {
                var SpawnPos = Owner.GetBackHandPosition(Player.CompositeArmStretchAmount.Full, swingDirection - MathHelper.PiOver2);
                SpawnPos += new Vector2(20, 0).RotatedBy(swingDirection);
                SpawnPos += Main.rand.NextVector2Circular(30, 30);
                var Damage = 999;
                var Velocity = swingDirection.ToRotationVector2() * 10;
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), SpawnPos, Velocity, ModContent.ProjectileType<LightSpear>(), Damage, 1);
            }
        }
    }

    #region setup

    private readonly float SlashDistance = 300;

    private float _slashScale = 1;

    private bool canHit;

    public GlassPlayer Glass => Owner.GetModPlayer<GlassPlayer>();

    public float AltFireInterpolant;

    public ref float t => ref Projectile.localAI[2];

    public ref float swingDirection => ref Projectile.localAI[1];

    public ref float Timer => ref Projectile.ai[0];

    public ref float swingInterp => ref Projectile.ai[1];

    public ref Player Owner => ref Main.player[Projectile.owner];

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 10;
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.scale = 1;
        Projectile.extraUpdates = 2;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;

        Projectile.localNPCHitCooldown = -1;
        Projectile.usesLocalNPCImmunity = true;
        const int slashLength = 24;
        _slashPositions = new Vector2[slashLength];
        _slashRotations = new float[slashLength];
    }

    #endregion

    #region HitCode

    public override void OnHitPlayer(Player target, Player.HurtInfo info) { }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) { }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.GetGlobalNPC<SilentLight_NPC>().Heat++;

        if (target.GetGlobalNPC<SilentLight_NPC>().Sun == null)
        {
            target.GetGlobalNPC<SilentLight_NPC>().Sun = Owner;
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (target.SuperArmor)
        {
            modifiers = modifiers with
            {
                SuperArmor = false
            };
        }

        if (stage == 2)
        {
            modifiers.SetCrit();
            modifiers.CritDamage = StatModifier.Default + 0.2f;
        }

        modifiers.DefenseEffectiveness = MultipliableFloat.One * 0;
    }

    public override bool? CanCutTiles()
    {
        return false;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        hitbox.Inflate(300, 300);
        hitbox.Location += new Vector2(0, 124 * Projectile.scale).RotatedBy(Projectile.rotation - MathHelper.PiOver2).ToPoint();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (canHit && !Thrown)
        {
            var dist = 300f;

            var offset = new Vector2(dist * Projectile.scale * 1.65f, 0).RotatedBy(Projectile.rotation);
            float _ = 0;

            return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center, Projectile.Center + offset, 120f, ref _);
        }

        if (Thrown && Projectile.velocity.Length() > 2)

        {
            return base.Colliding(projHitbox, targetHitbox);
        }

        return false;
    }

    #endregion

    #region SlashTrail

    private float TrailWidthFunction(float p)
    {
        return 250 * Projectile.scale * _slashScale * Projectile.direction;
    }

    private Color TrailColorFunction(float p)
    {
        return Color.Lerp
        (
            Color.Red with
            {
                A = 120
            },
            Color.DarkCyan with
            {
                A = 50
            },
            p
        );
    }

    public void ResetTrail(bool rotation = false)
    {
        for (var i = 0; i < Projectile.oldPos.Length; i++)
        {
            Projectile.oldPos[i] = Projectile.Center;

            if (rotation)
            {
                Projectile.oldRot[i] = Projectile.rotation;
            }
        }
    }

    public void UpdateTrail()
    {
        var playerPosOffset = Owner.position - Owner.oldPosition;

        if (Projectile.numUpdates == 0)
        {
            playerPosOffset = Vector2.Zero;

            for (var i = Projectile.oldPos.Length - 1; i > 0; i--)
            {
                Projectile.oldPos[i] = Projectile.oldPos[i - 1];
                Projectile.oldRot[i] = Projectile.rotation.AngleLerp(Projectile.oldRot[i - 1], 0.1f);
            }

            {
                Projectile.oldPos[0] = Projectile.Center + Projectile.velocity;
                Projectile.oldRot[0] = Projectile.rotation;
            }
        }
    }

    public void ResetSlash()
    {
        _slashScale = 1f;

        for (var i = _slashPositions.Length - 1; i > 0; i--)
        {
            _slashPositions[i] = new Vector2(SlashDistance * Projectile.scale, 0).RotatedBy(Projectile.rotation);
            _slashRotations[i] = Projectile.rotation + MathHelper.PiOver2 * Projectile.direction;
        }
    }

    public void UpdateSlash()
    {
        for (var i = _slashPositions.Length - 1; i > 0; i--)
        {
            _slashRotations[i] = _slashRotations[i - 1];
            _slashPositions[i] = _slashPositions[i - 1];
        }

        _slashRotations[0] = Projectile.rotation + MathHelper.PiOver2 * Projectile.direction;
        var offset = !Thrown ? SlashDistance : 200;

        _slashPositions[0] = Projectile.Center -
                             Owner.Center +
                             new Vector2(offset * Projectile.scale * _slashScale, 0).RotatedBy(Projectile.rotation + MathHelper.ToRadians(5 * Projectile.direction * stage == 0 ? 1 : -1));
    }

    #endregion

    #region DrawCode

    public override bool PreDraw(ref Color lightColor)
    {
        var tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Melee/DarkestNight/SwordPlaceholder").Value;
        var DrawPos = Projectile.Center - Main.screenPosition;

        Vector2 Origin;

        if (!Thrown)
        {
            Origin = new Vector2(tex.Width / 2, tex.Height);
        }
        else
        {
            Origin = new Vector2(tex.Width / 2, tex.Height / 2);
        }

        var AdjustedScale = new Vector2(1) * 0.2f * Projectile.scale;
        var Rot = Projectile.rotation + MathHelper.PiOver2;

        DrawAltfireStuff(DrawPos);

        if (t > 0.1f && t < 0.98f && stage != 2)
        {
            DrawSlash();
        }

        Main.EntitySpriteDraw(tex, DrawPos, null, Color.White, Rot, Origin, AdjustedScale, SpriteEffects.None);

        DrawBladeOverlay(DrawPos, Rot, AdjustedScale);

        //Utils.DrawBorderString(Main.spriteBatch, swingInterp.ToString(), DrawPos, Color.AntiqueWhite, anchory: 10);
        //Vector2 startPos = Projectile.Center;
        //Vector2 Endpos = Projectile.Center + new Vector2(300 * Projectile.scale * 1.65f, 0).RotatedBy(Projectile.rotation);
        //Utils.DrawLine(Main.spriteBatch, startPos, Endpos, Color.AntiqueWhite);
        return false;
    }

    private void DrawBladeOverlay(Vector2 DrawPos, float Rot, Vector2 AdjustedScale)
    {
        var tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Melee/DarkestNight/Sprite-0007").Value;
        var Origin = tex.Size() * 0.5f;
        var Scale = Vector2.One * Projectile.scale * 2.5f;
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        var trailShader = ShaderManager.GetShader("HeavenlyArsenal.MyFirstShader");
        trailShader.SetTexture(GennedAssets.Textures.Noise.MilkyNoise2, 1);
        trailShader.TrySetParameter("Color", RainbowColorGenerator.GenerateLinearColor().ToVector4());
        trailShader.TrySetParameter("Time", Main.GlobalTimeWrappedHourly * 1.5f);
        trailShader.TrySetParameter("EdgeColor", Color.Crimson);
        trailShader.TrySetParameter("CoreColor", Color.White * 0.75f);
        trailShader.TrySetParameter("FlareStart", 1f);
        trailShader.TrySetParameter("DistortionStrength", 0.075f);
        trailShader.Apply();
        float ThrowOffset = !Thrown ? 100 : 15;
        var Adjusted = DrawPos - new Vector2(0, ThrowOffset).RotatedBy(Rot) * Scale;
        Main.EntitySpriteDraw(tex, Adjusted, null, Color.White, Rot, Origin, Scale, SpriteEffects.None);
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
    }

    private void DrawAltfireStuff(Vector2 DrawPos)
    {
        if (!AltFire)
        {
            return;
        }

        var val = (float)Math.Sin(Main.GlobalTimeWrappedHourly);
        val = MathHelper.ToRadians(val * 12);

        Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, swingDirection - MathHelper.PiOver2 + val);
        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, swingDirection - MathHelper.PiOver2 - val);
        var GlowInterpolant = (float)Math.Sin(Main.GlobalTimeWrappedHourly);

        var Cone = AssetDirectory.Textures.GlowCone.Value;
        var Origin = new Vector2(0, Cone.Height / 2);

        //Main.EntitySpriteDraw(Cone, DrawPos, null, Color.AntiqueWhite, swingDirection, Origin, 1, SpriteEffects.None);
        //Utils.DrawBorderString(Main.spriteBatch, Timer.ToString(), DrawPos, Color.AntiqueWhite, 1);

        // Determine draw values.
        var circleDrawPosition = Owner.GetBackHandPosition
                                     (Player.CompositeArmStretchAmount.Full, swingDirection - MathHelper.PiOver2) -
                                 Main.screenPosition +
                                 new Vector2(20, 0).RotatedBy(swingDirection);

        var circleScale = Vector2.One * 0.1f * AltFireInterpolant * Projectile.Opacity * 1.5f;
        var circleColor = Projectile.GetAlpha(new Color(92, 40, 204)) * 1;

        var magicCircleTexture = GennedAssets.Textures.Projectiles.CosmicLightCircle.Value;
        var gd = Main.instance.GraphicsDevice;

        var ringDrawPosition = circleDrawPosition - swingDirection.ToRotationVector2() * 6f;
        var ringWidth = circleScale.X * magicCircleTexture.Width;
        var ringHeight = circleScale.Y * magicCircleTexture.Height;
        var rotation = Matrix.CreateRotationX(MathHelper.PiOver2 + 0.1f) * Matrix.CreateRotationZ(swingDirection + MathHelper.PiOver2);
        var scale = Matrix.CreateScale(ringWidth * 0.97f, -ringHeight, ringWidth * 0.35f) * rotation;
        var world = Matrix.CreateTranslation(ringDrawPosition.X, ringDrawPosition.Y, 0f);
        var projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -ringWidth, ringWidth);

        gd.RasterizerState = RasterizerState.CullNone;

        var spinRotation = Main.GlobalTimeWrappedHourly * -3.87f;
        var ringShader = ShaderManager.GetShader("NoxusBoss.NamelessMagicCircleRingShader");
        ringShader.TrySetParameter("uWorldViewProjection", scale * world * Main.GameViewMatrix.TransformationMatrix * projection);
        ringShader.TrySetParameter("localTime", spinRotation);

        ringShader.TrySetParameter
        (
            "generalColor",
            circleColor with
            {
                A = 0
            } *
            1
        );

        ringShader.TrySetParameter("glowColor", Color.White * 1);
        ringShader.SetTexture(GennedAssets.Textures.Projectiles.CosmicLightCircleRing, 1, SamplerState.LinearWrap);
        ringShader.Apply();

        gd.SetVertexBuffer(MeshRegistry.CylinderVertices);
        gd.Indices = MeshRegistry.CylinderIndices;
        gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, MeshRegistry.CylinderVertices.VertexCount, 0, MeshRegistry.CylinderIndices.IndexCount / 3);

        gd.SetVertexBuffer(null);
        gd.Indices = null;
        Main.spriteBatch.PrepareForShaders();

        // Apply the shader.
        var magicCircleShader = ShaderManager.GetShader("NoxusBoss.MagicCircleShader");
        CalculatePrimitiveMatrices(Main.screenWidth, Main.screenHeight, out var viewMatrix, out var projectionMatrix);
        magicCircleShader.TrySetParameter("orientationRotation", swingDirection);
        magicCircleShader.TrySetParameter("spinRotation", spinRotation);
        magicCircleShader.TrySetParameter("flip", Projectile.direction == -1f);
        magicCircleShader.TrySetParameter("uWorldViewProjection", viewMatrix * projectionMatrix);
        magicCircleShader.Apply();

        // Draw the circle. If the laser is present, it gains a sharp white glow.
        var magicCircleCenterTexture = GennedAssets.Textures.Projectiles.CosmicLightCircleCenter.Value;

        Main.EntitySpriteDraw
        (
            magicCircleTexture,
            circleDrawPosition,
            null,
            circleColor with
            {
                A = 0
            },
            0f,
            magicCircleTexture.Size() * 0.5f,
            circleScale,
            0
        );

        for (var d = 0f; d < 0.03f; d += 0.01f)
        {
            Main.EntitySpriteDraw
            (
                magicCircleTexture,
                circleDrawPosition,
                null,
                Color.White with
                {
                    A = 0
                } *
                GlowInterpolant *
                1,
                0f,
                magicCircleTexture.Size() * 0.5f,
                circleScale * (d * GlowInterpolant + 1f),
                0
            );
        }

        // Draw the eye on top of the circle.
        magicCircleShader.TrySetParameter("spinRotation", 0f);
        magicCircleShader.Apply();

        Main.EntitySpriteDraw
        (
            magicCircleCenterTexture,
            circleDrawPosition,
            null,
            Color.Lerp(circleColor, Color.White * 1, 0.5f) with
            {
                A = 0
            },
            0f,
            magicCircleCenterTexture.Size() * 0.5f,
            circleScale,
            0
        );
    }

    private Vector2[] _slashPositions;

    private float[] _slashRotations;

    private static VertexStrip _slashStrip;

    public void DrawSlash()
    {
        if (_slashPositions.Length < 1)
        {
            return;
        }

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        // Have a shader prepared, only special thing is that it uses a normalized matrix
        var trailShader = ShaderManager.GetShader("HeavenlyArsenal.DivineSlash");

        trailShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 0); // SamplerState.PointClamp);
        trailShader.TrySetParameter("uTime", Main.GlobalTimeWrappedHourly);
        trailShader.TrySetParameter("uWorldViewProjection", Main.GameViewMatrix.NormalizedTransformationmatrix);
        trailShader.TrySetParameter("BetterPi", float.Pi);
        trailShader.TrySetParameter("StartColor", Color.White);
        trailShader.TrySetParameter("EndColor", Color.Firebrick);
        trailShader.Apply();

        // Rendering primitives involves setting vertices of each triangle to form quads
        // This does it for us
        // Have a list of positions and rotations to create vertices, width function to determine how far vertices are from the center
        // Color function determines each vertex's color, which can be used in the shader
        _slashStrip ??= new VertexStrip();
        _slashStrip.PrepareStrip(_slashPositions, _slashRotations, TrailColorFunction, TrailWidthFunction, Owner.Center - Main.screenPosition, _slashPositions.Length, true);
        _slashStrip.DrawTrail();

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
    }

    #endregion
}