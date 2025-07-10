using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
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

    public enum BabyLeechState
    {
        disperse,

        Dead
    }
    class UmbralLeech_Baby : ModNPC
    {
        public float xoffset;
        public override void SetStaticDefaults()
        {
            NPCID.Sets.CantTakeLunchMoney[NPC.type] = true;
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
        }
        public override void SetDefaults()
        {
            //NPC.CloneDefaults(NPCID.EyeballFlyingFish);
            NPC.waterMovementSpeed = 10;
            NPC.lifeMax = 4873;
            NPC.noTileCollide = true;
            NPC.noGravity = true;
            NPC.aiStyle = -1;
            NPC.hide = false;
            NPC.width = 60;
            NPC.height = 40;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            base.SetBestiary(database, bestiaryEntry);
        }

        public override void AI()
        {
            
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            
            int value = (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3;
            int FrameCount = 5;

            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech_Baby").Value;
            Rectangle lech  = new Rectangle(0, value * (texture.Height/FrameCount), texture.Width, texture.Height/FrameCount);

            Vector2 origin = new Vector2(texture.Width / 2f, (texture.Height /FrameCount)/2f);

            Vector2 drawPos = NPC.Center - Main.screenPosition;
            //Main.NewText($"{value}");
            /*if (!NPC.IsABestiaryIconDummy)
            {
                // Have a shader prepared, only special thing is that it uses a normalized matrix
                ManagedShader trailShader = ShaderManager.GetShader("HeavenlyArsenal.thing");
                trailShader.SetTexture(GennedAssets.Textures.Noise.SwirlNoise, 0, SamplerState.PointClamp);
                trailShader.TrySetParameter("Time", Main.GlobalTimeWrappedHourly);
                trailShader.TrySetParameter("Resolution",1);
                //trailShader.TrySetParameter("uColor", Color.White.ToVector4() * 0.66f);
                trailShader.Apply();
            }*/
            //Main.EntitySpriteDraw(texture, drawPos, lech, drawColor, NPC.rotation, origin , 1, SpriteEffects.None, 0f);

            
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Texture2D sprite = TextureAssets.Npc[Type].Value;

            float num36 = Main.NPCAddHeight(NPC);

            Vector2 original = new Vector2(sprite.Width / 2, sprite.Height / FrameCount);

            Vector2 halfSize = new Vector2(TextureAssets.Npc[Type].Width() / 2, TextureAssets.Npc[Type].Height() /FrameCount/2);

            Vector2 position2 = NPC.Center - Main.screenPosition;
            //position2 -= new Vector2(texture.Width, texture.Height / Main.npcFrameCount[Type]) * NPC.scale / 2f;
            //position2 += halfSize * NPC.scale + new Vector2(0f, num36 + NPC.gfxOffY);
            //position2 += Vector2.UnitX * NPC.localAI[0];
            Main.EntitySpriteDraw(texture, drawPos,lech, NPC.GetAlpha(drawColor), NPC.rotation, halfSize, NPC.scale, spriteEffects, 0f);
            return false;
        }
    }
}
