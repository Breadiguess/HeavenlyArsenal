using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using NoxusBoss.Assets.Fonts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI.Chat;
using System.Collections.Generic;
using NoxusBoss.Content.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using NoxusBoss.Content.Tiles;
using CalamityMod;
using static NoxusBoss.Assets.GennedAssets.Sounds;
using System.Runtime.CompilerServices;
using Terraria.Audio;
using HeavenlyArsenal.Content.Items.Misc;
using HeavenlyArsenal.Content.Projectiles.Holdout;
using System;
using Terraria.DataStructures;
using ReLogic.Content;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Common.Utilities;
using ReLogic.Graphics;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged
{
    // This is a basic item template.
    // Please see tModLoader's ExampleMod for every other example:
    // https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
    public class FusionRifle : ModItem
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public Texture2D FusionRifle_Backpack { get; private set; }

        public const int ShootDelay = 32;

        public const int ArrowsPerBurst = 5;

        public const int ArrowShootRate = 4;

        public const int ArrowShootTime = ArrowsPerBurst * ArrowShootRate;

        public const int MaxChargeTime = 120;

        public const float ArrowTargetingRange = 1100f;

        public const float MaxChargeDamageBoost = 3.5f;

        public const float LightningDamageFactor = 0.36f;

        public const float ChargeLightningCreationThreshold = 0.8f;

        public static readonly SoundStyle ChargeSound = new("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_charge3");
            
        public static readonly SoundStyle FireSound = new("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_fire");

        public static readonly SoundStyle FullyChargedSound = new("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_FullyCharged");


        //public readonly new Texture2D FusionRifle_Backpack = 

        public override void SetStaticDefaults()
        { 
        }
        // The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.HeavenlyArsenal.hjson' file.
        public override void SetDefaults()
        {
            
            Item.rare = ModContent.RarityType<NamelessDeityRarity>();

            Item.damage = 50000;
            Item.DamageType = DamageClass.Ranged;
            Item.shootSpeed = 40f;
            Item.width = 40;
            Item.height = 32;
            Item.useTime = 4;
            Item.reuseDelay = 0;

            

            Item.useAmmo = AmmoID.Gel;

            //Item.consumeAmmoOnFirstShotOnly = true;
            Item.useAnimation = 0;
            Item.noUseGraphic = true;
            Item.useTurn = true;
            Item.channel = true;


            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6;
            
            //Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            //Item.shoot = ModContent.ProjectileType<ParasiteParadiseProjectile>();
            Item.shoot = ModContent.ProjectileType<FusionRifleHoldout>();
            Item.ChangePlayerDirectionOnShoot = true;
            Item.crit = 662;
            Item.noMelee = true;
            Item.Calamity().devItem = true;
           
        }
        private bool fusionOut = false;
        public override void HoldItem(Player player)
        {
            if (!fusionOut)
            {
                // Spawn the projectile
                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item), // Source of the projectile
                    player.Center.X,               // X coordinate of the spawn location
                    player.Center.Y,               // Y coordinate of the spawn location
                    0f, 0f,                        // Velocity (set to 0 for stationary)
                    ModContent.ProjectileType<FusionRifleHoldout>(), // Type of the projectile
                    Item.damage,                   // Damage of the projectile
                    Item.knockBack,                // Knockback of the projectile
                    player.whoAmI                  // Owner of the projectile
                );
                fusionOut = true; // Set the flag to true to prevent further spawns
            }
        }


        public override void UpdateInventory(Player player)
        {
            // Reset the flag if the item is not being held
            if (player.HeldItem.type != Item.type)
            {
               fusionOut = false;
            }
        }

       

        public override bool CanShoot(Player player) => false;

      
        public override void AddRecipes()
        {
            CreateRecipe().
                AddTile<GardenFountainTile>().
                AddIngredient(ModContent.ItemType<Incomplete_gun>()).
                AddIngredient(ModContent.ItemType<MetallicChunk>()).
                Register();
        }

        internal static void DrawHeldShiftTooltip(List<TooltipLine> tooltips, TooltipLine[] holdShiftTooltips, bool hideNormalTooltip = false)
        {
            // Do not override anything if the Left Shift key is not being held.
            if (!Main.keyState.IsKeyDown(Keys.LeftShift))
                return;

            // Acquire base tooltip data.
            int firstTooltipIndex = -1;
            int lastTooltipIndex = -1;
            int standardTooltipCount = 0;
            for (int i = 0; i < tooltips.Count; i++)
            {
                if (tooltips[i].Name.StartsWith("Tooltip"))
                {
                    if (firstTooltipIndex == -1)
                    {
                        firstTooltipIndex = i;
                    }
                    lastTooltipIndex = i;
                    standardTooltipCount++;
                }
            }

            // Replace tooltips.
            if (firstTooltipIndex != -1)
            {
                if (hideNormalTooltip)
                {
                    tooltips.RemoveRange(firstTooltipIndex, standardTooltipCount);
                    lastTooltipIndex -= standardTooltipCount;
                }
                tooltips.InsertRange(lastTooltipIndex + 1, holdShiftTooltips);
            }
        }


        public static Asset<Texture2D> backTexture;

        public override void Load()
        {
            backTexture = AssetUtilities.RequestImmediate<Texture2D>(Texture + "_Backpack");
            
        }
    }

    public class FusionRifle_BackpackLayer : PlayerDrawLayer
    {
        
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Backpacks);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.HeldItem.type == ModContent.ItemType<FusionRifle>();// && VanityUtilities.NoBackpackOn(ref drawInfo);

        private int frame;

        private int frameCounter;

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Texture2D texture = FusionRifle.backTexture.Value;
            //Texture2D swirlTexture = FusionRifle.backSwirlTexture.Value;
            //Texture2D antennaTexture = FusionRifle.backAntennaTexture.Value;

            Vector2 vec5 = drawInfo.BodyPosition() + new Vector2(-16 * drawInfo.drawPlayer.direction, -1 * drawInfo.drawPlayer.gravDir);
            vec5 = vec5.Floor();
            vec5.ApplyVerticalOffset(drawInfo);

            Vector2 aPos = vec5 + new Vector2(9 * drawInfo.drawPlayer.direction, -18 * drawInfo.drawPlayer.gravDir);
            

            //if (drawInfo.shadow == 0f)
           // {
           //     if (frameCounter++ > 5)
            //    {
            //        frame = (frame + 1) % 5;
                    frameCounter = 0;
            //    }
           // }

           // DrawData swirl = new DrawData(swirlTexture, vec5, swirlTexture.Frame(1, 5, 0, frame), Color.White * (1f - drawInfo.shadow), drawInfo.drawPlayer.bodyRotation, swirlTexture.Frame(1, 5, 0, frame).Size() * 0.5f, 1f, drawInfo.playerEffect);
           // drawInfo.DrawDataCache.Add(swirl);

            Rectangle itemFrame = texture.Frame(1, 1, 0, 
                (int)(drawInfo.drawPlayer.legFrame.Y / drawInfo.drawPlayer.legFrame.Height));

            DrawData item = new DrawData(texture, vec5, itemFrame, Lighting.GetColor(drawInfo.drawPlayer.MountedCenter.ToTileCoordinates()) * (1f - drawInfo.shadow), drawInfo.drawPlayer.bodyRotation, itemFrame.Size() * 0.5f, 1f, drawInfo.playerEffect);
            drawInfo.DrawDataCache.Add(item);
        }
    }

}
