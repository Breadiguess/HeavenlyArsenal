using CalamityMod;
using CalamityMod.Items.Weapons.Melee;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.SwordData;
using Luminance.Assets;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.Projectiles
{
    internal class TrioSwordProjectile : ModProjectile
    {
        public static int ExtraUpdates = 3;
        public int Time;
        public SwordAttackDef AttackDef;
        public SwordCombatPlayer SwordPlayer => Owner.GetModPlayer<SwordCombatPlayer>();
        public ref Player Owner => ref Main.player[Projectile.owner];
        #region Cool Texture Shenanigans;
        private static string Path => "HeavenlyArsenal/Content/Items/Weapons/Melee/EdgyDualSwords/Projectiles/";
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public SwordAttackID CurrentID;
        /// <summary>
        /// Stores Key-Value pairs for the texture of the sword for the currentID.
        /// where Int is the Height (in frames) of the texture.
        /// </summary>
        private static readonly Dictionary<SwordAttackID, (Asset<Texture2D>, int)> WeaponTex = [];
        public void AddPair(SwordAttackID Value, int FrameCount)
        {
            Asset<Texture2D> tex;
            if (ModContent.RequestIfExists<Texture2D>(Path + Value.ToString(), out var asset))
            {
                tex = asset;
                WeaponTex.Add(Value, new(tex, FrameCount));

                Mod.Logger.Info($"Loaded {Value.ToString()}.");
            }
            else
            {
                Mod.Logger.Info($"Failed to load {Value.ToString()}: The accompanying Texture was not found.");
            }
        }
        #endregion;
        public override void Load()
        {
            foreach (Enum a in Enum.GetValues(typeof(SwordAttackID)))
            {

            }
            AddPair(SwordAttackID.Large_Light, 9);
            AddPair(SwordAttackID.Small_Light1, 11);
        }
        public override void SetStaticDefaults()
        {

        }
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ContinuouslyUpdateDamageStats = true;
            Projectile.Size = new Vector2(10, 10);
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = ExtraUpdates;
        }
        public int InitialTime;
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Owner.Center.AngleTo(Owner.Calamity().mouseWorld);
            
        }

        public override bool PreAI()
        {
            if(Time== 1)
            {

                InitialTime = Projectile.timeLeft+1;
                Main.NewText(InitialTime);
            }


            Owner.heldProj = this.Projectile.whoAmI;

            Projectile.Center = Owner.Center;
            Projectile.velocity = Projectile.rotation.ToRotationVector2();

            Projectile.spriteDirection = Projectile.direction;
            Time++;
            return base.PreAI();
        }

        public override void AI()
        {
            Owner.SetDummyItemTime(2);

            base.AI();
        }



        #region Collisions

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Owner.GetModPlayer<SwordDevPlayer>().RegisterValidHit();

            SoundEngine.PlaySound
                (
                    (target.Organic() ? Murasama.OrganicHit : Murasama.InorganicHit) with
                    {
                        MaxInstances = 16,
                        Volume = 0.3f,
                        PitchVariance = 0.4f,
                        Pitch = -0.2f
                    }
                );
            TrioSwordHitEffect hitEffect = new TrioSwordHitEffect();
            float rot = target.AngleFrom(Projectile.Center) + MathHelper.ToRadians(Main.rand.NextFloat(-3, 3));
            Color color = SwordPlayer.CurrentMode == SwordMode.Large ? Color.White : Color.Red;
            hitEffect.Prepare(target.Center, rot, 20, color);

            ParticleEngine.ShaderParticles.Add(hitEffect);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitNPC(target, ref modifiers);
            if (AttackDef is null)
                return;

            modifiers.SourceDamage *= AttackDef.DamageMultiplier;

            modifiers.Knockback *= AttackDef.KnockbackMultiplier;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            base.ModifyHitPlayer(target, ref modifiers);
            if (AttackDef is null)
                return;

            modifiers.SourceDamage *= AttackDef.DamageMultiplier;
            modifiers.Knockback *= AttackDef.KnockbackMultiplier;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float progress = GetSwingProgress();

            // Direction the sword is facing.
            Vector2 facing = Projectile.rotation.ToRotationVector2();

            // How far the sword can reach.
            float BaseSize = SwordPlayer.CurrentMode == SwordMode.Small ? 80 : 150;
            float length = BaseSize * Projectile.scale * 1.5f;

            // Cone angle changes through the swing.
            // Early swing = narrower, mid swing = wider, late swing = narrower again.
            float coneHalfAngle = GetConeHalfAngle(progress);
            coneHalfAngle += Projectile.rotation;
            if (targetHitbox.IntersectsConeFastInaccurate(Projectile.Center, length, coneHalfAngle, MathHelper.ToRadians(30)))
                return true;

            return false;
            for (int i = 0; i < 20; i++)
            {
                Dust a = Dust.NewDustPerfect(Projectile.Center + new Vector2(float.Lerp(0, length, i/20f), 0).RotatedBy(coneHalfAngle), DustID.Cloud);
                a.velocity = Vector2.Zero;
                a.scale = 0.5f;
                a.noGravity = true;
            }
          
        }
        #endregion

        #region Helpers
        private float GetSwingProgress()
        {
            int lifetime = AttackDef?.ProjectileLifeTime ?? 60;
            return MathHelper.Clamp(1f - Projectile.timeLeft / (float)lifetime, 0f, 1f);
        }

        private float GetConeHalfAngle(float progress)
        {
            // Widest in the middle of the swing.
            // Tweak these values to taste.
            float start = MathHelper.ToRadians(-120f);
            float end = MathHelper.ToRadians(196f);

            return float.Lerp(start, end, progress) * Projectile.direction;
        }

        private bool TargetIntersectsCone(Rectangle targetHitbox, Vector2 coneOrigin, Vector2 coneDirection, float coneLength, float coneHalfAngle)
        {
            // Sample important points on the target hitbox.
            Span<Vector2> points = stackalloc Vector2[9];
            GetRectangleSamplePoints(targetHitbox, points);

            for (int i = 0; i < points.Length; i++)
            {
                if (PointInCone(points[i], coneOrigin, coneDirection, coneLength, coneHalfAngle))
                    return true;
            }

            return false;
        }

        private void GetRectangleSamplePoints(Rectangle rect, Span<Vector2> points)
        {
            points[0] = rect.TopLeft();
            points[1] = rect.TopRight();
            points[2] = rect.BottomLeft();
            points[3] = rect.BottomRight();
            points[4] = rect.Center.ToVector2();

            points[5] = new Vector2(rect.Left, rect.Center.Y);
            points[6] = new Vector2(rect.Right, rect.Center.Y);
            points[7] = new Vector2(rect.Center.X, rect.Top);
            points[8] = new Vector2(rect.Center.X, rect.Bottom);
        }

        private bool PointInCone(Vector2 point, Vector2 coneOrigin, Vector2 coneDirection, float coneLength, float coneHalfAngle)
        {
            Vector2 toPoint = point - coneOrigin;
            float distance = toPoint.Length();

            if (distance <= 0.001f)
                return true;

            if (distance > coneLength)
                return false;

            toPoint /= distance;
            coneDirection = coneDirection.SafeNormalize(Vector2.UnitX);

            float dot = Vector2.Dot(coneDirection, toPoint);

            // Compare against cosine of half-angle instead of using acos.
            float minDot = MathF.Cos(coneHalfAngle);
            return dot >= minDot;
        }
        #endregion

        #region DrawCode    


        private int GetAnimFrame(int MaxFrames)
        {
            int t = 0;
            int StartTime = InitialTime;
            if (AttackDef is not null)
            {
                //StartTime = AttackDef.ProjectileLifeTime;
            }


            t = (int)(MaxFrames * LumUtils.InverseLerp(StartTime, 0, Projectile.timeLeft));


            return t;
        }

        public override bool PreDraw(ref Color lightColor)
        {

            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            SpriteEffects flip = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;


            if (WeaponTex.TryGetValue(CurrentID, out var a))
            {
                Texture2D tex = a.Item1.Value;

                Rectangle Frame = tex.Frame(1, a.Item2, frameY: GetAnimFrame(a.Item2));




                Main.EntitySpriteDraw(tex, DrawPos, Frame, lightColor, Projectile.rotation, Frame.Size() / 2, Projectile.scale, flip);
            }






            return base.PreDraw(ref lightColor);
        }


        #endregion

    }
}
