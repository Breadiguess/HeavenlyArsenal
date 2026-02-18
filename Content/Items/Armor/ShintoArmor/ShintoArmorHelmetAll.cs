using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Items.Armor.Statigel;
using CalamityMod.Tiles.Furniture.CraftingStations;
using HeavenlyArsenal.Common.Utilities;
using HeavenlyArsenal.Content.Items.Materials;
using HeavenlyArsenal.Utilities.Extensions;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Player = Terraria.Player;

namespace HeavenlyArsenal.Content.Items.Armor.ShintoArmor;

[AutoloadEquip(EquipType.Head)]
public class ShintoArmorHelmetAll : ModItem
{
    public const float TeleportRange = 2000f;

    public static readonly int AdditiveGenericDamageBonus = 20;

    private static readonly int MaxMinionIncrease = 10;

    public const string HelmetEquippedName = "ShintoHelmetEquipped";
    public override string LocalizationCategory => "Items.Armor.ShintoArmor";

    // Boosted by Cross Necklace.
    internal static readonly int ShadowVeilIFrames = 80;

    public override void SetStaticDefaults()
    {
        ArmorIDs.Head.Sets.IsTallHat[Item.headSlot] = true;
        ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
        ArmorIDs.Head.Sets.PreventBeardDraw[Item.headSlot] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 32;
        Item.value = Item.sellPrice(7, 60, 40);
        Item.rare = ModContent.RarityType<AvatarRarity>();
        Item.defense = 60;
    }

    public override void AddRecipes()
    {
        var recipe = CreateRecipe()
            .AddIngredient(ModContent.ItemType<AvatarMaterial>())
            .AddIngredient<DemonshadeHelm>()
            .AddIngredient(ItemID.NinjaHood)
            .AddIngredient(ItemID.CrystalNinjaHelmet)
            .AddIngredient<StatigelHeadMelee>()
            .AddIngredient<OccultSkullCrown>()
            .AddTile<DraedonsForge>();

        RecipeExtensions.TryAddIngredient(recipe, "CalamityHunt", "ShogunHelm");

        recipe.Register();
    }

    public override bool IsArmorSet(Item head, Item body, Item legs)
    {
        return body.type == ModContent.ItemType<ShintoArmorBreastplate>() && legs.type == ModContent.ItemType<ShintoArmorLeggings>();
    }

    public override void UpdateArmorSet(Player player)
    {
        player.GetModPlayer<ShintoArmorIKArms>().Active = true;
        player.GetModPlayer<ShintoWingManager>().Active = true;
        player.GetModPlayer<ShintoArmorBarrier>().BarrierActive = true;
        player.GetModPlayer<ShintoArmorAvatarFall>().Active = true;
        player.setBonus = Language.GetOrRegister("Items.Armor.ShintoArmorHelmetAll.SetBonus").Value;
        player.setBonus = this.GetLocalizedValue("SetBonus");
        player.jumpSpeedBoost += 2f;
        //player.GetModPlayer<ShintoArmorPlayer>().SetActive = true;
        player.GetDamage(DamageClass.Generic) += 0.18f;
        player.maxMinions += MaxMinionIncrease;
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

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D facePixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
        Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;

        var EyeColor = Color.Crimson with
        {
            A = 0
        }; //* MathF.Sin(Main.GlobalTimeWrappedHourly);

        var offsets = new[]
        {
            new Vector2(2.5f, 4f),
            new Vector2(2.5f, 8f),
            new Vector2(-0.75f, 6f),
            new Vector2(5.75f, 6f)
        };

        foreach (var vect in offsets)
        {
            var DrawPos = position + vect * scale;
            Main.EntitySpriteDraw(facePixel, DrawPos, null, EyeColor, 0, facePixel.Size() / 2, scale, 0);
            Main.EntitySpriteDraw(Glow, DrawPos, null, EyeColor, 0, Glow.Size() / 2, scale * 0.05f, 0);
        }
    }

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        base.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
        Texture2D facePixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
        Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;

        var EyeColor = Color.Crimson with
                       {
                           A = 0
                       } *
                       MathF.Sin(Main.GlobalTimeWrappedHourly);

        var offsets = new[]
        {
            new Vector2(2.5f, 3.5f),
            new Vector2(2.5f, 7.5f),
            new Vector2(-0.75f, 5.5f),
            new Vector2(5.75f, 5.5f)
        };

        foreach (var vect in offsets)
        {
            var Adjust = Item.position + new Vector2(Item.width / 2, Item.height / 2) + new Vector2(0, 2.5f);
            var DrawPos = Adjust + vect - Main.screenPosition;
            Main.EntitySpriteDraw(facePixel, DrawPos, null, EyeColor, 0, facePixel.Size() / 2, 1, 0);
            Main.EntitySpriteDraw(Glow, DrawPos, null, EyeColor, 0, Glow.Size() / 2, 0.05f, 0);
        }
    }
}

public class HelmetFauld : PlayerDrawLayer
{
    public override bool IsHeadLayer => true;

    public override Position GetDefaultPosition()
    {
        return new AfterParent(PlayerDrawLayers.Head);
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return drawInfo.drawPlayer.head == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorHelmetAll), EquipType.Head);
    }


    public static InstancedRequestableTarget HaloTarget { get; private set; }

    public override void Load()
    {
        On_PlayerDrawLayers.DrawPlayer_21_Head_TheFace += hmm;
        //On_Player.UpdateItemDye += FindSkirtItemDyeShader;

        HaloTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(HaloTarget);
    }

    private void hmm(On_PlayerDrawLayers.orig_DrawPlayer_21_Head_TheFace orig, ref PlayerDrawSet drawinfo)
    {
        if (drawinfo.drawPlayer.GetValueRef<bool>(ShintoArmorHelmetAll.HelmetEquippedName))
        {
            hmm(orig, ref drawinfo);
            return;
        }
        orig(ref drawinfo);
    }

    

    /// <summary>
    ///     Renders the AoE Void Eyes onto the helmet.
    /// </summary>
    /// <param name="drawInfo"></param>
    /// <param name="modPlayer"></param>
    protected void DrawVoidEyes(ref PlayerDrawSet drawInfo, ShintoArmorPlayer modPlayer)
    {
        var player = drawInfo.drawPlayer;
        Texture2D facePixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
        Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;

        var baseHeadPos = drawInfo.HeadPosition();
        var walkOffset = player.gravDir * Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];

        var offsets = new[]
        {
            new Vector2(2.5f * player.direction, 0),
            new Vector2(2.5f * player.direction, -5),
            new Vector2(-0.75f * player.direction, -2.5f),
            new Vector2(5.75f * player.direction, -2.5f)
        };

        //Utils.DrawBorderString(Main.spriteBatch, $"Frame: {frameIndex},  WalkOffset: {walkOffset}", drawInfo.HeadPosition() + new Vector2(0, 60), Color.LightGreen);

        var BaseheadColor = Color.Red;

        if (drawInfo.cHead != 0)
        {
            BaseheadColor = drawInfo.colorArmorHead.MultiplyRGB(Color.WhiteSmoke);
        }

        var GravOffset = new Vector2(0, player.gravDir == 1 ? 0 : 16.5f);

        foreach (var offset in offsets)
        {
            var drawPos = baseHeadPos + offset.RotatedBy(player.headRotation) + walkOffset.RotatedBy(player.headRotation) + GravOffset;

            var dots = new DrawData
            (
                facePixel,
                drawPos,
                null,
                BaseheadColor * (1 - modPlayer.EnrageInterp),
                0f,
                facePixel.Size() * 0.5f,
                0.9f,
                SpriteEffects.None
            );

            dots.shader = drawInfo.cHead;

            drawInfo.DrawDataCache.Add(dots);

            if (player.Calamity().adrenalineModeActive)
            {
                continue;
            }

            var GlowingEyes = new DrawData
            (
                Glow,
                drawPos,
                null,
                BaseheadColor with
                {
                    A = 0
                } *
                (1 - modPlayer.EnrageInterp),
                0f,
                Glow.Size() * 0.5f,
                0.05f,
                SpriteEffects.None
            );

            GlowingEyes.shader = drawInfo.cHead;
            drawInfo.DrawDataCache.Add(GlowingEyes);
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="drawInfo"></param>
    /// <param name="modPlayer"> the shinto armor </param>
    protected void DrawFaulds(ref PlayerDrawSet drawInfo, ShintoArmorPlayer modPlayer)
    {
        var Left = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/ShintoArmor/ShintoArmor_LeftFauld").Value;
        var Right = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/ShintoArmor/ShintoArmor_RightFauld").Value;

        var BasePosition = drawInfo.HeadPosition();
        var time = Main.GameUpdateCount * 0.1f;
        float bobRotation = 0;

        var targetRotation = drawInfo.drawPlayer.gravDir * (drawInfo.drawPlayer.velocity.Y * 0.05f);
        bobRotation = MathHelper.Lerp(bobRotation, targetRotation, 0.1f);
        bobRotation += (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.01f;

        #region Left

        float Value = 4 * drawInfo.drawPlayer.direction;
        var GravOffset = drawInfo.drawPlayer.gravDir == 1 ? 2.05f : 5.95f;
        var LeftFauldPos = BasePosition + new Vector2(Value, GravOffset);

        var LOrigin = new Vector2
        (
            Left.Width / 2, //4*drawInfo.drawPlayer.direction, 
            Left.Height / 21 / 2f
        );

        var Dah = drawInfo.drawPlayer.direction == 1 ? drawInfo.drawPlayer.gravDir == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically :
            drawInfo.drawPlayer.gravDir == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;

        if (modPlayer.Enraged)
        {
            Dah = drawInfo.drawPlayer.direction == -1 ? drawInfo.drawPlayer.gravDir == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically :
                drawInfo.drawPlayer.gravDir == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;

            bobRotation = 0;
        }

        var data = new DrawData(Left, LeftFauldPos, drawInfo.drawPlayer.legFrame, Color.White.MultiplyRGB(drawInfo.colorArmorHead), bobRotation, LOrigin, 1f, Dah);

        data.color = drawInfo.colorArmorHead;
        data.shader = drawInfo.cHead;
        drawInfo.DrawDataCache.Add(data);

        #endregion

        #region Right

        float Value2 = drawInfo.drawPlayer.direction == -1 ? 0 : 0;

        float GravOffsetRight = drawInfo.drawPlayer.gravDir == 1 ? 2 : 6;
        var RightFauldPos = BasePosition + new Vector2(0, modPlayer.Enraged ? drawInfo.drawPlayer.gravDir == 1 ? 2.05f : 5.95f : GravOffset);

        var ROrigin = new Vector2
        (
            Right.Width / 2, //4*drawInfo.drawPlayer.direction, 
            Right.Height / 21 / 2f
        );

        var data2 = new DrawData(Right, RightFauldPos, drawInfo.drawPlayer.legFrame, Color.White.MultiplyRGB(drawInfo.colorArmorHead), -bobRotation, ROrigin, 1f, Dah);

        #endregion

        data2.color = drawInfo.colorArmorHead;
        data2.shader = drawInfo.cHead;
        drawInfo.DrawDataCache.Add(data2);
    }

    protected void DrawDeathMask(ref PlayerDrawSet drawInfo, ShintoArmorPlayer modPlayer)
    {
        Texture2D Mask = GennedAssets.Textures.SecondPhaseForm.AvatarOfEmptiness;

        var player = drawInfo.drawPlayer;

        var baseHeadPos = drawInfo.HeadPosition();
        var walkOffset = player.gravDir * Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];
        var MaskOrigin = new Vector2(Mask.Width / 2, Mask.Height / 24 / 2);
        var DrawOffset = new Vector2(2.5f * player.direction, -4);
        var drawPos = baseHeadPos + walkOffset + DrawOffset;

        var MaskFrame = Mask.Frame(1, 24, 0, 23);
        //Utils.DrawBorderString(Main.spriteBatch, $"Frame: {frameIndex},  WalkOffset: {walkOffset}", drawInfo.HeadPosition() + new Vector2(0, 60), Color.LightGreen);

        var MaskSize = new Vector2(0.075f, 0.08f);

        var flip = player.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        var dots = new DrawData
        (
            Mask,
            drawPos,
            MaskFrame,
            Color.White * modPlayer.EnrageInterp,
            MathHelper.ToRadians(-12.5f * player.direction),
            MaskOrigin,
            MaskSize,
            flip
        );

        dots.shader = drawInfo.cHead;

        drawInfo.DrawDataCache.Add(dots);
    }



    private static void RenderIntoTarget()
    {
        if (GennedAssets.Textures.GreyscaleTextures.WhitePixel.Uninitialized || !GennedAssets.Textures.GreyscaleTextures.WhitePixel.Asset.IsLoaded)
        {
            return;
        }

        if (GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint.Uninitialized || !GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint.Asset.IsLoaded)
        {
            return;
        }
        Texture2D facePixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
        Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;


        var offsets = new[]
        {
            new Vector2(2.5f , 0),
            new Vector2(2.5f, -5),
            new Vector2(-0.75f , -2.5f),
            new Vector2(5.75f, -2.5f)
        };


        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, default, null);

        foreach (var offset in offsets)
        {
            Vector2 pos = offset + new Vector2(WotGUtils.ViewportArea.Width / 2, WotGUtils.ViewportArea.Height / 2);
            Main.EntitySpriteDraw(facePixel, pos, null, Color.White, 0, facePixel.Size()/2, 1,0);
            Main.EntitySpriteDraw(Glow, pos, null, Color.White with { A = 0}, 0, Glow.Size() / 2, 0.03f, 0);
        }

        Main.spriteBatch.End();
    }


    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        var sPlayer = drawInfo.drawPlayer.GetModPlayer<ShintoArmorPlayer>();

        //drawInfo.drawPlayer.legFrame.Y = 12 * drawInfo.drawPlayer.legFrame.Height;

        // drawInfo.drawPlayer.GetModPlayer<HidePlayer>().ShouldHide = true;
        //DrawFaulds(ref drawInfo, sPlayer);
        DrawVoidEyes(ref drawInfo, sPlayer);
        //DrawDeathMask(ref drawInfo, sPlayer);

        return;
        if (drawInfo.shadow != 0f || drawInfo.drawPlayer.dead)
        {
            return;
        }
        HaloTarget.Request(14000, 14000, drawInfo.drawPlayer.whoAmI, RenderIntoTarget);

        if (!HaloTarget.TryGetTarget(drawInfo.drawPlayer.whoAmI, out var portalTexture) || portalTexture is null)
        {
            return;
        }
        var rift = new DrawData(portalTexture, drawInfo.HeadPosition(), null, Color.White, 0, portalTexture.Size() * 0.5f, 2f, 0);
        rift.shader = drawInfo.cHead;
        rift.color = drawInfo.colorArmorHead;
        drawInfo.DrawDataCache.Add(rift);
       
    }
}

public class Helmetwings : PlayerDrawLayer
{
    public override bool IsHeadLayer => false;

    public override Position GetDefaultPosition()
    {
        return new BeforeParent(PlayerDrawLayers.HeadBack);
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return drawInfo.drawPlayer.GetModPlayer<ShintoArmorPlayer>().EnrageInterp > 0.1;
        //drawInfo.drawPlayer.head == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorHelmetAll), EquipType.Head);
    }

    protected void DrawHaloThing(ref PlayerDrawSet drawInfo, ShintoArmorPlayer modPlayer)
    {
        var player = drawInfo.drawPlayer;
        Texture2D tex = GennedAssets.Textures.GreyscaleTextures.WhitePixel; //ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Particles/Metaballs/BasicCircle").Value;
        //GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint ;
        var baseHeadPos = drawInfo.HeadPosition() + new Vector2(0, player.gravDir == 1 ? 0 : 6);
        var walkOffset = Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];

        var drawPos = baseHeadPos + walkOffset;

        var val = (float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly));

        float xOffset = -6 * player.direction;

        var LevitateOffset = new Vector2(xOffset, (-30 + val * 4) * modPlayer.EnrageInterp * player.gravDir);
        drawPos += LevitateOffset;

        var GlowColor = Color.Lerp(Color.Red, Color.DarkRed, val);

        if (drawInfo.cHead != 0)
        {
            GlowColor = Color.White;
        }

        var data = new DrawData(tex, drawPos, null, GlowColor * modPlayer.EnrageInterp, MathHelper.ToRadians(45), tex.Size() * 0.5f, 20f * modPlayer.EnrageInterp, SpriteEffects.None);
        data.shader = drawInfo.cHead;
        drawInfo.DrawDataCache.Add(data);

        var Value = (float)Math.Clamp(Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly)), 0.55f, 1);

        Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall;

        var GlowingOrb = new DrawData
        (
            Glow,
            drawPos,
            null,
            GlowColor with
            {
                A = 0
            },
            0,
            Glow.Size() * 0.5f,
            Value * 0.5f * modPlayer.EnrageInterp,
            SpriteEffects.None
        );

        GlowingOrb.shader = drawInfo.cHead;
        drawInfo.DrawDataCache.Add(GlowingOrb);
    }

    protected void DrawWarthings(ref PlayerDrawSet drawInfo, ShintoArmorPlayer modPlayer)
    {
        var player = drawInfo.drawPlayer;
        var Warthings = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/ShintoArmor/ShintoArmor_Warthings").Value;

        var baseHeadPos = drawInfo.HeadPosition() + new Vector2(0, player.gravDir == 1 ? 0 : 6);
        var walkOffset = Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];

        var drawPos = baseHeadPos + walkOffset;

        var val = (float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly) * 10);

        float DrawOffset = player.direction == 1 ? -4 : 4;

        var drawPos3 = drawPos + new Vector2(-15 * modPlayer.EnrageInterp + DrawOffset, -12 * modPlayer.EnrageInterp * player.gravDir);
        var Rot = MathHelper.ToRadians(-60 + -val) * modPlayer.EnrageInterp * player.gravDir;

        var Gravity = player.gravDir == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
        var warthings = new DrawData(Warthings, drawPos3, null, Color.White.MultiplyRGB(drawInfo.colorArmorHead), Rot, Warthings.Size() * 0.5f, 1f, Gravity);
        drawInfo.DrawDataCache.Add(warthings);
        warthings.shader = drawInfo.cHead;

        var Rot2 = MathHelper.ToRadians(60 + val) * modPlayer.EnrageInterp * player.gravDir;

        var drawPos2 = drawPos + new Vector2(15 * modPlayer.EnrageInterp + DrawOffset, -12 * modPlayer.EnrageInterp * player.gravDir);

        var Gravity2 = player.gravDir == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;
        var SecondWarthing = new DrawData(Warthings, drawPos2, null, Color.White.MultiplyRGB(drawInfo.colorArmorHead), Rot2, Warthings.Size() * 0.5f, 1, Gravity2);

        drawInfo.DrawDataCache.Add(SecondWarthing);
        SecondWarthing.shader = drawInfo.cHead;
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        var sPlayer = drawInfo.drawPlayer.GetModPlayer<ShintoArmorPlayer>();

        if (sPlayer.EnrageInterp ! > 0.1)
        {
            DrawHaloThing(ref drawInfo, sPlayer);

            if (drawInfo.drawPlayer.head == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorHelmetAll), EquipType.Head))
            {
                DrawWarthings(ref drawInfo, sPlayer);
            }
        }
    }
}