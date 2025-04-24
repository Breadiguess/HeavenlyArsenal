using HeavenlyArsenal.Content.Subworlds;
using HeavenlyArsenal.Content.Tiles.Generic;
using Luminance.Common.Utilities;
using System.Linq;
using System.Threading;
using Terraria;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class ShrineRopeSystem : WorldOrientedTileObjectManager<ShrineRopeData>
{
    public override void OnModLoad()
    {
        ForgottenShrineSystem.OnEnter += SettleRopesOnEnteringWorldWrapper;
    }

    private void SettleRopesOnEnteringWorldWrapper()
    {
        new Thread(SettleRopesOnEnteringWorld).Start();
    }

    private void SettleRopesOnEnteringWorld()
    {
        foreach (ShrineRopeData rope in tileObjects)
        {
            for (int i = 0; i < 4; i++)
                rope.VerletRope.Settle();
        }
    }

    /// <summary>
    /// Registers a new rope into the set of ropes maintained by the world.
    /// </summary>
    public override void Register(ShrineRopeData rope)
    {
        bool ropeAlreadyExists = tileObjects.Any(r => (r.Start == rope.Start && r.End == rope.End) ||
                                                      (r.Start == rope.End && r.End == rope.Start));
        if (ropeAlreadyExists)
            return;

        base.Register(rope);
    }

    public override void PostDrawTiles()
    {
        if (tileObjects.Count <= 0)
            return;

        Main.spriteBatch.ResetToDefault(false);
        foreach (ShrineRopeData rope in tileObjects)
            rope.Render();

        Main.spriteBatch.End();
    }
}
