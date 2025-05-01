using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Content.Projectiles.Weapons;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons
{
    class DebugWeapon :ModItem
    {
        public override string Texture => "HeavenlyArsenal/Content/Projectiles/Misc/ChaliceOfFun_Juice";
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.NightsEdge);
            Item.damage = 40939;
            Item.DamageType = DamageClass.Generic;
            
           // Item.shoot = ModContent.ProjectileType<EyeOfTranscendenceProjectile>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Rift darkParticle = Rift.pool.RequestParticle();
            darkParticle.Prepare(player.Center + player.velocity, player.velocity, Color.AntiqueWhite, new Vector2(39, 39), player.fullRotation, 3, 3, 300);


            ParticleEngine.Particles.Add(darkParticle);
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }

    }
}
