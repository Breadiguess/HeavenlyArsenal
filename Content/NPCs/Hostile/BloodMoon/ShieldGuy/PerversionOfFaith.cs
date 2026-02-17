using HeavenlyArsenal.Content.Biomes;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.ShieldGuy;

internal partial class PerversionOfFaith : BloodMoonBaseNPC
{
    public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/ShieldGuy/PerversionOfFaith";

    public override bool canBeSacrificed => false;

    public override int bloodBankMax => 100;

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        NPC.noTileCollide = true;
        NPC.noGravity = true;
        NPC.Size = new Vector2(30, 30);
        NPC.defense = 400;
        NPC.damage = 300;
        NPC.lifeMax = 400;

        SpawnModBiomes =
        [
            ModContent.GetInstance<RiftEclipseBiome>().Type
        ];
    }

    public override void OnSpawn(IEntitySource source) { }

    public override void AI()
    {
        playerTarget = Main.player[NPC.FindClosestPlayer()];
        NPC.velocity = NPC.Center.AngleTo(playerTarget.Center).ToRotationVector2();
        //NPC.velocity += new Vector2(0, MathF.Sin(Time));

        Time++;
    }
}