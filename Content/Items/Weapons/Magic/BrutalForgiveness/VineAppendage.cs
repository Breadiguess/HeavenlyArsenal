using ReLogic.Content;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.BrutalForgiveness;

/// <summary>
///     Represents an appendage on a brutal vine.
/// </summary>
public class VineAppendage
{
    /// <summary>
    ///     The angle of this appendage.
    /// </summary>
    public float Angle;

    /// <summary>
    ///     The completion ratio upon which this appendage appears on the vine.
    /// </summary>
    public float VinePositionInterpolant;

    /// <summary>
    ///     The maximum scale of this appendage.
    /// </summary>
    public float MaxScale;

    /// <summary>
    ///     The origin of this appendage.
    /// </summary>
    public Vector2 Origin;

    /// <summary>
    ///     The texture of this appendage.
    /// </summary>
    public Asset<Texture2D> Texture;
}