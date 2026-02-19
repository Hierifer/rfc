using UnityEngine;
using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// 自动移动控制器 - 处理点击寻路后的自动移动
    /// 基于 sample/game.js 的自动移动逻辑
    /// </summary>
    public class AutoMoveController
    {
        private GameState state;
        private PlayerController playerController;
        private SnakeController snakeController;

        private List<Vector2Int> autoMovePath;
        private float autoMoveTimer;
        private bool isAutoMoving;

        private const float AUTO_MOVE_SPEED = 0.05f; // 50ms per step (from JS)

        public AutoMoveController(GameState gameState, PlayerController player, SnakeController snake)
        {
            state = gameState;
            playerController = player;
            snakeController = snake;
            autoMovePath = new List<Vector2Int>();
            autoMoveTimer = 0f;
            isAutoMoving = false;
        }

        /// <summary>
        /// 获取当前自动移动路径（用于渲染）
        /// </summary>
        public List<Vector2Int> GetCurrentPath()
        {
            return autoMovePath;
        }

        /// <summary>
        /// 检查是否正在自动移动
        /// </summary>
        public bool IsAutoMoving()
        {
            return isAutoMoving;
        }

        /// <summary>
        /// 开始自动移动沿着路径
        /// </summary>
        public void StartAutoMove(List<Vector2Int> path)
        {
            if (path == null || path.Count == 0)
            {
                Debug.Log("Path is empty, no auto-move started");
                return;
            }

            StopAutoMove();

            autoMovePath = new List<Vector2Int>(path);
            isAutoMoving = true;
            autoMoveTimer = 0f;

            Debug.Log($"Auto-move started with {autoMovePath.Count} steps");
        }

        /// <summary>
        /// 停止自动移动
        /// </summary>
        public void StopAutoMove()
        {
            if (isAutoMoving)
            {
                Debug.Log("Auto-move stopped");
            }

            autoMovePath.Clear();
            isAutoMoving = false;
            autoMoveTimer = 0f;
        }

        /// <summary>
        /// 更新自动移动（每帧调用）
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!isAutoMoving || autoMovePath.Count == 0)
                return;

            // 游戏结束时停止
            if (state.gameOver || state.gameWon)
            {
                StopAutoMove();
                return;
            }

            // 蛇正在移动时暂停自动移动
            if (state.isSnakeMoving)
                return;

            autoMoveTimer += deltaTime;

            // 每 50ms 移动一步
            if (autoMoveTimer >= AUTO_MOVE_SPEED)
            {
                autoMoveTimer = 0f;

                if (autoMovePath.Count > 0)
                {
                    Vector2Int nextPos = autoMovePath[0];
                    autoMovePath.RemoveAt(0);

                    Vector2Int playerPos = state.playerPos;

                    // 计算移动方向
                    Direction? direction = DirectionHelper.GetDirection(playerPos, nextPos);

                    if (direction.HasValue)
                    {
                        bool moved = playerController.MovePlayer(direction.Value);

                        if (!moved)
                        {
                            // 移动失败（路径被阻挡），停止自动移动
                            Debug.Log("Auto-move blocked, stopping");
                            StopAutoMove();
                        }
                        else
                        {
                            // 移动成功，触发蛇移动
                            snakeController.StartSnakeMovement();
                        }
                    }
                    else
                    {
                        // 不相邻，路径有问题
                        Debug.LogWarning($"Path error: {playerPos} -> {nextPos} not adjacent");
                        StopAutoMove();
                    }
                }

                // 路径走完，停止
                if (autoMovePath.Count == 0)
                {
                    StopAutoMove();
                }
            }
        }
    }
}
