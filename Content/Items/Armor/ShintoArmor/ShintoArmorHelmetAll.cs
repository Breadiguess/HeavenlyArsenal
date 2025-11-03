using CalamityMod;
using CalamityMod.Graphics.Renderers;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Items.Armor.Statigel;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Common;
using HeavenlyArsenal.Common.Utilities;
using HeavenlyArsenal.Content.Items.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.Graphics;
using rail;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Player = Terraria.Player;

namespace HeavenlyArsenal.Content.Items.Armor.ShintoArmor
{
    // The AutoloadEquip attribute automatically attaches an equip texture to this item.
    // Providing the EquipType.Head value here will result in TML expecting a X_Head.png file to be placed next to the item's main texture.
    [AutoloadEquip(EquipType.Head)]
    public class ShintoArmorHelmetAll : ModItem
    {
        public static readonly int AdditiveGenericDamageBonus = 20;
        public const float TeleportRange = 2000f;

        // Boosted by Cross Necklace.
        internal static readonly int ShadowVeilIFrames = 80;

        public override string LocalizationCategory => "Items.Armor.ShintoArmor";
       
        public override void SetStaticDefaults()
        {
            // If your head equipment should draw hair while drawn, use one of the following:
            ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = true; // Don't draw the head at all. Used by Space Creature Mask
                                                               // ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true; // Draw hair as if a hat was covering the top. Used by Wizards Hat
                                                               // ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true; // Draw all hair as normal. Used by Mime Mask, Sunglasses
                                                               // ArmorIDs.Head.Sets.DrawsBackHairWithoutHeadgear[Item.headSlot] = true;

            //SetBonusText = this.GetLocalization("SetBonus").WithFormatArgs(AdditiveGenericDamageBonus);
            ArmorIDs.Head.Sets.IsTallHat[Item.headSlot] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 30; // Width of the item
            Item.height = 32; // Height of the item
            Item.value = Item.sellPrice(platinum: 2, gold: 60, silver: 40); // How many coins the item is worth
            Item.rare = ModContent.RarityType<AvatarRarity>();  // The rarity of the item
            Item.defense = 60;
        }

        // IsArmorSet determines what armor pieces are needed for the setbonus to take effect
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<ShintoArmorBreastplate>() && legs.type == ModContent.ItemType<ShintoArmorLeggings>();
        }


        public override void UpdateArmorSet(Player player)
        {

            player.setBonus = Language.GetOrRegister(("Items.Armor.ShintoArmorHelmetAll.SetBonus")).Value;
            player.setBonus = this.GetLocalizedValue("SetBonus");
            player.jumpSpeedBoost += 2f;
            player.GetModPlayer<ShintoArmorPlayer>().SetActive = true;
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.maxMinions += 10;


            //modPlayer.GemTechSet = true;
        }


        public override void UpdateEquip(Player player)
        {

            var modPlayer = player.Calamity();
            modPlayer.laudanum = true;
            modPlayer.heartOfDarkness = true;
            modPlayer.stressPills = true;

            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetCritChance(DamageClass.Generic) += 15;
            player.GetAttackSpeed(DamageClass.Generic) += 0.15f;
            //player.GetModPlayer<ShintoArmorPlayer>().ShadowVeil = true;
        }
        //public override void ModifyTooltips(List<TooltipLine> list) => list.IntegrateHotkey(CalamityKeybinds.SpectralVeilHotKey);
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.keyState.PressingShift())
            {
                string specialTooltip = Main.specialSeedWorld
                    ? this.GetLocalizedValue("LoreTipGFB")
                    : this.GetLocalizedValue("LoreTip");

                // Which word to animate
                string targetWord = "Antishadow"; // Change to whatever you want
                Color animatedColor = GetAnimatedColor();

                // Process each line of the tooltip
                foreach (string line in specialTooltip.Split('\n'))
                {
                    if (line.Contains(targetWord, StringComparison.OrdinalIgnoreCase))
                    {
                        // Find where the target word is in the line
                        int index = line.IndexOf(targetWord, StringComparison.OrdinalIgnoreCase);

                        string before = line.Substring(0, index);
                        string word = line.Substring(index, targetWord.Length);
                        string after = line.Substring(index + targetWord.Length);

                        // Add before part (if any)
                        if (!string.IsNullOrWhiteSpace(before))
                            tooltips.Add(new TooltipLine(Mod, "LoreTipPartBefore", before));

                        // Add the animated word
                        TooltipLine wordLine = new TooltipLine(Mod, "LoreTipWord", word)
                        {
                            OverrideColor = animatedColor
                        };
                        tooltips.Add(wordLine);

                        // Add after part (if any)
                        if (!string.IsNullOrWhiteSpace(after))
                            tooltips.Add(new TooltipLine(Mod, "LoreTipPartAfter", after));


                    }
                    else
                    {
                        // Normal line, no special word
                        if (!string.IsNullOrWhiteSpace(line))
                            tooltips.Add(new TooltipLine(Mod, "LoreTip", line));
                    }
                }

                return;
            }
        }

        private Color GetAnimatedColor()
        {
            // Example rainbow cycle
            float hue = (Main.GameUpdateCount % 360) / 360f;
            return Main.hslToRgb(hue, 1f, 0.5f);
        }

        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
        public override void AddRecipes()
        {


            if (ModLoader.TryGetMod("CalamityHunt", out Mod CalamityHunt))
            {
                CreateRecipe()
                .AddIngredient(ModContent.ItemType<AvatarMaterial>())
                .AddIngredient<DemonshadeHelm>()
                .AddIngredient(ItemID.NinjaHood)
                .AddIngredient(ItemID.CrystalNinjaHelmet)
                .AddIngredient<StatigelHeadMelee>()
                .AddIngredient(CalamityHunt.Find<ModItem>("ShogunHelm").Type)
                .AddIngredient<OccultSkullCrown>()
                .AddTile<DraedonsForge>()
                .Register();
            }
            else
            {
                CreateRecipe()
                .AddIngredient(ModContent.ItemType<AvatarMaterial>())
               .AddIngredient<DemonshadeHelm>()
               .AddIngredient(ItemID.NinjaHood)
               .AddIngredient(ItemID.CrystalNinjaHelmet)
               .AddIngredient<StatigelHeadMelee>()
               .AddIngredient<OccultSkullCrown>()
               .AddTile<DraedonsForge>()
               .Register();
            }

        }
    }

    public class HelmetFauld : PlayerDrawLayer
    {
        
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);


        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.head == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorHelmetAll), EquipType.Head);

        public override bool IsHeadLayer => true;


        /// <summary>
        /// Renders the AoE Void Eyes onto the helmet.
        /// </summary>
        /// <param name="drawInfo"></param>
        /// <param name="modPlayer"></param>
        protected void DrawVoidEyes(ref PlayerDrawSet drawInfo, ShintoArmorPlayer modPlayer)
        {
            Player player = drawInfo.drawPlayer;
            Texture2D facePixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;

            Vector2 baseHeadPos = drawInfo.HeadPosition();
            Vector2 walkOffset =  player.gravDir*Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];

            Vector2[] offsets = new Vector2[]
            {
                    new Vector2(2.5f * player.direction, 0),
                    new Vector2(2.5f * player.direction, -5),
                    new Vector2(-0.75f * player.direction, -2.5f),
                    new Vector2(5.75f * player.direction, -2.5f),
            };
            
            //Utils.DrawBorderString(Main.spriteBatch, $"Frame: {frameIndex},  WalkOffset: {walkOffset}", drawInfo.HeadPosition() + new Vector2(0, 60), Color.LightGreen);

            Color BaseheadColor = Color.Red;
            if (drawInfo.cHead != 0)
            {
                BaseheadColor = drawInfo.colorArmorHead;
            }
            Vector2 GravOffset = new Vector2(0, player.gravDir == 1 ? 0 : 16.5f);
            foreach (var offset in offsets)
            {
                Vector2 drawPos = baseHeadPos + offset + walkOffset + GravOffset;
                DrawData dots = new DrawData(
                    facePixel, drawPos, null, BaseheadColor * (1 - modPlayer.EnrageInterp), 0f, facePixel.Size() * 0.5f, 0.9f, SpriteEffects.None, 0);
                dots.shader = drawInfo.cHead;

               
                drawInfo.DrawDataCache.Add(dots);


                DrawData GlowingEyes = new DrawData(Glow, drawPos, null, BaseheadColor with { A = 0 } * (1 - modPlayer.EnrageInterp), 0f, Glow.Size() * 0.5f, 0.05f, SpriteEffects.None, 0);


               
                GlowingEyes.shader = drawInfo.cHead;
                drawInfo.DrawDataCache.Add(GlowingEyes);


            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="drawInfo"></param>
        /// <param name="modPlayer"> the shinto armor </param>
        protected void DrawFaulds(ref PlayerDrawSet drawInfo, ShintoArmorPlayer modPlayer)
        {
            Texture2D Left = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/ShintoArmor/ShintoArmor_LeftFauld").Value;
            Texture2D Right = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/ShintoArmor/ShintoArmor_RightFauld").Value;

            Vector2 BasePosition = drawInfo.HeadPosition();
            float time = Main.GameUpdateCount * 0.1f;
            float bobRotation = 0;

            float targetRotation = drawInfo.drawPlayer.gravDir * (drawInfo.drawPlayer.velocity.Y * 0.05f);
            bobRotation = MathHelper.Lerp(bobRotation, targetRotation, 0.1f);
            bobRotation += (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.01f;


            #region Left
            float Value = 4 * drawInfo.drawPlayer.direction;
            float GravOffset = drawInfo.drawPlayer.gravDir == 1 ? 2.05f: 5.95f;
            Vector2 LeftFauldPos = BasePosition + new Vector2(Value, GravOffset);

            Vector2 LOrigin = new Vector2(Left.Width / 2,//4*drawInfo.drawPlayer.direction, 
                Left.Height / 21 / 2f);

            SpriteEffects Dah = drawInfo.drawPlayer.direction == 1 ? (drawInfo.drawPlayer.gravDir == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically) : drawInfo.drawPlayer.gravDir == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            if (modPlayer.Enraged)
            {
                Dah = drawInfo.drawPlayer.direction == -1 ? (drawInfo.drawPlayer.gravDir == 1? SpriteEffects.None: SpriteEffects.FlipVertically): drawInfo.drawPlayer.gravDir == 1? SpriteEffects.FlipHorizontally : SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;

                bobRotation = 0;
            }

            DrawData data = new DrawData(Left, LeftFauldPos, drawInfo.drawPlayer.legFrame, Color.White.MultiplyRGB(drawInfo.colorArmorHead), bobRotation, LOrigin, 1f, Dah);

            data.color = drawInfo.colorArmorHead;
            data.shader = drawInfo.cHead;
            drawInfo.DrawDataCache.Add(data);
            
            #endregion
            #region Right
            float Value2 = drawInfo.drawPlayer.direction == -1 ? 0:0;

            float GravOffsetRight = drawInfo.drawPlayer.gravDir == 1 ? 2 : 6;
            Vector2 RightFauldPos = BasePosition + new Vector2(0, modPlayer.Enraged? (drawInfo.drawPlayer.gravDir == 1? 2.05f: 5.95f) : GravOffset);

            Vector2 ROrigin = new Vector2(Right.Width / 2,//4*drawInfo.drawPlayer.direction, 
                Right.Height / 21 / 2f);




            DrawData data2 = new DrawData(Right, RightFauldPos, drawInfo.drawPlayer.legFrame, Color.White.MultiplyRGB(drawInfo.colorArmorHead), -bobRotation, ROrigin, 1f, Dah);
            #endregion

            data2.color = drawInfo.colorArmorHead;
            data2.shader = drawInfo.cHead;
            drawInfo.DrawDataCache.Add(data2);
        }

        protected void DrawDeathMask(ref PlayerDrawSet drawInfo, ShintoArmorPlayer modPlayer)
        {
            Texture2D Mask = GennedAssets.Textures.SecondPhaseForm.AvatarOfEmptiness;

            Player player = drawInfo.drawPlayer;
           

            Vector2 baseHeadPos = drawInfo.HeadPosition();
            Vector2 walkOffset = player.gravDir * Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];
            Vector2 MaskOrigin = new Vector2(Mask.Width / 2, Mask.Height /24/2);
            Vector2 DrawOffset = new Vector2(2.5f * player.direction, -4);
            Vector2 drawPos = baseHeadPos + walkOffset + DrawOffset;

            Rectangle MaskFrame = Mask.Frame(1,24, 0, 23);
            //Utils.DrawBorderString(Main.spriteBatch, $"Frame: {frameIndex},  WalkOffset: {walkOffset}", drawInfo.HeadPosition() + new Vector2(0, 60), Color.LightGreen);

            Vector2 MaskSize = new Vector2(0.075f, 0.08f);

            SpriteEffects flip = player.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            DrawData dots = new DrawData(Mask, drawPos, MaskFrame, Color.White * (modPlayer.EnrageInterp),
                MathHelper.ToRadians(-12.5f * player.direction), MaskOrigin, MaskSize, flip, 0);
            dots.shader = drawInfo.cHead;
           
            drawInfo.DrawDataCache.Add(dots);




            
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            ShintoArmorPlayer sPlayer = drawInfo.drawPlayer.GetModPlayer<ShintoArmorPlayer>();

            Texture2D DEBUG = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            //drawInfo.drawPlayer.legFrame.Y = 12 * drawInfo.drawPlayer.legFrame.Height;
           
           
            DrawFaulds(ref drawInfo, sPlayer);
            DrawVoidEyes(ref drawInfo, sPlayer);
            DrawDeathMask(ref drawInfo, sPlayer);
        }
    }
    public class Helmetwings : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.HeadBack);


        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.GetModPlayer<ShintoArmorPlayer>().EnrageInterp > 0.1;//drawInfo.drawPlayer.head == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorHelmetAll), EquipType.Head);

        public override bool IsHeadLayer => false;

        protected void DrawHaloThing(ref PlayerDrawSet drawInfo, ShintoArmorPlayer modPlayer)
        {

            Player player = drawInfo.drawPlayer;
            Texture2D tex = GennedAssets.Textures.GreyscaleTextures.WhitePixel;//ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Particles/Metaballs/BasicCircle").Value;
            //GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint ;
            Vector2 baseHeadPos = drawInfo.HeadPosition() + new Vector2(0, player.gravDir == 1? 0: 6);
            Vector2 walkOffset = Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];

            Vector2 drawPos = baseHeadPos + walkOffset;

            float val = (float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly)) ;
            
            float xOffset = -6 * player.direction;

            Vector2 LevitateOffset = new Vector2(xOffset, ((-30 + val*4) * modPlayer.EnrageInterp) * player.gravDir);
            drawPos += LevitateOffset;

            Color GlowColor = Color.Lerp(Color.Red, Color.DarkRed, val);
            if (drawInfo.cHead != 0)
            {
                GlowColor = Color.White;
            }


            DrawData data = new DrawData(tex, drawPos, null, GlowColor * modPlayer.EnrageInterp, MathHelper.ToRadians(45), tex.Size()*0.5f, (20f * modPlayer.EnrageInterp), SpriteEffects.None);
            data.shader = drawInfo.cHead;
            drawInfo.DrawDataCache.Add(data);
           

            float Value = (float)Math.Clamp(Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly)),0.55f, 1);


            Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall;

            DrawData GlowingOrb = new DrawData(Glow, drawPos, null, GlowColor with { A = 0 }, 0, Glow.Size() * 0.5f, (Value*0.5f) * modPlayer.EnrageInterp, SpriteEffects.None);

            GlowingOrb.shader = drawInfo.cHead;
            drawInfo.DrawDataCache.Add(GlowingOrb);

        }
        protected void DrawWarthings(ref PlayerDrawSet drawInfo, ShintoArmorPlayer modPlayer)
        {
            Player player = drawInfo.drawPlayer;
            Texture2D Warthings = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/ShintoArmor/ShintoArmor_Warthings").Value;

            Vector2 baseHeadPos = drawInfo.HeadPosition() + new Vector2(0, player.gravDir == 1 ? 0 : 6);
            Vector2 walkOffset = Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];

            Vector2 drawPos = baseHeadPos + walkOffset;



            float val = (float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly)*10);

            float DrawOffset = player.direction == 1 ? -4 : 4;

            Vector2 drawPos3 = drawPos + new Vector2(-15 * modPlayer.EnrageInterp + DrawOffset,( -12 * modPlayer.EnrageInterp) * player.gravDir);
            float Rot = MathHelper.ToRadians(-60 + -val) * (modPlayer.EnrageInterp) * player.gravDir;


            SpriteEffects Gravity = player.gravDir == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            DrawData warthings = new DrawData(Warthings, drawPos3, null, Color.White.MultiplyRGB(drawInfo.colorArmorHead), Rot, Warthings.Size() * 0.5f, 1f, Gravity, 0);
            drawInfo.DrawDataCache.Add(warthings);
            warthings.shader = drawInfo.cHead;

            float Rot2 = MathHelper.ToRadians(60 + val) * (modPlayer.EnrageInterp) * player.gravDir; 

            
            Vector2 drawPos2 = drawPos + new Vector2(15 * modPlayer.EnrageInterp + DrawOffset, (-12 * modPlayer.EnrageInterp) * player.gravDir);

            SpriteEffects Gravity2 = player.gravDir == 1 ? SpriteEffects.FlipHorizontally: SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;
            DrawData SecondWarthing = new DrawData(Warthings, drawPos2, null, Color.White.MultiplyRGB(drawInfo.colorArmorHead), Rot2, Warthings.Size() * 0.5f, 1, Gravity2);

            drawInfo.DrawDataCache.Add(SecondWarthing);
            SecondWarthing.shader = drawInfo.cHead;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            ShintoArmorPlayer sPlayer = drawInfo.drawPlayer.GetModPlayer<ShintoArmorPlayer>();
            if(sPlayer.EnrageInterp !> 0.1)
            {

                
                DrawHaloThing(ref drawInfo, sPlayer);

                if (drawInfo.drawPlayer.head == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorHelmetAll), EquipType.Head))
                DrawWarthings(ref drawInfo, sPlayer);
            }
        }
    }

}
