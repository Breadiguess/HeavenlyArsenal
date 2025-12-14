namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.ShieldGuy;

internal partial class PerversionOfFaith
{
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var tex = ModContent.Request<Texture2D>(Texture).Value;

        var DrawPos = NPC.Center - Main.screenPosition;
        var origin = tex.Size() * 0.5f;

        var effects = NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        Main.EntitySpriteDraw(tex, DrawPos, null, drawColor, 0, origin, 1, effects);

        return false; //base.PreDraw(spriteBatch, screenPos, drawColor);
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var tex = ModContent.Request<Texture2D>(Texture + "_Shield").Value;

        var DrawPos = NPC.Center - Main.screenPosition;

        var Rot = MathHelper.ToRadians(Time);
        Main.EntitySpriteDraw(tex, DrawPos, null, drawColor, Rot, tex.Size() * 0.5f, 1, SpriteEffects.None);
    }
}