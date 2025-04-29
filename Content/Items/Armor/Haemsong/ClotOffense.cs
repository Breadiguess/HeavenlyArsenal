using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.Haemsong
{
    public class ClotOffense : ModProjectile
    {
        private Projectiles.globalHomingAI HomingAI => Projectile.GetGlobalProjectile<Projectiles.globalHomingAI>();
        public override bool PreDraw(ref Color lightColor)
        {
            for (int i = 0; i < 5; i++)
            {
                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.oldPos[i] - Main.screenPosition, Projectile.getRect(), new Color(255, 255, 255, 255 - i * 51), Projectile.oldRot[i], Projectile.Size / 2f, 1, Microsoft.Xna.Framework.Graphics.SpriteEffects.None);
            }
            return true;
        }
        public override void AI()
        {
            if (Projectile.timeLeft == 255)
            {
                HomingAI.enabled = true;
                Projectile.friendly = true;
            }
            Projectile.damage = (int)Main.player[Projectile.owner].GetTotalDamage(DamageClass.Generic).ApplyTo(400);
        }
        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath39, Projectile.Center);
            }
        }
        public override void SetDefaults()
        {
            HomingAI.agility = 3;
            HomingAI.decel = 1.1f;
            HomingAI.wallHack = true;

            Projectile.extraUpdates = 1;
            Projectile.height = 22;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.width = 34;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
    }
}
