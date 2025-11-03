using CalamityMod;
using CalamityMod.DataStructures;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Items.Armor.Statigel;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Content.Items.Materials;
using Luminance.Core.Hooking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using NoxusBoss.Assets;
using NoxusBoss.Content.Rarities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.ShintoArmor
{
	
	[AutoloadEquip(EquipType.Body)]
	public class ShintoArmorBreastplate : ModItem
	{
        #region static values
        public static int BarrierCooldown = 18 * 60;
        
        public static int ShieldDurabilityMax = 200;
        public static int ShieldRechargeDelay = 725;
        public static int ShieldRechargeRate = 24;
        public static int TotalShieldRechargeTime = 575;

        public static readonly SoundStyle ShieldHurtSound = GennedAssets.Sounds.Avatar.DeadStarCoreCrack with { PitchVariance = 0.6f, Volume = 0.6f, MaxInstances = 0 };
        public static readonly SoundStyle ActivationSound = GennedAssets.Sounds.Avatar.DeadStarCoreCritical with { PitchVariance = 0.6f, Volume = 0.6f, MaxInstances = 0 };
        public static readonly SoundStyle BreakSound = GennedAssets.Sounds.Avatar.DeadStarCoreExplode with { PitchVariance = 0.6f, Volume = 0.6f, MaxInstances = 0 };

        public static Texture2D NoiseTex = GennedAssets.Textures.Noise.TurbulentNoise;
        public static Texture2D GFB = GennedAssets.Textures.Extra.Ogscule;

        private static readonly int MaxManaIncrease = 200;
        private static readonly int MaxMinionIncrease = 10;
        private static readonly int MaxLifeIncrease = 300;

        public static int WingSlotID
        {
            get;
            private set;
        }

        #endregion
        // public new string LocalizationCategory => "Items.Armor";
        // public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxManaIncrease, MaxMinionIncrease, MaxLifeIncrease);

        // gonna keep it real chief, this chestplate code is a mess lmao.
     

        public override string LocalizationCategory => "Items.Armor.ShintoArmor";
        public override void SetStaticDefaults()
        {
            var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            ArmorIDs.Body.Sets.HidesArms[equipSlot] = true;
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlot] = true;

            
        }

      
        public override void SetDefaults() {
            Item.wingSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Wings);
            Item.width = 38; 
			Item.height = 22; 
			Item.value = Item.sellPrice(gold: 4445);
			Item.rare = ModContent.RarityType<AvatarRarity>(); 
			Item.defense = 63; 
            Item.lifeRegen += 3; 
        }


        public override void UpdateEquip(Player player)
        {
            var modPlayer = player.Calamity();
            
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
        public override void Load()
        {
            EquipLoader.AddEquipTexture(Mod, Texture + "_Waist", EquipType.Waist, this);
            On_Main.DrawInfernoRings += On_Main_DrawInfernoRings;
        }
        public override void EquipFrameEffects(Player player, EquipType type)
        {
            if (player.body == Item.bodySlot)
            {
                player.waist = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Waist);
                player.cWaist = player.cBody;
            }
        }
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
                float scale = (baseScale + maxExtraScale * extraScalePulseInterpolant) * modPlayer.barrierSizeInterp;
                float ShieldHealthInterpolant = (float)player.GetModPlayer<ShintoArmorPlayer>().barrier / ShieldDurabilityMax;

                if (!alreadyDrawnShieldForPlayer)
                {
                    float visualShieldStrength = ShieldHealthInterpolant;

                    // The scale used for the noise overlay also grows and shrinks
                    float noiseScale = MathHelper.Lerp(0.28f, 0.38f, 5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 0.347f + i)) * modPlayer.barrierSizeInterp;


                    
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
                        Main.EntitySpriteDraw(glow, pos, null, Color.AntiqueWhite, rotation, glow.Size() / 2f, modPlayer.barrierSizeInterp*((baseShieldOpacity/20)+0.1f), 0);
                    
                    
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

                    //Utils.DrawBorderString(Main.spriteBatch, "Opacity: " + baseShieldOpacity.ToString(), -Vector2.UnitX *150 +pos - Vector2.UnitY*40, Color.AntiqueWhite);
                    //Utils.DrawBorderString(Main.spriteBatch, "Barrier: "+player.GetModPlayer<ShintoArmorPlayer>().barrier, -Vector2.UnitX * 150 + pos - Vector2.UnitY * 60, Color.AntiqueWhite);
                    //Utils.DrawBorderString(Main.spriteBatch, "TimeSinceLastHit: " + player.GetModPlayer<ShintoArmorPlayer>().timeSinceLastHit, - Vector2.UnitX * 150 + pos - Vector2.UnitY * 80, Color.AntiqueWhite);

                    Vector2 drawPosition = player.Center - Main.screenPosition;

                   

                    SpriteEffects direction = SpriteEffects.None;
                    Vector2 Gorigin = new Vector2(glow.Width / 2 , glow.Height / 2);

                    /*
                    ManagedScreenFilter suctionShader = ShaderManager.GetFilter("HeavenlyArsenal.SuctionSpiralShader");

                    suctionShader.TrySetParameter("suctionCenter", Vector2.Transform(player.Center - Main.screenPosition, Main.GameViewMatrix.TransformationMatrix));
                    suctionShader.TrySetParameter("zoomedScreenSize", Main.ScreenSize.ToVector2() / Main.GameViewMatrix.Zoom);
                    suctionShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom.X);
                    suctionShader.TrySetParameter("suctionOpacity", 1 * (player.GetModPlayer<ShintoArmorPlayer>().barrierSizeInterp - AvatarRiftSuckVisualsManager.ZoomInInterpolant) * 0.32f);
                    suctionShader.TrySetParameter("suctionBaseRange", 27f);
                    suctionShader.TrySetParameter("suctionFadeOutRange", 10f);
                    suctionShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
                    suctionShader.Activate();
                    */
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

        private void On_Main_DrawInfernoRings(On_Main.orig_DrawInfernoRings orig, Main self)
        {
            orig(self);
            Player player = Main.LocalPlayer;
            //if (!player.GetModPlayer<ShintoArmorPlayer>().isShadeTeleporting && !player.GetModPlayer<ShintoArmorPlayer>().JustTeleported)
            DrawDyeableShader(Main.spriteBatch);
            
                
            
        }
        public override void AddRecipes()
		{
            if (ModLoader.TryGetMod("CalamityHunt", out Mod CalamityHunt))
            {
                CreateRecipe()
                .AddIngredient(ModContent.ItemType<AvatarMaterial>())
                .AddIngredient<DemonshadeBreastplate>()
                .AddIngredient(ItemID.NinjaShirt)
                .AddIngredient(ItemID.CrystalNinjaChestplate)
                .AddIngredient(CalamityHunt.Find<ModItem>("ShogunChestplate").Type)
                .AddIngredient<TheSponge>()
                .AddTile<DraedonsForge>()
                .AddCustomShimmerResult(ModContent.ItemType<ShintoArmorWings>(), 1)
                .Register();
            }
            else
            {
                CreateRecipe()
                .AddIngredient(ModContent.ItemType<AvatarMaterial>())
                .AddIngredient<DemonshadeBreastplate>()
                .AddIngredient(ItemID.NinjaShirt)
                .AddIngredient(ItemID.CrystalNinjaChestplate)
                .AddIngredient<StatigelArmor>()
                .AddIngredient<TheSponge>()
                .AddTile<DraedonsForge>()
                .AddCustomShimmerResult(ModContent.ItemType<ShintoArmorWings>(), 1)
                .Register();
            }


        }

    }

    public class ShintoCraftPlayer : ModPlayer
    {
        public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback)
        {
            
            return base.AddMaterialsForCrafting(out itemConsumedCallback);
        }
    }
}
