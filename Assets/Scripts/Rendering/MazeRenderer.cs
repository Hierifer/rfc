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

        private GameObject[,] tileObjects;
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

        public void Initialize(GameState gameState)
        {
            state = gameState;
            snakeObjects = new List<GameObject>();
            stoneObjects = new Dictionary<Vector2Int, GameObject>();
            boxObjects = new Dictionary<Vector2Int, GameObject>();

            // 加载精灵图集
            spriteAtlasLoader = new SpriteAtlasLoader();
            spriteAtlasLoader.LoadAtlases();

            CalculateTileSize();
            CreateTileGrid();
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

            // 计算网格需要的世界空间尺寸
            // 网格是 19 宽 × 13 高
            float gridAspect = (float)GridConstants.Width / GridConstants.Height;

            // 计算瓦片大小（世界单位）
            // 选择较小的值以确保网格完全显示在屏幕内
            float tileSizeByWidth = cameraWidth / GridConstants.Width;
            float tileSizeByHeight = cameraHeight / GridConstants.Height;
            worldTileSize = Mathf.Min(tileSizeByWidth, tileSizeByHeight);

            // 计算网格的总世界空间尺寸
            float gridWorldWidth = worldTileSize * GridConstants.Width;
            float gridWorldHeight = worldTileSize * GridConstants.Height;

            // 计算居中偏移（世界坐标）
            // 网格左下角的世界坐标
            gridOffset = new Vector2(
                -gridWorldWidth / 2f,  // X 居中
                -gridWorldHeight / 2f  // Y 居中
            );

            Debug.Log($"Camera size: {cameraWidth}x{cameraHeight}, Tile size: {worldTileSize}, Grid offset: {gridOffset}");
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

            tileObjects = new GameObject[GridConstants.Height, GridConstants.Width];

            Debug.Log($"Creating grid: {GridConstants.Width}x{GridConstants.Height}, worldTileSize={worldTileSize}");

            for (int row = 0; row < GridConstants.Height; row++)
            {
                for (int col = 0; col < GridConstants.Width; col++)
                {
                    Vector2 worldPos = GridToWorld(new Vector2Int(col, row));
                    GameObject tile = CreateTileObject(worldPos, CellType.Floor);
                    tile.name = $"Tile_{row}_{col}";
                    tile.transform.SetParent(transform);
                    tileObjects[row, col] = tile;
                }
            }

            Debug.Log($"Grid created successfully! First tile at: {tileObjects[0, 0].transform.position}");
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
        /// </summary>
        private void UpdateTiles()
        {
            for (int row = 0; row < GridConstants.Height; row++)
            {
                for (int col = 0; col < GridConstants.Width; col++)
                {
                    CellType cell = state.gridManager.GetCell(row, col);
                    GameObject tileObj = tileObjects[row, col];
                    SpriteRenderer sr = tileObj.GetComponent<SpriteRenderer>();

                    if (sr != null)
                    {
                        // 使用精灵图集中的精灵
                        if (spriteAtlasLoader != null && spriteAtlasLoader.IsLoaded())
                        {
                            Sprite sprite = spriteAtlasLoader.GetTileSprite(cell);
                            if (sprite != null)
                            {
                                sr.sprite = sprite;
                                sr.color = Color.white; // 使用精灵原色
                            }
                            else
                            {
                                // 降级为纯色（如果没有对应精灵）
                                sr.color = GetColorForCell(cell);
                            }
                        }
                        else
                        {
                            // 降级为纯色（如果图集未加载）
                            sr.color = GetColorForCell(cell);
                        }
                    }
                }
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

            playerObject.transform.position = new Vector3(worldPos.x, worldPos.y, -1);
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
                snakeObjects[i].transform.position = new Vector3(worldPos.x, worldPos.y, -1);
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
                        stoneObj.transform.position = new Vector3(worldPos.x, worldPos.y, -1);
                    }
                }
                else if (anim.type == CellType.Box)
                {
                    Vector2Int targetGridPos = Vector2Int.RoundToInt(anim.targetPos);
                    if (boxObjects.TryGetValue(targetGridPos, out GameObject boxObj))
                    {
                        boxObj.transform.position = new Vector3(worldPos.x, worldPos.y, -1);
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
                        obj.transform.position = new Vector3(worldPos.x, worldPos.y, -1);
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

            // Set sorting order - movable objects above floor tiles
            sr.sortingOrder = 1;

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

            // Set sorting order - entities above floor tiles
            sr.sortingOrder = 1;

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
            if (tileObjects != null)
            {
                foreach (var tile in tileObjects)
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
