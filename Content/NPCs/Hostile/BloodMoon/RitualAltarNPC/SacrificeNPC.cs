using NoxusBoss.Assets;
using System.IO;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;

internal class SacrificeNPC : GlobalNPC
{
    public bool isSacrificed;
    public int SacrificeTimer;
    public int SacrificeDuration = 60 * 3;
    public Vector2 OriginalPosition;

    // Runtime reference only. Sync this through PriestWhoAmI.
    public RitualAltar Priest;
    public int PriestWhoAmI = -1;

    public override bool InstancePerEntity => true;

    public void StartSacrifice(NPC npc, RitualAltar priest, int duration = 60 * 3)
    {
        isSacrificed = true;
        SacrificeTimer = 0;
        SacrificeDuration = duration;
        OriginalPosition = npc.Center;

        Priest = priest;
        PriestWhoAmI = priest?.NPC?.whoAmI ?? -1;

        npc.netUpdate = true;
    }

    public void StopSacrifice(NPC npc)
    {
        isSacrificed = false;
        SacrificeTimer = 0;
        Priest = null;
        PriestWhoAmI = -1;

        npc.netUpdate = true;
    }

    public override bool PreAI(NPC npc)
    {
        // Rebuild Priest reference from whoAmI if needed.
        if (Priest == null && PriestWhoAmI >= 0 && PriestWhoAmI < Main.maxNPCs)
        {
            NPC priestNPC = Main.npc[PriestWhoAmI];
            if (priestNPC != null && priestNPC.active && priestNPC.ModNPC is RitualAltar altar)
                Priest = altar;
        }

        if (isSacrificed)
        {
            if (RitualSystem.IsNPCBuffed(npc))
            {
                StopSacrifice(npc);
                return true;
            }

            BaseBloodMoonNPC a = npc.ModNPC as BaseBloodMoonNPC;

            if (!npc.noGravity)
                npc.noGravity = true;

            npc.velocity = Vector2.Zero;
            npc.Center = Vector2.Lerp(
                OriginalPosition,
                OriginalPosition + new Vector2(0f, -75f),
                SacrificeTimer / (float)SacrificeDuration
            );

            // Only the server should do authoritative completion logic.
            if (SacrificeTimer >= SacrificeDuration)
            {
                if (Main.netMode != Terraria.ID.NetmodeID.MultiplayerClient)
                {
                    npc.StrikeInstantKill();

                    if (npc.life > 0)
                        npc.active = false;

                    if (Priest != null)
                    {
                        Priest.Blood += a.Blood;
                        Priest.SacrificeCooldown = 60 * 5;

                        if (Priest.NPC.life < Priest.NPC.lifeMax)
                        {
                            int healAmount = npc.lifeMax / 4;
                            Priest.NPC.life = Math.Clamp(Priest.NPC.life + healAmount, 0, Priest.NPC.lifeMax);
                            CombatText.NewText(Priest.NPC.Hitbox, Color.Crimson, "+" + healAmount);

                            // If the altar's health matters visually/gameplay-wise, sync it too.
                            Priest.NPC.netUpdate = true;
                        }

                        Priest.NPCTarget = null;

                        if (a.Blood <= 0)
                            Priest.Blood += Priest.MaxBlood / 5;

                        Priest.isSacrificing = false;
                    }

                    StopSacrifice(npc);
                }

                return false;
            }

            // Timer should only advance on the server in MP.
            if (Main.netMode != Terraria.ID.NetmodeID.MultiplayerClient)
            {
                SacrificeTimer++;

                npc.netUpdate = true;
            }

            return false;
        }

        OriginalPosition = npc.Center;
        return base.PreAI(npc);
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        bitWriter.WriteBit(isSacrificed);

        binaryWriter.Write(SacrificeTimer);
        binaryWriter.Write(SacrificeDuration);
        binaryWriter.WriteVector2(OriginalPosition);
        binaryWriter.Write(PriestWhoAmI);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        isSacrificed = bitReader.ReadBit();

        SacrificeTimer = binaryReader.ReadInt32();
        SacrificeDuration = binaryReader.ReadInt32();
        OriginalPosition = binaryReader.ReadVector2();
        PriestWhoAmI = binaryReader.ReadInt32();

        Priest = null;
        if (PriestWhoAmI >= 0 && PriestWhoAmI < Main.maxNPCs)
        {
            NPC priestNPC = Main.npc[PriestWhoAmI];
            if (priestNPC != null && priestNPC.active && priestNPC.ModNPC is RitualAltar altar)
                Priest = altar;
        }
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