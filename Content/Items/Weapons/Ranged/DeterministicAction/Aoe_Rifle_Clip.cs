using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    public class Aoe_Rifle_Clip
    {
        public readonly List<Item> Bullets;

        public int BulletCount => Bullets.Count;

        public Aoe_Rifle_Clip(int capacity)
        {
            Bullets = new List<Item>(capacity);
        }
    }
}
