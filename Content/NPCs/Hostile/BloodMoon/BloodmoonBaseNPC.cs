using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon
{
    internal class BlackListProjectileNPCs : ModSystem
    {
        //blacklisted NPCs are to be ignored as potential targets.
        private static HashSet<int> BlackListedNPCs = new HashSet<int>();

        //todo: create a modsystem that does this for us, and then write it back to this npc upon loading the world or some shit
        
        public override void PostSetupContent()
        {
            //doubtless doesn't work, but you know what who gaf, i'm writing in github i can fix it later.
            //the whole point is to have something in place already to work off of.
            for (int i = 0; i < NPCLoader.NPCCount; i++)
            {
                
                if (NPCID.Sets.ProjectileNPC[i] || Npc[i] )
                {
                    BlackListedNPCs.Add(i);
                }
            }
            BlackListedNPCs.Add(Noxusboss.Solyn.Type);
            BlackListedNPCs.Add(Noxusboss.Rift.Type);
            
            BlackListedNPCs.Add(CalamityMod.SuperDummy.Type);            
        }
    }
    public abstract class BloodmoonBaseNPC : ModNPC
    {   
        public override string Texture =>  MiscTexturesRegistry.InvisiblePixel.Path;
    
      
        
        ///<summary>
        /// the current blood in this npc.
        ///</summary>
        public int blood;
        ///<summary>
        /// the total cap of blood this npc can hold.
        ///</summary>
        public virtual int bloodBankMax = 100;

        //todo: a target NPC, a target Player (maybe use entity? and just exclude projectiles)
        public Player playerTarget = null;

        public NPC NPCTarget = null;

        public Entity currentTarget = null;
        
        #region Snackrifice:tm:
        /// <summary>
        /// How likely this npc is to recieve a buff compared to it's neighbors when another npc is sacrificed.
        /// </summary>
        public virtual float buffPrio = 0;

        /// <summary>
        /// Determine whether this npc can be sacrificed.
        /// this is a virtual because I feel like the ability to be sacrificed should be adjustable.
        /// </summary>
        public virtual bool canBeSacrificed;

        ///<summary>
        /// calculate the value of the sacrificed npc. we'll later multiply this depending on their value, but im coding blind at the time of writing this, so that should best be left for later.
        ///</summary>
        public virtual float calculateSacrificeValue(NPC npc)
        {
            float bloodPercent = blood / bloodBankMax;
            float lifePercent = npc.life / (float)npc.lifeMax;
            
            float value = Utils.Clamp(bloodPercent + lifePercent, 0, 1);
            return value;
        }
        #endregion
    }
}
