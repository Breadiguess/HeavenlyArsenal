using CalamityMod;
using HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.Projectiles;
using HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.SwordData;
using Luminance.Assets;
using NoxusBoss.Content.Rarities;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords
{
    public class TrioSword_Item: ModItem
    {
        public override string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.gunProj[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.rare = ModContent.RarityType<AvatarRarity>();
            Item.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
            Item.useAnimation = ItemUseStyleID.HiddenAnimation;
            Item.damage = 4_000;

            Item.crit = 6;

            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            //gets replaced in the shoot code but 
            Item.shoot = 1;
            Item.shootSpeed = 12;
            Item.useStyle = ItemUseStyleID.HiddenAnimation;
        }

        public override float UseSpeedMultiplier(Player player)
        {
            if (player.TryGetModPlayer<SwordCombatPlayer>(out var swordPlayer))
            {
                if (swordPlayer.QueuedAttack is SwordAttackID attackID)
                {
                    var attack = SwordAttackDatabase.Attacks[attackID];



                    return attack.UseTimeMultiplier;
                }



                
            }
            return base.UseSpeedMultiplier(player);
        }

        public override void HoldItem(Player player)
        {
            player.GetModPlayer<SwordCombatPlayer>().Active = true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.TryGetModPlayer<SwordCombatPlayer>(out var swordPlayer))
            {
                if (swordPlayer.QueuedAttack is not SwordAttackID attackID)
                    return false;

                var attack = SwordAttackDatabase.Attacks[attackID];

                Projectile proj = Projectile.NewProjectileDirect(
                    source,
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<TrioSwordProjectile>(),
                    damage,
                    knockback,
                    player.whoAmI
                );

                float attackSpeed = player.GetWeaponAttackSpeed(Item);

                float adjustedAttackSpeed = MathF.Max(attackSpeed * 0.6f, 1);
                Main.NewText($"base:{attackSpeed}, Adjusted: {adjustedAttackSpeed}");
                int adjustedLifetime = Math.Max(1, (int)(attack.ProjectileLifeTime / adjustedAttackSpeed));


                Main.NewText(adjustedLifetime);
                proj.timeLeft = adjustedLifetime;

                var swordProj = proj.As<TrioSwordProjectile>();
                swordProj.AttackDef = attack;
                swordProj.CurrentID = attackID;
            }

            return false;
        }
    }
}
