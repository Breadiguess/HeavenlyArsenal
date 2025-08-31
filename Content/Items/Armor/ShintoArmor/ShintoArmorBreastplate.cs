using CalamityMod;
using CalamityMod.DataStructures;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Items.Armor.Statigel;
using CalamityMod.Tiles.Furniture.CraftingStations;
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
        public override void Load()
        {
            

            EquipLoader.AddEquipTexture(Mod, Texture + "_Waist", EquipType.Waist, this);
            EquipLoader.AddEquipTexture(Mod, Texture.Replace("Breastplate", "Wings"), EquipType.Wings, this);
            On_Main.DrawInfernoRings += On_Main_DrawInfernoRings;
        }

        public override string LocalizationCategory => "Items.Armor.ShintoArmor";
        public override void SetStaticDefaults()
        {
            var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            ArmorIDs.Body.Sets.HidesArms[equipSlot] = true;
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlot] = true;

            Item.wingSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Wings);
            WingSlotID = Item.wingSlot;
            ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(100000000, 16.67f, 3.7f, true, 23.5f, 4f);
            new ManagedILEdit("Let Totally not divine wings Hover", Mod, edit =>
            {
                IL_Player.Update += edit.SubscriptionWrapper;
            }, edit =>
            {
                IL_Player.Update -= edit.SubscriptionWrapper;
            }, LetWingsHover).Apply();

            On_Player.WingMovement += UseHoverMovement;
        }

      
        public override void SetDefaults() {
            Item.wingSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Wings);
            Item.width = 38; 
			Item.height = 22; 
			Item.value = Item.sellPrice(gold: 4445); // How many coins the item is worth
			Item.rare = ModContent.RarityType<AvatarRarity>(); // The rarity of the item
			Item.defense = 63; // The amount of defense the item will give when equipped
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
           
            DrawDyeableShader(Main.spriteBatch);
            
                
            
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

        #region wing shit
        private static void LetWingsHover(ILContext context, ManagedILEdit edit)
        {
            ILCursor cursor = new ILCursor(context);

            /* This is the general layout of the code, with local variables cleaned up and extraneous comments added:
             *
             * bool usingWings = false;
             * if (((player.velocity.Y == 0f || player.sliding) && player.releaseJump) || (player.autoJump && player.justJumped))
             * {
             *     player.mount.ResetFlightTime(player.velocity.X);
             *     player.wingTime = (float)player.wingTimeMax;
             * }
             * 
             * // Performs the standard wings check.
             * if (player.wingsLogic > 0 && player.controlJump && player.wingTime > 0f && player.jump == 0 && player.velocity.Y != 0f)
             * {
             *     usingWings = true;
             * }
             * 
             * // Determine whether the player the player is using wings for a special hover.
             * // Notably, this does not include modded wing IDs.
             * if ((player.wingsLogic == 22 || player.wingsLogic == 28 || player.wingsLogic == 30 || player.wingsLogic == 32 || player.wingsLogic == 29 || player.wingsLogic == 33 || player.wingsLogic == 35 || player.wingsLogic == 37 || player.wingsLogic == 45) && player.controlJump && player.TryingToHoverDown && player.wingTime > 0f)
             * {
             *     usingWings = true;
             * }
             */

            // Search for the start of the if ((player.wingsLogic == 22 || player.wingsLogic == 28... || player.wingsLogic == 37 statement
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcI4(37)))
            {
                edit.LogFailure("The 'if ((player.wingsLogic == 37' check could not be found.");
                return;
            }

            // Find the local index of the usingWings bool by going backwards to the first usingWings = true line.
            int usingWingsIndex = 0;
            if (!cursor.TryGotoPrev(MoveType.After, i => i.MatchStloc(out usingWingsIndex)))
            {
                edit.LogFailure("The usingWings local variable's index could not be found.");
                return;
            }

            // Go back to the start of the method and find the place where the usingWings bool is initialized with the usingWings = false line.
            cursor.Goto(0);
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchStloc(usingWingsIndex)))
            {
                edit.LogFailure("The first initialization of the usingWings local variable could not be found.");
                return;
            }

            // Transform the usingWings = true line like so:
            // bool usingWings = true;
            // bool usingWings = true | (player.wingsLogic == WingSlotID && player.controlJump && player.TryingToHoverDown && player.wingTime > 0f);
            // Notice that this includes the same condition used for the "is the player using wings to hover right now?" check.

            // It would be more efficient to remove the true, but for defensive programming purposes this merely adds onto existing local variable definitions, rather than
            // completely replacing them.
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((Player player) => player.wingsLogic == WingSlotID && player.controlJump && player.TryingToHoverDown && player.wingTime > 0f);
            cursor.Emit(OpCodes.Or);
        }

        private void UseHoverMovement(On_Player.orig_WingMovement orig, Player player)
        {
            orig(player);
            if (player.wingsLogic == WingSlotID && player.TryingToHoverDown)
                player.velocity.Y = -0.0001f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.LocalPlayer.GetModPlayer<ShintoArmorPlayer>().SetActive)
            {
                int lastTooltipIndex = tooltips.FindLastIndex(t => t.Name.Contains("Tooltip"));

                tooltips.Add(new TooltipLine(Mod, "PressDownNotif", Language.GetTextValue("CommonItemTooltip.PressDownToHover")));

            }
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling = 2f;
            ascentWhenRising = 0.184f;
            maxCanAscendMultiplier = 1.2f;
            maxAscentMultiplier = 3.25f;
            constantAscend = 0.29f;
        }

        public override bool WingUpdate(Player player, bool inUse)
        {
            if (player.GetModPlayer<ShintoArmorPlayer>().SetActive)
            {
                if (player.controlJump && player.wingTime > 0 && player.velocity.Y != 0)
                {
                    int frameRate = 5; // FPS
                    int maxFrames = 7; // Total frames


                    if (player.wingFrame == 0)
                    {
                        player.wingFrame = 1;
                    }
                    // Reset frames
                    if (player.wingFrame >= maxFrames)
                    {
                        player.wingFrameCounter = 0;
                        player.wingFrame = 0;
                    }
                    // Animation
                    if (player.wingFrameCounter % frameRate == 0)
                    {
                        player.wingFrame++;
                    }
                    player.wingFrameCounter++;
                }
                else
                {
                    player.wingFrameCounter = 0;
                    player.wingFrame = 0; // On ground
                    if (player.velocity.Y != 0)
                    {
                        player.wingFrame = 1; // Falling
                        if (player.controlJump && player.velocity.Y > 0)
                            player.wingFrame = 1; // Gliding
                    }
                }
                return true;
            }
            else
                return false;
        }


        public override void EquipFrameEffects(Player player, EquipType type)
        {
            if (player.equippedWings != null)
            {
               //if (player.equippedWings.wingSlot == player.wingsLogic)
               // {
               //     player.wings = Item.wingSlot;
               //     player.cWings = player.cBody;
               // }
            }

            if (player.wingsLogic == Item.wingSlot && player.wings <= 0)
            {
               //player.wings = Item.wingSlot;
            }

            if (player.body == Item.bodySlot)
            {
               // player.waist = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Waist);
               // player.cWaist = player.cBody;
            }
        }
        #endregion
    }
}
