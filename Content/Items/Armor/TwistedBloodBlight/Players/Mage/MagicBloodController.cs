namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Mage
{
    internal class MagicBloodController : IBloodConstructController
    {

        private readonly BloodBlightParasite_Player symbiote;
        private readonly Player player;
        public MagicBloodController(BloodBlightParasite_Player symbiote)
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

        void IBloodConstructController.OnPurge()
        {
            
        }

        void IBloodConstructController.Update(Player player)
        {
            
        }
    }
}