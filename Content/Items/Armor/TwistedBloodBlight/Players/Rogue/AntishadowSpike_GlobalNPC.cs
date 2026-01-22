using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Rogue
{
    internal class AntishadowSpike_GlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return !entity.friendly && lateInstantiation;
        }

        public List<int> SpikeCount = new List<int>(10);
        public override void PostAI(NPC npc)
        {
            
        }
        public sealed override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            
        }
        public sealed override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.owner != -1)
            {
                if (Main.player[projectile.owner].TryGetModPlayer<BloodBlightParasite_Player>(out var parasite))
                {
                    if (parasite != null)
                    {
                        RogueBloodController controller = parasite.ConstructController as RogueBloodController;
                        if(controller != null)
                        {

                        }
                    }
                }
            }
        }
    }
}
