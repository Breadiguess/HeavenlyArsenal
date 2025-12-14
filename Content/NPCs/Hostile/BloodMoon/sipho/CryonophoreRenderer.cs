namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.sipho;

internal partial class Cryonophore
{
    public override void DrawBehind(int index)
    {
        base.DrawBehind(index);
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        base.PostDraw(spriteBatch, screenPos, drawColor);
    }

    private void RenderCore(Vector2 screenPos, Color drawColor)
    {
        var texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/sipho/Cryonophore_Core").Value;

        var DrawPos = NPC.Center - screenPos;
        var origin = new Vector2(texture.Width / 2, texture.Height - 12);
        Main.EntitySpriteDraw(texture, DrawPos, null, drawColor, 0, origin, 2, 0);
    }

    private void RenderLimbs(Vector2 screenPos, Color drawColor)
    {
        foreach (var plac in OwnedZooids)
        {
            var limb = plac.Value.Item2;

            if (limb != null)

            {
                continue;
            }

            var zooid = plac.Value.Item1;
            //todo: replace wiht appropriate texture paths later
            var tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/sipho/CryonophoreLimb_Fist").Value;

            var drawPos = zooid.position - screenPos;
            var rot = zooid.rotation; //MathHelper.ToRadians(MathF.Sin(Time/10.1f + plac.Value.Item1.id) * 30);
            Main.EntitySpriteDraw(tex, drawPos, null, drawColor, rot, new Vector2(tex.Width / 2, 0), new Vector2(1, 1), 0);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        RenderCore(screenPos, drawColor);
        RenderLimbs(screenPos, drawColor);
        Utils.DrawBorderString(spriteBatch, Time.ToString(), NPC.Center - screenPos, Color.AntiqueWhite);

        return base.PreDraw(spriteBatch, screenPos, drawColor);
    }
}