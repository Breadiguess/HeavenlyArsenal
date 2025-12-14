namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab;

internal partial class BloodCrab
{
    public override void FindFrame(int frameHeight) { }

    private void RenderSquidLauncher(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        RenderSquidLauncher(spriteBatch, screenPos, drawColor);

        Utils.DrawBorderString(spriteBatch, CurrentState.ToString(), NPC.Center - screenPos, Color.AntiqueWhite);

        return base.PreDraw(spriteBatch, screenPos, drawColor);
    }
}