using HeavenlyArsenal.Common.Utilities;
using HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor;

internal class BloodHelmetDrawLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return (drawInfo.drawPlayer.armor[10].IsAir && drawInfo.drawPlayer.armor[0].ModItem is AwakenedBloodHelm) || drawInfo.drawPlayer.armor[10].ModItem is AwakenedBloodHelm;
    }

    public override Position GetDefaultPosition()
    {
        return new AfterParent(PlayerDrawLayers.Head);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        var Owner = drawInfo.drawPlayer;

        var texString = "HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/AwakenedBloodHelm";
        var a = drawInfo.drawPlayer.GetModPlayer<AwakenedBloodPlayer>().Form.ToString();

        texString += $"{a}_Head";
        var helmet = ModContent.Request<Texture2D>(texString).Value;

        var Flip = Owner.direction == 1 ? 0 : SpriteEffects.FlipHorizontally;
        var Frame = helmet.Frame(1, 20, 0, drawInfo.drawPlayer.headFrame.Y);

        var helm = new DrawData
        (
            helmet,
            drawInfo.HeadPosition() + new Vector2(0, 5.6f),
            Owner.legFrame,
            drawInfo.colorArmorHead,
            drawInfo.drawPlayer.headRotation,
            Owner.legFrame.Size() * 0.5f,
            1f,
            Flip
        );

        drawInfo.DrawDataCache.Add(helm);
    }
}

internal class BloodChestplateDrawLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return (drawInfo.drawPlayer.armor[11].IsAir && drawInfo.drawPlayer.armor[1].ModItem is AwakenedBloodplate) || drawInfo.drawPlayer.armor[10].ModItem is AwakenedBloodplate;
    }

    public override Position GetDefaultPosition()
    {
        return new AfterParent(PlayerDrawLayers.Torso);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        var Owner = drawInfo.drawPlayer;
        var texString = "HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/AwakenedBloodplate";
        var a = drawInfo.drawPlayer.GetModPlayer<AwakenedBloodPlayer>().Form.ToString();
        texString += $"{a}_Body";
        var chestplate = ModContent.Request<Texture2D>(texString).Value;
        var Flip = Owner.direction == 1 ? 0 : SpriteEffects.FlipHorizontally;

        var bodyFrame = drawInfo.compTorsoFrame;
        var walkOffset = Owner.gravDir * Main.OffsetsPlayerHeadgear[Owner.bodyFrame.Y / Owner.bodyFrame.Height];

        // Utils.DrawBorderString(Main.spriteBatch, bodyFrame.ToString(), Owner.Center - Main.screenPosition, Color.AntiqueWhite, anchory: -2);
        var chest = new DrawData
        (
            chestplate,
            drawInfo.BodyPosition() + walkOffset + new Vector2(0, -2),
            bodyFrame,
            drawInfo.colorArmorBody,
            Owner.bodyRotation,
            bodyFrame.Size() * 0.5f,
            1f,
            Flip
        );

        chest.shader = drawInfo.cBody;

        //Utils.DrawBorderString(Main.spriteBatch, hting.ToString(), Owner.Center - Main.screenPosition, Color.AntiqueWhite, anchory: -2);
        var shoulder = new DrawData
        (
            chestplate,
            drawInfo.HeadPosition() + new Vector2(0, 3.6f) + walkOffset,
            drawInfo.compBackShoulderFrame,
            drawInfo.colorArmorBody,
            Owner.fullRotation,
            drawInfo.bodyVect,
            1f,
            drawInfo.playerEffect
        )
        {
            shader = drawInfo.cBody
        };

        PlayerDrawLayers.DrawCompositeArmorPiece(ref drawInfo, CompositePlayerDrawContext.BackShoulder, shoulder);
        PlayerDrawLayers.DrawCompositeArmorPiece(ref drawInfo, CompositePlayerDrawContext.Torso, chest);
    }
}

internal class BloodChestplateArmDrawLayer : PlayerDrawLayer
{
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return (drawInfo.drawPlayer.armor[11].IsAir && drawInfo.drawPlayer.armor[1].ModItem is AwakenedBloodplate) || drawInfo.drawPlayer.armor[10].ModItem is AwakenedBloodplate;
    }

    public override Position GetDefaultPosition()
    {
        return new BeforeParent(PlayerDrawLayers.Torso);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        var Owner = drawInfo.drawPlayer;
        var walkOffset = Owner.gravDir * Main.OffsetsPlayerHeadgear[Owner.bodyFrame.Y / Owner.bodyFrame.Height];

        var texString = "HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/AwakenedBloodplate";
        var a = drawInfo.drawPlayer.GetModPlayer<AwakenedBloodPlayer>().Form.ToString();
        texString += $"{a}_Body";
        var chestplate = ModContent.Request<Texture2D>(texString).Value;
        var Flip = Owner.direction == 1 ? 0 : SpriteEffects.FlipHorizontally;

        var data = new DrawData
        (
            chestplate,
            drawInfo.HeadPosition() + new Vector2(0, 3.6f) + walkOffset,
            drawInfo.compBackArmFrame,
            drawInfo.colorArmorBody,
            drawInfo.compositeBackArmRotation,
            drawInfo.compFrontArmFrame.Size() * 0.5f,
            1f,
            drawInfo.playerEffect
        );

        //drawInfo.DrawDataCache.Add(data);
        PlayerDrawLayers.DrawCompositeArmorPiece(ref drawInfo, CompositePlayerDrawContext.BackArm, data);
    }
}

public class MyArmOverlay_Front : PlayerDrawLayer
{
    public override Position GetDefaultPosition()
    {
        return new AfterParent(PlayerDrawLayers.ArmOverItem);
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return (drawInfo.drawPlayer.armor[11].IsAir && drawInfo.drawPlayer.armor[1].ModItem is AwakenedBloodplate) || drawInfo.drawPlayer.armor[10].ModItem is AwakenedBloodplate;
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (drawInfo.shadow != 0f)
        {
            return;
        }

        if (!drawInfo.usesCompositeTorso)
        {
            return;
        }

        if (drawInfo.armorHidesArms)
        {
            return;
        }

        var Owner = drawInfo.drawPlayer;
        var walkOffset = Owner.gravDir * Main.OffsetsPlayerHeadgear[Owner.bodyFrame.Y / Owner.bodyFrame.Height];

        var texString = "HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/AwakenedBloodplate";
        var a = drawInfo.drawPlayer.GetModPlayer<AwakenedBloodPlayer>().Form.ToString();
        texString += $"{a}_Body";
        var chestplate = ModContent.Request<Texture2D>(texString).Value;

        var data = new DrawData
        (
            chestplate,
            drawInfo.BodyPosition() + walkOffset,
            drawInfo.compFrontArmFrame,
            drawInfo.colorArmorBody,
            drawInfo.compositeFrontArmRotation,
            drawInfo.compFrontArmFrame.Size() * 0.5f,
            1f,
            drawInfo.playerEffect
        );

        data.shader = drawInfo.cBody;
        PlayerDrawLayers.DrawCompositeArmorPiece(ref drawInfo, CompositePlayerDrawContext.FrontArm, data);

        var shoulder = new DrawData
        (
            chestplate,
            drawInfo.HeadPosition() + new Vector2(0, 3.6f) + walkOffset,
            drawInfo.compFrontShoulderFrame,
            drawInfo.colorArmorBody,
            Owner.fullRotation,
            drawInfo.bodyVect,
            1f,
            drawInfo.playerEffect
        )
        {
            shader = drawInfo.cBody
        };

        //drawInfo.DrawDataCache.Add(shoulder);
        PlayerDrawLayers.DrawCompositeArmorPiece(ref drawInfo, CompositePlayerDrawContext.FrontShoulder, shoulder);
    }
}
/*
 *
public class MyArmOverlay_Front : PlayerDrawLayer
{
    // Token: 0x06003246 RID: 12870
    public override PlayerDrawLayer.Position GetDefaultPosition()
    {
        return new PlayerDrawLayer.AfterParent(PlayerDrawLayers.ArmOverItem);
    }

    // Token: 0x06003247 RID: 12871
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return drawInfo.drawPlayer.body == EquipLoader.GetEquipSlot(base.Mod, "AwakenedBloodplate", EquipType.Body);
    }

    // Token: 0x06003248 RID: 12872
    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (drawInfo.shadow != 0f)
        {
            return;
        }
        if (!drawInfo.usesCompositeTorso)
        {
            return;
        }
        if (drawInfo.armorHidesArms)
        {
            return;
        }
        Player Owner = drawInfo.drawPlayer;
        Vector2 walkOffset = Owner.gravDir * Main.OffsetsPlayerHeadgear[Owner.bodyFrame.Y / Owner.bodyFrame.Height];

        string texString = "HeavenlyArsenal/Content/Items/Armor/AwakenedBloodArmor/AwakenedBloodplate";
        string a = drawInfo.drawPlayer.GetModPlayer<AwakenedBloodPlayer>().CurrentForm.ToString();
        texString += $"{a}_Body";
        Texture2D chestplate = ModContent.Request<Texture2D>(texString).Value;
        Vector2 drawPos = new Vector2((float)((int)(drawInfo.Position.X - Main.screenPosition.X - (float)(drawInfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawInfo.drawPlayer.width / 2))), (float)((int)(drawInfo.Position.Y - Main.screenPosition.Y + (float)drawInfo.drawPlayer.height - (float)drawInfo.drawPlayer.bodyFrame.Height))) + new Vector2(20f, 30f);

        CompositePlayerDrawContext contexts = CompositePlayerDrawContext.Torso;
        DrawData drawData = new DrawData(chestplate, drawInfo.drawPlayer.bodyPosition + walkOffset - Main.screenPosition, new Rectangle?(drawInfo.compFrontArmFrame), drawInfo.colorArmorBody, drawInfo.compositeFrontArmRotation, drawInfo.compFrontArmFrame.Size() * 0.5f, 1f, drawInfo.playerEffect, 0f)
        {
            shader = drawInfo.cBody
        };
        PlayerDrawLayers.DrawCompositeArmorPiece(ref drawInfo, contexts, drawData);


        CompositePlayerDrawContext context = CompositePlayerDrawContext.FrontArm;
        PlayerDrawLayers.DrawCompositeArmorPiece(ref drawInfo, context, drawData);
        drawData = new DrawData(chestplate, drawPos + walkOffset, new Rectangle?(drawInfo.compFrontArmFrame), Color.White, drawInfo.compositeFrontArmRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0f)
        {
            shader = drawInfo.cBody
        };
        DrawData fArm = drawData;
        PlayerDrawLayers.DrawCompositeArmorPiece(ref drawInfo, CompositePlayerDrawContext.FrontArm, fArm);
    }
}
 */