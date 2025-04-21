namespace HeavenlyArsenal.Content.Subworlds.Generation.Bridges;

public class BridgeSetPlacementProfile
{
    /// <summary>
    /// The bridge generator responsible for this placement profile.
    /// </summary>
    public readonly BridgeSetGenerator Generator;

    /// <summary>
    /// The set of arch heights across the generation span.
    /// </summary>
    public readonly int[] ArchHeights;

    /// <summary>
    /// The set of arch horizontal arch interpolants across the generation span.
    /// </summary>
    public readonly float[] ArchHeightInterpolants;

    /// <summary>
    /// A set of values across the generation span that determined how much higher fences should be at each point.
    /// </summary>
    public readonly int[] FenceExtraHeightMap;

    /// <summary>
    /// A set of flags that across the generation span that determine whether an arch has descended (gone down one or more in Y height) or ascended (gone up one or more in Y height).
    /// </summary>
    public readonly bool[] FenceDescendingFlags;

    public BridgeSetPlacementProfile(BridgeSetGenerator generator)
    {
        Generator = generator;

        int horizontalSpan = generator.Right - generator.Left + 1;
        ArchHeights = new int[horizontalSpan];
        ArchHeightInterpolants = new float[horizontalSpan];
        FenceExtraHeightMap = new int[horizontalSpan];
        FenceDescendingFlags = new bool[horizontalSpan];

        for (int x = generator.Left; x <= generator.Right; x++)
        {
            int index = x - generator.Left;
            int previousHeight = generator.CalculateArchHeight(x - 1);
            int archHeight = generator.CalculateArchHeight(x, out float archHeightInterpolant);
            int nextArchHeight = generator.CalculateArchHeight(x + 1);
            bool ascending = archHeight > previousHeight;
            bool descending = archHeight > nextArchHeight;

            // Supply fence cache data.
            if (ascending)
                FenceExtraHeightMap[index] = archHeight - previousHeight;
            if (descending)
            {
                FenceExtraHeightMap[index] = archHeight - nextArchHeight;
                FenceDescendingFlags[index] = true;
            }

            // Store bridge arch data.
            ArchHeights[index] = archHeight;
            ArchHeightInterpolants[index] = archHeightInterpolant;
        }
    }
}
