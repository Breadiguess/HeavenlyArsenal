using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner.Thralls
{
    public enum ThrallOrderType
    {
        Idle,
        Assemble,
        Pressure,
        Frenzy,
        Retreat,
        ProtectCore,
        Die
    }
    public enum ThrallType
    {
        BasicThrall0,
        BasicThrall1,
        BasicThrall2,
        WingedThrall,
        FlowerThrall,
        HydraThrall,
        NerveWormThrall
    }
    public readonly struct ThrallOrder
    {
        public readonly ThrallOrderType Type;
        public readonly Vector2 TargetPosition;
        public readonly int Priority;

        public ThrallOrder(
            ThrallOrderType type,
            Vector2 targetPosition = default,
            int priority = 0)
        {
            Type = type;
            TargetPosition = targetPosition;
            Priority = priority;
        }
    }

    public interface IBloodThrall
    {

        ThrallType ThrallType { get; }

        int ProjectileWhoAmI { get; }

        bool IsAlive { get; }
        bool IsReady { get; }

        Vector2 Position { get; }
        Vector2 Velocity { get; }


        void AssignOvermind(BloodOvermind overmind);

        void ReceiveOrder(ThrallOrder order);
        void UpdateFromOvermind(OvermindContext context);
    }

}
