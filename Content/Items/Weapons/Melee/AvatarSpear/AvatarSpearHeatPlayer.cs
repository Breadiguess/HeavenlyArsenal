using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.AvatarSpear;

public class AvatarSpearHeatPlayer : ModPlayer
{
    public float HeatAcculumationTimer { get; private set; }

    public bool Empowered { get; set; }

    /// <summary>
    ///     Max is 1f
    /// </summary>
    public float Heat { get; private set; }

    public bool Active { get; private set; }

    public bool ConsumeHeat(float heat, bool pay = true)
    {
        if (Heat - heat > 0f)
        {
            if (pay)
            {
                Heat -= heat;
            }

            return true;
        }

        return false;
    }

    public void AddHeat(float heat)
    {
        Heat += heat;
        HeatAcculumationTimer = 120f;

        if (Heat >= 1f)
        {
            Heat = 1f;
            Active = true;
        }
    }

    public override void PostUpdateBuffs()
    {
        //Main.NewText(Heat);
        const float SevenSeconds = 1f / (7 * 60f);

        if (!Active && HeatAcculumationTimer > 0)
        {
            HeatAcculumationTimer--;
        }

        if (Heat > 0f && (Active || HeatAcculumationTimer <= 0))
        {
            Heat = Math.Max(Heat - SevenSeconds, 0f);
        }
        else
        {
            Active = false;
        }
    }

    public override void PostUpdateMiscEffects()
    {
        if (Active && Main.rand.NextBool(13))
        {
            var particle = HeatLightning.pool.RequestParticle();
            particle.Prepare(Player.MountedCenter, Player.velocity * 2f + Main.rand.NextVector2Circular(10, 10), Main.rand.NextFloat(-2f, 2f), Main.rand.Next(5, 15), Main.rand.NextFloat(0.3f, 1.5f));
            ParticleEngine.Particles.Add(particle);
        }
    }
}