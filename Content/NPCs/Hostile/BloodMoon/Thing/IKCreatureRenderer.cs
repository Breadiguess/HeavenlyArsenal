namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Thing;

internal partial class IkCreature
{
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
        {
            return false;
        }

        foreach (var leg in Legs)
        {
            leg.Draw(spriteBatch, screenPos, drawColor);
        }

        return base.PreDraw(spriteBatch, screenPos, drawColor);
    }
}