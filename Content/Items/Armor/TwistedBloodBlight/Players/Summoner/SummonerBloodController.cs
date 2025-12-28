using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner
{
    public class SummonerBloodController : IBloodConstructController
    {
        private readonly BloodBlightParasite_Player symbiote;
        private readonly Player player;
        public SummonerBloodController(BloodBlightParasite_Player symbiote)
        {
            this.symbiote = symbiote;
            this.player = symbiote.Player;
        }

        // Thrall tracking
        private readonly List<int> thrallIDs = new();

        // Limits
        private int maxThralls = 0;

        // State
        private BloodBand currentBand;
        private bool ascended;

        public bool overmindActive;
        int overmindID;

        #region helpers
        private void CleanupDeadThralls()
        {
            thrallIDs.RemoveAll(id =>
                !Main.projectile.IndexInRange(id) ||
                !Main.projectile[id].active);
        }

        private void KillAllThralls()
        {
            foreach (int id in thrallIDs)
            {
                if (Main.projectile.IndexInRange(id))
                    Main.projectile[id].Kill();
            }

            thrallIDs.Clear();
        }



        private void SpawnThrall()
        {
            if( thrallIDs.Count >= maxThralls)
                return;
            if (player.whoAmI != Main.myPlayer)
                return;

            Vector2 spawnPos = player.Center + Main.rand.NextVector2Circular(40f, 40f);

            int proj = Projectile.NewProjectile(
                player.GetSource_FromThis(),
                spawnPos,
                Vector2.Zero,
                ModContent.ProjectileType<BloodThrallProjectile>(),
                symbiote.GetThrallDamage(),
                0f,
                player.whoAmI
            );

            thrallIDs.Add(proj);
        }


        private void FormOvermind()
        {
            if (thrallIDs.Count == 0 || overmindActive)
                return;

            overmindActive = true;

            if (player.whoAmI == Main.myPlayer)
            {
                overmindID = Projectile.NewProjectile(
                    player.GetSource_FromThis(),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<BloodOvermind>(),
                    0,
                    0f,
                    player.whoAmI
                );
            }
        }
        #endregion

        void IBloodConstructController.OnAscensionStart()
        {
            FormOvermind();
        }

        void IBloodConstructController.OnBandChanged(BloodBand newBand)
        {
            
            currentBand = newBand;

            switch (newBand)
            {
                case BloodBand.Low:
                    maxThralls = 0;
                    KillAllThralls();
                    break;

                case BloodBand.MidLow: // 30–50%
                    maxThralls = 2;
                    break;

                case BloodBand.MidHigh: // 50–70%
                    maxThralls = 4;
                    break;

                case BloodBand.High: // 70–100%
                    maxThralls = 6;
                    break;
            }
        

        }

        void IBloodConstructController.OnCrash()
        {
            KillAllThralls();
            Main.projectile[overmindID].active = false;
        }
        void IBloodConstructController.OnPurge()
        {

        }
        void IBloodConstructController.Update(Player player)
        {
            CheckOvermind();
            CleanupDeadThralls();
            SpawnThrall();
        }

        private void CheckOvermind()
        {
            if (overmindActive)
            {
                overmindActive = Main.projectile[overmindID].active;
            }
        }
    }

}
