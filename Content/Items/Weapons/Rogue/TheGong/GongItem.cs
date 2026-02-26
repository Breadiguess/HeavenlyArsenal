using CalamityMod;
using Luminance.Assets;
using NoxusBoss.Content.Rarities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.TheGong
{
    internal class GongItem : ModItem
    {
        public override string Texture => MiscTexturesRegistry.PixelPath;

        public override string LocalizationCategory => "Items.Weapons.Rogue";
        public override void SetDefaults()
        {
            Item.damage = 1000;
            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.useAnimation = 40;
            Item.useTime = 40;
            Item.rare = ModContent.RarityType<AvatarRarity>();
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
        }
    }
}
