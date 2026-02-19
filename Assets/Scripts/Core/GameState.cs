using UnityEngine;
using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// Snake data structure
    /// </summary>
    [System.Serializable]
    public class SnakeData
    {
        public Vector2Int position;
        public Direction facing;

        public SnakeData(Vector2Int pos, Direction dir)
        {
            position = pos;
            facing = dir;
        }
    }

    /// <summary>
    /// Animation data for smooth movement transitions
    /// </summary>
    public class AnimationData
    {
        public CellType type;
        public Vector2 startPos;      // Grid position (can be fractional during animation)
        public Vector2 targetPos;
        public float startTime;        // Time.time when animation started
        public float duration;         // Animation duration in seconds

        public AnimationData(CellType cellType, Vector2 start, Vector2 target, float durationMs)
        {
            type = cellType;
            startPos = start;
            targetPos = target;
            startTime = Time.time;
            duration = durationMs / 1000f; // Convert ms to seconds
        }

        /// <summary>
        /// Get current interpolated position
        /// </summary>
        public Vector2 GetCurrentPosition()
        {
            float elapsed = Time.time - startTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            return Vector2.Lerp(startPos, targetPos, progress);
        }

        /// <summary>
        /// Check if animation is complete
        /// </summary>
        public bool IsComplete()
        {
            return Time.time >= startTime + duration;
        }
    }

    /// <summary>
    /// Central game state (from sample/utils/maze-state.js and maze-logic.js)
    /// </summary>
    public class GameState
    {
        // Grid management
        public GridManager gridManager;

        // Entity positions
        public Vector2Int playerPos;
        public Direction playerFacing;
        public List<SnakeData> snakes;
        public List<Vector2Int> pushableStones;
        public List<Vector2Int> fixedStones;
        public List<Vector2Int> dynamite;
        public List<Vector2Int> crackedStones;
        public List<Vector2Int> boxes;
        public List<List<Vector2Int>> cloudFogGroups; // First element = cloud, rest = fog
        public Vector2Int exitPos;

        // Game state
        public int dynamiteCount;
        public bool gameOver;
        public bool gameWon;

        // Animation queue
        public List<AnimationData> animations;

        // Snake movement state
        public bool isSnakeMoving;
        public float snakeStepTimer;
        public int snakeMoveSteps;

        public GameState()
        {
            gridManager = new GridManager();
            snakes = new List<SnakeData>();
            pushableStones = new List<Vector2Int>();
            fixedStones = new List<Vector2Int>();
            dynamite = new List<Vector2Int>();
            crackedStones = new List<Vector2Int>();
            boxes = new List<Vector2Int>();
            cloudFogGroups = new List<List<Vector2Int>>();
            animations = new List<AnimationData>();

            playerFacing = Direction.Right;
            dynamiteCount = 0;
            gameOver = false;
            gameWon = false;
            isSnakeMoving = false;
            snakeStepTimer = 0f;
            snakeMoveSteps = 0;
        }

        /// <summary>
        /// Initialize state from level data
        /// </summary>
        public void InitializeFromLevel(LevelData levelData)
        {
            // Reset grid
            gridManager.Reset();

            // Clear all lists
            snakes.Clear();
            pushableStones.Clear();
            fixedStones.Clear();
            dynamite.Clear();
            crackedStones.Clear();
            boxes.Clear();
            cloudFogGroups.Clear();
            animations.Clear();

            // Reset state
            playerPos = levelData.playerStart;
            exitPos = levelData.exitPos;
            playerFacing = Direction.Right;
            dynamiteCount = 0;
            gameOver = false;
            gameWon = false;
            isSnakeMoving = false;
            snakeStepTimer = 0f;
            snakeMoveSteps = 0;

            // Copy entity positions from level data
            foreach (var snake in levelData.snakes)
            {
                snakes.Add(new SnakeData(snake.position, snake.direction));
            }

            pushableStones.AddRange(levelData.pushableStones);
            fixedStones.AddRange(levelData.fixedStones);
            dynamite.AddRange(levelData.dynamite);
            crackedStones.AddRange(levelData.crackedStones);
            boxes.AddRange(levelData.boxes);

            // Copy cloud/fog groups
            foreach (var group in levelData.cloudFogGroups)
            {
                cloudFogGroups.Add(new List<Vector2Int>(group.positions));
            }

            // Populate grid with entities
            PopulateGrid();
        }

        /// <summary>
        /// Populate grid with all entities
        /// </summary>
        private void PopulateGrid()
        {
            // Set fixed stones
            foreach (var pos in fixedStones)
            {
                gridManager.SetCell(pos, CellType.FixedStone);
            }

            // Set pushable stones
            foreach (var pos in pushableStones)
            {
                gridManager.SetCell(pos, CellType.Stone);
            }

            // Set boxes
            foreach (var pos in boxes)
            {
                gridManager.SetCell(pos, CellType.Box);
            }

            // Set cracked stones
            foreach (var pos in crackedStones)
            {
                gridManager.SetCell(pos, CellType.CrackedStone);
            }

            // Set dynamite
            foreach (var pos in dynamite)
            {
                gridManager.SetCell(pos, CellType.Dynamite);
            }

            // Set cloud/fog groups
            foreach (var group in cloudFogGroups)
            {
                if (group.Count > 0)
                {
                    // First element is cloud
                    gridManager.SetCell(group[0], CellType.Cloud);

                    // Rest are fog
                    for (int i = 1; i < group.Count; i++)
                    {
                        gridManager.SetCell(group[i], CellType.Fog);
                    }
                }
            }

            // Set snakes
            foreach (var snake in snakes)
            {
                gridManager.SetCell(snake.position, CellType.Snake);
            }

            // Set exit
            gridManager.SetCell(exitPos, CellType.Exit);

            // Set player (last, so it's on top)
            gridManager.SetCell(playerPos, CellType.Player);
        }

        /// <summary>
        /// Clean up completed animations
        /// </summary>
        public void CleanupAnimations()
        {
            animations.RemoveAll(anim => anim.IsComplete());
        }
    }
}
