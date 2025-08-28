using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    public abstract class BloodmoonBaseNPC : ModNPC
    {   

        public override string Texture =>  MiscTexturesRegistry.InvisiblePixel.Path;
        ///<summary>
        /// the current blood in this npc.
        ///<summary/>
        public int blood;
        ///<summary>
        /// the total cap of blood this npc can hold.
        ///<summary/>
        public virtual int bloodBankMax;

        //todo: a target NPC, a target Player (maybe use entity? and just exclude projectiles)
        public Player playerTarget = null;

        public NPC NPCTarget = null;

        public Entity currentTarget = null;
        #region Snackrifice:tm:
        
        /// <summary>
        /// How likely this npc is to recieve a buff when another npc is sacrificed.
        /// </summary>
        public virtual float buffPrio = 0;

        /// <summary>
        /// Determine whether this npc can be sacrificed. 
        /// </summary>
        public virtual bool canBeSacrificed;
        
        
        #endregion
    }
}
