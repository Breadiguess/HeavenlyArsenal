using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace HeavenlyArsenal.Common
{
    public interface IMultiSegmentNPC
    {
        /// <summary>
        /// For the love of all networking DO NOT modify the amount of items in this list after SetDefaults, prepare all hitboxes on npc innitialization, use <see cref="ExtraNPCSegment.Active"/> to enable and disable hitboxes as needed.
        /// </summary>
        /// <returns></returns>
        public ref List<ExtraNPCSegment> ExtraHitBoxes();
        public void OnHitBoxCollide(int WhoAmI, Projectile origin)
        {
                
        }
        public void UpdateSegments()
        {
            foreach (ExtraNPCSegment hitbox in ExtraHitBoxes())
            {
                if (hitbox.ImmuneTime > 0)
                    hitbox.ImmuneTime--;
            }
        }
        public void NetSendExtraHitboxes(BinaryWriter writer)
        {
            foreach (ExtraNPCSegment hitbox in ExtraHitBoxes())
            {
                writer.Write(hitbox.ItemCollide);
                writer.Write(hitbox.ProjectileCollide);
                writer.Write(hitbox.ImmuneTime);
                writer.Write(hitbox.Active);
            }
        }
        public void NetReceiveExtraHitboxes(BinaryReader reader)
        {
            foreach (ExtraNPCSegment hitbox in ExtraHitBoxes())
            {
                hitbox.ItemCollide = reader.ReadBoolean();
                hitbox.ProjectileCollide = reader.ReadBoolean();
                hitbox.ImmuneTime = reader.ReadInt32();
                hitbox.Active = reader.ReadBoolean();
            }
        }
    }
    public class ExtraNPCSegment
    {
        public Rectangle Hitbox;
        public bool UniqueIframes;
        public bool DealsDamage;
        public int Immunity;
        public bool ItemCollide;
        public bool ProjectileCollide;
        public int ImmuneTime;
        public bool Active;

        public ExtraNPCSegment(Rectangle hitbox, bool dealsDamage = true, bool itemCollide = true, bool projectileCollide = true, bool uniqueIframes = false, int immunity = 60)
        {
            
            Hitbox = hitbox;
            UniqueIframes = uniqueIframes;
            DealsDamage = dealsDamage;
            ItemCollide = itemCollide;
            ProjectileCollide = projectileCollide;
            Immunity = immunity;
            Active = true;
        }
    }

}
