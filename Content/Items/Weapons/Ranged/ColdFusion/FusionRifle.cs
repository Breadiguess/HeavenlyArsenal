using CalamityMod;
using HeavenlyArsenal.Common;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Items.Misc;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Items;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Content.Tiles;
using NoxusBoss.Core.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using HeavenlyArsenal.Utilities;
using HeavenlyArsenal.Utilities.Extensions;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static NoxusBoss.Assets.GennedAssets.Sounds;
using Player = Terraria.Player;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ColdFusion
{
    // This is a basic item template.
    // Please see tModLoader's ExampleMod for every other example:
    // https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
    public class FusionRifle : ModItem
    {
        //public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override string LocalizationCategory => "Items.Weapons.Ranged";

        public Texture2D FusionRifle_Backpack { get; private set; }

        public static int ShootDelay = 32;

        public static int BoltsPerBurst= 5;

        public static int MaxChargeTime = 110;
 
        public static LocalizedText infoTooltip
        {
            get;
            private set;
        }
        public override void SetStaticDefaults()
        { 
            ItemID.Sets.IsRangedSpecialistWeapon[Item.type] = true;
            ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Item.type] = true;
            ItemID.Sets.gunProj[Item.type] = true;

            infoTooltip = this.GetLocalization("ShiftTooltip").WithFormatArgs(BoltsPerBurst, ShootDelay, MaxChargeTime);
        }

        public override void SetDefaults()
        {
            
            Item.rare = ModContent.RarityType<NamelessDeityRarity>();

            Item.damage = 4233;
            Item.crit = 46;
            Item.DamageType = DamageClass.Ranged;
            Item.shootSpeed = 40f;
            Item.width = Item.height = 40;
            Item.useTime = 4;
            Item.reuseDelay = 0;
            Item.useAmmo = AmmoID.Gel;
            Item.useAnimation = 0;
            
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FusionRifleHoldout>();
            
           
            Item.ChangePlayerDirectionOnShoot = true;
            Item.noMelee = true;
            Item.Calamity().devItem = true;
            Item.noUseGraphic = true;
            Item.useTurn = true;
            Item.channel = true;
        }
        public bool FusionOut(Player player) => player.ownedProjectileCounts[Item.shoot] > 0;


        public override void HoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (!FusionOut(player))
                {
                    Projectile spear = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
                    spear.rotation = -MathHelper.PiOver2 + 1f * player.direction;
                }
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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            
            if (Main.keyState.PressingShift())
            {
                // Remove the default tooltips.
                tooltips.RemoveAll(t => t.Name.Contains("Tooltip"));

                // Generate and use custom tooltips.
                string specialTooltip = this.GetLocalizedValue("ShiftTooltip");
                TooltipLine[] tooltipLines = specialTooltip.Split('\n').Select((t, index) =>
                {
                    
                    return new TooltipLine(Mod, $"ShiftTooltip{index + 1}", t);
                }).ToArray();

                // Color the last tooltip line.
                tooltipLines.Last().OverrideColor = DialogColorRegistry.NamelessDeityTextColor;
                tooltips.AddRange(tooltipLines);
                return;
            }

            // Make the final tooltip line about needing to pass the test use Nameless' dialog.
            TooltipLine tooltip = tooltips.FirstOrDefault(t => t.Name == "Tooltip1");
            if (tooltip is not null)
                
                tooltip.OverrideColor = DialogColorRegistry.NamelessDeityTextColor;
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

            Vector2 position = drawInfo.GetBodyDrawPosition() + new Vector2(-16 * drawInfo.drawPlayer.direction, -1 * drawInfo.drawPlayer.gravDir);
            position = position.Floor();
            
            var offset = Main.OffsetsPlayerHeadgear[drawInfo.drawPlayer.bodyFrame.Y / drawInfo.drawPlayer.bodyFrame.Height];
        
            offset.Y -= 2f;

            var direction = drawInfo.playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt();
        
            position -= offset * direction;

            Vector2 aPos = position + new Vector2(9 * drawInfo.drawPlayer.direction, -18 * drawInfo.drawPlayer.gravDir);
            

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
                drawInfo.drawPlayer.legFrame.Y / drawInfo.drawPlayer.legFrame.Height);

            DrawData item = new DrawData(texture, position, itemFrame, Lighting.GetColor(drawInfo.drawPlayer.MountedCenter.ToTileCoordinates()) * (1f - drawInfo.shadow), drawInfo.drawPlayer.bodyRotation, itemFrame.Size() * 0.5f, 1f, drawInfo.playerEffect);
            drawInfo.DrawDataCache.Add(item);
        }
    }
    public class FusionRifle_ShiftText : ModSystem
    {
       // private static ulong textID
       // {
       //     get
       //     {
       //         ulong result = 0uL;

       //         for (int i = 0; i < Main.LocalPlayer.name.Length; i++)
       //         {
       //             char nameCharacter = Main.LocalPlayer.name[i];
       //             unchecked
       //             {
       //                 result += (ulong)nameCharacter << i * 4;
       //             }
       //         }

       //         return result;
       //     }
       // }

        public static bool LookingAtItem
        {
            get;
            set;
        }

        public static float SeedTimer
        {
            get;
            private set;
        }

        public static int Seed1 => (int)(SeedTimer % 100000f);

        public static int Seed2 => Seed1 + 1;

        public static float SeedInterpolant => SeedTimer % 1f;

        // When enabled, lore is "personalized", with the Nameless Deity lore entry, varying based on the player's steam ID and only changing across long timespans.
        // When disabled, lore text slowly shifts and becomes something completely different if the player stops reading the text and then starts reading again later.
        

        // There is an exceedingly rare chance for a given lore text line to be manually replaced with special text.
        // Text affected by this is colored separately from everything else.
       // public const int EasterEggLineChance = 10000;

        

        public override void UpdateUI(GameTime gameTime)
        {
            // Lock the seeds in place if trolling mode is enabled.
            
          
            // Ensure that the seed timer cycles naturally otherwise.
            
                SeedTimer += LookingAtItem ? 0.003f : 1f;
                if (SeedTimer >= 2000f)
                    SeedTimer = 0f;
            

            // If the lore item isn't being looked at, reset the seed interpolant to zero by removing the fractional part.
            // This way, if the player looks at the lore text again it won't be in the middle of blending between two dialog sets.
            if (!LookingAtItem)
                SeedTimer = (int)SeedTimer;

            // Reset the looking at Nameless Deity lore item bool for the next frame.
            LookingAtItem = false;
        }
    }

    public class FusionRiflePlayer : ModPlayer
    {
        public bool FusionRifleHeld
        => ModContent
           .GetInstance<FusionRifle>()
           .FusionOut(Player);
        public bool ControlledBurstActive;

        public float BurstCounter;

        public float ControlledBurstTimer;

        public float BurstTier;

        public int MaxBurstTier = 10;
        public float VolCount; //something that says "you need to hit this amount of bursts before getting volatile rounds
        public bool VolatileRounds;
        public float VolatileRoundTimer;
        public override void PreUpdate()
        {
            

        }
        public override void PostUpdateMiscEffects()
        {
            if (VolatileRounds)
            {
                if(VolatileRoundTimer < 1)
                {
                    VolatileRounds = false;
                }
                VolatileRoundTimer--;
                
            }
            //Main.NewText($"FusionRifleHeld: {FusionRifleHeld} | BurstCounter: {BurstCounter} | Controlled Burst Active: {ControlledBurstActive} | ");
            if (FusionRifleHeld && BurstCounter % FusionRifle.BoltsPerBurst == 0 && BurstCounter != 0)
            {
                BurstCounter = 0;
                ControlledBurstTimer = 120f;
                ControlledBurstActive = true;
                if(BurstTier < MaxBurstTier)
                    BurstTier++;
                if (!VolatileRounds)
                {
                    VolCount+= 0.025f + BurstTier/100;
                    
                }
            }

            if(VolCount > 0.6 && !VolatileRounds && Main.rand.NextBool(VolCount))
            {
                VolatileRounds = true;
                VolatileRoundTimer = 360;
                VolCount = 0;
            }
            if (ControlledBurstTimer > 0 && ControlledBurstActive)
            {
                ControlledBurstTimer--;
            }
            if(ControlledBurstActive && ControlledBurstTimer== 0)
            {
                BurstTier = 0;
                SoundEngine.PlaySound(Genesis.Grow);
                ControlledBurstActive = false;


            }
            if (VolCount > 0)
                VolCount -= 0.0005f;
            else
                VolCount =0;
        }
    }

}
