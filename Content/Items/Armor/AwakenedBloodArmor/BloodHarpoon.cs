using HeavenlyArsenal.Common.utils;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor
{
    class BloodHarpoon : ModProjectile
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

        public Rope HarpoonTendril;
        public ref float Time => ref Projectile.ai[0];
        public ref float HarpoonedNPC => ref Projectile.ai[1];
        public ref float HarpoonState => ref Projectile.ai[2];
        public ref float HarpoonOffsetX => ref Projectile.localAI[0];
        public ref float HarpoonOffsetY => ref Projectile.localAI[1];
        public ref Player Player => ref Main.player[Projectile.owner];

        // Configurable fields:
        public float HarpoonSpeed => 30f;
        public float HarpoonBreakDistance => 780f;
        public float HarpoonRange => 600f;
        public float HarpoonDrainTime => 500f;
        public Vector2 HarpoonOffset => new Vector2(HarpoonOffsetX, HarpoonOffsetY);

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 64;
            
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 11;
            Projectile.timeLeft = 1000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            //Projectile.hide = true;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            HarpoonedNPC = -1;
            HarpoonState = 0;
            Time = 0;
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.DisgustingStarSever with { MaxInstances = 0, PitchVariance = 1f, Volume = 0.4f });
        }

        public override void AI()
        {
            // Terminate if player invalid
            if (Player.dead || !Player.active || Player.GetModPlayer<BloodArmorPlayer>().CurrentForm != BloodArmorForm.Defense)
            {
                Projectile.Kill();
                return;
            }

            if (Player.GetModPlayer<BloodArmorPlayer>().BloodArmorEquipped != true)
            {
                Projectile.Kill();
                return;
            }

            // Setup rope
            if (HarpoonTendril == null)
            {
                HarpoonTendril = new Rope(Projectile.Center, Player.MountedCenter, 30, 10f, Vector2.Zero);
            }
            for (int i = 0; i < HarpoonTendril.segments.Length - 1; i++)
            {
                if (Main.rand.NextBool(100))
                {
                    Dust blood = Dust.NewDustPerfect(HarpoonTendril.segments[i].position, DustID.CrimtaneWeapons, new Vector2(0, 0f), 10, Color.Crimson, 1);
                    blood.noGravity = true;
                    blood.rotation = Main.rand.NextFloat(-20, 20);

                }
                if (i <= HarpoonTendril.segments.Length / 2 && i > 3)
                {
                    //HarpoonTendril.segments[i].velocity += new Vector2(13 * Player.direction, 50);
                }
            }
            HarpoonTendril.segments[0].position = Projectile.Center;
            HarpoonTendril.segments[^1].position = Player.MountedCenter;
            HarpoonTendril.gravity = -Vector2.UnitX * Player.direction * 0.05f - Vector2.UnitY * 0.01f;
            HarpoonTendril.damping = Utils.GetLerpValue(20, 0, Player.velocity.Length(), true) * 0.05f;
            HarpoonTendril.Update();

            // THE WHITE WHALEEEE
            if (HarpoonState == 0)
            {
                // Firing: shoot in the direction from player to mouse, not from projectile
                Vector2 direction = Main.MouseWorld - Player.MountedCenter;
                direction.Normalize();
                Projectile.velocity = direction * HarpoonSpeed;
                Projectile.rotation = Projectile.velocity.ToRotation();
                // Retract immediately if max range reached without a hit
                if (HarpoonedNPC < 0 && Vector2.Distance(Player.MountedCenter, Projectile.Center) >= HarpoonRange)
                {
                    HarpoonState = 2;
                }
            }
            else if (HarpoonState == 1)
            {
                // Attached to NPC
                Time++;
                if (HarpoonedNPC >= 0 && HarpoonedNPC < Main.maxNPCs)
                {
                    NPC target = Main.npc[(int)HarpoonedNPC];
                    if (target.active)
                    {
                        Projectile.Center = target.Center + HarpoonOffset;

                        if(Time % 4 == 0)
                        {
                            var bloodArmorPlayer = Player.GetModPlayer<BloodArmorPlayer>();

                            if (Player.statLifeMax2 != Player.statLife)
                                if (bloodArmorPlayer.Clot > 0)
                                    bloodArmorPlayer.EatClot();
                                else if (bloodArmorPlayer.CurrentBlood != 1) 
                                {
                                    bloodArmorPlayer.AddBloodUnit();
                                }


                        }
                        if (Vector2.Distance(Player.MountedCenter, Projectile.Center) > HarpoonBreakDistance)
                            HarpoonState = 3;
                        if (Time >= HarpoonDrainTime)
                            HarpoonState = 2;
                        return;
                    }
                }
                HarpoonState = 2;
            }
            else if (HarpoonState == 2)
            {
                // Retracting
                Vector2 toPlayer = Player.MountedCenter - Projectile.Center;
                toPlayer.Normalize();
                Projectile.velocity = toPlayer * HarpoonSpeed * 1.5f;
                if (Vector2.Distance(Projectile.Center, Player.MountedCenter) < 20f)
                    Projectile.Kill();
            }
            else if (HarpoonState == 3)
            {
                // Break
                SoundEngine.PlaySound(GennedAssets.Sounds.NPCKilled.DeltaruneExplosion with { PitchVariance = 0.45f, MaxInstances = 0 }, Projectile.Center);
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            
            Texture2D head = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/BloodHarpoon").Value;
            
            Texture2D body = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/RegSeg").Value;

            List<Vector2> points = new List<Vector2>();
            points.AddRange(HarpoonTendril.GetPoints());
            points.Add(Player.MountedCenter);


            for (int i = points.Count - 1; i > 0; i--)
            {

                float rot = points[i].AngleTo(points[i - 1]); // MathHelper.PiOver2;
                Vector2 stretch = new Vector2((1.1f - (float)i / points.Count ) * Projectile.scale, 
                    i > points.Count - 2 ? points[i].Distance(points[i - 1]) 
                    / (body.Height / 2f) : 1.1f);
                // This line calculates the "stretch" of a segment of the HarpoonTendril (rope) being drawn. 
                // It determines the scale and length of the segment based on its position in the list of points (i.e., how far along the HarpoonTendril it is).
                // - The X component of the Vector2 (horizontal scale) decreases as the segment index (i) increases, making the HarpoonTendril taper toward the end.
                // - The Y component (vertical stretch) is calculated differently for the last segment to ensure it stretches properly to connect to the next point.
                //   Otherwise, it uses a default stretch value of 1.1f.
                // This ensures the HarpoonTendril looks smooth and natural as it connects between points.
                Vector2 drawPos = points[i] - Main.screenPosition;
                Main.EntitySpriteDraw(body, drawPos, body.Frame(), Color.White, rot, body.Size() * 0.5f, new Vector2(0.5f, 0.5f), 0, 0);
                
            }
            Main.EntitySpriteDraw(head, Projectile.Center - Main.screenPosition, head.Frame(), lightColor, Projectile.rotation, head.Size()*0.5f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        public override void EmitEnchantmentVisualsAt(Vector2 boxPosition, int boxWidth, int boxHeight)
        {
            base.EmitEnchantmentVisualsAt(boxPosition, boxWidth, boxHeight);
        }

        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitNPC(target, ref modifiers);
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (HarpoonedNPC == -1)
            {
                // Latch onto first hit NPC
                HarpoonedNPC = target.whoAmI;
                HarpoonState = 1;

                // Calculate an attach point on NPC's edge so the harpoon visibly pierces it
                Vector2 dir = Projectile.velocity;
                if (dir != Vector2.Zero) dir.Normalize();
                Vector2 attachPoint = target.Center + dir * (target.width / 2f);

                // Move projectile to that point and store offset
                Projectile.Center = attachPoint;
                Vector2 offset = attachPoint -target.Center;
                HarpoonOffsetX = offset.X;
                HarpoonOffsetY = offset.Y;

                Time = 0;
                SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
            }
            base.OnHitNPC(target, hit, damageDone);
        }
    }
}
