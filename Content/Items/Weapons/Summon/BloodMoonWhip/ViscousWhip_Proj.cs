using System.Collections.Generic;
using NoxusBoss.Assets;
using Terraria.Audio;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip;

internal class ViscousWhip_Proj : CleanBaseWhip
{
    public Vector2 lastTop = Vector2.Zero;

    private ModularWhipController _controller;

    public override Color StringColor => Color.Crimson;

    public ref Player Owner => ref Main.player[Projectile.owner];

    public override SoundStyle? WhipSound => GennedAssets.Sounds.Common.Glitch with
    {
        Volume = 0.5f,
        PitchVariance = 0.2f
    };

    private float Timer
    {
        get => Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override void OnSpawn(IEntitySource source)
    {
        base.OnSpawn(source);

        SetController();
    }

    public void SetController()
    {
        //  Vector2 arm = Main.GetPlayerArmPosition(Projectile);
        //   Vector2 c1 = arm + new Vector2(50 * Projectile.spriteDirection, -80f);
        // Vector2 c2 = arm + new Vector2(150 * Projectile.spriteDirection, 100f);
        //  Vector2 end = arm + new Vector2(200 * Projectile.spriteDirection, 0f);

        //var curve = new BezierCurve(new Vector2(0, 0), new Vector2(60, 80), new Vector2(160, 90), new Vector2(220, 0));

        var thing = 1 - Math.Abs(2 * FlyProgress - 1);
        var item = Owner.HeldItem.ModItem as ViscousWhip_Item;

        if (item.SwingStage == 0)
        {
            _controller = new ModularWhipController(new VanillaWhipMotion());
        }
        else if (item.SwingStage == 1)
        {
            _controller = new ModularWhipController(new BraidedMotion());
        }
        else
        {
            _controller = new ModularWhipController(new VanillaWhipMotion());
        }
        //_controller.AddModifier(new TwirlModifier(0, Segments/2, 0.15f * -Owner.direction));

        _controller.AddModifier(new SmoothSineModifier(0, Segments, 1f, 10f, 1f));

        //_controller.AddModifier(new TwirlModifier(4, Segments, -0.12f * Projectile.direction * thing, true));
        //_controller.AddModifier(new TwirlModifier(8, 16, -0.12f* thing * Projectile.direction, false));
        //_controller.AddModifier(new TwirlModifier(17,  Segments, -0.15f * Projectile.direction));
    }

    protected override void ModifyWhipSettings(ref float outFlyTime, ref int outSegments, ref float outRangeMult)
    {
        var item = Owner.HeldItem.ModItem as ViscousWhip_Item;

        if (item.SwingStage == 1)
        {
            outSegments = 70;
        }
        else
        {
            outSegments = 120;
        }

        //if (item.SwingStage == 1)
        //     outRangeMult = 0.8f;
    }

    public override void ModifyControlPoints(List<Vector2> controlPoints)
    {
        GetWhipSettingsBetter(Projectile, out var timeToFlyOut, out var segments, out var rangeMultiplier);
        rangeMultiplier *= Main.player[Projectile.owner].whipRangeMultiplier;

        var progress = FlyProgress;
        progress = MathHelper.Clamp(progress, 0f, 1f);
        _controller.Clear();

        SetController();
        _controller.Apply(controlPoints, Projectile, segments, rangeMultiplier, progress);
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        Projectile.MaxUpdates = 10;
        // Projectile.localNPCHitCooldown = 20;
    }

    protected override void WhipAI()
    {
        var owner = Main.player[Projectile.owner];

        float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;

        if (swingTime <= 0f)
        {
            swingTime = 20f * Projectile.MaxUpdates;
        }

        var swingProgress = Timer / swingTime;

        List<Vector2> points = new();
        ModifyControlPoints(points);

        if (points.Count == 0)
        {
            return;
        }

        lastTop = points[^1];
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        // rectangle centered on lastTop with 72x72 area
        var rect = new Rectangle((int)lastTop.X - 36, (int)lastTop.Y - 36, 42, 42);

        if (rect.Intersects(target.Hitbox))
        {
            modifiers.SourceDamage *= 1.25f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(ModContent.BuffType<BloodwhipBuff>(), 240);

        Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;

        Projectile.damage = (int)(Projectile.damage * 0.9f);

        SoundEngine.PlaySound(SoundID.Item14, target.Center);
    }
    /*
    private void DrawLine(List<Vector2> list)
    {
        Texture2D texture = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
        Rectangle frame = texture.Frame();
        Vector2 origin = new Vector2(0f, 0.5f);

        Vector2 pos = list[0];
        for (int i = 0; i < list.Count - 1; i++)
        {
            Vector2 element = list[i];
            Vector2 diff = list[i + 1] - element;

            float rotation = diff.ToRotation();
            Color color = Color.Crimson;
            Vector2 scale = new Vector2(diff.Length() + 2f, 2f);
            if (i == list.Count - 2)
            {
                scale.X -= 5f;
            }

            Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);
            Utils.DrawBorderString(Main.spriteBatch, i.ToString(), pos - Main.screenPosition, Color.AntiqueWhite, 0.35f);
            pos += diff;
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        List<Vector2> list = new();
        ModifyControlPoints(list);
        if (list.Count == 0) return false;


        DrawLine(list);

        SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Vector2 pos = list[0];

        Texture2D thing = GennedAssets.Textures.GreyscaleTextures.HollowCircleSoftEdge;
        for (int i = 0; i < list.Count - 1; i++)
        {
            // These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
            // You can change them if they don't!
            Rectangle frame = new Rectangle(0, 0, 10, 26); // The size of the Handle (measured in pixels)
            Vector2 origin = new Vector2(5, 8); // Offset for where the player's hand will start measured from the top left of the image.
            float scale = 1;

            // These statements determine what part of the spritesheet to draw for the current segment.
            // They can also be changed to suit your sprite.
            if (i == list.Count - 2)
            {
                // This is the head of the whip. You need to measure the sprite to figure out these values.
                frame.Y = 74; // Distance from the top of the sprite to the start of the frame.
                frame.Height = 18; // Height of the frame.


                Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                float t = Time / timeToFlyOut;
                Vector2 a = list[list.Count - 2] - list[2];
                float rot = a.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.

                scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
                Vector2 thingOffset = thing.Size() * 0.5f + new Vector2(-50, 100);
                //Main.spriteBatch.End();
                //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

                //Main.EntitySpriteDraw(thing, pos - Main.screenPosition, null, Color.Crimson with { A = 0 }, rot, thingOffset, scale * 0.05f, SpriteEffects.None);
                //Main.spriteBatch.End();
                //Main.spriteBatch.Begin();
            }
            else if (i > 10)
            {
                // Third segment
                frame.Y = 58;
                frame.Height = 16;
            }
            else if (i > 5)
            {
                // Second Segment
                frame.Y = 42;
                frame.Height = 16;
            }
            else if (i > 0)
            {
                // First Segment
                frame.Y = 26;
                frame.Height = 16;
            }

            Vector2 element = list[i];
            Vector2 diff = list[i + 1] - element;

            float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
            Color color = Lighting.GetColor(element.ToTileCoordinates());

            //Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

            pos += diff;
        }

        return false;
    }*/
}