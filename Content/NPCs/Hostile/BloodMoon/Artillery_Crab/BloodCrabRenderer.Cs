

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Artillery_Crab
{
    public partial class BloodCrab
    {
        public override void FindFrame(int frameHeight) { }

        private void RenderSquidLauncher(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }


        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return false;
            RenderSquidLauncher(spriteBatch, screenPos, drawColor);

            if (_bloodCrabLegs is not null)
            {
                foreach (var leg in _bloodCrabLegs)
                {
                    leg.DrawLeg(leg, spriteBatch, screenPos);
                }
            }

            Texture2D tex = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            for (int i = 0; i < ActualLimbOffsets.Length; i++)
            {
                Main.EntitySpriteDraw(tex, NPC.Center + ActualLimbOffsets[i] - screenPos, null, Color.Blue, 0, tex.Size() / 2,5, 0);
            }
            //Utils.DrawBorderString(spriteBatch, NPC.noGravity.ToString(), NPC.Center - screenPos, Color.AntiqueWhite);

            return  base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    }
}
