using HeavenlyArsenal.Content.Biomes;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.ShieldGuy;

internal partial class PerversionOfFaith : BaseBloodMoonNPC
{
    public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/ShieldGuy/PerversionOfFaith";
    public override int MaxBlood => 3;

    public override BloodMoonBalanceStrength Strength => new();
    protected override void SetDefaults2()
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

  
}