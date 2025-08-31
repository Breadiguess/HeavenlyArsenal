using CalamityMod.Particles;
using HeavenlyArsenal.Common;
using Luminance.Assets;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ColdFusion
{
    class VolatileExplosion : ModProjectile
    {

        public ref float Time => ref Projectile.ai[0];
       
        public ref float IsChaining => ref Projectile.ai[2];

        public bool Chaining => IsChaining == 1 ? false : true;
        public int VolDamage
        {
            get;
            set;
        }
        // TODO: set the owner upon creation
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = Projectile.height = 600;
           
            Projectile.velocity = Vector2.Zero;
            
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = Projectile.timeLeft = 60;
            Projectile.penetrate = -1;

        }
        public ref Terraria.Player Owner => ref Main.player[Projectile.owner];
        public bool triggered;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //TODO: make it so that it doesnt instantly trigger the next volatile explosion, because it would be fun to make them happen sequentially.
            triggered = true;
            if (target.GetGlobalNPC<VolatileRounds>().VolatileActive)
            {
                target.GetGlobalNPC<VolatileRounds>().ChainExplosion(Owner, target, damageDone, 600, Chaining);
            }
            
            base.OnHitNPC(target, hit, damageDone);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitNPC(target, ref modifiers);
        }
        public override void OnSpawn(IEntitySource source)
        {
            triggered = false;
            Time = 0;
            SoundEngine.PlaySound(GennedAssets.Sounds.Common.Twinkle);
            // see todo above Owner = source.
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CanDistortWater[Projectile.type] = true;
            ProjectileID.Sets.CanHitPastShimmer[Projectile.type] = true;
            
        }

        public override void AI()
        {
            if(Time == 0)
            {
                for(int i = 0; i< 50; i++)
                    Dust.NewDust(Projectile.Center, 0, 0, DustID.Shadowflame, 0, 0, 100, default, 10);
                Particle pulse = new DirectionalPulseRing(Projectile.Center - Projectile.velocity, Projectile.velocity / 1.5f, Color.Fuchsia, new Vector2(5f, 5f), Projectile.velocity.ToRotation(), 0.82f, 0.32f, 60);
                GeneralParticleHandler.SpawnParticle(pulse);
            }
            Time++;
        }
        public override bool PreAI()
        {
            return true;
        }
        public override bool PreDraw(ref Color lightColor)
        {

            Utils.DrawBorderString(Main.spriteBatch, "Volatile Cooldown: " + triggered.ToString(), Projectile.Center - Vector2.UnitY * 220 - Main.screenPosition, Color.White);

            return base.PreDraw(ref lightColor);
        }
        public override bool? CanCutTiles() => false;
        public override bool? CanDamage() => !triggered;

    }
}
