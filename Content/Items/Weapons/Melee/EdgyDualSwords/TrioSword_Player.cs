using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords
{
    public class TrioSword_Player : ModPlayer
    {
       



        public override void Load()
        {
            
        }

        

        public override void PostUpdateMiscEffects()
        {
            
        }









        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            base.SyncPlayer(toWho, fromWho, newPlayer);
        }
        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            base.SendClientChanges(clientPlayer);
        }
        public override void CopyClientState(ModPlayer targetCopy)
        {
            base.CopyClientState(targetCopy);
        }
        
    }
}
