using CalamityMod;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner.Thralls;
using System.Collections.Generic;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Rogue
{
    public class RogueBloodController : IBloodConstructController
    {
        private readonly BloodBlightParasite_Player symbiote;
        private readonly Player player;
        private BloodBand currentBand;
        private float StealthBoost;

        public List<int> BloodPhantoms { get; set; }
        public RogueBloodController(BloodBlightParasite_Player symbiote)
        {
            this.symbiote = symbiote;
            this.player = symbiote.Player;
            BloodPhantoms = new List<int>(5);

        }


        public void CreateBloodPhantom()
        {
            if (BloodPhantoms.Count >= BloodPhantoms.Capacity)
                return;

            var a = Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<BloodPhantom>(), 0, 0);
           
            BloodPhantoms.Add(a.whoAmI);

        }
        private void CheckPhantoms()
        {
            // Iterate backwards to safely remove entries while enumerating.
            for (int idx = BloodPhantoms.Count - 1; idx >= 0; idx--)
            {
                int projId = BloodPhantoms[idx];

                // Remove if out of valid range.
                if (projId < 0 || projId >= Main.maxProjectiles)
                {
                    BloodPhantoms.RemoveAt(idx);
                    continue;
                }

                Projectile p = Main.projectile[projId];

                // If the projectile is not active or is not a BloodPhantom, remove its id.
                if (p == null || !p.active || p.type != ModContent.ProjectileType<BloodPhantom>())
                {
                    BloodPhantoms.RemoveAt(idx);
                }
            }
        }
        void IBloodConstructController.OnAscensionStart()
        {

        }

        void IBloodConstructController.OnBandChanged(BloodBand newBand)
        {
            currentBand = newBand;

            switch (newBand)
            {
                case BloodBand.Low:
                    StealthBoost = 0.0f;
                    CreateBloodPhantom();
                    return;

                case BloodBand.MidLow:

                    CreateBloodPhantom();
                    StealthBoost = 0.2f;
                    break;

                case BloodBand.MidHigh:
                    CreateBloodPhantom();
                    StealthBoost = 0.6f;
                    break;

                case BloodBand.High:
                    CreateBloodPhantom();
                    CreateBloodPhantom();
                    StealthBoost = 1.2f;
                    break;
            }
        }

        void IBloodConstructController.OnCrash()
        {

        }

        void IBloodConstructController.OnPurge()
        {

        }

        void IBloodConstructController.Update(Player player)
        {
            CheckPhantoms();
            player.Calamity().rogueStealthMax += StealthBoost;

        }


    }
}