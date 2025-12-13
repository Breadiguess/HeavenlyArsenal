using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.FleshlingCultist
{
    internal class MaskProj : ModProjectile
    {
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/FleshlingCultist/FleshlingCultistMask";
        public bool Fragment
        {
            get => Projectile.ai[0] >= 1f;
            set => Projectile.ai[0] = value ? 1f : 0f;

        }
        public int variant
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.damage = 0;
            Projectile.timeLeft = 450;
            if (Fragment)
                Projectile.timeLeft = 300;
            Projectile.Size = new(10, 10);

        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.netUpdate = true;
            if (Fragment)
            {
                Projectile.frame = variant;
            }
            else
            {
                Projectile.frame = 0;
            }
            Projectile.netUpdate = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.Length();
            Projectile.velocity.Y = float.Lerp(Projectile.velocity.Y, Projectile.velocity.Y + 6,0.1f);
            if (Fragment)
            {
                Projectile.velocity.X *= 0.97f;
                if (Projectile.velocity.Length() < 1)
                    if (Projectile.alpha < 255)
                    {
                        Projectile.alpha  += 1;
                    }
                    else
                        Projectile.Kill();
                        
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!Fragment)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2Circular(10, 10);
                    int frame = Main.rand.Next(1, 4); // pick random fragment variant (adjust range as needed)

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos,
                        Main.rand.NextVector2Circular(3, 3) * 3,
                        ModContent.ProjectileType<MaskProj>(),
                        0,
                        0f,
                        -1,
                        1f, // ai[0] = Fragment = true
                        frame // ai[1] = variant
                    );

                }
                for (int d = 0; d < 10; d++)
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Glass, newColor: Color.AntiqueWhite);
            }
               
            else
                return false;
            return base.OnTileCollide(oldVelocity);
        }
    }
}
