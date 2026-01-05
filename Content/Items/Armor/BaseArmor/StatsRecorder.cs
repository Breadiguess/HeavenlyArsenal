using System.Collections.Generic;

namespace HeavenlyArsenal.Content.Items.Armor.BaseArmor
{

    internal static class StatsRecorder
    {
        internal enum Mode { None, Preview, Runtime }

        private static Mode currentMode = Mode.None;
        private static int currentItemType = -1;

        private static readonly Dictionary<int, List<RecordedStat>> recordedStats = new();

        internal static bool IsPreview => currentMode == Mode.Preview;

        internal static void BeginRuntime(int itemType)
        {
            currentMode = Mode.Runtime;
            currentItemType = itemType;
            EnsureList(itemType, clear: true);
        }

        internal static void BuildPreview(int itemType, Player contextPlayer, System.Action run)
        {
            currentMode = Mode.Preview;
            currentItemType = itemType;
            EnsureList(itemType, clear: true);

            run?.Invoke();

            End();
        }

        internal static void End()
        {
            currentMode = Mode.None;
            currentItemType = -1;
        }

        internal static void Record(RecordedStat stat)
        {
            if (currentItemType < 0)
                return;

            EnsureList(currentItemType, clear: false).Add(stat);
        }

        internal static void InjectTooltips(int itemType, List<TooltipLine> tooltips)
        {
            if (!recordedStats.TryGetValue(itemType, out var stats) || stats.Count == 0)
                return;

            int index = tooltips.FindIndex(t => t.Name == "Tooltip0");
            if (index < 0)
                index = tooltips.Count;

            foreach (var stat in stats)
                tooltips.Insert(index++, stat.ToTooltipLine());
        }

        private static List<RecordedStat> EnsureList(int itemType, bool clear)
        {
            if (!recordedStats.TryGetValue(itemType, out var list))
                recordedStats[itemType] = list = new List<RecordedStat>();

            if (clear)
                list.Clear();

            return list;
        }
    }




}
