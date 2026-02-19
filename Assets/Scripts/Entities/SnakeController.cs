using UnityEngine;
using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// Snake AI controller with Manhattan distance pursuit (from sample/utils/maze-logic.js)
    /// </summary>
    public class SnakeController
    {
        private GameState state;
        private System.Action onGameOver;

        private const float SNAKE_STEP_INTERVAL = 0.02f; // 20ms per step
        private const int MAX_SNAKE_STEPS = 100; // Prevent infinite loops

        public SnakeController(GameState gameState)
        {
            state = gameState;
        }

        public void SetGameOverCallback(System.Action callback)
        {
            onGameOver = callback;
        }

        /// <summary>
        /// Start continuous snake movement
        /// Called after player makes a move
        /// </summary>
        public void StartSnakeMovement()
        {
            state.isSnakeMoving = true;
            state.snakeStepTimer = 0f;
            state.snakeMoveSteps = 0;
        }

        /// <summary>
        /// Update snake movement (called every frame)
        /// From maze-logic.js:62-90
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!state.isSnakeMoving)
                return;

            if (state.gameOver || state.gameWon)
            {
                state.isSnakeMoving = false;
                return;
            }

            state.snakeStepTimer += deltaTime;

            // Move one step every 20ms
            if (state.snakeStepTimer >= SNAKE_STEP_INTERVAL)
            {
                state.snakeStepTimer = 0f;

                // Update snakes one step
                if (!UpdateSnakesOneStep())
                {
                    // Stop moving if snakes can't move anymore
                    state.isSnakeMoving = false;
                }
            }
        }

        /// <summary>
        /// Update all snakes one step using two-pass algorithm
        /// From maze-logic.js:386-511
        /// Returns true if any snake moved (continue animation)
        /// Returns false if all snakes stopped or caught player (stop animation)
        /// </summary>
        private bool UpdateSnakesOneStep()
        {
            if (state.snakes.Count == 0)
                return false;

            if (state.gameOver || state.gameWon)
                return false;

            // Prevent infinite loops
            state.snakeMoveSteps++;
            if (state.snakeMoveSteps > MAX_SNAKE_STEPS)
                return false;

            // Clear all snake positions from grid (before calculating moves)
            foreach (var snake in state.snakes)
            {
                state.gridManager.SetCell(snake.position, CellType.Empty);
            }

            Vector2Int playerPos = state.playerPos;
            bool snakeBitPlayer = false;
            bool anySnakeMoved = false;

            // Two-pass update to prevent order-dependent bugs

            // PASS 1: Calculate all next positions
            List<(SnakeData snake, Vector2Int nextPos, Direction nextDir, bool moved)> nextStates = new List<(SnakeData, Vector2Int, Direction, bool)>();

            foreach (var snake in state.snakes)
            {
                Vector2Int bestNextPos = snake.position;
                Direction nextDirection = snake.facing;
                bool foundMove = false;

                // Current Manhattan distance to player
                int currentDistance = DirectionHelper.ManhattanDistance(playerPos, snake.position);

                // If already on player, stay put (game will end)
                if (currentDistance == 0)
                {
                    snakeBitPlayer = true;
                    nextStates.Add((snake, snake.position, snake.facing, false));
                    continue;
                }

                int minDistance = currentDistance;

                // Try horizontal move
                if (playerPos.x != snake.position.x)
                {
                    int step = playerPos.x > snake.position.x ? 1 : -1;
                    Vector2Int targetPos = new Vector2Int(snake.position.x + step, snake.position.y);
                    int distance = DirectionHelper.ManhattanDistance(playerPos, targetPos);

                    if (distance < minDistance && MovementValidator.CanSnakeWalk(state, targetPos))
                    {
                        minDistance = distance;
                        bestNextPos = targetPos;
                        foundMove = true;
                        nextDirection = step > 0 ? Direction.Right : Direction.Left;
                    }
                }

                // Try vertical move
                if (playerPos.y != snake.position.y)
                {
                    int step = playerPos.y > snake.position.y ? 1 : -1;
                    Vector2Int targetPos = new Vector2Int(snake.position.x, snake.position.y + step);
                    int distance = DirectionHelper.ManhattanDistance(playerPos, targetPos);

                    if (distance < minDistance && MovementValidator.CanSnakeWalk(state, targetPos))
                    {
                        minDistance = distance;
                        bestNextPos = targetPos;
                        foundMove = true;
                        nextDirection = step > 0 ? Direction.Down : Direction.Up;
                    }
                }

                nextStates.Add((snake, bestNextPos, nextDirection, foundMove));

                if (foundMove)
                    anySnakeMoved = true;
            }

            // PASS 2: Apply all movements
            for (int i = 0; i < nextStates.Count; i++)
            {
                var (snake, nextPos, nextDir, moved) = nextStates[i];

                if (moved)
                {
                    snake.position = nextPos;
                    snake.facing = nextDir;
                }

                // Check if snake caught player after moving
                if (snake.position == playerPos)
                {
                    snakeBitPlayer = true;
                }
            }

            // Restore snake positions to grid
            RestoreSnakeCells();

            // If player was caught, trigger game over and stop animation
            if (snakeBitPlayer)
            {
                GameOver();
                return false;
            }

            // Continue if any snake moved
            return anySnakeMoved;
        }

        /// <summary>
        /// Restore snake cells on grid after movement
        /// </summary>
        private void RestoreSnakeCells()
        {
            foreach (var snake in state.snakes)
            {
                if (state.gridManager.IsInside(snake.position))
                {
                    state.gridManager.SetCell(snake.position, CellType.Snake);
                }
            }
        }

        /// <summary>
        /// Trigger game over
        /// </summary>
        private void GameOver()
        {
            state.gameOver = true;
            onGameOver?.Invoke();
        }
    }
}
