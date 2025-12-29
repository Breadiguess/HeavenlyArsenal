using Luminance.Assets;
using NoxusBoss.Assets;
using static HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner.BloodOvermind;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner
{
    internal class BloodThrallProjectile : ModProjectile
    {
        private float IdleRadius = 120f;
        private float Inertia = 20f;
        private float MoveSpeed = 8f;

        public OvermindDirective CurrentDirective;
        public Projectile proj; // the overmind itself

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public ref Player Owner => ref Main.player[Projectile.owner];
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.Size = new Vector2(40, 40);
            Projectile.minion = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public bool OvermindIsActive
        {
            get
            {
                SummonerBloodController controller = Owner.GetModPlayer<BloodBlightParasite_Player>().ConstructController as SummonerBloodController;
                return controller != null && controller.overmindActive;
            }
        }

        public override void AI()
        {
            Projectile.timeLeft++;

            if (OvermindIsActive)
            {
                FollowOvermindDirective();
            }
            else
            {
                DefaultBehavior();
            }
        }

        private void DefaultBehavior()
        {

            
            if (!Owner.active || Owner.dead)
            {
                Projectile.Kill();
                return;
            }

            NPC target = Projectile.FindTargetWithinRange(1000, true);

            if (target != null)
                AttackTarget(target);
            else
                IdleAroundOwner(Owner);

            Projectile.rotation += 0.05f;
        }
        private void AttackTarget(NPC target)
        {
            Vector2 desiredVelocity =
                (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * MoveSpeed;

            Projectile.velocity =
                (Projectile.velocity * (Inertia - 1) + desiredVelocity) / Inertia;
        }

        private void IdleAroundOwner(Player owner)
        {
            Vector2 idlePos =
                owner.Center +
                new Vector2(IdleRadius, 0f).RotatedBy(Main.GameUpdateCount * 0.02f + Projectile.whoAmI);

            Vector2 desiredVelocity =
                (idlePos - Projectile.Center) * 0.1f;

            Projectile.velocity =
                (Projectile.velocity * (Inertia - 1) + desiredVelocity) / Inertia;
        }

        private void FollowOvermindDirective()
        {
            BloodOvermind overmind = FindOvermind();
            if (overmind == null)
            {
                DefaultBehavior();
                return;
            }

            switch (overmind.CurrentDirective)
            {
                case OvermindDirective.Assemble:
                    AssembleBehavior(overmind);
                    break;

                case OvermindDirective.Pressure:
                    PressureBehavior(overmind);
                    break;

                case OvermindDirective.Frenzy:
                    FrenzyBehavior(overmind);
                    break;
            }
        }

        private void AssembleBehavior(BloodOvermind overmind)
        {
            Vector2 toOvermind = overmind.Projectile.Center - Projectile.Center;

            float speed = 10f;
            float inertia = 30f; // very smooth, controlled

            Vector2 desiredVelocity =
                toOvermind.SafeNormalize(Vector2.Zero) * speed;

            Projectile.velocity =
                (Projectile.velocity * (inertia - 1) + desiredVelocity) / inertia;


            Projectile.rotation += 0.03f;

        }

        private void PressureBehavior(BloodOvermind overmind)
        {
            int index = Projectile.whoAmI % 6;
            float angle = MathHelper.TwoPi * index / 6f;

            Vector2 formationOffset =
                new Vector2(80f, 0f).RotatedBy(angle);

            Vector2 desiredPosition =
                overmind.Projectile.Center + formationOffset;

            float speed = 8f;
            float inertia = 18f;

            Vector2 desiredVelocity =
                (desiredPosition - Projectile.Center)
                .SafeNormalize(Vector2.Zero) * speed;

            Projectile.velocity =
                (Projectile.velocity * (inertia - 1) + desiredVelocity) / inertia;


            Projectile.rotation += 0.06f;
        }


        private void FrenzyBehavior(BloodOvermind overmind)
        {
            NPC target = Projectile.FindTargetWithinRange(1200f, true);

            if (target != null)
            {
                float speed = 14f;
                float inertia = 6f; 

                Vector2 desiredVelocity =
                    (target.Center - Projectile.Center)
                    .SafeNormalize(Vector2.Zero) * speed;

                Projectile.velocity =
                    (Projectile.velocity * (inertia - 1) + desiredVelocity) / inertia;
            }
            else
            {
                // Even without a target, thralls move erratically
                Projectile.velocity += Main.rand.NextVector2Circular(2f, 2f);
            }


            Projectile.rotation += 0.2f;
        }


        private BloodOvermind FindOvermind()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active &&
                    p.owner == Projectile.owner &&
                    p.ModProjectile is BloodOvermind overmind)
                {
                    return overmind;
                }
            }
            return null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D orb = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;

            Utils.DrawLine(Main.spriteBatch, Projectile.Center, Projectile.Center + new Vector2(0, -100), Color.Crimson, Color.Transparent, 4);
            Main.EntitySpriteDraw(orb, Projectile.Center - Main.screenPosition, null, Color.Crimson with { A = 0 }, 0, orb.Size() / 2, 0.2f, 0);
            return false;
        }
    }
}