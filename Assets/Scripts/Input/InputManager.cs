using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MazeGame
{
    /// <summary>
    /// 处理玩家输入（键盘、鼠标、触摸）
    /// 基于 sample/game.js 的输入处理逻辑
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        private GameManager gameManager;
        private MazeRenderer mazeRenderer;

        // 触摸/滑动检测
        private Vector2 touchStartPos;
        private float touchStartTime;
        private const float MIN_SWIPE_DISTANCE = 30f; // 最小滑动距离（像素）
        private const float MAX_TAP_DURATION = 0.3f;  // 最大点击时间（秒）

        private void Start()
        {
            gameManager = GameManager.Instance;

            // 获取 MazeRenderer（需要用于屏幕坐标转换）
            if (gameManager != null)
            {
                mazeRenderer = gameManager.GetComponent<MazeRenderer>();
            }
        }

        private void Update()
        {
            HandleKeyboardInput();
            HandleMouseInput();
            HandleTouchInput();
        }

        /// <summary>
        /// 处理键盘输入 (WASD / 方向键)
        /// </summary>
        private void HandleKeyboardInput()
        {
            if (gameManager == null) return;

            // 方向键和 WASD
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                gameManager.HandlePlayerMove(Direction.Up);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                gameManager.HandlePlayerMove(Direction.Down);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                gameManager.HandlePlayerMove(Direction.Left);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                gameManager.HandlePlayerMove(Direction.Right);
            }

            // R 重置关卡
            if (Input.GetKeyDown(KeyCode.R))
            {
                gameManager.ResetLevel();
            }

            // N 下一关（测试用）
            if (Input.GetKeyDown(KeyCode.N))
            {
                gameManager.NextLevel();
            }

            // ESC 停止自动移动
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // gameManager.StopAutoMove();
            }
        }

        /// <summary>
        /// 处理鼠标输入（点击寻路）
        /// </summary>
        private void HandleMouseInput()
        {
            if (gameManager == null || mazeRenderer == null) return;

            // 鼠标左键点击
            if (Input.GetMouseButtonDown(0))
            {
                // 如果鼠标点击在UI上，不处理游戏内的点击
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    Debug.Log("Clicked on UI, ignored game input");
                    return;
                }

                Vector2 mousePos = Input.mousePosition;
                HandleTapOrClick(mousePos);
            }
        }

        /// <summary>
        /// 处理触摸输入（滑动和点击）
        /// 基于 sample/game.js:468-605
        /// </summary>
        private void HandleTouchInput()
        {
            if (gameManager == null || mazeRenderer == null) return;
            if (Input.touchCount == 0) return;

            Touch touch = Input.GetTouch(0);

            // 检查触摸是否在UI上
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                // 如果是按下的瞬间在UI上，记录但不处理游戏逻辑
                // 这里简单处理：只要手指在UI上操作，就忽略游戏操作
                return;
            }

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
                touchStartTime = Time.time;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                Vector2 touchEndPos = touch.position;
                float touchDuration = Time.time - touchStartTime;
                Vector2 delta = touchEndPos - touchStartPos;
                float distance = delta.magnitude;

                // 判断是点击还是滑动
                if (distance < MIN_SWIPE_DISTANCE && touchDuration < MAX_TAP_DURATION)
                {
                    // 点击 - 寻路移动
                    HandleTapOrClick(touchEndPos);
                }
                else
                {
                    // 滑动 - 直接移动一步
                    HandleSwipe(delta);
                }
            }
        }

        /// <summary>
        /// 处理点击事件（鼠标或触摸）
        /// 基于 sample/game.js:609-710
        /// </summary>
        private void HandleTapOrClick(Vector2 screenPos)
        {
            if (gameManager == null || mazeRenderer == null) return;

            GameState state = gameManager.GetGameState();
            if (state == null || state.gameOver || state.gameWon) return;

            // 屏幕坐标转网格坐标
            Vector2Int gridPos = mazeRenderer.ScreenToGrid(screenPos);

            // 检查是否在网格范围内
            if (!state.gridManager.IsInside(gridPos))
            {
                Debug.Log($"Click outside grid: {gridPos}");
                return;
            }

            Vector2Int playerPos = state.playerPos;

            // 如果点击的是玩家当前位置，停止自动移动
            if (gridPos == playerPos)
            {
                // gameManager.StopAutoMove();
                Debug.Log("Clicked on player, stopped auto-move");
                return;
            }

            // 1. 检查是否点击了相邻格子（直接移动或交互）
            if (DirectionHelper.IsAdjacent(playerPos, gridPos))
            {
                Direction? direction = DirectionHelper.GetDirection(playerPos, gridPos);
                if (direction.HasValue)
                {
                    // gameManager.StopAutoMove();
                    gameManager.HandlePlayerMove(direction.Value);
                    Debug.Log($"Adjacent move: {direction.Value}");
                }
                return;
            }

            // 2. 不相邻，需要寻路
            CellType targetCell = state.gridManager.GetCell(gridPos);
            bool isInteractable = targetCell == CellType.Stone
                || targetCell == CellType.Box
                || targetCell == CellType.CrackedStone;

            List<Vector2Int> path = null;

            // 尝试直接寻路到目标
            path = Pathfinding.FindPath(
                playerPos,
                gridPos,
                pos => MovementValidator.CanPlayerWalk(state, pos)
            );

            // 如果直接寻路失败，但目标是可交互物体，寻路到其相邻格子
            if (path == null && isInteractable)
            {
                Debug.Log($"Target {gridPos} unreachable, trying adjacent cells");

                Vector2Int? bestAdjacent = Pathfinding.FindBestAdjacentTarget(
                    playerPos,
                    gridPos,
                    pos => MovementValidator.CanPlayerWalk(state, pos)
                );

                if (bestAdjacent.HasValue)
                {
                    // 寻路到相邻格子
                    path = Pathfinding.FindPath(
                        playerPos,
                        bestAdjacent.Value,
                        pos => MovementValidator.CanPlayerWalk(state, pos)
                    );

                    // 将目标物体本身加入路径末尾，以便到达后自动交互
                    if (path != null)
                    {
                        path.Add(gridPos);
                        Debug.Log($"Path to adjacent cell found, added target for interaction");
                    }
                }
            }

            // 开始自动移动
            if (path != null && path.Count > 0)
            {
                Debug.Log($"Starting auto-move with {path.Count} steps to {gridPos}");
                gameManager.StartAutoMove(path);
            }
            else
            {
                Debug.Log($"No path found to {gridPos}");
            }
        }

        /// <summary>
        /// 处理滑动手势
        /// 基于 sample/game.js:712-734
        /// </summary>
        private void HandleSwipe(Vector2 delta)
        {
            if (gameManager == null) return;

            GameState state = gameManager.GetGameState();
            if (state == null || state.gameOver || state.gameWon) return;

            // 停止自动移动
            // gameManager.StopAutoMove();

            // 判断滑动方向
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                // 水平滑动
                if (delta.x > MIN_SWIPE_DISTANCE)
                {
                    gameManager.HandlePlayerMove(Direction.Right);
                }
                else if (delta.x < -MIN_SWIPE_DISTANCE)
                {
                    gameManager.HandlePlayerMove(Direction.Left);
                }
            }
            else
            {
                // 垂直滑动
                if (delta.y > MIN_SWIPE_DISTANCE)
                {
                    gameManager.HandlePlayerMove(Direction.Up);
                }
                else if (delta.y < -MIN_SWIPE_DISTANCE)
                {
                    gameManager.HandlePlayerMove(Direction.Down);
                }
            }

            Debug.Log($"Swipe detected: {delta}");
        }
    }
}
