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

namespace HeavenlyArsenal.Content.Items.Armor
{
    class AntiShadowCloak_DrawLayer : PlayerDrawLayer
    {

        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BackAcc);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.body == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorBreastplate), EquipType.Body);

        public override bool IsHeadLayer => false;
        
    

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            drawInfo.drawPlayer.GetModPlayer<ShintoArmorPlayer>().ShadowVeil = true;

            int segmentCount = ShintoArmorPlayer.segmentCount;
            Texture2D chainTexture = ShintoArmorPlayer.chainTexture;
            Vector2[] verletPoints = drawInfo.drawPlayer.GetModPlayer<ShintoArmorPlayer>().verletPoints;
            for (int i = 0; i < segmentCount - 1; i++)
            {
                Vector2 posA = verletPoints[i];
                Vector2 posB = verletPoints[i + 1];
                Vector2 segmentVector = posB - posA;
                float rotation = (float)Math.Atan2(segmentVector.Y, segmentVector.X);
                float scale = segmentVector.Length() / chainTexture.Width; // stretch texture to fill segment length

                //Main.spriteBatch.Draw(chainTexture, posA, null, Color.White, rotation, new Vector2(0, chainTexture.Height / 2f), new Vector2(scale, 1f), SpriteEffects.None, 0f);
                drawInfo.DrawDataCache.Add(new DrawData(chainTexture, verletPoints[segmentCount - 1], null, Color.White, 0f, new Vector2(0, chainTexture.Height / 2f), 1f, SpriteEffects.None, 0));
            }
           
        }
    }
}
