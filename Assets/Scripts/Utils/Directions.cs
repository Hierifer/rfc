using UnityEngine;

namespace MazeGame
{
    /// <summary>
    /// Direction enum for movement (from sample/utils/maze-types.js)
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// Helper methods for direction operations
    /// </summary>
    public static class DirectionHelper
    {
        /// <summary>
        /// Get the opposite direction
        /// </summary>
        public static Direction GetOpposite(Direction dir)
        {
            return dir switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => dir
            };
        }

        /// <summary>
        /// Get the offset vector for a direction
        /// </summary>
        public static Vector2Int GetOffset(Direction dir)
        {
            return dir switch
            {
                Direction.Up => new Vector2Int(0, -1),      // row -1
                Direction.Down => new Vector2Int(0, 1),     // row +1
                Direction.Left => new Vector2Int(-1, 0),    // col -1
                Direction.Right => new Vector2Int(1, 0),    // col +1
                _ => Vector2Int.zero
            };
        }

        /// <summary>
        /// Get direction from two positions
        /// </summary>
        public static Direction? GetDirection(Vector2Int from, Vector2Int to)
        {
            Vector2Int delta = to - from;

            if (delta.x == 0 && delta.y == -1) return Direction.Up;
            if (delta.x == 0 && delta.y == 1) return Direction.Down;
            if (delta.x == -1 && delta.y == 0) return Direction.Left;
            if (delta.x == 1 && delta.y == 0) return Direction.Right;

            return null; // Not adjacent or diagonal
        }

        /// <summary>
        /// Check if two positions are adjacent (orthogonally)
        /// </summary>
        public static bool IsAdjacent(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        /// <summary>
        /// Calculate Manhattan distance between two positions
        /// </summary>
        public static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}
