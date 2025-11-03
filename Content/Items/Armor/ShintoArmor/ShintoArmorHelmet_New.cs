using CalamityEntropy.Common;
using HeavenlyArsenal.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.ShintoArmor
{
    [AutoloadEquip(EquipType.Head)]
    internal class ShintoArmorHelmet_New : ModItem
    {
        public override void SetStaticDefaults()
        {
            ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
            ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] =false ;
            ArmorIDs.Head.Sets.IsTallHat[Item.headSlot] = true;
            ArmorIDs.Head.Sets.PreventBeardDraw[Item.headSlot] = true;
        }
        public override void SetDefaults()
        {
           
        }
    }
    public class ShintoArmorHelmet_NewDraw : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);


        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.head == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorHelmet_New), EquipType.Head);

        public override bool IsHeadLayer => true;

        protected void DrawVoidEyes(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            Texture2D facePixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;

            Vector2 baseHeadPos = drawInfo.HeadPosition();
            Vector2 walkOffset = player.gravDir * Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];

            Vector2[] offsets = new Vector2[]
            {
                    new Vector2(3f * player.direction, 2.75f),
                    new Vector2(3f * player.direction, -3),
                    new Vector2(-1f * player.direction, -1.25f),
                    new Vector2(7f * player.direction, -1.25f),
            };
            Color BaseheadColor = Color.Red;
            if (drawInfo.cHead != 0)
            {
                BaseheadColor = drawInfo.colorArmorHead;
            }
            Vector2 GravOffset = new Vector2(0, player.gravDir == 1 ? 0 : 16.5f);

            float thing = 1;// (1 - modPlayer.EnrageInterp);
            foreach (var offset in offsets)
            {
                Vector2 drawPos = baseHeadPos + offset + walkOffset + GravOffset;
                DrawData dots = new DrawData(
                    facePixel, drawPos, null, BaseheadColor * thing, 0f, facePixel.Size() * 0.5f, 0.9f, SpriteEffects.None, 0);
                dots.shader = drawInfo.cHead;
                DrawData GlowingEyes = new DrawData(Glow, drawPos, null, BaseheadColor with { A = 0 } * thing, 0f, Glow.Size() * 0.5f, 0.05f, SpriteEffects.None, 0);
                GlowingEyes.shader = drawInfo.cHead;
                drawInfo.DrawDataCache.Add(GlowingEyes);


            }

        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;

            Texture2D Head = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/ShintoArmor/ShintoArmorHelmet_New_Head_Real").Value;
            Vector2 baseHeadPos = drawInfo.HeadPosition();
            Vector2 walkOffset = player.gravDir * Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];

            Rectangle Frame = player.legFrame;
            Vector2 DrawPos = baseHeadPos + new Vector2(0,-0.4f);

            SpriteEffects a = player.direction == 1 ? 0 : SpriteEffects.FlipHorizontally;
            Color c = drawInfo.colorArmorHead;
            DrawData Helmet = new DrawData(Head, DrawPos, Frame, c, 0, Frame.Size() * 0.5f, 1, a);
            Helmet.shader = drawInfo.cHead;
            drawInfo.DrawDataCache.Add(Helmet);
            DrawVoidEyes(ref drawInfo);
        }
    }
}
