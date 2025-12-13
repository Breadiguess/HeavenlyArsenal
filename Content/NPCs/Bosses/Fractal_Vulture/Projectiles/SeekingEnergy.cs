using Luminance.Assets;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles
{
    internal class SeekingEnergy : ModProjectile
    {
        public NPC Impaled;
        public NPC Owner;
        public Player target;
        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.Size = new Vector2(30, 30);
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (Owner.As<voidVulture>().currentState != voidVulture.Behavior.placeholder2)
                Projectile.active = false;
            Projectile.Center = Impaled.Center;
            Time++;
            Projectile.timeLeft++;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Utils.DrawLine(Main.spriteBatch, Owner.As<voidVulture>().HeadPos, Projectile.Center, Color.AntiqueWhite);
            return base.PreDraw(ref lightColor);
        }
    }
}
