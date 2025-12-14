namespace HeavenlyArsenal.Content.Items.Weapons.Magic.RocheLimit;

public interface IDrawsOverRocheLimitDistortion
{
    public float Layer { get; }

    void RenderOverDistortion();
}