using NoxusBoss.Assets;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;

internal class SacrificeNPC : GlobalNPC
{
    public bool isSacrificed;

    public int SacrificeTimer;

    public int SacrificeDuration = 60 * 3;

    public Vector2 OriginalPosition;

    public RitualAltar Priest;

    public override bool InstancePerEntity => true;

    public override bool PreAI(NPC npc)
    {
        if (isSacrificed)
        {
            if (RitualSystem.IsNPCBuffed(npc))
            {
                isSacrificed = false;
            }

            var a = npc.ModNPC as BloodMoonBaseNPC;

            if (!npc.noGravity)
            {
                npc.noGravity = false;
            }

            npc.Center = Vector2.Lerp(OriginalPosition, OriginalPosition + new Vector2(0, -75), SacrificeTimer / (float)SacrificeDuration);

            if (SacrificeTimer >= SacrificeDuration)
            {
                // SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.BloodCry with { MaxInstances = 0 }, npc.Center);
                npc.StrikeInstantKill();

                if (npc.life > 0)
                {
                    npc.active = false;
                }

                Priest.blood += a.blood;
                Priest.SacrificeCooldown = 60 * 5;

                if (Priest.NPC.life < Priest.NPC.lifeMax)
                {
                    Priest.NPC.life = Math.Clamp(Priest.NPC.life + npc.lifeMax / 4, 0, Priest.NPC.lifeMax);
                    CombatText.NewText(Priest.NPC.Hitbox, Color.Crimson, "+" + npc.lifeMax / 4);
                }

                Priest.NPCTarget = null;

                if (a.blood <= 0)
                {
                    Priest.blood += Priest.bloodBankMax / 5;
                }

                Priest.isSacrificing = false;
                SacrificeTimer = 0;
                isSacrificed = false;
            }

            SacrificeTimer++;

            return false; //lambs don't fight the slaughter
        }

        OriginalPosition = npc.Center;

        return base.PreAI(npc);
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D Outline = GennedAssets.Textures.GreyscaleTextures.Corona;
        var drawPos = npc.Center - screenPos;

        if (isSacrificed)
        {
            var scale = (float)SacrificeTimer / SacrificeDuration;
            var alpha = 1f - (float)SacrificeTimer / SacrificeDuration;

            spriteBatch.Draw
            (
                Outline,
                drawPos,
                null,
                Color.Red with
                {
                    A = 0
                } *
                alpha,
                0f,
                Outline.Size() / 2,
                scale,
                SpriteEffects.None,
                0f
            );
        }

        //if(npc.type != ModContent.NPCType<RitualAltar>())
        //   Utils.DrawBorderString(spriteBatch, $"{SacrificeTimer}/{SacrificeDuration}",drawPos +Vector2.UnitY*40, Color.Red,1);

        return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
    }
}