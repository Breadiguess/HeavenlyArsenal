using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players
{
    public interface IBloodConstructController
    {
        void Update(Player player);
        void OnBandChanged(BloodBand newBand);
        void OnAscensionStart();
        void OnPurge();
        void OnCrash();
    }
}
