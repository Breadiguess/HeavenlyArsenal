namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Ranged
{
    internal class RangerBloodController : IBloodConstructController
    {
        private readonly BloodBlightParasite_Player symbiote;
        private readonly Player player;
        public RangerBloodController(BloodBlightParasite_Player symbiote)
        {
            this.symbiote = symbiote;
            this.player = symbiote.Player;
        }

        void IBloodConstructController.OnAscensionStart()
        {

        }

        void IBloodConstructController.OnBandChanged(BloodBand newBand)
        {
            
        }

        void IBloodConstructController.OnCrash()
        {
            
        }
        public void OnPurge()
        {
            
        }


        void IBloodConstructController.Update(Player player)
        {
            
        }
    }
}