using HeavenlyArsenal.ArsenalPlayer;
using HeavenlyArsenal.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor;

public class AntiShadowCloak_DrawLayer : PlayerDrawLayer
{

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BackAcc);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.body == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorBreastplate), EquipType.Body);

    public override bool IsHeadLayer => false;
    


    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        ShintoArmorCapePlayer capePlayer = drawInfo.drawPlayer.GetModPlayer<ShintoArmorCapePlayer>();
        if (!capePlayer.IsReady() || drawInfo.shadow > 0f)
            return;

        drawInfo.drawPlayer.GetModPlayer<ShintoArmorPlayer>().ShadowVeil = true;

        DrawData data = capePlayer.GetRobeTarget();
        data.position = drawInfo.BodyPosition() + new Vector2(2 * drawInfo.drawPlayer.direction, (drawInfo.drawPlayer.gravDir < 0 ? 11 : 0) + -8 * drawInfo.drawPlayer.gravDir);
        data.color = Color.White;
        data.effect = Main.GameViewMatrix.Effects;
        data.shader = drawInfo.cBody;
        //Main.NewText($"Position: {data.position}", Color.AntiqueWhite);
        drawInfo.DrawDataCache.Add(data);
        
       
    }
}
