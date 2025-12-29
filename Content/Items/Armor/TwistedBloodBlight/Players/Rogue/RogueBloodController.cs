namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Rogue
{
    internal class RogueBloodController : ModPlayer, IBloodConstructController
    {
        private readonly BloodBlightParasite_Player symbiote;
        private readonly Player player;
        public RogueBloodController(BloodBlightParasite_Player symbiote)
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