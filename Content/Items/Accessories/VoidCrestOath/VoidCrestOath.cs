using System;
using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Projectiles.Summon;
using HeavenlyArsenal.Content.Items.Accessories.Cosmetic;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Assets.GennedAssets.Sounds;

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
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 6));
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

        //todo: hard cap on the amount of projectiels that can be deleted at once, blacklist, balancing
        //i think im gonna have to ehavily nerf it
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

    public class VoidcrestOathDrawlayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.balloonFront == EquipLoader.GetEquipSlot(Mod, nameof(VoidCrestOath), EquipType.Balloon);
     

        public override bool IsHeadLayer => true;

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {

            Vector2 position = new Vector2(50f, 20f);

            Vector2 particleDrawCenter = position + new Vector2(0f, 0f);
            Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;

            Main.EntitySpriteDraw(glow, particleDrawCenter - Main.screenPosition, glow.Frame(), Color.Red with { A = 200 }, 0, glow.Size() * 0.5f, new Vector2(0.12f, 0.25f), 0, 0);
            Texture2D innerRiftTexture = AssetDirectory.Textures.VoidLake.Value;
            Color edgeColor = new Color(1f, 0.06f, 0.06f);
            float timeOffset = Main.myPlayer * 2.5552343f;

            ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
            riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.2f + timeOffset);
            riftShader.TrySetParameter("baseCutoffRadius", 0.24f);
            riftShader.TrySetParameter("swirlOutwardnessExponent", 0.2f);
            riftShader.TrySetParameter("swirlOutwardnessFactor", 3f);
            riftShader.TrySetParameter("vanishInterpolant", 0.01f );
            riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
            riftShader.TrySetParameter("edgeColorBias", 0.1f);
            riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
            riftShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap);
            riftShader.Apply();

            Main.spriteBatch.Draw(innerRiftTexture, particleDrawCenter, null, Color.White, 0 + MathHelper.Pi, innerRiftTexture.Size() * 0.5f, new Vector2(0.1f, 0.05f), 0, 0);

            //Utils.DrawBorderString(Main.spriteBatch, ", drawInfo.drawPlayer.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.White);



            Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        }
    }
}
