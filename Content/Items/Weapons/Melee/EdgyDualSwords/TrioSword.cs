using CalamityMod;
using Luminance.Assets;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords
{
    internal class TrioSword: ModItem
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
            Item.useAnimation = ItemUseStyleID.HiddenAnimation;

        }


    }
}
