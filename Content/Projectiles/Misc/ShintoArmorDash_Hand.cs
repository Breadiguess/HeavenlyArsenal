using System.Collections.Generic;
using System.Linq;
using HeavenlyArsenal.Common.utils;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;

namespace HeavenlyArsenal.Content.Projectiles.Misc;

public class ShintoArmorDash_Hand : ModProjectile
{
    //TODO: sync
    public float TimeInner;

    public Rope tentacle;

    public ref float Time => ref Projectile.ai[0];

    public ref float Attack => ref Projectile.ai[1];

    public ref float Index => ref Projectile.ai[2];

    public ref Player Player => ref Main.player[Projectile.owner];

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.MinionTargettingFeature[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.aiStyle = -1;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.manualDirectionChange = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
    }

    public override void AI()
    {
        if (Player.dead || !Player.active) //|| !Player.GetModPlayer<ShintoArmorPlayer>().IsDashing)
        {
            Projectile.Kill();

            return;
        }

        var count = Main.projectile.Count(n => n.active && n.type == Type && n.owner == Player.whoAmI && n.whoAmI != Projectile.whoAmI);
        Index = 0;

        foreach (var proj in Main.projectile.Where(n => n.active && n.owner == Player.whoAmI && n.type == Type))
        {
            if (proj.whoAmI > Projectile.whoAmI)
            {
                Index++;
            }

            if (proj.whoAmI == Projectile.whoAmI)
            {
                continue;
            }

            if (Projectile.Distance(proj.Center) < 40)
            {
                Projectile.velocity -= Projectile.DirectionFrom(proj.Center).SafeNormalize(Vector2.Zero) * 13;
                proj.velocity -= proj.DirectionFrom(Projectile.Center).SafeNormalize(Vector2.Zero) * 13;
            }
        }

        if (Time == 0 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Attack = (int)(Index % 2);
            Projectile.netUpdate = true;
        }

        Projectile.timeLeft = 20;
        Projectile.rotation = Projectile.AngleFrom(Player.MountedCenter) - MathHelper.PiOver2;
        var target = Projectile.FindTargetWithinRange(320);

        var trackSpeed = 0.6f;
        var sideOffset = Index == 0 ? -110f : 110f;

        var jitter = new Vector2
                     (
                         MathF.Sin(Time * 0.05f + Index * 4.5f),
                         MathF.Cos(Time * 0.05f + Index * 1.5f)
                     ) *
                     5f;

        var homePos = Player.MountedCenter + new Vector2(sideOffset, 0f) - Player.velocity * 10f + jitter;

        if (target != null)
        {
            Projectile.rotation = Player.AngleTo(target.Center) - MathHelper.PiOver2;

            switch (Attack)
            {
                case 0:

                    trackSpeed = 1f;

                    if (Projectile.Distance(target.Center) < 300)
                    {
                        TimeInner++;
                    }

                    homePos = target.Center + new Vector2(MathHelper.SmoothStep(-200, 200, MathF.Sqrt(TimeInner / 25f)) * Projectile.direction, 0).RotatedBy(Projectile.rotation);

                    if (TimeInner > 25 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        TimeInner = 0;

                        if (Main.rand.NextBool(2))
                        {
                            Attack++;
                        }

                        Projectile.direction *= -1;

                        Projectile.netUpdate = true;
                    }

                    break;

                case 1:

                    TimeInner++;

                    homePos = target.Center;

                    if (Projectile.Distance(homePos) < 30)
                    {
                        //Projectile.owner;
                    }

                    if ((TimeInner > 120 || Projectile.Distance(target.Center) > 300) && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        TimeInner = 0;

                        Attack++;

                        Projectile.netUpdate = true;
                    }

                    break;

                default:

                    Attack = 0;

                    break;
            }
        }

        if (target == null)
        {
            Projectile.Center += Player.velocity * 0.5f;
            TimeInner = 0;
        }

        Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(homePos) * Projectile.Distance(homePos) * 0.1f, trackSpeed);

        if (Projectile.Distance(homePos) < 3f)
        {
            Projectile.Center = homePos;
        }

        Time++;

        if (tentacle == null)
        {
            tentacle = new Rope(Projectile.Center, Player.MountedCenter, 30, 10f, Vector2.Zero);
            //tentacle.segments[0].pinned = false;
            //tentacle.segments[^1].pinned = false;
        }

        for (var i = 0; i < tentacle.segments.Length; i++)
        {
            if (Main.rand.NextBool(100))
            {
                var blood = Dust.NewDustPerfect(tentacle.segments[i].position, DustID.CrimtaneWeapons, new Vector2(0, -3f), 10, Color.Crimson);
                blood.noGravity = true;
            }
        }

        tentacle.segments[0].position = Projectile.Center;
        tentacle.segments[^1].position = Player.MountedCenter;
        tentacle.gravity = -Vector2.UnitX * Player.direction * 0.05f - Vector2.UnitY * 0.01f;
        tentacle.damping = Utils.GetLerpValue(20, 0, Player.velocity.Length(), true) * 0.05f;
        tentacle.Update();

        if (Projectile.Distance(Player.MountedCenter) > 300)
        {
            Projectile.Center = Player.MountedCenter + new Vector2(300, 0).RotatedBy(Projectile.AngleFrom(Player.MountedCenter));
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Projectiles/Misc/ShintoArmorDash_Hand").Value;

        var light = Lighting.GetColor(Player.Center.ToTileCoordinates());
        Main.spriteBatch.PrepareForShaders();
        //new Texture Placeholder = GennedAssets.Textures.Extra.Code;
        var postProcessingShader = ShaderManager.GetShader("HeavenlyArsenal.FusionRifleClothPostProcessingShader");
        postProcessingShader.TrySetParameter("textureSize", texture.Size() * 3);
        postProcessingShader.TrySetParameter("edgeColor", new Color(228, 37, 40).ToVector4());
        postProcessingShader.SetTexture(GennedAssets.Textures.FirstPhaseForm.RiftInnerTexture, 0, SamplerState.LinearWrap);
        postProcessingShader.Apply();

        if (tentacle != null)
        {
            var points = new List<Vector2>();
            points.AddRange(tentacle.GetPoints());
            points.Add(Player.MountedCenter);

            for (var i = points.Count - 1; i > 0; i--)
            {
                var tentacleFrame = texture.Frame(2, 10, 0, 7 - (int)((float)i / points.Count * 6));
                var tentacleGlowFrame = texture.Frame(2, 10, 1, 7 - (int)((float)i / points.Count * 6));

                var rot = points[i].AngleTo(points[i - 1]) - MathHelper.PiOver2;
                var stretch = new Vector2((1.1f - (float)i / points.Count * 0.6f) * Projectile.scale, i > points.Count - 2 ? points[i].Distance(points[i - 1]) / (tentacleFrame.Height - 2f) : 1.1f);

                if (i == 1)
                {
                    tentacleFrame = texture.Frame(2, 5, 0, 4);
                    tentacleGlowFrame = texture.Frame(2, 5, 1, 4);
                }

                if (i == points.Count - 1)
                {
                    tentacleFrame = texture.Frame(2, 5);
                    tentacleGlowFrame = texture.Frame(2, 5, 1);
                    stretch = new Vector2(Projectile.scale * 0.6f, points[i].Distance(points[i - 1]) / (tentacleFrame.Height - 2f) * 1.2f);
                }

                Main.EntitySpriteDraw
                (
                    texture,
                    points[i] - Main.screenPosition,
                    tentacleFrame,
                    Color.Lerp(light, Color.White, 1f - (float)i / points.Count),
                    rot,
                    tentacleFrame.Size() * new Vector2(0.5f, 0f),
                    stretch,
                    0
                );

                var glowColor = Color.Crimson;

                //new GradientColor(SlimeUtils.GoozOilColors, 0.5f, 0.5f).ValueAt(Main.GlobalTimeWrappedHourly * 150 + i * 10) * 0.5f;
                glowColor.A /= 2;
                //Main.EntitySpriteDraw(texture, points[i] - Main.screenPosition, tentacleGlowFrame, glowColor.MultiplyRGBA(Color.Lerp(light, Color.White, 1f - (float)i / points.Count)) * 1.5f, rot, tentacleFrame.Size() * new Vector2(0.5f, 0f), stretch, 0, 0);
            }

            //Utils.DrawBorderString(Main.spriteBatch, Index.ToString(), Projectile.Center - Vector2.UnitY * 50 - Main.screenPosition, Color.White);
        }

        Main.spriteBatch.ResetToDefault();

        return false;
    }
}