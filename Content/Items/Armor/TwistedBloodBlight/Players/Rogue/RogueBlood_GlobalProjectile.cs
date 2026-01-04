using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Rogue
{
    public class RogueBlood_GlobalProjectile : GlobalProjectile
    {
        public override void Load()
        {
            On_Player.ItemCheck_Shoot += StorePlayerShot;
        }

        private void StorePlayerShot(On_Player.orig_ItemCheck_Shoot orig, Player self, int i, Item sItem, int weaponDamage)
        {
            orig(self, i, sItem, weaponDamage);

            self.TryGetModPlayer<BloodBlightParasite_Player>(out var parasite);
            if(parasite != null)
            {
                if (!parasite.Active)
                    return;
                var rogue = parasite.ConstructController as RogueBloodController;
                if (rogue == null)
                    return;

              
                
            }

        }

        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return entity.CountsAsClass(ModContent.GetInstance<RogueDamageClass>()) && entity.type != ModContent.ProjectileType<Rogue_BloodSlashes>() &&lateInstantiation;
        }

        public override void PostAI(Projectile projectile)
        {

        }
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            int damage = 40;
            if(projectile.Calamity().stealthStrike)
            {
                Projectile a = Projectile.NewProjectileDirect(Main.player[projectile.owner].GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<Rogue_BloodSlashes>(), damage, 0);
            }
        }
    }
}
