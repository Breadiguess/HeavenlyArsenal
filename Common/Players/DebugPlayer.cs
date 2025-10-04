using CalamityMod;
using HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath;
using HeavenlyArsenal.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Common.Players
{
    internal class DebugPlayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Torso);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player Owner = drawInfo.drawPlayer;



            string msg = "";



            msg = $"modStealth: {Owner.Calamity().modStealth} \n"
                + $"rogueStealth: {Owner.Calamity().rogueStealth}\n"
                + $"Stealth Max:{Owner.Calamity().rogueStealthMax * 100}\n"
                + $"StealthAcceleration: {Owner.Calamity().stealthAcceleration}\n"
                + $"{Owner.Calamity().stealthGenMoving}";
            Utils.DrawBorderString(Main.spriteBatch, msg, Owner.Center - Main.screenPosition, Color.AntiqueWhite, 1, 0.2f, -1.2f);
        }
    }
    public class DebugProj : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override void PostDraw(Projectile projectile, Color lightColor)
        {
           // if (!projectile.isAPreviewDummy && projectile.type != ModContent.ProjectileType<VoidCrest_Spear>())
            //    Utils.DrawBorderString(Main.spriteBatch, $"{projectile.whoAmI}", projectile.Center - Main.screenPosition, Color.AntiqueWhite, 1, 0.2f, -0.2f);
        }
    }
}
