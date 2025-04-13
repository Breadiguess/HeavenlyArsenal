using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Rogue
{
    class HeldLifeCessation_StealthStrike : ModProjectile
    {
        public override string Texture => "HeavenlyArsenal/Content/Projectiles/Weapons/Rogue/FlowerShuriken_Proj";//"CalamityMod/Projectiles/InvisibleProj";
        public override void SetStaticDefaults()
        {
            //Projectile.
        }
        public override void SetDefaults()
        {
            Projectile.damage = 40;
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle= 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 4;
        }
        public override void AI()
        {
            Projectile.rotation++;
        }
        public override void OnKill(int timeLeft)
        {
            Main.NewText("I should die");
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Burning, 400, true);
            // KMS target.
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 40;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return base.Colliding(projHitbox, targetHitbox);
        }



        //todo: make actually cool, make actually do something
        //actual functionality of the stealth strike:
        /*
         
                  

        so atm i have a few idea
        some kind of burst where one half of it freezes and the other half burns at 2 trillion kelvin
        some kind of sphere that swaps between freezing and burning
        a huge burst like the ones on the sun or the roche limit
        and then discount rainbow gun
         */
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = GennedAssets.Textures.LoreItems.LoreAvatar;
            Rectangle silly = texture.Frame(1, 1, 0, 0);
            SpriteEffects None = SpriteEffects.None;
            float rot = (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3;
            Vector2 origin = new Vector2(texture.Width/2,texture.Height/2);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, silly, lightColor, rot, origin, Projectile.scale, None, 0);

            return false;
        }

        public override bool? CanDamage()
        {
            return base.CanDamage();
        }
    }
    
}