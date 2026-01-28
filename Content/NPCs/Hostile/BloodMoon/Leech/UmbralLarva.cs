using System.Collections.Generic;
using Luminance.Assets;
using NoxusBoss.Assets;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;

public class UmbralLarva : BloodMoonBaseNPC
{

    public override bool ResistantToTrueMelee => false;

    public override int bloodBankMax => 2000;
    public override void SetDefaults()
    {
        NPC.width = 30;
        NPC.height = 30;
        NPC.lifeMax = 30_000;
        NPC.damage = 80;
        NPC.defense = 50;
        NPC.npcSlots = 0.1f;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.Size = new Vector2(20, 20);

        SpawnModBiomes =
        [
            ModContent.GetInstance<RiftEclipseBloodMoon>().Type
        ];
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange
        (
            new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
                new FlavorTextBestiaryInfoElement("Mods.HeavenlyArsenal.Bestiary.UmbralLarva")
            }
        );
    }

    public override bool CheckActive()
    {
        return false;
    }

}

public class BloodSpat : ModProjectile
{
    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.hostile = true;
        Projectile.friendly = false;

        Projectile.width = Projectile.height = 14;
        Projectile.timeLeft = 300;

        Projectile.penetrate = 1;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.aiStyle = -1;
    }

    public override void AI()
    {
        var b = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Rain_BloodMoon, 0, 0, 0, Color.Crimson);

        var a = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Blood, 0, 0, 0, Color.Purple);

        a.velocity = Projectile.velocity;
        b.velocity = Projectile.velocity;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
}