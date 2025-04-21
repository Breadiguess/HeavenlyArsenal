using HeavenlyArsenal.Content.Subworlds;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class ShrineRopeSystem : ModSystem
{
    private static readonly List<ShrineRopeData> ropes = new List<ShrineRopeData>(32);

    public override void OnModLoad()
    {
        ForgottenShrineSystem.OnEnter += SettleRopesOnEnteringWorldWrapper;
    }

    private static void SettleRopesOnEnteringWorldWrapper()
    {
        new Thread(SettleRopesOnEnteringWorld).Start();
    }

    private static void SettleRopesOnEnteringWorld()
    {
        foreach (ShrineRopeData rope in ropes)
        {
            for (int i = 0; i < 4; i++)
                rope.VerletRope.Settle();
        }
    }

    /// <summary>
    /// Registers a new rope into the set of ropes maintained by the world.
    /// </summary>
    public static void Register(ShrineRopeData rope)
    {
        bool ropeAlreadyExists = ropes.Any(r => (r.Start == rope.Start && r.End == rope.End) ||
                                                (r.Start == rope.End && r.End == rope.Start));
        if (ropeAlreadyExists)
            return;

        ropes.Add(rope);
    }

    /// <summary>
    /// Removes a given existing rope from the set of ropes maintained by the world.
    /// </summary>
    public static void Remove(ShrineRopeData rope) => ropes.Remove(rope);

    public override void SaveWorldData(TagCompound tag)
    {
        tag["RopeCount"] = ropes.Count;

        TagCompound ropesTag = new TagCompound();
        for (int i = 0; i < ropes.Count; i++)
            ropesTag.Add($"Rope{i}", ropes[i].Serialize());

        tag["Ropes"] = ropesTag;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        ropes.Clear();

        if (!tag.TryGet("RopeCount", out int ropeCount) || !tag.TryGet("Ropes", out TagCompound ropeTag))
            return;

        for (int i = 0; i < ropeCount; i++)
            ropes.Add(ShrineRopeData.Deserialize(ropeTag.GetCompound($"Rope{i}")));
    }

    public override void PostUpdatePlayers()
    {
        for (int i = 0; i < ropes.Count; i++)
        {
            ShrineRopeData rope = ropes[i];
            rope.Update_Standard();

            // Account for the case in which a rope gets removed in the middle of the loop.
            if (!ropes.Contains(rope))
                i--;
        }
    }

    public override void PostDrawTiles()
    {
        if (ropes.Count <= 0)
            return;

        Main.spriteBatch.ResetToDefault(false);
        foreach (ShrineRopeData rope in ropes)
            rope.Render(true, Color.White);

        Main.spriteBatch.End();
    }
}
