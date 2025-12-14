namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight;

public static class RainbowColorGenerator
{
    /// <summary>
    ///     Just Outputs a random color.
    ///     literally the exact same as trailcolor function, but without the same control.
    /// </summary>
    /// <returns></returns>
    public static Color GenerateRandomColor()
    {
        var a = Main.rand.NextFloat(0, 1);

        return TrailColorFunction(a);
    }

    public static Color GenerateLinearColor()
    {
        var a = (float)Math.Tanh(Main.GlobalTimeWrappedHourly);

        return TrailColorFunction(a);
    }

    /// <summary>
    ///     outputs a Color based on the number you put in (ranging from 0.0f to 1.0f).
    ///     only exists because i got tired of copypasting the same code.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static Color TrailColorFunction(float p)
    {
        // Cycle hue over [0, 360), scaled by p
        // You can also multiply p to make the cycle repeat faster/slower
        //float hue = (p * 360f) % 360f;
        var hue = (p * 360f + Main.GlobalTimeWrappedHourly * 120f) % 360f;

        return HsvToColor(hue, 0.75f, 1f, 0);
    }

    public static Color HsvToColor(float h, float s, float v, byte alpha = 255)
    {
        var hi = (int)(h / 60f) % 6;
        var f = h / 60f - MathF.Floor(h / 60f);

        v = v * 255f;
        var vi = (int)v;
        var p = (int)(v * (1f - s));
        var q = (int)(v * (1f - f * s));
        var t = (int)(v * (1f - (1f - f) * s));

        return hi switch
        {
            0 => new Color(vi, t, p, alpha),
            1 => new Color(q, vi, p, alpha),
            2 => new Color(p, vi, t, alpha),
            3 => new Color(p, q, vi, alpha),
            4 => new Color(t, p, vi, alpha),
            _ => new Color(vi, p, q, alpha)
        };
    }
}