using NoxusBoss.Core.AdvancedProjectileOwnership;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    internal class Aoe_Rifle_GlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return true;
        }

        public int AoeRifle_HitCount;
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.type != ModContent.ProjectileType<Aoe_Rifle_Laser>())
                return;

            if(AoeRifle_HitCount > 2)
            {
                Player Owner = Main.player[projectile.owner];
                npc.NewProjectileBetter(Owner.GetSource_FromThis(), npc.Center, npc.AngleFrom(Owner.Center).ToRotationVector2(), ModContent.ProjectileType<Aoe_Rifle_RealityTear>(), 10_000, 0, Owner.whoAmI);
            }
            AoeRifle_HitCount++;


        }
        public void CreateRealityTear(Player Owner, NPC npc)
        {

        }
    }
}
