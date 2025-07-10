using CalamityMod;
using CalamityMod.Balancing;
using CalamityMod.CalPlayer;
using CalamityMod.Cooldowns;
using CalamityMod.DataStructures;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Items.Armor.Statigel;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.ArsenalPlayer;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Rarities;
using ReLogic.Content;
using System;
using System.Diagnostics.Metrics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.ShintoArmor
{
	// The AutoloadEquip attribute automatically attaches an equip texture to this item.
	// Providing the EquipType.Body value here will result in TML expecting a X_Body.png file to be placed next to the item's main texture.
	[AutoloadEquip(EquipType.Body)]
	public class ShintoArmorBreastplate : ModItem
	{
        #region static values
        public static int BarrierCooldown = 20 * 60;
        
        public static int ShieldDurabilityMax = 200;
        public static int ShieldRechargeDelay = 400;
        public static int ShieldRechargeRate = 25;
        public static int TotalShieldRechargeTime = 220;

        public static int AbyssDash_Iframes = 10 * 60;
        public static int AbyssDash_Cooldown = 60;
        public static readonly SoundStyle AbyssDash_Start = GennedAssets.Sounds.Avatar.AngryDistant with { PitchVariance = 0.4f, Volume = 0.6f, MaxInstances = 0 };
        public static readonly SoundStyle ShieldHurtSound = GennedAssets.Sounds.Avatar.DeadStarCoreCrack with { PitchVariance = 0.6f, Volume = 0.6f, MaxInstances = 0 };
        public static readonly SoundStyle ActivationSound = GennedAssets.Sounds.Avatar.DeadStarCoreCritical with { PitchVariance = 0.6f, Volume = 0.6f, MaxInstances = 0 };
        public static readonly SoundStyle BreakSound = GennedAssets.Sounds.Avatar.DeadStarCoreExplode with { PitchVariance = 0.6f, Volume = 0.6f, MaxInstances = 0 };
        public static Texture2D NoiseTex = GennedAssets.Textures.Noise.TurbulentNoise;
        public static Texture2D GFB = GennedAssets.Textures.Extra.Ogscule;

        private static readonly int MaxManaIncrease = 200;
        private static readonly int MaxMinionIncrease = 10;
        private static readonly int MaxLifeIncrease = 300;
        #endregion
        // public new string LocalizationCategory => "Items.Armor";
        // public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxManaIncrease, MaxMinionIncrease, MaxLifeIncrease);


        public override string LocalizationCategory => "Items.Armor.ShintoArmor";
        public override void SetStaticDefaults()
        {
            var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            ArmorIDs.Body.Sets.HidesArms[equipSlot] = true;
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlot] = true;
        }
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
            player.thorns += 10f;
            player.statLifeMax2 += MaxLifeIncrease;
            player.statManaMax2 += MaxManaIncrease;
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
                float maxExtraScale = 0.055f;
                float extraScalePulseInterpolant = MathF.Pow(4f, MathF.Sin(Main.GlobalTimeWrappedHourly * 0.791f + i) - 1);
                float scale = baseScale + maxExtraScale * extraScalePulseInterpolant;
                float ShieldHealthInterpolant = (float)player.GetModPlayer<ShintoArmorPlayer>().barrier / ShieldDurabilityMax;

                if (!alreadyDrawnShieldForPlayer)
                {
                    float visualShieldStrength = ShieldHealthInterpolant;

                    // The scale used for the noise overlay also grows and shrinks
                    float noiseScale = MathHelper.Lerp(0.28f, 0.38f, 5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 0.347f + i));


                    
                    Effect shieldEffect = Filters.Scene["CalamityMod:RoverDriveShield"].GetShader().Shader;
                    shieldEffect.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 0.0813f);
                    shieldEffect.Parameters["blowUpPower"].SetValue(3f);
                    shieldEffect.Parameters["blowUpSize"].SetValue(0.56f);
                    shieldEffect.Parameters["noiseScale"].SetValue(noiseScale);

                    float baseShieldOpacity = 1.2f *Utils.SmoothStep(-15f, 15f, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f)) - 0.2f;
                                              //(float)Utils.SmoothStep(0,0.5f,Math.Sin(Main.GlobalTimeWrappedHourly * 0.45f));
                                              //(float)Utils.Lerp(0, 1, Math.Clamp((MathF.Sin(Main.GlobalTimeWrappedHourly * 0.95f)),0,1));//(0.2f) + 0.2f * (player.statLife / player.statLifeMax) * MathF.Sin(Main.GlobalTimeWrappedHourly * 0.76f);

                    
                    float minShieldStrengthOpacityMultiplier = 1f;
                    float finalShieldOpacity = baseShieldOpacity * MathHelper.Lerp(minShieldStrengthOpacityMultiplier, 1f, visualShieldStrength);
                    shieldEffect.Parameters["shieldOpacity"].SetValue(finalShieldOpacity);
                    shieldEffect.Parameters["shieldEdgeBlendStrenght"].SetValue(1f);

                    Color shieldColor = new Color(163, 0, 41); // #189CCC
                    Color primaryEdgeColor = shieldColor;
                    Color secondaryEdgeColor = new Color(220,20,71); // #22E0E3                   
                   // Main.NewText($"Shield Opacity: {baseShieldOpacity}", Color.AliceBlue);
                    Color edgeColor = CalamityUtils.MulticolorLerp(Main.GlobalTimeWrappedHourly * ShieldHealthInterpolant, primaryEdgeColor, secondaryEdgeColor);

                    shieldEffect.Parameters["shieldColor"].SetValue(shieldColor.ToVector3());
                    shieldEffect.Parameters["shieldEdgeColor"].SetValue(edgeColor.ToVector3());

                    shieldEffect.CurrentTechnique.Passes[0].Apply();


                    Main.pixelShader.CurrentTechnique.Passes[0].Apply();


                    float rotation = Utils.AngleLerp(0, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 0.02f),0.4f);
                    //Texture2D ShieldNoise = AssetDirectory.Textures.VoidLake.Value;
                    Texture2D glow = GennedAssets.Textures.Noise.MoltenNoise;
                    Texture2D ogg = GennedAssets.Textures.Extra.Ogscule;
                    Main.spriteBatch.End();

                   
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, shieldEffect, Main.GameViewMatrix.TransformationMatrix);
                    // Fetch shield noise overlay texture (this is the polygon texture fed to the shader)
                    Vector2 pos = player.MountedCenter + player.gfxOffY * Vector2.UnitY - Main.screenPosition;

                    if (Main.remixWorld)
                    {
                        Main.EntitySpriteDraw(ogg, pos, null, Color.AntiqueWhite, rotation, ogg.Size() / 2f, 0.05f, 0, 0);
                    }
                    else 
                        Main.EntitySpriteDraw(glow, pos, null, Color.AntiqueWhite, rotation, glow.Size() / 2f, (baseShieldOpacity/20)+0.1f, 0);
                    
                    
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

                    //Utils.DrawBorderString(Main.spriteBatch, "Opacity: " + baseShieldOpacity.ToString(), -Vector2.UnitX *150 +pos - Vector2.UnitY*40, Color.AntiqueWhite);
                    Utils.DrawBorderString(Main.spriteBatch, "Barrier: "+player.GetModPlayer<ShintoArmorPlayer>().barrier, -Vector2.UnitX * 150 + pos - Vector2.UnitY * 60, Color.AntiqueWhite);
                    Utils.DrawBorderString(Main.spriteBatch, "ShieldHealth Interp: " + ShieldHealthInterpolant.ToString(), - Vector2.UnitX * 150 + pos - Vector2.UnitY * 80, Color.AntiqueWhite);

                    Vector2 drawPosition = player.Center - Main.screenPosition;

                   

                    SpriteEffects direction = SpriteEffects.None;
                    Vector2 Gorigin = new Vector2(glow.Width / 2 , glow.Height / 2);


                    //Main.spriteBatch.Draw(glow, drawPosition, null, Color.Crimson, rotation, Gorigin, 0.05f, direction, 0f);

                }

                alreadyDrawnShieldForPlayer = true;

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
                //EquipLoader.AddEquipTexture(Mod, Texture + "_Bulk", EquipType.Front, this);
                //EquipLoader.AddEquipTexture(Mod, Texture + "_Waist", EquipType.Waist, this);
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

                player.front = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Front);
                player.cFront = player.cBody;
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            base.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
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
                .AddIngredient<StatigelArmor>()
                .AddIngredient<TheSponge>()
                .AddTile<DraedonsForge>()
                .Register();
            }


        }
    }
    public class ShintoArmorBreastplate_DrawLayer : PlayerDrawLayer
    {

        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.FrontAccFront);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.body == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorBreastplate), EquipType.Body);

        public override bool IsHeadLayer => false;



        protected override void Draw(ref PlayerDrawSet drawInfo)
        {



        }
    }

}
