using System.Collections.Generic;
using NoxusBoss.Assets;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip;

internal class ViscousWhip_Proj : CleanBaseWhip
{
    public Vector2 lastTop = Vector2.Zero;

    private ModularWhipController _controller;

    public override Color StringColor => Color.Crimson;

    public ref Player Owner => ref Main.player[Projectile.owner];


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

        _controller.AddModifier(new SmoothSineModifier(0, Segments, 1f,1f,1f));

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
    private BasicEffect whipEffect;


    private void DrawWhipPrimitive(List<Vector2> points, float baseWidth = 6f)
    {
        if (points.Count < 2)
            return;

        GraphicsDevice gd = Main.graphics.GraphicsDevice;

        whipEffect ??= new BasicEffect(gd)
        {
            TextureEnabled = true,
            VertexColorEnabled = true,
            LightingEnabled = false
        };

        Texture2D texture = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

        whipEffect.Texture = texture;
        whipEffect.View = Main.GameViewMatrix.TransformationMatrix;
        whipEffect.Projection = Matrix.CreateOrthographicOffCenter(
            0, Main.screenWidth,
            Main.screenHeight, 0,
            -1f, 1f
        );
        whipEffect.World = Matrix.Identity;

        List<VertexPositionColorTexture> verts = new();

        float totalLength = 0f;
        for (int i = 0; i < points.Count - 1; i++)
            totalLength += Vector2.Distance(points[i], points[i + 1]);

        float accumulated = 0f;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 p = points[i];
            Vector2 dir;

            if (i == 0)
                dir = points[1] - p;
            else if (i == points.Count - 1)
                dir = p - points[i - 1];
            else
                dir = points[i + 1] - points[i - 1];

            if (dir.LengthSquared() < 0.001f)
                continue;

            dir.Normalize();

            Vector2 normal = dir.RotatedBy(MathHelper.PiOver2);

            float t = accumulated / totalLength;
            float width = baseWidth * MathHelper.Lerp(1.2f, 0.4f, t); // taper

            Color color = Color.Crimson.MultiplyRGB(Lighting.GetColor(p.ToTileCoordinates()));
            float u = t;

            Vector2 screen = p - Main.screenPosition;

            verts.Add(new VertexPositionColorTexture(
                new Vector3(screen + normal * width, 0f),
                color,
                new Vector2(u, 0f)
            ));

            verts.Add(new VertexPositionColorTexture(
                new Vector3(screen - normal * width, 0f),
                color,
                new Vector2(u, 1f)
            ));

            if (i < points.Count - 1)
                accumulated += Vector2.Distance(points[i], points[i + 1]);
        }

        if (verts.Count < 4)
            return;

        gd.RasterizerState = RasterizerState.CullNone;
        gd.BlendState = BlendState.AlphaBlend;
        gd.DepthStencilState = DepthStencilState.None;
        gd.SamplerStates[0] = SamplerState.LinearClamp;

        foreach (EffectPass pass in whipEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            gd.DrawUserPrimitives(
                PrimitiveType.TriangleStrip,
                verts.ToArray(),
                0,
                verts.Count - 2
            );
        }
    }

    private List<Vector2> ResamplePolyline(
    List<Vector2> points,
    float spacing
)
    {
        List<Vector2> result = new();
        if (points.Count < 2)
            return result;

        result.Add(points[0]);

        Vector2 prev = points[0];
        float carry = 0f;

        for (int i = 1; i < points.Count; i++)
        {
            Vector2 curr = points[i];
            Vector2 delta = curr - prev;
            float length = delta.Length();

            if (length <= 0.0001f)
                continue;

            Vector2 dir = delta / length;

            float dist = spacing - carry;
            while (dist <= length)
            {
                Vector2 sample = prev + dir * dist;
                result.Add(sample);
                dist += spacing;
            }

            carry = length - (dist - spacing);
            prev = curr;
        }

        // Ensure the tip is included
        if (result[^1] != points[^1])
            result.Add(points[^1]);

        return result;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        List<Vector2> list = new();
        ModifyControlPoints(list);
        if (list.Count == 0) return false;

        float lodSpacing = 1f;
        List<Vector2> dense = ResamplePolyline(list, lodSpacing);

        DrawWhipPrimitive(dense, baseWidth: 4f);



        return false;
    }
}