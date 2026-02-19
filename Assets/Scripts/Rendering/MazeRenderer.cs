using UnityEngine;
using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// Main renderer for the maze game (from sample/utils/maze-renderer.js)
    /// Handles grid tiles and entities
    /// </summary>
    public class MazeRenderer : MonoBehaviour
    {
        [Header("Rendering Settings")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float uiWidth = 140f; // Left sidebar width in pixels

        private GameObject[,] floorTiles;      // 背景层：所有格子的 floor
        private GameObject[,] overlayTiles;   // 叠加层：静态物体（wall, exit, dynamite 等）
        private GameObject playerObject;
        private List<GameObject> snakeObjects;
        private Dictionary<Vector2Int, GameObject> stoneObjects;
        private Dictionary<Vector2Int, GameObject> boxObjects;

        private GameState state;
        private float tileSize; // World space tile size
        private Vector2 gridOffset; // World space offset to center the grid
        private float worldTileSize; // Actual world unit size per tile

        // 精灵图集加载器
        private SpriteAtlasLoader spriteAtlasLoader;

        // 屏幕尺寸检测
        private int lastScreenWidth;
        private int lastScreenHeight;
        private ScreenOrientation lastOrientation;

        public void Initialize(GameState gameState)
        {
            state = gameState;
            snakeObjects = new List<GameObject>();
            stoneObjects = new Dictionary<Vector2Int, GameObject>();
            boxObjects = new Dictionary<Vector2Int, GameObject>();

            // 强制横屏模式
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;

            // 加载精灵图集
            spriteAtlasLoader = new SpriteAtlasLoader();
            spriteAtlasLoader.LoadAtlases();

            CalculateTileSize();
            CreateTileGrid();

            // 记录初始屏幕尺寸
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            lastOrientation = Screen.orientation;
        }

        /// <summary>
        /// 检测屏幕方向变化
        /// </summary>
        private void Update()
        {
            // 检测屏幕尺寸或方向变化
            if (Screen.width != lastScreenWidth ||
                Screen.height != lastScreenHeight ||
                Screen.orientation != lastOrientation)
            {
                Debug.Log($"Screen changed: {lastScreenWidth}x{lastScreenHeight} -> {Screen.width}x{Screen.height}, Orientation: {lastOrientation} -> {Screen.orientation}");

                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                lastOrientation = Screen.orientation;

                // 重新计算和渲染
                OnScreenChanged();
            }
        }

        /// <summary>
        /// 屏幕变化时重新渲染
        /// </summary>
        private void OnScreenChanged()
        {
            // 重新计算瓦片大小
            CalculateTileSize();

            // 清除旧的网格
            if (floorTiles != null)
            {
                foreach (var tile in floorTiles)
                {
                    if (tile != null) Destroy(tile);
                }
            }

            if (overlayTiles != null)
            {
                foreach (var tile in overlayTiles)
                {
                    if (tile != null) Destroy(tile);
                }
            }

            // 重新创建网格
            CreateTileGrid();

            // 强制重新渲染所有内容
            Render();
        }

        /// <summary>
        /// Calculate tile size and grid offset based on camera orthographic size
        /// 使用世界坐标系统，支持横屏和正确居中
        /// </summary>
        private void CalculateTileSize()
        {
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera not assigned to MazeRenderer!");
                return;
            }

            // 获取相机的世界空间尺寸
            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * mainCamera.aspect;

            // 将 UI 宽度从像素转换为世界单位
            float uiWorldWidth = (uiWidth / Screen.width) * cameraWidth;

            // 可用的游戏区域宽度（减去 UI 侧边栏）
            float availableWidth = cameraWidth - uiWorldWidth;
            float availableHeight = cameraHeight;

            // 计算瓦片大小（世界单位）
            // 横屏时优先填充高度，让棋盘尽可能大
            float tileSizeByWidth = availableWidth / GridConstants.Width;
            float tileSizeByHeight = availableHeight / GridConstants.Height;
            worldTileSize = Mathf.Min(tileSizeByWidth, tileSizeByHeight);

            // 计算网格的总世界空间尺寸
            float gridWorldWidth = worldTileSize * GridConstants.Width;
            float gridWorldHeight = worldTileSize * GridConstants.Height;

            // 计算居中偏移（世界坐标）
            // 考虑 UI 侧边栏，将棋盘向右偏移
            float horizontalOffset = (availableWidth - gridWorldWidth) / 2f + uiWorldWidth;

            gridOffset = new Vector2(
                -cameraWidth / 2f + horizontalOffset,  // X 考虑 UI 宽度后居中
                -gridWorldHeight / 2f  // Y 居中
            );

            Debug.Log($"Camera size: {cameraWidth}x{cameraHeight}, Available: {availableWidth}x{availableHeight}, Tile size: {worldTileSize}, Grid offset: {gridOffset}");
        }

        /// <summary>
        /// Create the tile grid background
        /// </summary>
        private void CreateTileGrid()
        {
            if (worldTileSize <= 0)
            {
                Debug.LogError("worldTileSize is not initialized! Make sure CalculateTileSize() was called first.");
                return;
            }

            floorTiles = new GameObject[GridConstants.Height, GridConstants.Width];
            overlayTiles = new GameObject[GridConstants.Height, GridConstants.Width];

            Debug.Log($"Creating grid: {GridConstants.Width}x{GridConstants.Height}, worldTileSize={worldTileSize}");

            // 创建 floor 背景层（所有格子都是 floor，永不改变）
            for (int row = 0; row < GridConstants.Height; row++)
            {
                for (int col = 0; col < GridConstants.Width; col++)
                {
                    Vector2 worldPos = GridToWorld(new Vector2Int(col, row));
                    GameObject tile = CreateFloorTile(worldPos);
                    tile.name = $"Floor_{row}_{col}";
                    tile.transform.SetParent(transform);
                    floorTiles[row, col] = tile;
                }
            }

            Debug.Log($"Grid created successfully! First tile at: {floorTiles[0, 0].transform.position}");
        }

        /// <summary>
        /// Render the current game state
        /// </summary>
        public void Render()
        {
            if (state == null) return;

            // Update tile appearances
            UpdateTiles();

            // Update entities
            UpdateEntities();
        }

        /// <summary>
        /// Update all tile visuals based on grid state
        /// Floor 保持不变，只更新 overlay 层
        /// </summary>
        private void UpdateTiles()
        {
            for (int row = 0; row < GridConstants.Height; row++)
            {
                for (int col = 0; col < GridConstants.Width; col++)
                {
                    CellType cell = state.gridManager.GetCell(row, col);

                    // 判断是否需要 overlay tile（静态物体，非动态物体）
                    bool needsOverlay = cell == CellType.Wall
                        || cell == CellType.Exit
                        || cell == CellType.Dynamite
                        || cell == CellType.CrackedStone
                        || cell == CellType.FixedStone
                        || cell == CellType.Cloud
                        || cell == CellType.Fog;

                    if (needsOverlay)
                    {
                        // 创建或更新 overlay tile
                        if (overlayTiles[row, col] == null)
                        {
                            Vector2 worldPos = GridToWorld(new Vector2Int(col, row));
                            GameObject overlayTile = CreateOverlayTile(worldPos, cell);
                            overlayTile.name = $"Overlay_{row}_{col}_{cell}";
                            overlayTile.transform.SetParent(transform);
                            overlayTiles[row, col] = overlayTile;
                        }
                        else
                        {
                            // 更新现有 overlay tile
                            UpdateOverlayTileSprite(overlayTiles[row, col], cell);
                        }
                    }
                    else
                    {
                        // 移除 overlay tile（显示底层 floor）
                        if (overlayTiles[row, col] != null)
                        {
                            Destroy(overlayTiles[row, col]);
                            overlayTiles[row, col] = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 更新 overlay tile 的精灵
        /// </summary>
        private void UpdateOverlayTileSprite(GameObject overlayTile, CellType cellType)
        {
            SpriteRenderer sr = overlayTile.GetComponent<SpriteRenderer>();
            if (sr == null) return;

            if (spriteAtlasLoader != null && spriteAtlasLoader.IsLoaded())
            {
                Sprite sprite = spriteAtlasLoader.GetTileSprite(cellType);
                if (sprite != null)
                {
                    sr.sprite = sprite;
                    sr.color = Color.white;
                }
                else
                {
                    sr.color = GetColorForCell(cellType);
                }
            }
            else
            {
                sr.color = GetColorForCell(cellType);
            }
        }

        /// <summary>
        /// Update entity game objects
        /// </summary>
        private void UpdateEntities()
        {
            // Update player
            UpdatePlayer();

            // Update snakes
            UpdateSnakes();

            // Update movable objects (with animations)
            UpdateMovableObjects();
        }

        /// <summary>
        /// Update player position
        /// </summary>
        private void UpdatePlayer()
        {
            Vector2 worldPos = GridToWorld(state.playerPos);

            if (playerObject == null)
            {
                playerObject = CreateEntityObject(worldPos, TileColors.Player, "player");
                playerObject.name = "Player";
            }

            playerObject.transform.position = new Vector3(worldPos.x, worldPos.y, 0);
        }

        /// <summary>
        /// Update snake positions
        /// </summary>
        private void UpdateSnakes()
        {
            // Remove excess snake objects
            while (snakeObjects.Count > state.snakes.Count)
            {
                GameObject toRemove = snakeObjects[snakeObjects.Count - 1];
                snakeObjects.RemoveAt(snakeObjects.Count - 1);
                Destroy(toRemove);
            }

            // Add missing snake objects
            while (snakeObjects.Count < state.snakes.Count)
            {
                GameObject snake = CreateEntityObject(Vector2.zero, TileColors.Snake, "snake");
                snake.name = $"Snake_{snakeObjects.Count}";
                snakeObjects.Add(snake);
            }

            // Update positions
            for (int i = 0; i < state.snakes.Count; i++)
            {
                Vector2 worldPos = GridToWorld(state.snakes[i].position);
                snakeObjects[i].transform.position = new Vector3(worldPos.x, worldPos.y, 0);
            }
        }

        /// <summary>
        /// Update movable objects (stones, boxes) with animation support
        /// </summary>
        private void UpdateMovableObjects()
        {
            // Update stones (使用 tile 精灵而非 entity 精灵)
            UpdateMovableSet(state.pushableStones, stoneObjects, CellType.Stone, "Stone");

            // Update boxes
            UpdateMovableSet(state.boxes, boxObjects, CellType.Box, "Box");

            // Handle animations
            foreach (var anim in state.animations)
            {
                Vector2 currentPos = anim.GetCurrentPosition();
                Vector2 worldPos = GridToWorld(currentPos);

                // Find the object being animated
                if (anim.type == CellType.Stone)
                {
                    Vector2Int targetGridPos = Vector2Int.RoundToInt(anim.targetPos);
                    if (stoneObjects.TryGetValue(targetGridPos, out GameObject stoneObj))
                    {
                        stoneObj.transform.position = new Vector3(worldPos.x, worldPos.y, 0);
                    }
                }
                else if (anim.type == CellType.Box)
                {
                    Vector2Int targetGridPos = Vector2Int.RoundToInt(anim.targetPos);
                    if (boxObjects.TryGetValue(targetGridPos, out GameObject boxObj))
                    {
                        boxObj.transform.position = new Vector3(worldPos.x, worldPos.y, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Update a set of movable objects (generic)
        /// </summary>
        private void UpdateMovableSet(List<Vector2Int> positions, Dictionary<Vector2Int, GameObject> objects, CellType cellType, string namePrefix)
        {
            // Remove objects that no longer exist
            List<Vector2Int> toRemove = new List<Vector2Int>();
            foreach (var kvp in objects)
            {
                if (!positions.Contains(kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                    Destroy(kvp.Value);
                }
            }
            foreach (var key in toRemove)
            {
                objects.Remove(key);
            }

            // Add new objects
            foreach (var pos in positions)
            {
                if (!objects.ContainsKey(pos))
                {
                    Vector2 worldPos = GridToWorld(pos);
                    GameObject obj = CreateTileEntityObject(worldPos, cellType);
                    obj.name = $"{namePrefix}_{pos.x}_{pos.y}";
                    objects[pos] = obj;
                }
            }

            // Update positions (for non-animated objects)
            foreach (var pos in positions)
            {
                if (objects.TryGetValue(pos, out GameObject obj))
                {
                    // Only update if not currently animating
                    bool isAnimating = state.animations.Exists(anim =>
                        Vector2Int.RoundToInt(anim.targetPos) == pos);

                    if (!isAnimating)
                    {
                        Vector2 worldPos = GridToWorld(pos);
                        obj.transform.position = new Vector3(worldPos.x, worldPos.y, 0);
                    }
                }
            }
        }

        /// <summary>
        /// Convert grid coordinates to world coordinates
        /// 网格坐标转世界坐标（直接计算，不经过屏幕坐标）
        /// </summary>
        public Vector2 GridToWorld(Vector2Int gridPos)
        {
            return GridToWorld(new Vector2(gridPos.x, gridPos.y));
        }

        public Vector2 GridToWorld(Vector2 gridPos)
        {
            // 直接在世界空间计算
            // gridPos.x 是列（col），gridPos.y 是行（row）
            // 翻转 Y 轴：row 0 在顶部，row 增加向下
            float worldX = gridOffset.x + (gridPos.x + 0.5f) * worldTileSize;
            float worldY = gridOffset.y + (GridConstants.Height - gridPos.y - 0.5f) * worldTileSize;

            return new Vector2(worldX, worldY);
        }

        /// <summary>
        /// Convert screen coordinates to grid coordinates
        /// 屏幕坐标转网格坐标
        /// </summary>
        public Vector2Int ScreenToGrid(Vector2 screenPos)
        {
            // 将屏幕坐标转换为世界坐标
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10));

            // 从世界坐标计算网格位置
            float gridX = (worldPos.x - gridOffset.x) / worldTileSize;
            // 翻转 Y 轴：从底部到顶部转换为从顶部到底部
            float gridY = GridConstants.Height - (worldPos.y - gridOffset.y) / worldTileSize;

            return new Vector2Int(Mathf.FloorToInt(gridX), Mathf.FloorToInt(gridY));
        }

        /// <summary>
        /// 创建 floor tile（背景层，sortingOrder = 0）
        /// </summary>
        private GameObject CreateFloorTile(Vector2 worldPos)
        {
            GameObject tile = new GameObject();
            SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();

            // 使用精灵图集
            if (spriteAtlasLoader != null && spriteAtlasLoader.IsLoaded())
            {
                Sprite sprite = spriteAtlasLoader.GetTileSprite(CellType.Floor);
                if (sprite != null)
                {
                    sr.sprite = sprite;
                    sr.color = Color.white;
                }
                else
                {
                    // 降级为纯色
                    Texture2D tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, Color.white);
                    tex.Apply();
                    sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
                    sr.color = TileColors.Floor;
                }
            }
            else
            {
                // 降级为纯色
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
                sr.color = TileColors.Floor;
            }

            // Floor 在最底层
            sr.sortingOrder = 0;

            tile.transform.position = new Vector3(worldPos.x, worldPos.y, 0);
            tile.transform.localScale = Vector3.one * this.worldTileSize;

            return tile;
        }

        /// <summary>
        /// 创建 overlay tile（叠加层，sortingOrder = 1）
        /// </summary>
        private GameObject CreateOverlayTile(Vector2 worldPos, CellType type)
        {
            GameObject tile = new GameObject();
            SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();

            // 使用精灵图集
            if (spriteAtlasLoader != null && spriteAtlasLoader.IsLoaded())
            {
                Sprite sprite = spriteAtlasLoader.GetTileSprite(type);
                if (sprite != null)
                {
                    sr.sprite = sprite;
                    sr.color = Color.white;
                }
                else
                {
                    // 降级为纯色
                    Texture2D tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, Color.white);
                    tex.Apply();
                    sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
                    sr.color = GetColorForCell(type);
                }
            }
            else
            {
                // 降级为纯色
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
                sr.color = GetColorForCell(type);
            }

            // Overlay 在 floor 之上，但在动态物体之下
            sr.sortingOrder = 1;

            tile.transform.position = new Vector3(worldPos.x, worldPos.y, 0);
            tile.transform.localScale = Vector3.one * this.worldTileSize;

            return tile;
        }

        /// <summary>
        /// Create a tile GameObject
        /// </summary>
        private GameObject CreateTileObject(Vector2 worldPos, CellType type)
        {
            GameObject tile = new GameObject();
            SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();

            // Create a simple square sprite
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();

            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
            sr.color = GetColorForCell(type);

            // Set sorting order - floor tiles at bottom layer
            sr.sortingOrder = 0;

            // 统一使用 z = 0，通过 sortingOrder 控制层级
            tile.transform.position = new Vector3(worldPos.x, worldPos.y, 0);

            // 使用计算好的世界空间瓦片大小
            tile.transform.localScale = Vector3.one * this.worldTileSize;

            return tile;
        }

        /// <summary>
        /// Create a tile-based entity (for stones and boxes that use tile sprites)
        /// </summary>
        private GameObject CreateTileEntityObject(Vector2 worldPos, CellType cellType)
        {
            GameObject entity = new GameObject();
            SpriteRenderer sr = entity.AddComponent<SpriteRenderer>();

            // 使用 tile 精灵图集
            bool usedSprite = false;
            if (spriteAtlasLoader != null && spriteAtlasLoader.IsLoaded())
            {
                Sprite sprite = spriteAtlasLoader.GetTileSprite(cellType);
                if (sprite != null)
                {
                    sr.sprite = sprite;
                    sr.color = Color.white;
                    usedSprite = true;
                }
            }

            // 降级为纯色
            if (!usedSprite)
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
                sr.color = GetColorForCell(cellType);
            }

            // Set sorting order - movable objects above overlay tiles
            sr.sortingOrder = 2;

            // 统一使用 z = 0，通过 sortingOrder 控制层级
            entity.transform.position = new Vector3(worldPos.x, worldPos.y, 0);

            // 使用计算好的世界空间瓦片大小
            entity.transform.localScale = Vector3.one * this.worldTileSize;

            return entity;
        }

        /// <summary>
        /// Create an entity GameObject (player, snake, stone, box)
        /// </summary>
        private GameObject CreateEntityObject(Vector2 worldPos, Color color, string entityType = null)
        {
            GameObject entity = new GameObject();
            SpriteRenderer sr = entity.AddComponent<SpriteRenderer>();

            // 尝试使用精灵图集中的精灵
            bool usedSprite = false;
            if (spriteAtlasLoader != null && spriteAtlasLoader.IsLoaded() && !string.IsNullOrEmpty(entityType))
            {
                Sprite sprite = spriteAtlasLoader.GetEntitySprite(entityType);
                if (sprite != null)
                {
                    sr.sprite = sprite;
                    sr.color = Color.white; // 使用精灵原色
                    usedSprite = true;
                }
            }

            // 如果没有精灵，降级为程序生成的圆形
            if (!usedSprite)
            {
                Texture2D tex = CreateCircleTexture(64, color);
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), Vector2.one * 0.5f, 64f);
            }

            // Set sorting order - entities above overlay tiles
            sr.sortingOrder = 2;

            entity.transform.position = new Vector3(worldPos.x, worldPos.y, 0);

            // 实体稍微小一点（80%），看起来不会太拥挤
            entity.transform.localScale = Vector3.one * this.worldTileSize * 0.8f;

            return entity;
        }

        /// <summary>
        /// Create a circle texture
        /// </summary>
        private Texture2D CreateCircleTexture(int size, Color color)
        {
            Texture2D tex = new Texture2D(size, size);
            Vector2 center = Vector2.one * size / 2f;
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist < radius)
                    {
                        tex.SetPixel(x, y, color);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Get color for a cell type
        /// </summary>
        private Color GetColorForCell(CellType type)
        {
            return type switch
            {
                CellType.Floor => TileColors.Floor,
                CellType.Wall => TileColors.Wall,
                CellType.Exit => TileColors.Exit,
                CellType.Dynamite => TileColors.Dynamite,
                CellType.CrackedStone => TileColors.CrackedStone,
                CellType.Cloud => TileColors.Cloud,
                CellType.Fog => TileColors.Fog,
                CellType.FixedStone => TileColors.FixedStone,
                _ => TileColors.Floor
            };
        }

        /// <summary>
        /// Clean up all rendered objects
        /// </summary>
        public void Clear()
        {
            // 清理 floor tiles
            if (floorTiles != null)
            {
                foreach (var tile in floorTiles)
                {
                    if (tile != null) Destroy(tile);
                }
            }

            // 清理 overlay tiles
            if (overlayTiles != null)
            {
                foreach (var tile in overlayTiles)
                {
                    if (tile != null) Destroy(tile);
                }
            }

            if (playerObject != null) Destroy(playerObject);

            foreach (var snake in snakeObjects)
            {
                if (snake != null) Destroy(snake);
            }

            foreach (var stone in stoneObjects.Values)
            {
                if (stone != null) Destroy(stone);
            }

            foreach (var box in boxObjects.Values)
            {
                if (box != null) Destroy(box);
            }

            snakeObjects.Clear();
            stoneObjects.Clear();
            boxObjects.Clear();
        }
    }
}
