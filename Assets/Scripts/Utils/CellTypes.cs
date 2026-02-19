using UnityEngine;

namespace MazeGame
{
    /// <summary>
    /// Cell types for the maze grid (from sample/utils/maze-types.js)
    /// </summary>
    public enum CellType
    {
        Empty,
        Player,
        Floor,
        Snake,
        Wall,
        Stone,          // Pushable stone (slides until obstacle)
        FixedStone,     // Immovable stone
        Dynamite,       // Collectible explosive
        CrackedStone,   // Requires dynamite to destroy
        Box,            // Pushable one step only
        Cloud,          // Step on to clear fog group
        Fog,            // Blocks movement, cleared with cloud
        Exit            // Goal position
    }

    /// <summary>
    /// Grid constants
    /// </summary>
    public static class GridConstants
    {
        public const int Width = 19;
        public const int Height = 13;
    }

    /// <summary>
    /// Color configuration for tile rendering
    /// </summary>
    public static class TileColors
    {
        public static readonly Color Background = new Color(0.067f, 0.094f, 0.153f); // #111827
        public static readonly Color Floor = new Color(0.122f, 0.161f, 0.216f);      // #1f2937
        public static readonly Color Wall = new Color(0.2f, 0.255f, 0.333f);         // #334155
        public static readonly Color Player = new Color(0.231f, 0.510f, 0.965f);     // #3b82f6
        public static readonly Color Snake = new Color(0.937f, 0.267f, 0.267f);      // #ef4444
        public static readonly Color Stone = new Color(0.886f, 0.910f, 0.941f);      // #e2e8f0
        public static readonly Color FixedStone = new Color(0.392f, 0.455f, 0.545f); // #64748b
        public static readonly Color Dynamite = new Color(0.961f, 0.620f, 0.043f);   // #f59e0b
        public static readonly Color CrackedStone = new Color(0.984f, 0.443f, 0.522f); // #fb7185
        public static readonly Color Box = new Color(0.573f, 0.251f, 0.055f);        // #92400e
        public static readonly Color Cloud = new Color(0.376f, 0.647f, 0.980f);      // #60a5fa
        public static readonly Color Fog = new Color(0.545f, 0.361f, 0.965f);        // #8b5cf6
        public static readonly Color Exit = new Color(0.133f, 0.773f, 0.369f);       // #22c55e
    }
}
