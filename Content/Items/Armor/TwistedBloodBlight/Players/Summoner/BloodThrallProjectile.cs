using Luminance.Assets;
using NoxusBoss.Assets;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner
{
    internal class BloodThrallProjectile : ModProjectile
    {
        private float IdleRadius = 120f;
        private float Inertia = 20f;
        private float MoveSpeed = 8f;
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public ref Player Owner => ref Main.player[Projectile.owner];
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.Size = new Vector2(40, 40);
            Projectile.minion = true;
            Projectile.penetrate = -1;
            Projectile.minionSlots = 0;
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
              
                return;
            }

            DefaultBehavior();
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