using HeavenlyArsenal.Core;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.LeverAction;

public class AvatarRifle_MuzzleFlash : BaseParticle
{
    public static ParticlePool<AvatarRifle_MuzzleFlash> pool = new(500, GetNewParticle<AvatarRifle_MuzzleFlash>);

    public Vector2 Position;

    public float Rotation;

    public int MaxTime;

    public int TimeLeft;

    public void Prepare(Vector2 position, float Rotation, int Maxtime)
    {
        MaxTime = Maxtime;
        Position = position;
        this.Rotation = Rotation;
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        MaxTime = 1;
        TimeLeft = 0;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        TimeLeft++;

        if (TimeLeft > MaxTime)
        {
            ShouldBeRemovedFromRenderer = true;
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        var texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Ranged/LeverAction/AvatarRifle_MuzzleFLash").Value;
        var progress = (float)TimeLeft / MaxTime;
        var frameCount = (int)MathF.Floor(MathF.Sqrt(progress) * 7);
        var frame = texture.Frame(1, 7, 0, frameCount);

        var alpha = 1f - progress;

        var drawColor = Color.AntiqueWhite;
        var anchorPosition = new Vector2(frame.Width / 2, frame.Height / 6);

        var DrawPos = Position - settings.AnchorPosition;
        spritebatch.Draw(texture, DrawPos, frame, drawColor, Rotation, texture.Size() * 0.5f, 1, 0, 0);
    }
}