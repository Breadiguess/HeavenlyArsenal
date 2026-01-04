using Luminance.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner.Thralls
{
    public abstract class BloodThrallBase: ModProjectile, IBloodThrall
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public abstract ThrallType ThrallType { get; }

        public virtual ref Player Owner => ref Main.player[Projectile.owner];
        public BloodOvermind Overmind { get; private set; }
        protected ThrallOrder CurrentOrder;

        public int ProjectileWhoAmI => Projectile.whoAmI;

        public bool IsAlive => Projectile.active;
        public virtual bool IsReady => true;
        public Vector2 Position => Projectile.Center;
        public Vector2 Velocity => Projectile.velocity;

        public void AssignOvermind(BloodOvermind overmind)
            => Overmind = overmind;

        public virtual void ReceiveOrder(ThrallOrder order)
        {
            CurrentOrder = order;
            OnOrderReceived(order);
        }

        protected virtual void OnOrderReceived(ThrallOrder order) { }

        public abstract void UpdateFromOvermind(OvermindContext context);
    }

}
