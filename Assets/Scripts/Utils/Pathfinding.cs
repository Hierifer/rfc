using UnityEngine;
using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// BFS pathfinding system (from sample/utils/pathfinding.js)
    /// </summary>
    public static class Pathfinding
    {
        /// <summary>
        /// Delegate for checking if a cell is walkable
        /// </summary>
        public delegate bool CanWalkDelegate(Vector2Int pos);

        /// <summary>
        /// Find shortest path using BFS (Breadth-First Search)
        /// Returns null if no path exists
        /// </summary>
        public static List<Vector2Int> FindPath(
            Vector2Int start,
            Vector2Int target,
            CanWalkDelegate canWalk)
        {
            // Early exit if start == target
            if (start == target)
                return new List<Vector2Int>();

            // BFS queue: each entry contains (position, path to that position)
            Queue<(Vector2Int pos, List<Vector2Int> path)> queue = new Queue<(Vector2Int, List<Vector2Int>)>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            // Start BFS
            queue.Enqueue((start, new List<Vector2Int>()));
            visited.Add(start);

            // 4 cardinal directions
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, -1),  // Up
                new Vector2Int(0, 1),   // Down
                new Vector2Int(-1, 0),  // Left
                new Vector2Int(1, 0)    // Right
            };

            while (queue.Count > 0)
            {
                var (currentPos, currentPath) = queue.Dequeue();

                // Try all 4 directions
                foreach (var dir in directions)
                {
                    Vector2Int nextPos = currentPos + dir;

                    // Check if we reached the target
                    if (nextPos == target)
                    {
                        // Build final path
                        List<Vector2Int> finalPath = new List<Vector2Int>(currentPath);
                        finalPath.Add(nextPos);
                        return finalPath;
                    }

                    // Skip if already visited
                    if (visited.Contains(nextPos))
                        continue;

                    // Skip if not walkable
                    if (!canWalk(nextPos))
                        continue;

                    // Add to queue
                    visited.Add(nextPos);
                    List<Vector2Int> newPath = new List<Vector2Int>(currentPath);
                    newPath.Add(nextPos);
                    queue.Enqueue((nextPos, newPath));
                }
            }

            // No path found
            return null;
        }

        /// <summary>
        /// Find best adjacent cell to target (for when target itself is unreachable)
        /// Used when clicking on stones/boxes - find path to adjacent walkable cell
        /// </summary>
        public static Vector2Int? FindBestAdjacentTarget(
            Vector2Int start,
            Vector2Int target,
            CanWalkDelegate canWalk)
        {
            Vector2Int[] neighbors = new Vector2Int[]
            {
                target + new Vector2Int(0, -1),  // Up
                target + new Vector2Int(0, 1),   // Down
                target + new Vector2Int(-1, 0),  // Left
                target + new Vector2Int(1, 0)    // Right
            };

            List<Vector2Int> bestPath = null;
            Vector2Int? bestAdjacent = null;
            int minLength = int.MaxValue;

            foreach (var adjacent in neighbors)
            {
                // Skip if not walkable
                if (!canWalk(adjacent))
                    continue;

                // Find path to this adjacent cell
                var path = FindPath(start, adjacent, canWalk);

                if (path != null && path.Count < minLength)
                {
                    minLength = path.Count;
                    bestPath = path;
                    bestAdjacent = adjacent;
                }
            }

            return bestAdjacent;
        }
    }

    /// <summary>
    /// Movement validation functions (from sample/utils/maze-state.js)
    /// </summary>
    public static class MovementValidator
    {
        /// <summary>
        /// Check if player can walk on a cell
        /// Players can walk on: empty, floor, dynamite, clouds, exits
        /// Players CANNOT walk on: walls, stones, fixed stones, cracked stones, boxes, fog, snakes
        /// </summary>
        public static bool CanPlayerWalk(GameState state, Vector2Int pos)
        {
            CellType cell = state.gridManager.GetCell(pos);

            return cell != CellType.Wall
                && cell != CellType.Stone
                && cell != CellType.FixedStone
                && cell != CellType.CrackedStone
                && cell != CellType.Box
                && cell != CellType.Fog
                && cell != CellType.Snake;
        }

        /// <summary>
        /// Check if snake can walk on a cell
        /// Snakes are more restricted than players
        /// Snakes CANNOT walk on: walls, stones, fixed stones, cracked stones, dynamite, boxes, other snakes, fog, clouds
        /// This creates strategic defensive positions (block snakes with clouds/dynamite)
        /// </summary>
        public static bool CanSnakeWalk(GameState state, Vector2Int pos)
        {
            CellType cell = state.gridManager.GetCell(pos);

            return cell != CellType.Wall
                && cell != CellType.Stone
                && cell != CellType.FixedStone
                && cell != CellType.CrackedStone
                && cell != CellType.Dynamite    // KEY: Snakes can't step on dynamite
                && cell != CellType.Box
                && cell != CellType.Snake
                && cell != CellType.Fog
                && cell != CellType.Cloud;      // KEY: Snakes can't step on clouds
        }

        /// <summary>
        /// Check if a cell is standable (for general purposes)
        /// </summary>
        public static bool IsStandable(GameState state, Vector2Int pos)
        {
            CellType cell = state.gridManager.GetCell(pos);

            return cell == CellType.Empty
                || cell == CellType.Floor
                || cell == CellType.Dynamite
                || cell == CellType.Cloud
                || cell == CellType.Exit
                || cell == CellType.Player;
        }
    }
}
