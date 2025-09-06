using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Core;
using Luminance.Common.Easings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor
{
    internal class AwakenedBloodTendril : ModProjectile
    {
        #region setup
        public Vector2 HomePos = Vector2.Zero;
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Timer => ref Projectile.ai[0];
        public ref float AttackInterp => ref Projectile.ai[1];

        public BezierCurve Curve;
        public PiecewiseCurve AttackCurve;
        public float t = 0;
        public int npcIndex
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }
        public Rope tentacle;
        public int Tindex
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {   
            writer.Write7BitEncodedInt(Tindex);
            writer.WritePackedVector2(HomePos);

        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            reader.Read7BitEncodedInt();
            reader.ReadPackedVector2();
        }
        public override string Texture => "HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/BloodNeedle_Head";
        public override void SetStaticDefaults() 
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            
        }

        public override void SetDefaults()
        {
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;

            Projectile.Size = new Vector2(45, 45);

            Projectile.CountsAsClass(DamageClass.Generic);
        }
        #endregion
        public override void AI()
        {
            
            npcIndex = -1;
            CheckDespawnConditions();
           
            InitializeCurve();
            ManageTendril();

            UpdateHomePos();
           
            AttackTarget();
            


            Projectile.Center = Vector2.Lerp(Projectile.Center, HomePos, 0.4f);
            Timer++;
        }
        #region Helpers

        public void InitializeCurve()
        {
            if (Curve == null)
            {
                float RandomOffset = Main.rand.NextFloat(10, 30);
                float SmallerOffset = RandomOffset - 5;

                RandomOffset *= Main.rand.Next(-1, 2);
               
                if (Main.MouseWorld.X - Owner.Center.X! < 0)
                {
                    RandomOffset = RandomOffset * -1;
                }
                SmallerOffset *= Math.Sign(RandomOffset);
                Curve = new BezierCurve(
                new Vector2(0, SmallerOffset),
                new Vector2(10, RandomOffset),
                new Vector2(20, SmallerOffset),
                new Vector2(30, 0)
                );
                

                //Main.NewText($"{Math.Sign(RandomOffset)}");
            }

            
        }
        public void CheckDespawnConditions()
        {
            Projectile.timeLeft = 4;
            AwakenedBloodPlayer a = Owner.GetModPlayer<AwakenedBloodPlayer>();
            if (!Owner.active || !a.AwakenedBloodSetActive || a.CurrentForm != AwakenedBloodPlayer.Form.Offsense)
            {
                Projectile.active = false;
                return;
            }
        }
        public void UpdateHomePos()
        {
            float val = Tindex % 2 == 0 ? -80 : 100;
            
            val *=  Owner.direction;
            if (Tindex == 1)
                val -= 20 * Owner.direction;
            float wave = (float)Math.Sin((Timer/5 + Tindex*10)/10)*4 - 50;
           
            Vector2 s = tentacle.segments[0].position;
            Vector2 d = tentacle.segments[2].position;
            if (Vector2.Distance(s, d) > 10)
                Projectile.rotation = s.AngleTo(d) + MathHelper.Pi;

            tentacle.segments[^1].position = Owner.MountedCenter;

            HomePos = Vector2.Lerp(HomePos, Owner.Center + new Vector2(val, wave), 0.1f);
        }
        public int TargetNPC()
        {
            int closestIndex = -1;
            float closestDistance = 900f; 
            Vector2 playerCenter = Owner.Center;



            foreach (NPC npc in Main.ActiveNPCs)
            {

               

                if (!npc.active || npc.friendly || npc.dontTakeDamage) // skip invalid targets
                    continue;
             
                float distance = Vector2.Distance(playerCenter, npc.Center);
               
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = npc.whoAmI;
                }
               
            }
            return closestIndex; 
        }

        /// <summary>
        /// Manages Attacking and using the Piecewisecurve.
        /// </summary>
        public void AttackTarget()
        {

            Projectile.extraUpdates = 3;

            if (AttackCurve == null)
                AttackCurve = new PiecewiseCurve()
                    .Add(EasingCurves.Exp, EasingType.InOut, 0f, 0.3f, 1f)
                    .Add(EasingCurves.Linear, EasingType.InOut, 0, 0.35f)
                    .Add(EasingCurves.Cubic, EasingType.Out, 1f, 1f);
            
            npcIndex = TargetNPC();
            if (npcIndex != -1)
                t = Utils.Clamp(t + 0.005f, 0, 1);
            else
            {
                t = 0;
                AttackInterp = 0;
                return;

            }


            NPC victim = Main.npc[npcIndex];
            //t is the value that actually controls the attack.
            //attack interp is used to move the tendril, sure, but t is what does the heavy lifting.
            AttackInterp = 1 - AttackCurve.Evaluate(t);
           
            Vector2 AverageCenter = (HomePos + Owner.Center)/2;

            float Angle = victim.Center.AngleFrom(AverageCenter);
            float RangeMulti = 0;
            RangeMulti = (float)Math.Clamp(Vector2.Distance(Owner.MountedCenter, victim.Center)/26.4f, 0,10);
            Vector2 CurvePath = Curve.Evaluate(AttackInterp).RotatedBy(Angle) * RangeMulti;
            tentacle.segments[^1].position = Owner.MountedCenter;
            
            
            HomePos += CurvePath/10 * AttackInterp;
            if (t == 1)
            {
                t = 0;
                Curve = null;
            }

            /*for(int i = 1; i< 100; i++)
          {
              Vector2 dustPos = Owner.Center + Curve.Evaluate(i / 100f).RotatedBy(Angle - MathHelper.ToRadians(15))*RangeMulti;
              Dust c = Dust.NewDustPerfect(dustPos, DustID.Cloud, Vector2.Zero);
              c.noGravity = true;
              c.color = Color.Red;
              c.fadeIn = 0.2f;

          }*/
        }
        public void ManageTendril()
        {

            if (tentacle == null)
            {
                tentacle = new Rope(Projectile.Center, Owner.MountedCenter, 15, 10, Vector2.Zero);
            }
            for (int i = 1; i < tentacle.segments.Length - 1; i++)
            {
                if (Main.rand.NextBool(85) && i < tentacle.segments.Length - tentacle.segments.Length / 6)
                {
                    Dust blood = Dust.NewDustPerfect(tentacle.segments[i].position, DustID.Blood, new Vector2(0, -3f), 10, Color.Crimson, 1);
                    blood.noGravity = true;
                    blood.rotation = Main.rand.NextFloat(-89, 89);
                }

                if (t >= 0.5f || t <= 0.35f && i <= 10)
                {
                    tentacle.segments[i].position = Vector2.Lerp(tentacle.segments[i].position, tentacle.segments[i].position + new Vector2(10 * -Owner.direction, -10 + Tindex % 2),0.5f);
                }
                else if(AttackInterp == 0)
                {

                    
                }
                float difference = Main.MouseWorld.X - Owner.Center.X;
               
                    tentacle.segments[i].position += Curve.Evaluate((i + 1) / tentacle.segments.Length) * Math.Sign(difference) * AttackInterp;

               
            }

            Vector2 jitter = new Vector2(MathF.Sin(Timer * 0.05f + Tindex * 1.5f),
                MathF.Cos(Timer * 0.05f + Tindex * 1.5f)
            ) * Main.rand.NextFloat(2);

            
            tentacle.gravity = new Vector2(-Projectile.velocity.X / tentacle.segments.Length, -Projectile.velocity.Y);

            tentacle.damping = Utils.GetLerpValue(40, 20, Owner.velocity.Length(), true) * 0.65f;

            //todo: delete this shit
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / 8;
            Rectangle HeadFrame = new Rectangle(0, 1 * frameHeight, tex.Width, 192);
            float scale = Projectile.scale;
            Vector2 localTip = new Vector2(
                0f,
                HeadFrame.Height / 5
            );
            Vector2 tipOffset = localTip.RotatedBy(Projectile.rotation);

            tentacle.segments[0].position = Projectile.Center;
            tentacle.segments[1].position = tentacle.segments[0].position - tipOffset + Projectile.velocity;
            tentacle.segments[^1].position = Owner.MountedCenter;
            

            Projectile.Center += jitter / 10;
            tentacle.Update();
        }
        #endregion
        /// <summary>
        /// For the visual effect on the needles
        /// </summary>
        public float pulsateAmount = 0;

        public override bool? CanDamage()
        {
            if (t > 0.35)
                return false;
            else
                return base.CanDamage();
        }
        #region drawshit
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
           
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.isAPreviewDummy)
            {
                return base.PreDraw(ref lightColor);
            }

            DrawLine();

            DrawTendril(ref lightColor);
            Texture2D needle = ModContent.Request<Texture2D>(Texture).Value;

            Vector2 DrawPos = Projectile.Center - Main.screenPosition;

            Rectangle frm = needle.Frame(1, 8, 0, 0);
            Vector2 Origin = new Vector2(needle.Width/2 ,frm.Height/2);

            float Rot = Projectile.rotation - MathHelper.PiOver2;
            Main.EntitySpriteDraw(needle, DrawPos, frm, lightColor, Rot, Origin, 1, SpriteEffects.None);

            Utils.DrawBorderString(Main.spriteBatch, "T: "+t.ToString(), DrawPos, Color.Red);
            //Utils.DrawBorderString(Main.spriteBatch, "AttackInterp: " + AttackInterp.ToString(), DrawPos - Vector2.UnitY*-20, Color.Red);
            Utils.DrawBorderString(Main.spriteBatch, $"{Projectile.ai[2]}" + t.ToString(), DrawPos - Vector2.UnitY*-40, Color.Red);

            return false;
        }
        public void DrawTendril(ref Color lightColor)
        {
            if (tentacle != null)
            {

                Texture2D body = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/RegSeg").Value;

                List<Vector2> points = new List<Vector2>();

                points.AddRange(tentacle.GetPoints());
                points.Add(Owner.MountedCenter);
                Vector2 tentacleVel = -Projectile.rotation.ToRotationVector2() * 16;

                // DrawLine(points);
                int pulsateIndex = (int)(Main.GlobalTimeWrappedHourly * 20) % points.Count; // Determine which point to pulsate
                pulsateAmount = (float)(Math.Cos(Main.GlobalTimeWrappedHourly) % 1f * 0.2f);

                int BodyframeHeight = body.Height / 2;
                Rectangle BodyFrame = body.Frame(1, 2, 0, 0);
                Vector2 Borigin = new Vector2(body.Width / 2f, BodyframeHeight / 2f);

                //for (int i = points.Count - 1; i > 0; i--)
                for (int i = 1; i < points.Count - 1; i++)
                {

                    float rot = points[i].AngleTo(points[i - 1]);
                    float currentPulsate = 0f;

                    // Apply pulsate amount to the current point, and half to adjacent points
                    if (i == pulsateIndex)
                    {
                        currentPulsate = pulsateAmount;
                    }
                    else if (i == pulsateIndex - 2 || i == pulsateIndex + 2)
                    {
                        currentPulsate = pulsateAmount * 0.01f;
                    }

                    Vector2 stretch = new Vector2((1.3f - (float)i / points.Count * 0.6f) * Projectile.scale,
                        i > points.Count ? points[i].Distance(points[i - 1]) / (body.Height / 2f) : 1.1f + currentPulsate);


                    Main.EntitySpriteDraw(body, points[i] - Main.screenPosition, BodyFrame, lightColor, rot, Borigin, stretch, SpriteEffects.None, 0);

                }
            }
            else
                return;
        }
        private void DrawLine()
        {
            Texture2D texture = TextureAssets.FishingLine.Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(frame.Width / 2, 2);

            List<Vector2> points = new List<Vector2>();

            points.AddRange(tentacle.GetPoints());
            points.Add(Owner.MountedCenter);

            Vector2 pos = points[0] + new Vector2(-2,0);
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 element = points[i];
                Vector2 diff = points[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.Crimson);
                Vector2 scale = new Vector2(2, (diff.Length() + 2) / frame.Height);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

                pos += diff;
            }
        }

        #endregion
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
           
        }
    }
}
