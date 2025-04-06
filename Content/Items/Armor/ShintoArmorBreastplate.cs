using CalamityMod;
using CalamityMod.Balancing;
using CalamityMod.CalPlayer;
using CalamityMod.Cooldowns;
using CalamityMod.DataStructures;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.ArsenalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Rarities;
using ReLogic.Content;
using System;
using System.Diagnostics.Metrics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor
{
	// The AutoloadEquip attribute automatically attaches an equip texture to this item.
	// Providing the EquipType.Body value here will result in TML expecting a X_Body.png file to be placed next to the item's main texture.
	[AutoloadEquip(EquipType.Body)]
	public class ShintoArmorBreastplate : ModItem
	{
        
        public static int BarrierCooldown = 20 * 60;

        public static readonly int MaxManaIncrease = 200;
		public static readonly int MaxMinionIncrease = 10;
        public static int ShieldDurabilityMax = 340;
        public static int ShieldRechargeDelay = 400;
        public static int ShieldRechargeRate = 25;
        public static int TotalShieldRechargeTime = 200;

        public static readonly SoundStyle ShieldHurtSound = GennedAssets.Sounds.Avatar.DeadStarCoreCrack with { PitchVariance = 0.6f, Volume = 0.6f, MaxInstances = 0 };
        public static readonly SoundStyle ActivationSound = GennedAssets.Sounds.Avatar.DeadStarCoreCritical with { PitchVariance = 0.6f, Volume = 0.6f, MaxInstances = 0 };
        public static readonly SoundStyle BreakSound = GennedAssets.Sounds.Avatar.DeadStarCoreExplode with { PitchVariance = 0.6f, Volume = 0.6f, MaxInstances = 0 };



        public static Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Items/Accessories/TheSpongeShield").Value;
        public static Texture2D NoiseTex = GennedAssets.Textures.Noise.TurbulentNoise;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxManaIncrease, MaxMinionIncrease);

		public override void SetDefaults() {
			Item.width = 18; // Width of the item
			Item.height = 18; // Height of the item
			Item.value = Item.sellPrice(gold: 4445); // How many coins the item is worth
			Item.rare = ModContent.RarityType<AvatarRarity>(); // The rarity of the item
			Item.defense = 56; // The amount of defense the item will give when equipped

		}


        public override void UpdateEquip(Player player)
        {
            var modPlayer = player.Calamity();
            modPlayer.shadeRegen = true;
            player.thorns += 100f;
            player.statLifeMax2 += 350;
            player.statManaMax2 += 400;
            player.GetDamage<GenericDamageClass>() += 0.15f;
            player.GetCritChance<GenericDamageClass>() += 18;
            player.GetAttackSpeed<GenericDamageClass>() += 0.25f;
            player.GetModPlayer<ShintoArmorPlayer>().ChestplateEquipped = true;
        }
        public override void UpdateVanity(Player player)
        {
            player.GetModPlayer<ShintoArmorPlayer>().ChestplateEquipped = true;
        }

        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
        public float RenderDepth => IDyeableShaderRenderer.SpongeShieldDepth;

      
        // Renders the bubble shield over the item in the world.
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            //Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Items/Accessories/TheSpongeShield").Value;
            spriteBatch.Draw(tex, Item.Center - Main.screenPosition + new Vector2(0f, 0f), Main.itemAnimations[Item.type].GetFrame(tex), Color.Cyan * 0.5f, 0f, new Vector2(tex.Width / 2f, (tex.Height / 30f) * 0.8f), 1f, SpriteEffects.None, 0);
        }


        public static void DrawDyeableShader(SpriteBatch spriteBatch)
        {
          
            // TODO -- Control flow analysis indicates that this hook is not stable (as it was copied from Rover Drive).
            // Sponge shields will be drawn for each player with the Sponge equipped, yes.
            // But there is no guarantee that the shields will be in the right condition for each player.
            // Visibility is not net synced, for example.
            bool alreadyDrawnShieldForPlayer = false;

            foreach (Player player in Main.ActivePlayers)
            {
                if (player.GetModPlayer<ShintoArmorPlayer>().SetActive == false) 
                {
                    continue;
                }
                if (player.outOfRange || player.dead)
                    continue;

                ShintoArmorPlayer modPlayer = player.GetModPlayer<ShintoArmorPlayer>();

                // Determine if the shield should be rendered
                // Use modPlayer.active (or another appropriate flag) and check that the barrier value is positive.
                bool isVanityOnly = modPlayer.ShadowShieldVisible && modPlayer.SetActive; // if these properties exist
                bool shieldExists = modPlayer.barrier > 0;
                if (!shieldExists)
                    continue;

                // Scale the shield as drawn. The shield gently grows and shrinks; it should be largely imperceptible.
                // The "i" parameter is to desync each player's shield animation.
                int i = player.whoAmI;
                float baseScale = 1f;
                float maxExtraScale = 0.025f;
                float extraScalePulseInterpolant = MathF.Pow(4f, MathF.Sin(Main.GlobalTimeWrappedHourly * 0.791f + i) - 1);
                float scale = baseScale + maxExtraScale * extraScalePulseInterpolant;

                if (!alreadyDrawnShieldForPlayer)
                {
                    float visualShieldStrength = 1f;

                    // The scale used for the noise overlay also grows and shrinks
                    float noiseScale = MathHelper.Lerp(0.28f, 0.38f, 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 0.347f + i));


                    
                    Effect shieldEffect = Filters.Scene["CalamityMod:RoverDriveShield"].GetShader().Shader;
                    shieldEffect.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 0.0813f);
                    shieldEffect.Parameters["blowUpPower"].SetValue(3f);
                    shieldEffect.Parameters["blowUpSize"].SetValue(0.56f);
                    shieldEffect.Parameters["noiseScale"].SetValue(noiseScale);

                    float baseShieldOpacity = 0.9f + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 1.95f);
                    float minShieldStrengthOpacityMultiplier = 0.25f;
                    float finalShieldOpacity = baseShieldOpacity * MathHelper.Lerp(minShieldStrengthOpacityMultiplier, 1f, visualShieldStrength);
                    shieldEffect.Parameters["shieldOpacity"].SetValue(finalShieldOpacity);
                    shieldEffect.Parameters["shieldEdgeBlendStrenght"].SetValue(4f);

                    Color shieldColor = new Color(220, 20, 70); // #189CCC
                    Color primaryEdgeColor = shieldColor;
                    Color secondaryEdgeColor = new Color(0, 0 , 0); // #22E0E3                   
                   // Main.NewText($"Shield Opacity: {baseShieldOpacity}", Color.AliceBlue);
                    Color edgeColor = CalamityUtils.MulticolorLerp(Main.GlobalTimeWrappedHourly * 0.2f, primaryEdgeColor, secondaryEdgeColor);

                    shieldEffect.Parameters["shieldColor"].SetValue(shieldColor.ToVector3());
                    shieldEffect.Parameters["shieldEdgeColor"].SetValue(edgeColor.ToVector3());
                    
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, shieldEffect, Main.GameViewMatrix.TransformationMatrix);
                }

                alreadyDrawnShieldForPlayer = true;

                // Fetch shield noise overlay texture (this is the polygon texture fed to the shader)
                Vector2 pos = player.MountedCenter + player.gfxOffY * Vector2.UnitY - Main.screenPosition;
                
                //Main.spriteBatch.Draw(NoiseTex, pos, null, Color.White, 0, tex.Size() / 2f, scale, 0, 0);
                Main.EntitySpriteDraw(tex, pos, null, Color.AntiqueWhite, 0, NoiseTex.Size()/2, 60, 0, 0);

            }

            if (alreadyDrawnShieldForPlayer)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            }
        }



        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                //register the faulds texture. This appears either when the leggings  or the chestplate is equipped (both works)

                EquipLoader.AddEquipTexture(Mod, Texture + "_Waist", EquipType.Waist, this);
                //EquipLoader.AddEquipTexture(Mod, "HeavenlyArsenal/Content/Items/Armor/ShintoArmorFaulds_Waist", EquipType.Waist, name: "ShintoArmorFaulds");
            }
             On_Main.DrawInfernoRings += On_Main_DrawInfernoRings;
        }

        private void On_Main_DrawInfernoRings(On_Main.orig_DrawInfernoRings orig, Main self)
        {
            orig(self);
           
            DrawDyeableShader(Main.spriteBatch);
            
                
            
        }

        public override void EquipFrameEffects(Player player, EquipType type)
        {
          if (player.body == Item.bodySlot)
            {
                player.waist = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Waist);
                player.cWaist = player.cBody;
            }
        }



        public override void AddRecipes()
		{
            if (ModLoader.TryGetMod("CalamityHunt", out Mod CalamityHunt))
            {
                CreateRecipe()
                .AddIngredient<DemonshadeBreastplate>()
                .AddIngredient(ItemID.NinjaShirt)
                .AddIngredient(ItemID.CrystalNinjaChestplate)
                .AddIngredient(CalamityHunt.Find<ModItem>("ShogunChestplate").Type)
                .AddIngredient<TheSponge>()
                .AddTile<DraedonsForge>()
                .Register();
            }
            else
            {
                CreateRecipe()
                .AddIngredient<DemonshadeBreastplate>()
                .AddIngredient(ItemID.NinjaShirt)
                .AddIngredient(ItemID.CrystalNinjaChestplate)
                .AddIngredient<TheSponge>()
                .AddTile<DraedonsForge>()
                .Register();
            }


        }
    }
}
