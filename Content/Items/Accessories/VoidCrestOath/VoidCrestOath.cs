using System;
using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath
{
    [AutoloadEquip(EquipType.Balloon)]
    
    public class VoidCrestOath : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        internal const float MaxBonus = 0.2f;
        internal const float MaxDistance = 480f;

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 5));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 78;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
            Item.accessory = true;
        }
        /*
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.warbannerOfTheSun = true;

            float bonus = CalculateBonus(player);
            player.GetAttackSpeed<MeleeDamageClass>() += bonus;
            player.GetDamage<MeleeDamageClass>() += bonus;
            player.GetDamage<TrueMeleeDamageClass>() += bonus;

            player.GetModPlayer<VoidCrestOathPlayer>().voidCrestOathEquipped = true;

        }
        */
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            VoidCrestOathPlayer modPlayer = player.GetModPlayer<VoidCrestOathPlayer>();

           
            modPlayer.voidCrestOathEquipped = true;
            modPlayer.NotVanity = false; 

           
        }

        private static float CalculateBonus(Player player)
        {
            float bonus = 0f;

            int closestNPC = -1;
            foreach (NPC nPC in Main.ActiveNPCs)
            {
                if (nPC.IsAnEnemy() && !nPC.dontTakeDamage)
                {
                    closestNPC = nPC.whoAmI;
                    break;
                }
            }
            float distance = -1f;
            foreach (NPC nPC in Main.ActiveNPCs)
            {
                if (nPC.IsAnEnemy() && !nPC.dontTakeDamage)
                {
                    float distance2 = Math.Abs(nPC.position.X + nPC.width / 2 - (player.position.X + player.width / 2)) + Math.Abs(nPC.position.Y + nPC.height / 2 - (player.position.Y + player.height / 2));
                    if (distance == -1f || distance2 < distance)
                    {
                        distance = distance2;
                        closestNPC = nPC.whoAmI;
                    }
                }
            }

            if (closestNPC != -1)
            {
                NPC actualClosestNPC = Main.npc[closestNPC];

                float generousHitboxWidth = Math.Max(actualClosestNPC.Hitbox.Width / 2f, actualClosestNPC.Hitbox.Height / 2f);
                float hitboxEdgeDist = actualClosestNPC.Distance(player.Center) - generousHitboxWidth;

                if (hitboxEdgeDist < 0)
                    hitboxEdgeDist = 0;

                if (hitboxEdgeDist < MaxDistance)
                {
                    bonus = MathHelper.Lerp(0f, MaxBonus, 1f - hitboxEdgeDist / MaxDistance);

                    if (bonus > MaxBonus)
                        bonus = MaxBonus;
                }
            }

            return bonus;
        }


        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            CalamityUtils.DrawInventoryCustomScale(
                spriteBatch,
                texture: TextureAssets.Item[Type].Value,
                position,
                frame,
                drawColor,
                itemColor,
                origin,
                scale,
                wantedScale: 0.6f,
                drawOffset: new(0f, -2f)
            );
            return false;
        }
    }
}
