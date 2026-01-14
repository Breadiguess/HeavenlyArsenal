using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Common.IK;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner.Thralls;
using Luminance.Assets;
using NoxusBoss.Core.Physics.VerletIntergration;
using System.Collections.Generic;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner
{
    public partial class BloodOvermind : ModProjectile
    {
        public Vector2 OvermindHeadPos;
        public enum OvermindDirective
        {
            Dormant,
            Assemble,
            Pressure,
            Placeholder0,
            Placeholder1,
            Frenzy,
            Collapse
        }

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public ref Player Owner => ref Main.player[Projectile.owner];

        public OvermindDirective CurrentDirective { get; private set; }


        public class BloodOvermindLimb
        {
            public IKSkeleton Skeleton;
            public Vector2 EndPosition;

            public BloodOvermindLimb(IKSkeleton skeleton, Vector2 endPosition)
            {
                Skeleton = skeleton;
                EndPosition = endPosition;
            }

        }
        public BloodOvermindLimb limb;

        public const int MAX_TENDRILS = 6;
        public override void SetDefaults()
        {

            Segments = new List<List<VerletSimulatedSegment>>(MAX_TENDRILS);
            for (int i = 0; i < MAX_TENDRILS; i++)
            {

                Segments.Add(new List<VerletSimulatedSegment>(10));
                for (int x = 0; x < 10; x++)
                {
                    Segments[i].Add(new VerletSimulatedSegment(Projectile.Center, Vector2.Zero));
                }
            }
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.Size = new Vector2(100, 100);
            Projectile.minion = true;
            Projectile.penetrate = -1;
            Projectile.minionSlots = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;

            IKSkeleton skeleton = new IKSkeleton((40, new IKSkeleton.Constraints
            {
                MinAngle = MathHelper.ToRadians(-80),
                MaxAngle = MathHelper.ToRadians(-40)

            }),
                (20, new IKSkeleton.Constraints()), (40, new IKSkeleton.Constraints()));

            limb = new BloodOvermindLimb(skeleton, Projectile.Center + new Vector2(40, 100));
        }

        public override void AI()
        {
            Projectile.timeLeft++;
            if (!Owner.active || Owner.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = Vector2.Lerp(Projectile.Center, Owner.Center + new Vector2(-50 * Owner.direction, -100 + MathF.Cos(Main.GameUpdateCount *0.05f)*10), 0.5f);


            OvermindContext context = BuildContext();

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];

                if (!proj.active)
                    continue;

                if (proj.owner != Owner.whoAmI)
                    continue;

                if (proj.ModProjectile is IBloodThrall thrall)
                {
                    thrall.UpdateFromOvermind(context);
                }
            }


            CurrentDirective = DecideDirective();

            switch (CurrentDirective)
            {
                case OvermindDirective.Assemble:

                    break;

                case OvermindDirective.Pressure:

                    break;

                case OvermindDirective.Frenzy:

                    break;
            }


        }
        private OvermindContext BuildContext()
        {
            return new OvermindContext(this.CurrentDirective);
        }
        public Vector2 BaseLimbPos;


        public List<List<VerletSimulatedSegment>> Segments;
        public void DoAssemble()
        {

        }

        public Vector2[] Offsets = new Vector2[MAX_TENDRILS]
        {
            new Vector2(40,-60),
            new Vector2(30,-50),
            new Vector2(20,-50),
            new Vector2(-10,-40),
            new Vector2(-24,-50),

            new Vector2(-40,-55)
        };

        public override void PostAI()
        {
            Offsets = new Vector2[MAX_TENDRILS]
        {
            new Vector2(40,-60),
            new Vector2(30,-50),
            new Vector2(20,-50),
            new Vector2(-10,-40),
            new Vector2(-24,-50),

            new Vector2(-40,-65)



        };


            OvermindHeadPos = Projectile.Center + new Vector2(0, -90);
            BaseLimbPos = Projectile.Center + new Vector2(30, -60);
            limb.EndPosition = BaseLimbPos + new Vector2(40 + MathF.Sin(Main.GameUpdateCount / 10f) * 10, 30);
            limb.Skeleton.Update(BaseLimbPos, limb.EndPosition);
            if (Main.GameUpdateCount % 2 == 0)
            {
                BloodOvermind_Particle particle = new BloodOvermind_Particle();
                particle.Prepare(Projectile.Center + new Vector2(0, -20), new Vector2(0, -10), 0, 60, Color.Red, Color.Red, 1);
                ParticleEngine.BehindProjectiles.Add(particle);
            }

            if (Segments != null)
            {
                for (int i = 0; i < Segments.Count; i++)
                {
                    Segments[i][0].Position = Projectile.Center + Offsets[i] * 0.7f;
                    Segments[i][0].Locked = true;

                    NoxusBoss.Core.Physics.VerletIntergration.VerletSimulations.TileCollisionVerletSimulation(Segments[i], 4, Vector2.Zero);
                }
            }
        }
        private OvermindDirective DecideDirective()
        {

            return OvermindDirective.Dormant;

        }



    }
}