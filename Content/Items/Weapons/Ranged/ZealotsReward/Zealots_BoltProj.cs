using CalamityMod;
using CalamityMod.Particles;
using CalRemix.Content.NPCs.Bosses.Origen;
using Luminance.Assets;
using Luminance.Core.Graphics;
using NoxusBoss.Core.Graphics.Automators;
using System.Linq;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ZealotsReward
{
    internal class Zealots_BoltProj : ModProjectile, IDrawSubtractive
    {
        private Vector2[] oldPos;
        public int Time
        {
            get;
            set;
        }

        public ref Player Owner => ref Main.player[Projectile.owner];
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;


        public bool FirstHit
        {
            get;
            set;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CanHitPastShimmer[Projectile.type] = true;
            ProjectileID.Sets.TrailingMode[Type] = 3;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
        }

        public override void SetDefaults()
        {

            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = Projectile.height = 18;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;



            Projectile.idStaticNPCHitCooldown = 8;
            Projectile.extraUpdates =3;
        }

        public override void PostAI()
        {

            for (var i = oldPos.Length - 2; i > 0; i--)
            {
                oldPos[i] = oldPos[i - 1];
            }

            oldPos[0] = Projectile.Center + Projectile.velocity * 1;

            Projectile.light = 0.5f;
            base.PostAI();
        }

        public override void AI()
        {

            if (!FirstHit)
                return;

            if(Projectile.IsFinalExtraUpdate()&& Time % 2==0)
            {

                Rectangle r = Projectile.Hitbox;

                r.Inflate(60, 60);
                Zealots_FreezeGore.AddFreezeZone(r, 70, 0, false);
            }

            

            MediumMistParticle mist = new MediumMistParticle(Projectile.Center, Vector2.Zero,
                Main.rand.NextBool(3) ? Color.LightSteelBlue : Color.SteelBlue, Color.CadetBlue, Main.rand.NextFloat(0.1f, 0.285f), 150);
            if(Main.rand.NextBool()&& FirstHit)
            GeneralParticleHandler.SpawnParticle(mist, true);


            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.velocity *= 1.03f;

            float maxHomingRange = 900f;
            float maxAimLineDistance = 620f; // How far from the owner's aim line a target can be before it gets heavily penalized.
            float bestScore = float.MaxValue;
            NPC bestTarget = null;

            Vector2 lineStart = Owner.Center;
            Vector2 mouseWorld = Owner.Calamity().mouseWorld;
            Vector2 lineDirection = (mouseWorld - lineStart).SafeNormalize(Vector2.UnitX);

            float lineLength = Vector2.Distance(lineStart, mouseWorld);
            if (lineLength < 1f)
                lineLength = 1f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!npc.CanBeChasedBy(Projectile) || npc.friendly)
                    continue;

                Vector2 toNpc = npc.Center - Projectile.Center;
                float projectileDist = toNpc.Length();

                if (projectileDist > maxHomingRange)
                    continue;

                // Projection of the NPC onto the owner's aim line.
                Vector2 ownerToNpc = npc.Center - lineStart;
                float alongLine = Vector2.Dot(ownerToNpc, lineDirection);

                // Reject things far behind the player.
                if (alongLine < -40f)
                    continue;

                // Clamp to the line segment, but allow a little extension past the mouse.
                float clampedAlongLine = MathHelper.Clamp(alongLine, 0f, lineLength + 120f);
                Vector2 closestPointOnLine = lineStart + lineDirection * clampedAlongLine;

                float distFromAimLine = Vector2.Distance(npc.Center, closestPointOnLine);

                // Optional hard rejection for things way off the aim line.
                if (distFromAimLine > maxAimLineDistance * 2f)
                    continue;

                // Lower score = better target.
                // Strongly favor targets near the aim line, then secondarily nearby ones.
                float score =
                    distFromAimLine * 4f +
                    projectileDist * 0.8f -
                    MathHelper.Clamp(alongLine, 0f, 2000f) * 0.15f;

                // Optional: slightly favor what the projectile is already moving toward.
                Vector2 currentDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Vector2 dirToNpc = (npc.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float alignment = Vector2.Dot(currentDir, dirToNpc); // -1 to 1
                score -= MathHelper.Max(alignment, 0f) * 35f;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = npc;
                }
            }

            if (bestTarget != null)
            {
                Vector2 desiredDirection = (bestTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

                float speed = Projectile.velocity.Length();
                if (speed < 0.001f)
                    speed = 0.001f;

                Vector2 currentDirection = Projectile.velocity / speed;

                // Turn rate can scale with speed so faster projectiles don't feel sluggish.
                // This is radians per tick.
                float turnRate = MathHelper.ToRadians(1f);

                float currentAngle = currentDirection.ToRotation();
                float desiredAngle = desiredDirection.ToRotation();
                float newAngle = currentAngle.AngleTowards(desiredAngle, turnRate);

                Projectile.velocity = newAngle.ToRotationVector2() * speed;
            }
            if(Projectile.IsFinalExtraUpdate())
            Time++;
        }



        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Owner.HeldItem.type == ModContent.ItemType<Zealots_Item>())
            {
                Owner.HeldItem.ModItem.OnHitPvp(Owner, target, info);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //todo: onhit effect
            var metalball = ModContent.GetInstance<Zealots_HitEffect>();
            for (int i = 0; i < 1; i++)
            {
                float gasSize = 20 * Main.rand.NextFloat(0.32f, 1.6f);
                metalball.CreateParticle(Projectile.Center + Projectile.velocity*0.1f, Projectile.velocity*0, gasSize);
            }
            if (Owner.HeldItem.type == ModContent.ItemType<Zealots_Item>())
            {
                Owner.HeldItem.ModItem.OnHitNPC(Owner, target, hit, damageDone);
            }



            Projectile.velocity *= 0;
            Projectile.damage = -1;
            FirstHit = false;   
        }



        public override void OnSpawn(IEntitySource source)
        {
            FirstHit = true;

            oldPos = Enumerable.Repeat(Projectile.Center, 10).ToArray();
            for (var i = 0; i < Projectile.oldPos.Length; i++)
            {
                Projectile.oldPos[i] = Projectile.Center;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var Glowball = AssetDirectory.Textures.BigGlowball.Value;
            var Gorigin = Glowball.Size() / 2f;

            float thing = Projectile.velocity.LengthSquared()/100f;
            thing = LumUtils.InverseLerp(0, 60, thing);
           //Main.NewText(thing);
            Color glowColor = Color.Lerp(Color.CadetBlue, new (212,226,228), thing) with { A = 200 }; 
            if (FirstHit)
            {
                Main.spriteBatch.UseBlendState(BlendState.Additive);
                for (var i = 0; i < Projectile.oldPos.Length; i++)
                {
                    float interp = i / (float)Projectile.oldPos.Length;
                    var glowScale = new Vector2(0.1f, 0.05f) *(1-interp);
                    var DrawPos = Projectile.oldPos[i] + Projectile.Hitbox.Size() / 2f - Main.screenPosition;
                    Main.spriteBatch.Draw(Glowball, DrawPos, null,
                          glowColor * (1-interp), Projectile.velocity.ToRotation(),
                         Gorigin, glowScale, SpriteEffects.None, 0f);
                }


                Main.EntitySpriteDraw(Glowball, Projectile.Center - Main.screenPosition, null, Color.White with {A = 200}, 0, Gorigin, 0.09f, 0);
                Main.spriteBatch.ResetToDefault();
            }

            if (oldPos == null || oldPos.Length == 0)
                return false;

            float WidthFunction(float p) => 15f * MathF.Sin(p + 0.22f);
            Color ColorFunction(float p) => Color.Lerp(glowColor, Color.Transparent, p);

            var trailShader = ShaderManager.GetShader("HeavenlyArsenal.FusionRifle_Bullet");
            trailShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly + Projectile.velocity.Length() / 8f + Projectile.identity / 72.113f);
            trailShader.TrySetParameter("spin", 0.5f * Math.Sign(Projectile.velocity.X));
            trailShader.TrySetParameter("brightness", 1.5f);
            trailShader.SetTexture(CrackedNoiseA, 0, SamplerState.LinearWrap);
            trailShader.SetTexture(LiquidNoise, 1, SamplerState.PointWrap);
            trailShader.SetTexture(DendriticNoiseZoomedOut, 2, SamplerState.LinearWrap);
            
            
            PrimitiveRenderer.RenderTrail(oldPos, new PrimitiveSettings(WidthFunction, ColorFunction, _ => Vector2.Zero, Shader: trailShader, Smoothen: true), oldPos.Length);







            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            base.PostDraw(lightColor);
        }

        void IDrawSubtractive.DrawSubtractive(SpriteBatch spriteBatch)
        {
          

        }
    }
}
