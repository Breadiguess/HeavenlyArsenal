using System.Collections.Generic;
using Terraria.GameContent.Animations;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Mage
{
    public class MagicBloodController : ModPlayer, IBloodConstructController
    {
        public override void Load()
        {
            On_Player.ItemCheck_Shoot += StoreShootStats;
            On_Player.GetManaCost += On_Player_GetManaCost;
        }

        private static void StoreShootStats(On_Player.orig_ItemCheck_Shoot orig, Player self, int i, Item sItem, int weaponDamage)
        {
            orig(self, i, sItem, weaponDamage);
            if(self.GetModPlayer<MagicBloodController>().symbiote.ConstructController == self.GetModPlayer<MagicBloodController>())
            {

            }
        }

        private static int On_Player_GetManaCost(On_Player.orig_GetManaCost orig, Player self, Item item)
        {
            return orig(self, item);
        }

       


        private readonly BloodBlightParasite_Player symbiote;
        private readonly Player player;
        public List<int> CloneIDs = new List<int>(2);
        public MagicBloodController(BloodBlightParasite_Player symbiote)
        {
            this.symbiote = symbiote;
            this.player = symbiote.Player;
        }

        void IBloodConstructController.OnAscensionStart()
        {
            for(int i = 0; i< 2; i++)
            {
                Projectile a = Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<BloodEcho_Projectile>(), 1,1);
                a.As<BloodEcho_Projectile>().Index = i;
                CloneIDs.Add(a.whoAmI);
            }
        }

        void IBloodConstructController.OnBandChanged(BloodBand newBand)
        {
            
        }

        void IBloodConstructController.OnCrash()
        {
            
        }

        void IBloodConstructController.OnPurge()
        {
            
        }

        void IBloodConstructController.Update(Player player)
        {
            ClearDeadclones();
        }

        #region Helpers

        void ClearDeadclones()
        {
            CloneIDs.RemoveAll(id => !Main.projectile.IndexInRange(id) || !Main.projectile[id].active || Main.projectile[id].type != ModContent.ProjectileType<BloodEcho_Projectile>());
        }

        #endregion
    }
}