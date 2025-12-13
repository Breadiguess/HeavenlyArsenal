using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech
{
    public class LeechSystem : ModSystem
    {
       
        public const int SlotWidth = 1000;  
        public const int SlotHeight = 64;   
        public static int MaxLeeches
        {
            get => ActiveLeeches.Count>0? (ActiveLeeches.Count) : 0;
        }
        
        public static int ActiveIndex(NPC npc)
        {
            return ActiveLeeches.IndexOf(npc);
        }

        public static void AddLeech(NPC npc)
        {
            ActiveLeeches.Add(npc);
        }
        public static void RemoveLeech(NPC npc)
        {
            ActiveLeeches.Remove(npc);
        }
       
        private static List<NPC> ActiveLeeches = new List<NPC>(Main.maxNPCs);
        public override void OnWorldLoad()
        {
            ActiveLeeches.Clear();
        }
        public override void OnWorldUnload()
        {
            ActiveLeeches.Clear();
        }
        public override void PostUpdateNPCs()
        {
            //ActiveLeeches.RemoveWhere(npc => npc == null || !npc.active || npc.ModNPC is not newLeech);
            ActiveLeeches.RemoveAll(npc => npc == null || !npc.active || npc.ModNPC is not newLeech);
        }

    }
    public class leechSystemHelper : GlobalNPC
    {
        public int StripSlot = -1;
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return (entity.ModNPC is newLeech);
        }
        public override bool InstancePerEntity => true;
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

            StripSlot = LeechSystem.ActiveIndex(npc);
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if(npc.type == ModContent.NPCType<newLeech>())
                LeechSystem.AddLeech(npc);

            StripSlot = LeechSystem.ActiveIndex(npc);
        }
    }
}
