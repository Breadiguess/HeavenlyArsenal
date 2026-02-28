

using Terraria.GameContent;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Artillery_Crab
{
    public partial class BloodCrab
    {
        public override void FindFrame(int frameHeight) { }

        private void RenderSquidLauncher(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

        
        private void DrawClawAndArm()
        {
            if (claw is null)
                return;

            claw.DrawClaw();

        }
        private void DrawGunAndArm()
        {
            if (GunArm is null)
                return;

            GunArm.DrawClaw();
        }
        private void DrawBody(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;

            float rot = NPC.rotation + tangent.ToRotation()*0.2f;

            SpriteEffects flip = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 DrawPos = NPC.Center - screenPos + new Vector2(0, -12);
            
            Main.EntitySpriteDraw(tex, DrawPos, null, drawColor, rot, tex.Size() / 2, NPC.scale, flip);
        }
        private void DrawDebugRectangles()
        {
            foreach(var rect in _ExtraHitBoxes)
            {
                if (rect == null)
                    continue;

                else
                    Utils.DrawRectangle(Main.spriteBatch, rect.Hitbox.TopLeft(), rect.Hitbox.BottomRight(), Color.White, Color.White, 4);
            }
        }
        private void DrawDebugButterflyAttachPoints()
        {
            if(ButterflyAttachPoints is null)
                return;
            
            Texture2D tex = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

            for (int i = 0; i < ButterflyAttachPoints.Length; i++)
            {
                var attach = ButterflyAttachPoints[i];

                if (attach is null)
                    continue;

                Main.EntitySpriteDraw(tex, attach.Position- Main.screenPosition, null, Color.Pink, 0, tex.Size() / 2, 10, 0);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return false;
            RenderSquidLauncher(spriteBatch, screenPos, drawColor);

            if (_bloodCrabLegs is not null)
            {
                foreach (var leg in _bloodCrabLegs)
                {
                    if(leg is not null)
                    leg.DrawLeg(leg, spriteBatch, screenPos);
                }
            }
            
            Texture2D tex = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            for (int i = 0; i < ActualLimbOffsets.Length; i++)
            {
                Main.EntitySpriteDraw(tex, NPC.Center + ActualLimbOffsets[i] - screenPos, null, Color.Blue, 0, tex.Size() / 2,5, 0);
            }
            //Utils.DrawBorderString(spriteBatch, NPC.noGravity.ToString(), NPC.Center - screenPos, Color.AntiqueWhite);


            DrawBody(spriteBatch, screenPos, drawColor);

            DrawDebugRectangles();
            DrawClawAndArm();
            DrawGunAndArm();
            DrawDebugButterflyAttachPoints();

            Utils.DrawBorderString(spriteBatch, (Time).ToString(), NPC.Center - screenPos, Color.White);
            return false;//base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    }
}
