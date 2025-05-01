using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech
{
    class UmbralLeech_Baby : ModNPC
    {
        public override void SetStaticDefaults()
        {


        }
        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.EyeballFlyingFish);
            NPC.waterMovementSpeed = 10;
            NPC.lifeMax = 4873;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            base.SetBestiary(database, bestiaryEntry);
        }

        public override void AI()
        {
            base.AI();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Rectangle lech  = new Rectangle(1,5, 0 , (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3);

            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

            Main.EntitySpriteDraw(texture, NPC.Center - Main.screenPosition, lech, drawColor, NPC.rotation, origin , NPC.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
