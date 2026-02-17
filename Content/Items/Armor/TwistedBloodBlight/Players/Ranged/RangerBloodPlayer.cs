using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Ranged
{
    internal class RangerBloodPlayer : ModPlayer
    {
        private BloodBlightParasite_Player symbiote;
        public override void PostUpdateMiscEffects()
        {
            symbiote = Player.GetModPlayer<BloodBlightParasite_Player>();

        }
        public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            return base.Shoot(item, source, position, velocity, type, damage, knockback);
        }
    }
}
