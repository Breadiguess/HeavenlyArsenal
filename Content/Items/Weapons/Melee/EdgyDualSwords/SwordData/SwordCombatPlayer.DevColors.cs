using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords.SwordData
{
    public partial class SwordCombatPlayer
    {

        public static readonly List<DevColorRule> DevColorRules = new();

        public static void LoadDevColorRules()
        {
            DevColorRules.Clear();

            RegisterPalette(
                id: "Default",
                priority: 0,
                condition: _ => true,
                colors:
                [
                    Color.White,
                Color.LightGray,
                Color.Gray
                ]);

            RegisterPalette(
                id: "RyanWithPet",
                priority: 100,
                condition: player =>
                {
                    string name = player.name ?? string.Empty;

                    bool matchingName =
                        name.Contains("ryan", StringComparison.OrdinalIgnoreCase) ||
                        name.Contains("graham", StringComparison.OrdinalIgnoreCase);

                    bool hasSpecificPet = player.petFlagDD2Gato || player.blackCat; // example flags, replace with your actual pet condition

                    return matchingName && hasSpecificPet;
                },
                colors:
                [
                    Color.Cyan,
                Color.DeepSkyBlue,
                Color.BlueViolet
                ]);

            RegisterPalette(
                id: "NameContainsDev",
                priority: 50,
                condition: player =>
                {
                    string name = player.name ?? string.Empty;
                    return name.Contains("dev", StringComparison.OrdinalIgnoreCase);
                },
                colors:
                [
                    Color.HotPink,
                Color.Orange,
                Color.Yellow
                ]);
        }

        public static void RegisterPalette(string id, int priority, Func<Player, bool> condition, params Color[] colors)
        {
            DevColorRules.Add(new DevColorRule(
                id,
                priority,
                condition,
                new DevColorPalette(colors)
            ));

            DevColorRules.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        public static DevColorPalette GetPaletteFor(Player player)
        {
            for (int i = 0; i < DevColorRules.Count; i++)
            {
                if (DevColorRules[i].Condition(player))
                    return DevColorRules[i].Palette;
            }

            return default;
        }

        public readonly struct DevColorRule
        {
            public readonly string Id;
            public readonly int Priority;
            public readonly Func<Player, bool> Condition;
            public readonly DevColorPalette Palette;

            public DevColorRule(string id, int priority, Func<Player, bool> condition, DevColorPalette palette)
            {
                Id = id;
                Priority = priority;
                Condition = condition;
                Palette = palette;
            }
        }

        public readonly struct DevColorPalette
        {
            public readonly int Length;
            public readonly Color[] SmallBladeColors;
            public readonly Color[] LargeBladeColors;
            public readonly Color[] CombinedBladeColors;
            public readonly Color[] Colors;

            public DevColorPalette(Color[] colors)
            {
                Colors = colors ?? Array.Empty<Color>();
                Length = Colors.Length;
            }

            public Color this[int index] => Colors[index];
        }

    }
}
