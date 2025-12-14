using System.Runtime.CompilerServices;
using HeavenlyArsenal.Common.IK;
using Luminance.Common.Easings;
using NoxusBoss.Assets;
using Terraria.Audio;
using Terraria.DataStructures;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.Items.Armor;

internal class ShintoArmorIKArms : ModPlayer
{
    public int Time;

    public int LimbCount = 2;

    public ShintoArmorLimb[] _limbs;

    public bool ArmsOut;

    public float OpenInterpolant;

    public bool Active;

    private float t;

    private PiecewiseCurve TearOpenMotion;

    public override void OnEnterWorld()
    {
        if (!Player.isDisplayDollOrInanimate)
        {
            CreateLimbs();
        }
    }

    public override void ArmorSetBonusActivated()
    {
        if (Active)
        {
            ArmsOut = ArmsOut == false ? true : false;

            if (ArmsOut)
            {
                CreateLimbs();
            }

            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ErasureRiftOpen, Player.Center);
        }
    }

    public override void PostUpdateMiscEffects()
    {
        //CreateLimbs();

        if (Active)
        {
            TearOpenMotion = new PiecewiseCurve();
            TearOpenMotion.Add(EasingCurves.Quintic, EasingType.Out, 1f, 0.5f);
            TearOpenMotion.Add(EasingCurves.Elastic, EasingType.InOut, 0.7f, 0.56f);
            TearOpenMotion.Add(EasingCurves.Quadratic, EasingType.Out, 1, 1);
            OpenInterpolant = TearOpenMotion.Evaluate(t);

            //Main.NewText(t);
            if (ArmsOut)
            {
                t = Math.Clamp(t + 0.005f, 0, 1);

                //float interp = MathF.Abs(MathF.Sin(Time / 10.1f) + 1);
                for (var i = 0; i < LimbCount; i++)
                {
                    var thing = 1.5f * InverseLerp(0, 30, Player.velocity.Y);

                    _limbs[i].Opacity = float.Lerp(_limbs[i].Opacity, 1, 0.1f);
                    var offset = (i % 2 == 0 ? 20 : -20) * Math.Max(thing, 1 - OpenInterpolant);
                    _limbs[i].TargetPosition = Player.Center + new Vector2(offset, -50 * Math.Clamp(t + 0.3f, 0, 1.05f)) + Player.velocity;

                    UpdateLimbState(ref _limbs[i], Player.Center + new Vector2(i % 2 == 0 ? -1 : 1, 0) + Player.velocity, 0.5f, 2);
                }
            }
            else
            {
                t = 0;
                OpenInterpolant = float.Lerp(OpenInterpolant, 0, 0.31f);

                for (var i = 0; i < LimbCount; i++)
                {
                    _limbs[i].Opacity = float.Lerp(_limbs[i].Opacity, 0, 0.2f);
                    _limbs[i].TargetPosition = Player.Center;
                    UpdateLimbState(ref _limbs[i], Player.Center + new Vector2(0, -Player.height / 1.4f), 0.3f, 2);
                }
            }

            Time++;
        }
        else
        {
            t = 0;
            OpenInterpolant = float.Lerp(OpenInterpolant, 0, 0.31f);

            for (var i = 0; i < LimbCount; i++)
            {
                _limbs[i].Opacity = float.Lerp(_limbs[i].Opacity, 0, 0.2f);
                _limbs[i].TargetPosition = Player.Center;
                UpdateLimbState(ref _limbs[i], Player.Center + new Vector2(0, -Player.height / 1.4f), 0.3f, 2);
            }
        }
    }

    public override void ResetEffects()
    {
        if (!Active)
        {
            ArmsOut = false;
        }

        Active = false;
    }

    #region LimbStruct

    internal record struct ShintoArmorLimb(IKSkeleton skeleton, bool anchored = false, bool hasTarget = false)
    {
        public IKSkeleton Skeleton = skeleton;

        public Vector2 TargetPosition = Vector2.Zero;

        public Vector2 EndPosition = Vector2.Zero;

        public bool IsAnchored = anchored;

        public float Opacity = 1;

        public void DrawArm(ref ShintoArmorLimb limb, SpriteEffects effects, int index, PlayerDrawSet drawInfo)
        {
            if (limb.Opacity <= 0.01f)
            {
                return;
            }

            var armTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/ShintoArmorArm").Value;
            var glowTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/ShintoArmorArm_glow").Value;
            var defaultForearmFrame = new Rectangle(0, 0, 84, 32);
            var anchoredForearmFrame = new Rectangle(0, 32, 84, 32);

            var currentFrame = limb.IsAnchored ? anchoredForearmFrame : defaultForearmFrame;

            Vector2 StartPos;

            StartPos = limb.Skeleton.Position(0) - Main.screenPosition;
            var frame1 = glowTexture.Frame(3, 1, 1);

            var a = new DrawData
            (
                glowTexture,
                StartPos,
                frame1,
                Color.Red with
                {
                    A = 0
                } *
                limb.Opacity,
                (limb.Skeleton.Position(0) - limb.Skeleton.Position(1)).ToRotation(),
                glowTexture.Frame(3, 1, 1).Size() / 2 + new Vector2(30, 0),
                0.9f,
                effects
            );

            drawInfo.DrawDataCache.Add(a);

            StartPos = limb.Skeleton.Position(1) - Main.screenPosition;
            var frame2 = glowTexture.Frame(3);

            var b = new DrawData
            (
                glowTexture,
                StartPos,
                frame2,
                Color.Red with
                {
                    A = 0
                } *
                limb.Opacity,
                (limb.Skeleton.Position(1) - limb.Skeleton.Position(2)).ToRotation(),
                glowTexture.Frame(3).Size() / 2 + new Vector2(30, 0),
                0.5f,
                effects
            );

            drawInfo.DrawDataCache.Add(b);

            StartPos = limb.Skeleton.Position(0) - Main.screenPosition;

            var c = new DrawData
            (
                armTexture,
                StartPos,
                new Rectangle(94, 0, 48, 24),
                Color.White * limb.Opacity,
                (limb.Skeleton.Position(0) - limb.Skeleton.Position(1)).ToRotation(),
                new Vector2(134 - 94, 12),
                1,
                effects
            );

            drawInfo.DrawDataCache.Add(c);

            StartPos = limb.Skeleton.Position(1) - Main.screenPosition;

            var e = new DrawData
            (
                armTexture,
                StartPos,
                currentFrame,
                Color.White * limb.Opacity,
                (limb.Skeleton.Position(1) - limb.Skeleton.Position(2)).ToRotation(),
                new Vector2(72, 14),
                0.5f,
                effects
            );

            drawInfo.DrawDataCache.Add(e);

            /*
                Main.EntitySpriteDraw(glowTexture, StartPos, glowTexture.Frame(3, 1, 1, 0), Color.Red with { A = 0 } * limb.Opacity,
                (limb.Skeleton.Position(0) - limb.Skeleton.Position(1)).ToRotation(),
                glowTexture.Frame(3, 1, 1, 0).Size() / 2 + new Vector2(30,0),
                0.9f, effects);

            Main.EntitySpriteDraw(glowTexture, limb.Skeleton.Position(1) - Main.screenPosition, glowTexture.Frame(3, 1, 0, 0), Color.Red with { A = 0 } * limb.Opacity,
                (limb.Skeleton.Position(1) - limb.Skeleton.Position(2)).ToRotation(),  glowTexture.Frame(3,1,0,0).Size()/2 + new Vector2(30, index % 2 == 0? 0:-3), 0.49f, effects);

            Main.spriteBatch.Draw(
                armTexture,
                StartPos,
                new Rectangle(94, 0, 48, 24),
                Color.AntiqueWhite * limb.Opacity,
                (limb.Skeleton.Position(0) - limb.Skeleton.Position(1)).ToRotation(),
                new Vector2(134 - 94, 12),
                new Vector2(1, 1),
                effects,
                0f
            );
            StartPos = limb.Skeleton.Position(1) - Main.screenPosition;



            Main.spriteBatch.Draw(armTexture, StartPos, currentFrame, Color.AntiqueWhite * limb.Opacity,
              (limb.Skeleton.Position(1) - limb.Skeleton.Position(2)).ToRotation(),
              new Vector2(72, 14), new Vector2(1) * 0.5f, effects, 0f);
            for (int i = 0; i < limb.skeleton.PositionCount - 1; i++)
            {

                //Utils.DrawLine(Main.spriteBatch, n(2))limb.Skeleton.Position(i), limb.Skeleton.Position(i + 1), Color.AntiqueWhite * limb.Opacity, Color.AntiqueWhite*limb.Opacity, 5);
            }
            Texture2D debug = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

            Main.EntitySpriteDraw(debug, TargetPosition - Main.screenPosition, null, Color.AntiqueWhite, 0, debug.Size() / 2, 1, 0);
            Main.spriteBatch.ResetToDefault();
            */
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateLimbState(ref ShintoArmorLimb ShintoArmorLimb, Vector2 basePos, float lerpSpeed, float anchorThreshold)
    {
        ShintoArmorLimb.EndPosition = Vector2.Lerp(ShintoArmorLimb.EndPosition, ShintoArmorLimb.TargetPosition, lerpSpeed);
        ShintoArmorLimb.Skeleton.Update(basePos, ShintoArmorLimb.EndPosition);
        ShintoArmorLimb.IsAnchored = Vector2.Distance(ShintoArmorLimb.EndPosition, ShintoArmorLimb.TargetPosition) < anchorThreshold;
    }

    private void CreateLimbs()
    {
        if (_limbs == null)
        {
            _limbs = new ShintoArmorLimb[2];
        }

        for (var i = 0; i < LimbCount; i++)
        {
            _limbs[i] = new ShintoArmorLimb
            (
                new IKSkeleton
                (
                    (40f, new IKSkeleton.Constraints()),
                    (35f, new IKSkeleton.Constraints
                    {
                        MinAngle = i % 2 == 0 ? -MathHelper.Pi : 0,
                        MaxAngle = i % 2 == 0 ? 0 : MathHelper.Pi
                    })
                )
            );

            _limbs[i].Opacity = 0;
            _limbs[i].EndPosition = Player.Center;
            _limbs[i].TargetPosition = Player.Center;
        }
    }

    #endregion
}

public class ShintoArmorIKDrawLayer : PlayerDrawLayer
{
    public override bool IsHeadLayer => false;

    public override Position GetDefaultPosition()
    {
        return new BeforeParent(PlayerDrawLayers.Backpacks);
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return true;
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        var owner = drawInfo.drawPlayer;

        if (owner.GetModPlayer<ShintoArmorIKArms>()._limbs == null || owner.GetModPlayer<ShintoArmorIKArms>()._limbs.Length <= 0)
        {
            return;
        }

        for (var i = 0; i < owner.GetModPlayer<ShintoArmorIKArms>().LimbCount; i++)
        {
            var limb = owner.GetModPlayer<ShintoArmorIKArms>()._limbs[i];
            SpriteEffects effect;

            if (i % 2 == 0)
            {
                effect = SpriteEffects.FlipVertically;
            }
            else
            {
                effect = SpriteEffects.None;
            }

            limb.DrawArm(ref limb, effect, i, drawInfo);
            //owner.GetModPlayer<HidePlayer>().ShouldHide = true;
        }
        //Texture2D debug = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
        //Main.EntitySpriteDraw(debug, drawInfo.HeadPosition() - new Vector2(0, 40), null, Color.Red, MathHelper.ToRadians(45), debug.Size() / 2, 4,0);
    }
}