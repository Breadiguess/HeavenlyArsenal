using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using Humanizer;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip
{
    public class ViscousWhip_Player: ModPlayer
    {
        public override void PostUpdate()
        {
          
        }
    }
    public class ViscousWhip_Item : ModItem
    {
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(BloodwhipBuff.TagDamage);

        public override string LocalizationCategory => "Items.Weapons.Summon";
        public int SwingStage = 0;
        public bool throwingDaggers;
        public override bool MeleePrefix()
        {
            return true;
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.gunProj[Type] = true;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.channel = true;
            Item.DamageType = DamageClass.SummonMeleeSpeed;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.shootSpeed = 40f;
            Item.knockBack = 3f;
            Item.reuseDelay = 2;
            Item.rare = ModContent.RarityType<Rarities.BloodMoonRarity>();
            Item.value = Item.buyPrice(0, 46, 30, 2);
            Item.shoot = ModContent.ProjectileType<ViscousWhip_Proj>();
            Item.damage = 1200;
            Item.Size = new Vector2(40, 40);
          
            

            Item.DefaultToWhip(ModContent.ProjectileType<ViscousWhip_Proj>(), Item.damage, Item.knockBack, 6.31f, 42);
            Item.shootSpeed = 6.31f;
            Item.UseSound = null;
            Item.autoReuse = true;

            Item.crit = 12;
        }

        public override float UseAnimationMultiplier(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                return 0.5f;
            }
            else
            {
                Item.useAnimation = 42;
                Item.useTime = 42;
            }

            return 1;
            return base.UseAnimationMultiplier(player);
        }
    
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string text = $"[c/FF2C00:Debug:]\n";
            text += $"Swingstage: {SwingStage}";
          
            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                if (BlacklistedProjectiles.BlackListedProjectiles.Contains(projectile.type))
                    continue;

                if (projectile.sentry)
                    continue;


                if (projectile.DamageType != DamageClass.Summon)
                    continue;

                if (projectile.owner != Main.LocalPlayer.whoAmI)
                    continue;
                int Damage = projectile.originalDamage / 4 + projectile.damage / 2;

                text += $"\n {projectile.Name}=> Spit Damage: {Damage}";
            }
            TooltipLine line = new TooltipLine(Mod, "Debug", text);
            tooltips.Add(line);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
           
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            

            if (player.altFunctionUse != 2)
            {
                SoundEngine.PlaySound(SoundID.Item152, player.Center, null);
                Projectile Whip = Projectile.NewProjectileDirect(source, position, velocity, Item.shoot, damage, knockback, ai1: SwingStage);
                SwingStage++;
                if (SwingStage > 2)
                    SwingStage = 0;
            }
            else
            {
                PlaceholderName bloodPlayer;
                player.TryGetModPlayer<PlaceholderName>(out bloodPlayer);
                if(bloodPlayer.blood >= 30)
                { 
                    SoundEngine.PlaySound(SoundID.Item106, player.Center);
                    velocity *= 1.5f;
                    int adjustedDamage = (int)(damage / 3 * 0.33f);
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 Adj = velocity.RotatedBy(MathHelper.PiOver4 * (i / 3f) - MathHelper.ToRadians(15));
                        Projectile Dart = Projectile.NewProjectileDirect(source, position, Adj, ModContent.ProjectileType<BloodDart>(), adjustedDamage, knockback, player.whoAmI);
                        Dart.scale = 0.5f;
                        bloodPlayer.blood -= 10;
                    }

                }
            }
            return false;
        }
        public override bool AltFunctionUse(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ViscousWhip_Proj>()] < 1)
            {
                PlaceholderName bloodPlayer;
                player.TryGetModPlayer<PlaceholderName>(out bloodPlayer);
                if (bloodPlayer.blood >= 30)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

       
    }
}
