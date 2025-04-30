using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.Haemsong
{
	[AutoloadEquip(EquipType.Head)]
	public class BloodArmorHead : ModItem
	{

		public override void SetDefaults()
        {
            Item.defense = 32;
            Item.height = 28;
            Item.rare = ItemRarityID.Red;
            Item.value = 200000;
			Item.width = 38;
        }

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ModContent.ItemType<BloodArmorBody>() && legs.type == ModContent.ItemType<BloodArmorLegs>();
		}

		public override void UpdateArmorSet(Player player)
		{
            player.GetModPlayer<BloodPlayer>().fullBloodArmor = true;
			player.setBonus = "Fuck you";
			player.maxMinions += 3;
			if (player.GetModPlayer<BloodPlayer>().offenseMode)
			{
				player.GetArmorPenetration(DamageClass.Generic) += 50;
				player.GetAttackSpeed(DamageClass.Generic) += 0.3f;
				player.GetCritChance(DamageClass.Generic) += 46;
				player.GetDamage(DamageClass.Generic) += 0.6f;
                player.GetDamage(DamageClass.Summon) += (player.maxMinions - player.slotsMinions) * 0.2f;
				player.moveSpeed += 0.2f;
                player.statDefense *= 0.8f;
				player.statManaMax2 += 200;
			}
			else
			{
				player.aggro += 900;
                player.endurance += player.endurance * (1 - player.endurance);
                player.statDefense *= 1.25f;
				player.statLifeMax2 += 200;
				player.moveSpeed -= 0.2f;
			}
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D which = TextureAssets.Item[Type].Value;
            Rectangle sourceRect = which.Frame(1, 2, 0, Main.LocalPlayer.GetModPlayer<BloodPlayer>().offenseMode ? 0 : 1);
            spriteBatch.Draw(which, position, sourceRect, drawColor, 0, origin, scale * 2, SpriteEffects.None, 0);
            return false;
        }
    }
	public class BloodPlayer : ModPlayer
    {
        public int bloodExpiration;
        public bool canParry = true;
        public bool fullBloodArmor;
        public bool offenseMode = true;
		public List<int> blood = [];
        bool bloodBoost = false;
        bool canGetBlood = true;
        int boostDecay = 0;
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<BloodTentacle>() && proj.ai[0] < 2)
            {
                int bloodInsert = blood.Count;
                while (blood.Count > 0 && bloodInsert > 0 && blood[bloodInsert - 1] < 0)
                {
                    bloodInsert--;
                }
                if (bloodBoost)
                {
                    if (offenseMode)
                    {
                        Player.statLife += damageDone / 25;
                        Player.HealEffect(damageDone / 25);
                    }
                    blood.Insert(bloodInsert, 0);
                }
                else if (canGetBlood)
                {
                    blood.Insert(bloodInsert, 0);
                }
            }
        }
        public override void PostUpdateMiscEffects()
        {
			int poonCount = Player.ownedProjectileCounts[ModContent.ProjectileType<BloodTentacle>()];
            if (fullBloodArmor)
            {
                if (bloodBoost)
                {
                    if (offenseMode)
                    {
                        Player.GetAttackSpeed(DamageClass.Generic) += 0.3f;
                        Player.GetCritChance(DamageClass.Generic) = 100;
                        Player.GetDamage(DamageClass.Generic) += 0.6f;
                    }
                    else
                    {
                        Player.endurance += Player.endurance * (1 - Player.endurance);
                        Player.statDefense *= 1.5f;
                    }
                    if (++boostDecay % 3 == 0)
                    {
                        blood.RemoveAt(0);
                    }
                    if (boostDecay % 12 == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, new Vector2(5).RotatedByRandom(Math.PI), ModContent.ProjectileType<BloodTentacle>(), 100, 5, Player.whoAmI, 3);
                        }
                        boostDecay = 0;
                    }
                    if (blood.Count == 0)
                    {
                        bloodBoost = false;
                        canGetBlood = true;
                        if (Main.netMode != NetmodeID.Server)
                        {
                            SoundEngine.PlaySound(SoundID.Item57, Player.Center);
                        }
                    }
                }
                else
                {
                    if (offenseMode)
                    {
                        bloodExpiration = 720;
                    }
                    else
                    {
                        bloodExpiration = 360;
                    }
                    for (int i = 0; i < blood.Count; i++)
                    {
                        blood[i]++;
                    }
                    if (blood.Count > 99)
                    {
                        bloodBoost = true;
                        canGetBlood = false;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            while (offenseMode && blood[9] > bloodExpiration)
                            {
                                blood.RemoveRange(0, 10);
                                Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, new Vector2(Main.rand.NextFloat(0, 15)).RotatedByRandom(MathHelper.Pi), ModContent.ProjectileType<ClotOffense>(), 800, 10, Player.whoAmI);
                            }
                            int clotAmount = 0;
                            while (blood[0] > bloodExpiration)
                            {
                                blood.RemoveAt(0);
                                clotAmount++;
                            }
                            if (!offenseMode)
                            {
                                Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Player.DirectionTo(Main.MouseWorld) * 15, ModContent.ProjectileType<BloodTentacle>(), 400, 10, Player.whoAmI, 2, clotAmount);
                            }
                        }
                        if (Main.netMode != NetmodeID.Server)
                        {
                            SoundEngine.PlaySound(SoundID.NPCDeath10, Player.Center);
                        }
                    }
                }
                if (Main.myPlayer == Player.whoAmI && poonCount < 2)
                {
                    Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<BloodTentacle>(), 400, 10, Player.whoAmI, poonCount == 0 ? -1 : 1);
                }
            }
            else
            {
                blood.Clear();
            }
        }
        public override void ResetEffects()
        {
            fullBloodArmor = false;
        }
        public override void UpdateBadLifeRegen()
        {
            if (fullBloodArmor)
            {
                Player.lifeRegen = 0;
                Player.lifeRegenTime = 0;
            }
        }
        public override void OnHurt(Player.HurtInfo info)
        {
            if (fullBloodArmor && blood.Count > 0)
            {
                for (int i = 0; i < blood.Count; i++)
                {
                    blood[i] -= bloodExpiration;
                }
            }
        }
        public override void UpdateDead()
        {
            blood.Clear();
        }
    }
}
