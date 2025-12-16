using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Gores;

public class BulletGore : ModGore
{
    public override void OnSpawn(Gore gore, IEntitySource source)
    {
        base.OnSpawn(gore, source);
        
        Dust.NewDustPerfect(gore.position, DustID.Sandnado, gore.velocity, 150);
    }
}