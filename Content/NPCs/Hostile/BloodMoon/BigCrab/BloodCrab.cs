using Terraria.GameContent.Bestiary;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab;

internal partial class BloodCrab : BloodMoonBaseNPC
{
    public bool Anchored = false;

    public override int blood => 0;

    public override int bloodBankMax => 50;

    public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/BigCrab/ArtilleryCrab";

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange
        (
            [
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
                new FlavorTextBestiaryInfoElement("Mods.HeavenlyArsenal.Bestiary.ArtilleryCrab1")
            ]
        );
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        NPC.width = 100;
        NPC.height = 55;
        NPC.damage = 200;
        NPC.defense = 130 / 2;
        NPC.lifeMax = 38470;
        NPC.value = 10000;
        NPC.aiStyle = -1;
        NPC.npcSlots = 3f;
        NPC.knockBackResist = 0.5f;
    }

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 13;
    }

    public override void AI()
    {
        StateMachine();
        Time++;
    }

    public override void PostAI() { }
}