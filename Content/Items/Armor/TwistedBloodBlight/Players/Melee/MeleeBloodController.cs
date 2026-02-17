namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Melee
{
    internal class MeleeBloodController : IBloodConstructController
    {
        private readonly BloodBlightParasite_Player symbiote;
        private readonly Player player;
        public MeleeBloodController(BloodBlightParasite_Player symbiote)
        {
            this.symbiote = symbiote;
            this.player = symbiote.Player;
        }

        public List<int> Needles = new List<int>();
        #region helpers
        void CheckNeedles()
        {
            for (int i = 0; i < Needles.Count; i++)
            {
                var proj = (Main.projectile[Needles[i]]);
                if (proj.type != ModContent.ProjectileType<ParasiteNeedle>() || !proj.active)
                {
                    Needles[i] = 0;
                }
            }
        }
        void SpawnNeedles()
        {
            if (Needles.Count !=2)
            {
                Needles.Clear();
                for (int i = 0; i < 2; i++)
                    Needles.Add(0);
            }
            for (int i = 0; i < Needles.Count; i++)
            {
                if (Needles[i] == 0)
                {
                    Needles[i] = Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<ParasiteNeedle>(), 10, 0);
                }
            }
        }
        #endregion
        void IBloodConstructController.OnAscensionStart()
        {

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
            SpawnNeedles();
            CheckNeedles();
        }
    }
}