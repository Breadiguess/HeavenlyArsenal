using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Gores;

internal class BulletGore : ModGore
{
    public override string Texture => base.Texture;

    public override void OnSpawn(Gore gore, IEntitySource source)
    {
        Dust.NewDustPerfect(gore.position, DustID.Sandnado, gore.velocity, 150);
        //base.OnSpawn(gore, source);
    }
}