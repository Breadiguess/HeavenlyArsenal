using HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.SwordData;
using Luminance.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.Projectiles
{
    public abstract class BaseSwordProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            
        }
        public override string Texture =>  MiscTexturesRegistry.InvisiblePixelPath;
        public SwordAttackDef AttackDef;
        protected SwordAttackDef Attack;
        public ref Player Owner => ref Main.player[Projectile.owner];
        public override void OnSpawn(IEntitySource source)
        {
            AttackDef = SwordAttackDatabase.Attacks[(SwordAttackID)Projectile.ai[0]];
        }

        public override void AI()
        {
            HandleMovement();
            HandleLifetime();
        }

        protected virtual void HandleMovement() { }
        protected virtual void HandleLifetime() { }



    }

}
