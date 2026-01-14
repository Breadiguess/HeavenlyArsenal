using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner.Thralls
{
    public class FlowerThrall : BloodThrallBase
    {
        public override ThrallType ThrallType => ThrallType.FlowerThrall;

        public override void UpdateFromOvermind(OvermindContext context)
        {

        }


        public override void SetDefaults()
        {
            Projectile.minion = true;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 2;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }


        public override void AI()
        {

            Projectile.timeLeft = 2;
            Projectile.Center = Vector2.Lerp(Projectile.Center,  Owner.Center + new Vector2(60 * -Owner.direction, 0),0.5f);
        }

        public override void PostAI()
        {
            
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/TwistedBloodBlight/Players/Summoner/Thralls/FlowerPlaceholder").Value;

            Vector2 DrawPos = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(tex, DrawPos, null, Color.White, 0, tex.Size() / 2, 0.4f, 0);
            return false;
        }
    }
}
