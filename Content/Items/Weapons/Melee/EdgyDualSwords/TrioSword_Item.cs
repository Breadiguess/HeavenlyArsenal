using CalamityMod;
using HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.SwordData;
using Luminance.Assets;
using NoxusBoss.Content.Rarities;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords
{
    public class TrioSword_Item: ModItem
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.rare = ModContent.RarityType<AvatarRarity>();
            Item.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
            Item.useAnimation = ItemUseStyleID.HiddenAnimation;
            Item.damage = 4_000;

            Item.crit = 6;

            Item.useTime = 1;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.shoot = ProjectileID.None; 
        }


       
        public override void HoldItem(Player player)
        {
            player.GetModPlayer<SwordCombatPlayer>().Active = true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage,float knockback)
        {
            var swordPlayer = player.GetModPlayer<SwordCombatPlayer>();

            if (swordPlayer.QueuedAttack is not SwordAttackID attackID)
                return false;

            var attack = SwordAttackDatabase.Attacks[attackID];

            Projectile.NewProjectile(
                source,
                player.Center,
                Vector2.Zero,
                attack.ProjectileType,
                damage,
                knockback,
                player.whoAmI,
                ai0: (float)attackID
            );

            swordPlayer.QueuedAttack = null;
            return false; 
        }

    }
}
