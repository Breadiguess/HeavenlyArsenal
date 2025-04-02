using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Melee.AvatarSpear;

public class AvatarSpearHeatPlayer : ModPlayer
{
    public float HeatAcculumationTimer { get; private set; }

    // Max is 1f
    public float Heat { get; private set; }

    public bool Active { get; private set; }

    public bool ConsumeHeat(float heat)
    {
        if (Heat - heat > 0f)
        {
            Heat -= heat;
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
        const float SevenSeconds = 1f / (7f * 60f);

        if (!Active && HeatAcculumationTimer > 0)
            HeatAcculumationTimer--;

        if (Heat > 0f && (Active || HeatAcculumationTimer <= 0))
            Heat = Math.Max(Heat - SevenSeconds, 0f);
        else
            Active = false;
    }
}
