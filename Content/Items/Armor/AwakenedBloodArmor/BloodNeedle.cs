using CalamityMod;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Items.Armor.NewFolder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles.Metaballs;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor
{
    class BloodNeedle : ModProjectile
    {
        private Vector2 hitPosition;
        public override string Texture => "HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/BloodNeedle_Head";
        public float TimeInner;
        public ref float Time => ref Projectile.ai[0];
        public ref float Attack => ref Projectile.ai[1];
        public ref float Index => ref Projectile.ai[2];
        private bool canDamage;
        public ref Player Player => ref Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.damage = 500;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.width = Projectile.height = 64;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            // Despawn check
            if (Player.dead || !Player.active || Player.GetModPlayer<BloodArmorPlayer>().CurrentForm != BloodArmorForm.Offense)
            {
                Projectile.Kill();
                return;
            }

            if (Player.GetModPlayer<BloodArmorPlayer>().BloodArmorEquipped != true)
            {
                Projectile.Kill();
                return;
            }
            // Determine index among siblings
            Index = 0;
            foreach (Projectile proj in Main.projectile.Where(n => n.active && n.owner == Player.whoAmI && n.type == Type))
            {
                if (proj.whoAmI > Projectile.whoAmI) Index++;
            }

            // Keep alive
            Projectile.timeLeft = 20;

            // Compute home position
            float sideOffset = (Index == 0 ? -90f : 100f);
            Vector2 homePos = Player.MountedCenter + new Vector2(sideOffset, 0f) - Player.velocity * 10f;

            // Target acquisition
            NPC target = Projectile.FindTargetWithinRange(320);

            const float trackSpeed = 1f;
            const float stabSpeed = 20f;
            const float windUpDuration = 5f;
            const float retractSpeed = 0.5f; // faster retract

            if (target != null)
            {
                if (Attack == 0f)
                {
                    // Begin wind-up
                    Attack = 1f;
                    TimeInner = 0f;
                    canDamage = true;
                    Projectile.rotation = Projectile.velocity.ToRotation()+MathHelper.PiOver2;
                }

                if (Attack == 1f)
                {
                    Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
                    
                    Vector2 direction = Projectile.SafeDirectionTo(target.Center);

                    if (TimeInner < windUpDuration)
                    {
                        // wind-up (no movement)
                        Projectile.velocity = Vector2.Zero;
                    }
                    else
                    {
                        // stab through target
                        Projectile.velocity = direction * stabSpeed;
                        if (TimeInner > 20)
                        {
                            Attack = 2f;
                        }
                    }
                    TimeInner++;
                }
                else if (Attack == 2f)
                {
                    // retract
                    Vector2 toHome = homePos - Projectile.Center;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toHome.SafeNormalize(Vector2.Zero) * toHome.Length() * retractSpeed, trackSpeed);
                    Projectile.rotation = MathHelper.ToRadians(-Player.direction * 45);
                    if (Vector2.Distance(Projectile.Center, homePos) < 5f)
                    {
                        // reset
                        Attack = 0f;
                        TimeInner = 0f;
                        Projectile.Center = homePos;
                    }
                }
            }
            else
            {
                // Idle follow
                Attack = 0f;
                TimeInner = 0f;
                Projectile.Center += Player.velocity * 0.5f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, (homePos - Projectile.Center) * 0.1f, trackSpeed);

                Projectile.rotation = MathHelper.ToRadians(-Player.direction * 45);
            }

            // Tentacle rope logic
            if (tentacle == null)
            {
                tentacle = new Rope(Projectile.Center, Player.MountedCenter, 20, 10f, Vector2.Zero);
            }
            for (int i = 1; i < tentacle.segments.Length - 1; i++)
            {
                if (Main.rand.NextBool(100))
                {
                    Dust blood = Dust.NewDustPerfect(tentacle.segments[i].position, DustID.CrimtaneWeapons, new Vector2(0, -3f), 10, Color.Crimson, 1);
                    blood.noGravity = true;
                    blood.rotation = Main.rand.NextFloat(-89, 89);
                }
                if (i <= tentacle.segments.Length / 2 && i > 3)
                {
                    tentacle.segments[i].position += new Vector2(13 * -Player.direction, -4 + Index + Main.rand.NextFloat(-1, 1));
                }
            }

            Vector2 jitter = new Vector2(
                MathF.Sin(Time * 0.05f + Index * 4.5f),
                MathF.Cos(Time * 0.05f + Index * 1.5f)
            ) * 5f;

            tentacle.segments[0].position = Projectile.Center;
            tentacle.segments[^1].position = Player.MountedCenter + jitter;
            Projectile.Center += jitter / 10;
            tentacle.gravity = -Vector2.UnitX * Player.direction * 0.05f - Vector2.UnitY * 0.01f;
            tentacle.damping = Utils.GetLerpValue(20, 0, Player.velocity.Length(), true) * 0.5f;
            tentacle.Update();

            Time++;
        }

        public Rope tentacle;

        public override bool? CanHitNPC(NPC target) => Attack!= 0;

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            base.ModifyDamageHitbox(ref hitbox);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitNPC(target, ref modifiers);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Common.MediumBloodSpill with { Volume = 0.5f, Pitch = 1f, PitchVariance = 0.5f, MaxInstances = 0 }, Projectile.Center);
            BloodMetaball metaball = ModContent.GetInstance<BloodMetaball>();
            for (int i = 0; i < 15; i++)
            {
                Vector2 bloodSpawnPosition = Projectile.Center + new Vector2(10 * Projectile.scale).RotatedBy(Projectile.rotation);
                Vector2 bloodVelocity = (Main.rand.NextVector2Circular(8f, 8f) - Projectile.velocity) * Main.rand.NextFloat(0.2f, 1.2f);
                metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(10f, 40f), Main.rand.NextFloat(2f));
            }
            float threshold = target.Size.Length() / 2f + Projectile.width / 2f;
            
            if (Vector2.Distance(Projectile.Center, target.position) > threshold&& Attack == 1f)
            {
                Attack = 2f;
                TimeInner = 0f;
            }

            if (Player.GetModPlayer<BloodArmorPlayer>().Frenzy)
            {
                Player.statLife++;
            }
            for(int i = 0; i < 2; i++)
                Player.GetModPlayer<BloodArmorPlayer>().AddBloodUnit();
            base.OnHitNPC(target, hit, damageDone);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D head = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D body = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;

            Vector2 glowOffset = new Vector2(0f, 1f);

            Vector2 glowscale = new Vector2(0.1f, 0.1f);

            if (tentacle != null)
            {
                List<Vector2> points = new List<Vector2>();
                points.AddRange(tentacle.GetPoints());
                points.Add(Player.MountedCenter);
                Vector2 tentacleVel = -Projectile.rotation.ToRotationVector2() * 16;

                for (int i = points.Count - 1; i > 0; i--)
                {
                    float rot = points[i].AngleTo(points[i - 1]); // MathHelper.PiOver2;
                    //Vector2 stretch = new Vector2((1.1f - (float)i / points.Count * 0.6f) * Projectile.scale, i > points.Count - 2 ? points[i].Distance(points[i - 1]) / (body.Height / 2f) : 1.1f);
                    // This line calculates the "stretch" of a segment of the tentacle (rope) being drawn. 
                    // It determines the scale and length of the segment based on its position in the list of points (i.e., how far along the tentacle it is).
                    // - The X component of the Vector2 (horizontal scale) decreases as the segment index (i) increases, making the tentacle taper toward the end.
                    // - The Y component (vertical stretch) is calculated differently for the last segment to ensure it stretches properly to connect to the next point.
                    //   Otherwise, it uses a default stretch value of 1.1f.
                    // This ensures the tentacle looks smooth and natural as it connects between points.
                    //stretch = new Vector2(Projectile.scale * 0.6f, points[i].Distance(points[i - 1]) / (body.Height - 2f) * 1.2f);
                    Main.EntitySpriteDraw(body, points[i] - Main.screenPosition, body.Frame(), Color.Crimson, rot, body.Size() * 0.5f, new Vector2(16, 16), 0, 0);

                }

                // Utils.DrawBorderString(Main.spriteBatch, Index.ToString(), Projectile.Center - Vector2.UnitY * 50 - Main.screenPosition, Color.White);
            }

            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition + glowOffset, glow.Frame(), Color.Crimson, Projectile.rotation, glow.Size() * 0.5f, new Vector2(0.1f, 0.1f), 0, 0);


            Main.EntitySpriteDraw(head, Projectile.Center - Main.screenPosition, head.Frame(), Color.White, Projectile.rotation, new Vector2(head.Width / 2, 0f), new Vector2(0.9f, 1.5f), 0, 0);

            return false;
        }
    }
}
