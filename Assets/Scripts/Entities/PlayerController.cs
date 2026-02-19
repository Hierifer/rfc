using UnityEngine;
using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// Player movement and interaction controller (from sample/utils/maze-logic.js)
    /// </summary>
    public class PlayerController
    {
        private GameState state;
        private System.Action onWin;
        private System.Action onLose;
        private System.Action<int> onDynamiteChange;

        public PlayerController(GameState gameState)
        {
            state = gameState;
        }

        public void SetCallbacks(System.Action winCallback, System.Action loseCallback, System.Action<int> dynamiteCallback)
        {
            onWin = winCallback;
            onLose = loseCallback;
            onDynamiteChange = dynamiteCallback;
        }

        /// <summary>
        /// Attempt to move player in a direction
        /// Returns true if movement succeeded
        /// </summary>
        public bool MovePlayer(Direction direction)
        {
            if (state.gameOver || state.gameWon)
                return false;

            // Don't allow player input while snakes are moving
            if (state.isSnakeMoving)
                return false;

            Vector2Int offset = DirectionHelper.GetOffset(direction);
            Vector2Int playerPos = state.playerPos;
            Vector2Int newPos = playerPos + offset;

            // Update player facing direction
            state.playerFacing = direction;

            // Check bounds
            if (!state.gridManager.IsInside(newPos))
                return false;

            CellType targetCell = state.gridManager.GetCell(newPos);

            // Try direct movement
            if (MovementValidator.CanPlayerWalk(state, newPos))
            {
                MovePlayerTo(newPos);
                return true;
            }

            // Try pushing stone (slides until obstacle)
            if (targetCell == CellType.Stone)
            {
                if (TryPushStone(newPos, direction))
                {
                    MovePlayerTo(newPos);
                    return true;
                }
            }

            // Try pushing box (one step only)
            if (targetCell == CellType.Box)
            {
                if (TryPushBox(newPos, direction))
                {
                    MovePlayerTo(newPos);
                    return true;
                }
            }

            // Try using dynamite on cracked stone
            if (targetCell == CellType.CrackedStone)
            {
                if (state.dynamiteCount > 0)
                {
                    UseDynamite(newPos);
                    MovePlayerTo(newPos);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Move player to new position (internal method)
        /// Handles item collection, cloud consumption, win condition
        /// </summary>
        private void MovePlayerTo(Vector2Int newPos)
        {
            Vector2Int oldPos = state.playerPos;

            // Clear old position
            state.gridManager.SetCell(oldPos, CellType.Floor);

            CellType targetCell = state.gridManager.GetCell(newPos);

            // Collect dynamite
            if (targetCell == CellType.Dynamite)
            {
                CollectDynamite(newPos);
            }

            // Consume cloud group
            if (targetCell == CellType.Cloud)
            {
                ConsumeCloudGroup(newPos);
            }

            // Update position
            state.playerPos = newPos;
            state.gridManager.SetCell(newPos, CellType.Player);

            // Check win condition
            if (newPos == state.exitPos)
            {
                CheckWinCondition();
            }
        }

        /// <summary>
        /// Try to push a stone (slides until hitting obstacle)
        /// From maze-logic.js:204-273
        /// </summary>
        private bool TryPushStone(Vector2Int stonePos, Direction direction)
        {
            Vector2Int offset = DirectionHelper.GetOffset(direction);

            // Calculate how far the stone can slide
            Vector2Int current = stonePos;

            while (true)
            {
                Vector2Int next = current + offset;

                // Check bounds
                if (!state.gridManager.IsInside(next))
                    break;

                CellType nextCell = state.gridManager.GetCell(next);

                // Stone can only slide on empty or floor cells
                bool isPassable = nextCell == CellType.Empty || nextCell == CellType.Floor;

                if (!isPassable)
                    break;

                // Continue sliding
                current = next;
            }

            // If stone didn't move (blocked immediately), push fails
            if (current == stonePos)
                return false;

            // Find stone in list
            int stoneIndex = state.pushableStones.FindIndex(s => s == stonePos);
            if (stoneIndex == -1)
                return false;

            // Calculate animation duration (50ms per cell moved)
            int moveDistance = DirectionHelper.ManhattanDistance(stonePos, current);

            if (moveDistance > 0)
            {
                // Add sliding animation
                state.animations.Add(new AnimationData(
                    CellType.Stone,
                    stonePos,
                    current,
                    moveDistance * 50 // ms
                ));
            }

            // Update stone position in list
            state.pushableStones[stoneIndex] = current;

            // Update grid
            state.gridManager.SetCell(stonePos, CellType.Floor);
            state.gridManager.SetCell(current, CellType.Stone);

            return true;
        }

        /// <summary>
        /// Try to push a box (one step only, doesn't slide)
        /// From maze-logic.js:279-314
        /// </summary>
        private bool TryPushBox(Vector2Int boxPos, Direction direction)
        {
            Vector2Int offset = DirectionHelper.GetOffset(direction);
            Vector2Int targetPos = boxPos + offset;

            // Check if target is in bounds
            if (!state.gridManager.IsInside(targetPos))
                return false;

            CellType targetCell = state.gridManager.GetCell(targetPos);

            // Box can only be pushed to empty or floor cells
            bool isEmpty = targetCell == CellType.Empty || targetCell == CellType.Floor;

            if (!isEmpty)
                return false;

            // Find box in list
            int boxIndex = state.boxes.FindIndex(b => b == boxPos);
            if (boxIndex == -1)
                return false;

            // Add push animation (150ms for box)
            state.animations.Add(new AnimationData(
                CellType.Box,
                boxPos,
                targetPos,
                150 // ms
            ));

            // Update box position
            state.boxes[boxIndex] = targetPos;

            // Update grid
            state.gridManager.SetCell(boxPos, CellType.Floor);
            state.gridManager.SetCell(targetPos, CellType.Box);

            return true;
        }

        /// <summary>
        /// Collect dynamite at position
        /// </summary>
        private void CollectDynamite(Vector2Int pos)
        {
            int index = state.dynamite.FindIndex(d => d == pos);
            if (index != -1)
            {
                state.dynamite.RemoveAt(index);
                state.dynamiteCount++;

                onDynamiteChange?.Invoke(state.dynamiteCount);
            }
        }

        /// <summary>
        /// Use dynamite to destroy cracked stone
        /// </summary>
        private void UseDynamite(Vector2Int pos)
        {
            int index = state.crackedStones.FindIndex(s => s == pos);
            if (index != -1)
            {
                state.crackedStones.RemoveAt(index);
                state.dynamiteCount--;
                state.gridManager.SetCell(pos, CellType.Empty);

                onDynamiteChange?.Invoke(state.dynamiteCount);
            }
        }

        /// <summary>
        /// Consume entire cloud/fog group when stepping on cloud
        /// From maze-logic.js:356-377
        /// </summary>
        private void ConsumeCloudGroup(Vector2Int cloudPos)
        {
            // Find the group containing this cloud (cloud is always first element)
            int groupIndex = -1;

            for (int i = 0; i < state.cloudFogGroups.Count; i++)
            {
                if (state.cloudFogGroups[i].Count > 0 && state.cloudFogGroups[i][0] == cloudPos)
                {
                    groupIndex = i;
                    break;
                }
            }

            if (groupIndex != -1)
            {
                var group = state.cloudFogGroups[groupIndex];

                // Clear all cells in the group (cloud + fog)
                foreach (var cell in group)
                {
                    state.gridManager.SetCell(cell, CellType.Empty);
                }

                // Remove group from list
                state.cloudFogGroups.RemoveAt(groupIndex);
            }
        }

        /// <summary>
        /// Check if player reached exit
        /// </summary>
        private void CheckWinCondition()
        {
            state.gameWon = true;
            onWin?.Invoke();
        }
    }
}
