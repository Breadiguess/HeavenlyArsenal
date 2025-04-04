using CalamityMod;
using CalamityMod.Balancing;
using CalamityMod.CalPlayer;
using CalamityMod.DataStructures;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.ArsenalPlayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using ReLogic.Content;
using System;
using Terraria;
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
		public static readonly int MaxManaIncrease = 200;
		public static readonly int MaxMinionIncrease = 10;

       

        static Texture2D NoiseTex = GennedAssets.Textures.Noise.SwirlNoise2;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxManaIncrease, MaxMinionIncrease);

		public override void SetDefaults() {
			Item.width = 18; // Width of the item
			Item.height = 18; // Height of the item
			Item.value = Item.sellPrice(gold: 1); // How many coins the item is worth
			Item.rare = ItemRarityID.Green; // The rarity of the item
			Item.defense = 56; // The amount of defense the item will give when equipped

		}

        public override void UpdateEquip(Player player)
        {
            var modPlayer = player.Calamity();
            modPlayer.shadeRegen = true;
            player.thorns += 100f;
            player.statLifeMax2 += 200;
            player.statManaMax2 += 200;
            player.GetDamage<GenericDamageClass>() += 0.15f;
            player.GetCritChance<GenericDamageClass>() += 15;
            player.GetAttackSpeed<MeleeDamageClass>() += 0.25f;
        }

        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
        public float RenderDepth => IDyeableShaderRenderer.SpongeShieldDepth;

      
        // Renders the bubble shield over the item in the world.
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Items/Accessories/TheSpongeShield").Value;
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
                if (player.outOfRange || player.dead)
                    continue;

                ShintoArmorPlayer modPlayer = player.GetModPlayer<ShintoArmorPlayer>(); ;


                // Do not render the shield if its visibility is off (or it does not exist)
                bool isVanityOnly = modPlayer.ShadowShieldVisible && !modPlayer.SetActive;
                bool shieldExists = isVanityOnly || modPlayer.barrier > 0;
                if (!modPlayer.ShadowShieldVisible || !shieldExists)
                    continue;

                // Scale the shield is drawn at. The Sponge shield gently grows and shrinks; it should be largely imperceptible.
                // The "i" parameter is to make different player's shields not be perfectly synced.
                int i = player.whoAmI;
                float baseScale = 0.155f;
                float maxExtraScale = 0.025f;
                float extraScalePulseInterpolant = MathF.Pow(4f, MathF.Sin(Main.GlobalTimeWrappedHourly * 0.791f + i) - 1);
                float scale = baseScale + maxExtraScale * extraScalePulseInterpolant;

                if (!alreadyDrawnShieldForPlayer)
                {

                    float visualShieldStrength = 1f;
                    if (!isVanityOnly)
                    {
                        // Again, I believe there is no way this looks correct when two players have The Sponge equipped.
                        ShintoArmorPlayer localModPlayer = player.GetModPlayer<ShintoArmorPlayer>(); ;
                        float shieldDurabilityRatio = localModPlayer.barrier / (float)560;
                        visualShieldStrength = MathF.Pow(shieldDurabilityRatio, 0.5f);
                    }

                    // The scale used for the noise overlay also grows and shrinks
                    // This is intentionally out of sync with the shield, and intentionally desynced per player
                    // Don't put this anywhere less than 0.15f or higher than 0.75f. The higher it is, the denser / more zoomed out the noise overlay is.
                    // Changing this too quickly and/or too much makes the noise grow and shrink visibly, so be careful with that.
                    float noiseScale = MathHelper.Lerp(0.28f, 0.38f, 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 0.347f + i));

                    // Define shader parameters
                    Effect shieldEffect = Filters.Scene["CalamityMod:RoverDriveShield"].GetShader().Shader;
                    shieldEffect.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 0.0813f); // Scrolling speed of polygonal overlay
                    shieldEffect.Parameters["blowUpPower"].SetValue(3f);
                    shieldEffect.Parameters["blowUpSize"].SetValue(0.56f);
                    shieldEffect.Parameters["noiseScale"].SetValue(noiseScale);

                    // Shield opacity multiplier slightly changes, this is independent of current shield strength
                    float baseShieldOpacity = 0.9f + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 1.95f);
                    float minShieldStrengthOpacityMultiplier = 0.25f;
                    float finalShieldOpacity = baseShieldOpacity * MathHelper.Lerp(minShieldStrengthOpacityMultiplier, 1f, visualShieldStrength);
                    shieldEffect.Parameters["shieldOpacity"].SetValue(finalShieldOpacity);
                    shieldEffect.Parameters["shieldEdgeBlendStrenght"].SetValue(4f);

                    Color shieldColor = new Color(24, 156, 204); // #189CCC
                    Color primaryEdgeColor = shieldColor;
                    Color secondaryEdgeColor = new Color(34, 224, 227); // #22E0E3                   

                    // Final shield edge color, which lerps about
                    Color edgeColor = CalamityUtils.MulticolorLerp(Main.GlobalTimeWrappedHourly * 0.2f, primaryEdgeColor, secondaryEdgeColor);

                    // Define shader parameters for shield color
                    shieldEffect.Parameters["shieldColor"].SetValue(shieldColor.ToVector3());
                    shieldEffect.Parameters["shieldEdgeColor"].SetValue(edgeColor.ToVector3());

                    // GOD I LOVE END BEGIN CAN THIS GAME PLEASE BE SWALLOWED BY THE FIRES OF HELL THANKS
                    // yes I copy pasted that comment, I hate end begin that much
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, shieldEffect, Main.GameViewMatrix.TransformationMatrix);
                }

                alreadyDrawnShieldForPlayer = true;
                //modPlayer.drawnAnyShieldThisFrame = true;

                // Fetch shield noise overlay texture (this is the polygons fed to the shader)
               
                Vector2 pos = player.MountedCenter + player.gfxOffY * Vector2.UnitY - Main.screenPosition;
                Texture2D tex = NoiseTex;
                Main.spriteBatch.Draw(tex, pos, null, Color.White, 0, tex.Size() / 2f, scale, 0, 0);
            }

            if (alreadyDrawnShieldForPlayer)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            }
        }


        public override void Load()
        {
            On_Main.DrawInfernoRings += On_Main_DrawInfernoRings;
        }

        private void On_Main_DrawInfernoRings(On_Main.orig_DrawInfernoRings orig, Main self)
        {
            orig(self);
                bool result = false;
                foreach (Player player in Main.ActivePlayers)
                {
                    if (player.outOfRange || player.dead)
                        continue;

                    ShintoArmorPlayer modPlayer = player.GetModPlayer<ShintoArmorPlayer>();

                    // Do not render the shield if its visibility is off (or it does not exist)
                    bool isVanityOnly = modPlayer.ShadowShieldVisible && !modPlayer.SetActive;
                    bool shieldExists = isVanityOnly || modPlayer.barrier > 0;
                    bool shouldntDraw = !modPlayer.ShadowShieldVisible || !shieldExists;
                    result |= !shouldntDraw;
                }
            if (result == true)
            {
                DrawDyeableShader(Main.spriteBatch);
            }
                
            
        }

        public override void AddRecipes()
		{
            if (ModLoader.TryGetMod("CalamityHunt", out Mod CalamityHunt))
            {
                CreateRecipe()
                .AddIngredient<DemonshadeBreastplate>()
                .AddIngredient<TheSponge>()
                .AddIngredient(ItemID.NinjaShirt)
                .AddIngredient(ItemID.CrystalNinjaChestplate)
                .AddIngredient(CalamityHunt.Find<ModItem>("ShogunChestplate").Type)

                .AddTile<DraedonsForge>()
                .Register();
            }


        }
    }
}
