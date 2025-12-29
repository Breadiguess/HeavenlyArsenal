using Luminance.Assets;
using NoxusBoss.Assets;
using System.Collections.Generic;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner
{
    internal class BloodOvermind : ModProjectile
    {
        public enum OvermindDirective
        {
            Dormant,        // No coordination, thralls act independently
            Assemble,       // Pull thralls inward, regroup
            Pressure,       // Sustained coordinated aggression
            Pulse,          // Timed burst windows (synchronized strikes)
            Frenzy,         // High aggression, sloppy movement
            Collapse        // Loss of coherence (pre-crash / crash)
        }

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public ref Player Owner => ref Main.player[Projectile.owner];

        public OvermindDirective CurrentDirective { get; private set; }

        float ThrallMaxDistance = 460f;
        float PlayerFrenzyDistance = 170f;

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.Size = new Vector2(100, 100);
            Projectile.minion = true;
            Projectile.penetrate = -1;
            Projectile.minionSlots = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Projectile.timeLeft++;
            if (!Owner.active || Owner.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = Owner.Center + new Vector2(0, -100);

            CurrentDirective = DecideDirective();

            switch (CurrentDirective)
            {
                case OvermindDirective.Assemble:

                    break;

                case OvermindDirective.Pressure:

                    break;

                case OvermindDirective.Frenzy:

                    break;
            }


        }
        private OvermindDirective DecideDirective()
        {

            var ThrallIDs = Owner.GetModPlayer<BloodBlightParasite_Player>()
                .ConstructController is SummonerBloodController controller
                ? controller.thrallIDs
                : new List<int>();

            foreach (int id in ThrallIDs)
            {
                if (!Main.projectile.IndexInRange(id))
                    continue;

                Projectile thrall = Main.projectile[id];
                if (!thrall.active)
                    continue;

                float dist = Vector2.Distance(thrall.Center, Projectile.Center);
                if (dist > ThrallMaxDistance)
                    return OvermindDirective.Assemble;
            }

            NPC currentTarget = Main.npc[Projectile.FindTargetWithLineOfSight(900)];
            float playerDist = Vector2.Distance(currentTarget.Center, Owner.Center);
            if (playerDist <= PlayerFrenzyDistance)
                return OvermindDirective.Frenzy;


            return OvermindDirective.Pressure;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D G = GennedAssets.Textures.GreyscaleTextures.Corona;

            Main.EntitySpriteDraw(G, Projectile.Center - Main.screenPosition, null, Color.DarkRed with { A = 0 }, MathHelper.ToRadians(30), G.Size() / 2, 0.2f, 0);

            Utils.DrawBorderString(Main.spriteBatch, CurrentDirective.ToString(), Projectile.Center - Main.screenPosition, Color.White);
            return base.PreDraw(ref lightColor);
        }
    }
}