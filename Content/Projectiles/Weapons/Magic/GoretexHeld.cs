using CalamityMod.Items.Weapons.Rogue;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Magic;

public class GoretexHeld : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.timeLeft = 10000;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ownerHitCheck = true;
        Projectile.hide = true;
        Projectile.manualDirectionChange = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 6;
        Projectile.DamageType = DamageClass.Magic;
    }

    public ref float Time => ref Projectile.ai[0];
    public ref float Size => ref Projectile.ai[1];



    public ref float Charge => ref Projectile.ai[2];
    public ref Player Owner => ref Main.player[Projectile.owner];



    public override void AI()
    {
        if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
        {
            Projectile.Kill();
            return;
        }

        bool canKill = false;

        Owner.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
        Owner.SetDummyItemTime(4);

        Owner.heldProj = Projectile.whoAmI;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), Owner.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.Zero), 0.08f) * Owner.HeldItem.shootSpeed;
        Projectile.Center = Owner.MountedCenter + Projectile.velocity.SafeNormalize(Vector2.Zero) * 25 + new Vector2(0, Owner.gfxOffY) + Main.rand.NextVector2Circular(2, 2) * Projectile.ai[2];
        Projectile.rotation = Projectile.velocity.ToRotation();

        


        if (Time % (5 + Owner.itemAnimationMax) == 1)
        {


            int firingInterval = Math.Max(5, (int)(Owner.itemAnimationMax / (10 - Charge / 100)));

            if (Time % firingInterval == 0)
            {

                Owner.CheckMana(15, true);


            }
        }
        if ((Time - 8) % 5 == 1)
        {
            Vector2 piercerVelocity = Projectile.velocity;

            if (Main.myPlayer == Projectile.owner)
            {
                piercerVelocity = (Main.MouseWorld - Projectile.Center).RotatedByRandom(0.2f) * (0.06f / MathHelper.E);
                Projectile.netUpdate = true;
            }
           // Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center + Projectile.velocity * 5f + Main.rand.NextVector2Circular(10, 10), piercerVelocity, ModContent.ProjectileType<CrystalPiercer>(), Owner.HeldItem.damage, 1f, Owner.whoAmI, ai1: Projectile.whoAmI);
        }



        if (Owner.channel == true)
        {
           
        }
        else
        {

        }


        if (!Owner.channel || !Owner.CheckMana(15))
        {
            canKill = true;
        }

        if (!canKill)
        {
            Projectile.timeLeft = 10000;
        }

        if (canKill && Projectile.timeLeft > 30)
        {
            Projectile.timeLeft = 30;
        }

        Time++;

        Projectile.localAI[0] += Projectile.ai[2];


        // guess: controls the rate of book rotation
        Projectile.localAI[2] = MathF.Sqrt(Utils.GetLerpValue(0, 50, Time, true) * Utils.GetLerpValue(10, 30, Projectile.timeLeft, true));
        Size = 500;
        Projectile.spriteDirection = Owner.direction;

        if (Time < 10)
        {

            newRot = Projectile.rotation;
        }

        newRot = newRot.AngleLerp(Projectile.rotation, 0.90f);

        if (Projectile.frameCounter++ > 5)
        {
            if (Projectile.localAI[2] > 0.7f)
            {
                if (++Projectile.frame > 7)
                {
                    Projectile.frame = 3;
                }

                Projectile.frameCounter = 0;
            }
            else
            {
                Projectile.frame = (int)(Projectile.localAI[2] * 3);
            }
        }
        //suck gore i think?
        //after a bit of testing, this is correct.
        foreach (Gore gore in Main.gore.Where(n => n.active))
        {
            Rectangle goreRec = new Rectangle((int)(gore.position.X - gore.scale * 10), (int)(gore.position.Y - gore.scale * 10), (int)(gore.scale * 20), (int)(gore.scale * 20));
            if (goreRec.IntersectsConeFastInaccurate(Projectile.Center, Size, Projectile.rotation, MathHelper.Pi / 8f))
            {
                gore.velocity = Vector2.Lerp(gore.velocity, gore.position.DirectionTo(Projectile.Center).SafeNormalize(Vector2.Zero) * 15, 0.2f);
            }

            if (gore.position.Distance(Projectile.Center) < 70)
            {
                gore.scale *= 0.9f;
            }

            if (gore.position.Distance(Projectile.Center) < 40)
            {
                gore.active = false;
                
            }
        }

        foreach (Dust dust in Main.dust.Where(n => n.active && !n.noGravity))
        {
            Rectangle dustRec = new Rectangle((int)(dust.position.X - dust.scale * 3), (int)(dust.position.Y - dust.scale * 3), (int)(dust.scale * 6), (int)(dust.scale * 6));
            if (dustRec.IntersectsConeFastInaccurate(Projectile.Center, Size, Projectile.rotation, MathHelper.Pi / 8f))
            {
                dust.velocity = Vector2.Lerp(dust.velocity, dust.position.DirectionTo(Projectile.Center).SafeNormalize(Vector2.Zero) * 15, 0.2f);
            }

            if (dust.position.Distance(Projectile.Center) < 50)
            {
                dust.scale *= 0.9f;
            }

            if (dust.position.Distance(Projectile.Center) < 30)
            {
                dust.active = false;
            }
        }

        foreach (Item item in Main.item.Where(n => n.active))
        {
            if (item.Hitbox.IntersectsConeFastInaccurate(Projectile.Center, Size, Projectile.rotation, MathHelper.Pi / 8f))
            {
                //controls how fast the book tracks the cursor
                item.velocity = Vector2.Lerp(item.velocity, item.DirectionTo(Projectile.Center).SafeNormalize(Vector2.Zero) * 15, 02f);
            }
        }

        //Color glowColor = new GradientColor(SlimeUtils.GoozOilColors, 0.2f, 0.2f).ValueAt(Time + 10);

        Vector2 inward = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.5f) * Main.rand.NextFloat(Size * Projectile.ai[2] * 1.5f);
        ShadowEffect(Projectile.Center - Main.screenPosition, inward, new Vector2(40, 40));
       
    }
    
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {

        //Dust.NewDustPerfect(Projectile.Center+new Vector2(targetHitbox.Width + Projectile.ai[2]*(Size*(Charge/10000)),0), DustID.Adamantite, Vector2.Zero, 100,Color.AntiqueWhite,1);

        return targetHitbox.IntersectsConeFastInaccurate(Projectile.Center, Size * Projectile.ai[2], Projectile.rotation, MathHelper.Pi / 7f);

    }

    //public LoopingSound windSoundLoop;


    // public void HandleSound()
    //  {
    //      if (windSoundLoop == null)
    //      {
    //          windSoundLoop = new LoopingSound(AssetDirectory.Sounds.Weapons.GoomoireWindLoop, new ProjectileAudioTracker(Projectile).IsActiveAndInGame);
    //      }

    //     windSoundLoop.PlaySound(() => Projectile.Center, () => Projectile.ai[2] * 0.5f, () => Projectile.ai[2] - 0.9f);
    //  }

    //public static Asset<Texture2D> BeadRopeTexture;
    public static Asset<Texture2D> laserTexture;
    public static Asset<Texture2D> laserTexture1;
    public static Asset<Texture2D> laserTexture2;
    public static Asset<Texture2D> laserTexture3;
    public override void Load()
    {
        // BeadRopeTexture = AssetUtilities.RequestImmediate<Texture2D>(Texture + "Ribbon");
        laserTexture = AssetUtilities.RequestImmediate<Texture2D>(Texture + "_Laser" + 0);
        laserTexture1 = AssetUtilities.RequestImmediate<Texture2D>(Texture + "_Laser" + 1);
        laserTexture2 = AssetUtilities.RequestImmediate<Texture2D>(Texture + "_Laser" + 3);
        laserTexture3 = AssetUtilities.RequestImmediate<Texture2D>(Texture + "_Laser" + 5);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle frame = texture.Frame(1, 8, 0, Projectile.frame);

        SpriteEffects bookEffect = Owner.direction * Owner.gravDir > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

        //DrawRibbon(lightColor);

        Main.EntitySpriteDraw(texture, Projectile.Center + new Vector2(-2, -2 * Owner.direction * Owner.gravDir).RotatedBy(Projectile.rotation) - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() * 0.5f, 1f, bookEffect, 0);

        DrawLaserCone();

        return false;
    }


    
    private float newRot;

    private void ShadowEffect(Vector2 pos, Vector2 Velocity, Vector2 Scale)
    {

        Vector2 Something = new Vector2(Projectile.Center.Y - Projectile.height / 2);
        int fireBrightness = Main.rand.Next(0, 20);
        Color fireColor = new Color(fireBrightness, fireBrightness, fireBrightness);
        Color bigColorColor = fireColor;
        //AntishadowSmokeParticleSystemManager.ParticleSystem.CreateNew(new Vector2(Projectile.Center.X, Projectile.Center.Y + Projectile.height / 2), Main.rand.NextVector2Circular(60f, 60f), Vector2.One * Main.rand.NextFloat(20f, 90f) * 3f, bigColorColor);
        AntishadowFireParticleSystemManager.CreateNew(Projectile.owner, false, pos, Velocity, Scale, bigColorColor);
    }


    public void DrawLaserCone()
    {

        Vector2 sparklePos = Projectile.Center + new Vector2(6, 0).RotatedBy(Projectile.rotation);
        Texture2D sparkle = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Extra/Sparkle").Value;
        Color sparkleColor = new Color(255, 20, 0);//new GradientColor(SlimeUtils.GoozColors, 0.2f, 0.2f).ValueAt(Time + 10);
        sparkleColor.A = 0;

        Vector2 sparkleScaleX = new Vector2(1.5f, 1.33f) * Projectile.ai[2];
        Vector2 sparkleScaleY = new Vector2(1.5f, 1.33f) * Projectile.ai[2];
        Main.EntitySpriteDraw(sparkle, sparklePos - Main.screenPosition, sparkle.Frame(), Color.Black * 0.3f, 0f, sparkle.Size() * 0.5f, sparkleScaleX, 0, 0);
        Main.EntitySpriteDraw(sparkle, sparklePos - Main.screenPosition, sparkle.Frame(), Color.Black * 0.3f, MathHelper.PiOver2, sparkle.Size() * 0.5f, sparkleScaleY, 0, 0);

        Vector2[] positions = new Vector2[500];
        float[] rotations = new float[500];
        for (int i = 0; i < 500; i++)
        {
            rotations[i] = newRot.AngleLerp(Projectile.rotation, MathF.Sqrt(i / 500f)) + MathF.Sin(Time * 0.2f - i / 50f) * 0.1f * (1f - i / 500f) * Projectile.ai[2];
            positions[i] = sparklePos + new Vector2(Size * (i / 500f) * Projectile.ai[2], 0).RotatedBy(rotations[i]);


        }

        VertexStrip strip = new VertexStrip();
        strip.PrepareStripWithProceduralPadding(positions, rotations, StripColor, StripWidth, -Main.screenPosition, true);
        VertexStrip Agony = new VertexStrip();
        Agony.PrepareStripWithProceduralPadding(positions, rotations, StripColor2, StripWidth, -Main.screenPosition, true);


        Effect lightningEffect = AssetDirectory.Effects.GoomoireWindR.Value;
        lightningEffect.Parameters["uTransformMatrix"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
        lightningEffect.Parameters["uTexture0"].SetValue(laserTexture.Value);
        lightningEffect.Parameters["uTexture1"].SetValue(laserTexture1.Value);
        lightningEffect.Parameters["uTexture2"].SetValue(laserTexture2.Value);

        lightningEffect.Parameters["uTexture3"].SetValue(laserTexture3.Value);
        lightningEffect.Parameters["uTime"].SetValue(Projectile.localAI[0] * 0.1f);
        lightningEffect.Parameters["uFreq"].SetValue(2f);
        lightningEffect.Parameters["uMiddleBrightness"].SetValue(1);
        lightningEffect.Parameters["uBackPhaseShift"].SetValue(0.5f);
        lightningEffect.Parameters["uSlant"].SetValue(0f);
        lightningEffect.CurrentTechnique.Passes[0].Apply();

        strip.DrawTrail();
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();



        Effect Silly = AssetDirectory.Effects.GoomoireWindL.Value;
        Silly.Parameters["uTransformMatrix"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
        Silly.Parameters["uTexture0"].SetValue(laserTexture1.Value);
        Silly.Parameters["uTexture1"].SetValue(laserTexture.Value);
        Silly.Parameters["uTexture2"].SetValue(laserTexture2.Value);

        Silly.Parameters["uTexture3"].SetValue(laserTexture3.Value);
        Silly.Parameters["uTime"].SetValue(Projectile.localAI[0] * -0.1f);
        Silly.Parameters["uFreq"].SetValue(2f);
        Silly.Parameters["uMiddleBrightness"].SetValue(1);
        Silly.Parameters["uBackPhaseShift"].SetValue(0.5f);
        Silly.Parameters["uSlant"].SetValue(0f);
        Silly.CurrentTechnique.Passes[0].Apply();

        Agony.DrawTrail();
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();



        Main.EntitySpriteDraw(sparkle, sparklePos - Main.screenPosition, sparkle.Frame(), sparkleColor, 0f, sparkle.Size() * 0.5f, sparkleScaleX, 0, 0);
        Main.EntitySpriteDraw(sparkle, sparklePos - Main.screenPosition, sparkle.Frame(), sparkleColor, MathHelper.PiOver2, sparkle.Size() * 0.5f, sparkleScaleY, 0, 0);

        Main.EntitySpriteDraw(sparkle, sparklePos - Main.screenPosition, sparkle.Frame(), new Color(255, 255, 255, 0), 0f, sparkle.Size() * 0.5f, sparkleScaleX * 0.5f, 0, 0);
        Main.EntitySpriteDraw(sparkle, sparklePos - Main.screenPosition, sparkle.Frame(), new Color(255, 255, 255, 0), MathHelper.PiOver2, sparkle.Size() * 0.5f, sparkleScaleY * 0.5f, 0, 0);
    }

    public Color StripColor(float progress)
    {
        Color color = new Color(0, 0, 255);// GradientColor(SlimeUtils.GoozColors, 0.2f, 0.2f).ValueAt(Time + 10 + progress * 40f) * 0.45f;
        //color.A /= 2;
        return color; //* Projectile.ai[2];
    }

    public Color StripColor2(float progress)
    {
        Color color = new Color(255, 0, 0);// GradientColor(SlimeUtils.GoozColors, 0.2f, 0.2f).ValueAt(Time + 10 + progress * 40f) * 0.45f;
        //color.A /= 2;
        return color; //* Projectile.ai[2];
    }



    public float StripWidth(float progress)
    {

        // controls how bunched up it is, hard to describe but essentially the smaller the number the wider the start point will be
        float start = MathHelper.SmoothStep(MathF.Pow(progress, 0.6f) *

            //its complicated but esentially this controls how twisted the distortion is
            -1f,
            2, progress);
        float grow = (float)Math.Pow(Projectile.ai[2], 3f);
        return start * grow * Size * Math.Clamp(1, 0, 3);
    }
}