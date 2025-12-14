namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.sipho;

internal partial class CryonophoreLimb
{
    private void RenderLimbChain(Color drawColor)
    {
        var tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/sipho/CryonophoreLimb_Chain").Value;

        for (var i = 1; i < LimbSegmentPos.Length - 1; i++)
        {
            var DrawPos = LimbSegmentPos[i] - Main.screenPosition;

            var rotation = LimbSegmentPos[i].AngleTo(LimbSegmentPos[i + 1]);

            Main.EntitySpriteDraw(tex, DrawPos, null, drawColor, rotation + MathHelper.PiOver2, tex.Size() / 2, 0.5f, 0);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var debug = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/sipho/CryonophoreLimb_Fist").Value;

        Main.EntitySpriteDraw(debug, NPC.Center - screenPos, null, drawColor, NPC.rotation - MathHelper.PiOver2, debug.Size() / 2, 1, 0);
        Utils.DrawBorderString(spriteBatch, self.type.ToString(), NPC.Center - screenPos, Color.AntiqueWhite, 0.4f);

        if (!NPC.IsABestiaryIconDummy)
        {
            RenderLimbChain(drawColor);
        }

        return base.PreDraw(spriteBatch, screenPos, drawColor);
    }
}