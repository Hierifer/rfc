using UnityEngine;
using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// 精灵图集加载器 - 从图集中切割出单个精灵
    /// 基于 sample/utils/maze-renderer.js 的 ATLAS_CONFIG
    /// </summary>
    public class SpriteAtlasLoader
    {
        // 图集配置（对应 JS 版本）
        private const int TILE_ATLAS_SIZE = 1024;
        private const int TILE_GRID_SIZE = 256; // 1024 / 4 = 256
        private const int TILE_COLS = 4;

        private const int ENTITY_ATLAS_SIZE = 512;
        private const int ENTITY_GRID_SIZE = 126; // 512 / 2 = 256
        private const int ENTITY_COLS = 4;

        // 缓存的精灵字典
        private Dictionary<string, Sprite> tileSprites;
        private Dictionary<string, Sprite> entitySprites;

        // 图集纹理
        private Texture2D tileAtlas;
        private Texture2D entityAtlas;

        public SpriteAtlasLoader()
        {
            tileSprites = new Dictionary<string, Sprite>();
            entitySprites = new Dictionary<string, Sprite>();
        }

        /// <summary>
        /// 加载图集纹理
        /// </summary>
        public void LoadAtlases()
        {
            // 从 Assets/Spirits/ 加载图集
            tileAtlas = Resources.Load<Texture2D>("Spirits/tile-atlas-small");
            entityAtlas = Resources.Load<Texture2D>("Spirits/entity-atlas-small");

            if (tileAtlas == null)
            {
                Debug.LogError("Failed to load tile-atlas-small.png! Make sure it's in Resources/Spirits/");
            }
            else
            {
                Debug.Log($"Loaded tile atlas: {tileAtlas.width}x{tileAtlas.height}");
            }

            if (entityAtlas == null)
            {
                Debug.LogError("Failed to load entity-atlas-small.png! Make sure it's in Resources/Spirits/");
            }
            else
            {
                Debug.Log($"Loaded entity atlas: {entityAtlas.width}x{entityAtlas.height}");
            }

            // 预切割所有精灵
            SliceAllSprites();
        }

        /// <summary>
        /// 切割所有精灵（基于 JS 版本的映射）
        /// </summary>
        private void SliceAllSprites()
        {
            if (tileAtlas != null)
            {
                // Tile 图集映射（对应 JS ATLAS_CONFIG.tile.mapping）
                SliceTileSprite("floor", 1, 0);
                SliceTileSprite("wall", 2, 0);
                SliceTileSprite("exit", 3, 0);
                SliceTileSprite("stone", 0, 1);
                SliceTileSprite("fixedStone", 1, 1);
                SliceTileSprite("dynamite", 2, 1);
                SliceTileSprite("crackedStone", 3, 1);
                SliceTileSprite("box", 0, 2);
                SliceTileSprite("cloud", 2, 2);
                SliceTileSprite("fog", 3, 2);
            }

            if (entityAtlas != null)
            {
                // Entity 图集映射（对应 JS ATLAS_CONFIG.entity.mapping）
                SliceEntitySprite("player", 0, 0);
                SliceEntitySprite("snake", 0, 2);
            }
        }

        /// <summary>
        /// 从 tile 图集切割单个精灵
        /// </summary>
        private void SliceTileSprite(string name, int col, int row)
        {
            if (tileAtlas == null) return;

            // 计算源矩形（从图集中的位置）
            int x = col * TILE_GRID_SIZE;
            int y = tileAtlas.height - (row + 1) * TILE_GRID_SIZE; // Unity 纹理坐标从底部开始

            Rect rect = new Rect(x, y, TILE_GRID_SIZE, TILE_GRID_SIZE);
            Sprite sprite = Sprite.Create(
                tileAtlas,
                rect,
                new Vector2(0.5f, 0.5f), // 中心点
                TILE_GRID_SIZE // pixels per unit
            );

            sprite.name = name;
            tileSprites[name] = sprite;
        }

        /// <summary>
        /// 从 entity 图集切割单个精灵
        /// </summary>
        private void SliceEntitySprite(string name, int col, int row)
        {
            if (entityAtlas == null) return;

            // 计算源矩形
            int x = col * ENTITY_GRID_SIZE;
            int y = entityAtlas.height - (row + 1) * ENTITY_GRID_SIZE;

            Rect rect = new Rect(x, y, ENTITY_GRID_SIZE, ENTITY_GRID_SIZE);
            Sprite sprite = Sprite.Create(
                entityAtlas,
                rect,
                new Vector2(0.5f, 0.5f),
                ENTITY_GRID_SIZE
            );

            sprite.name = name;
            entitySprites[name] = sprite;
        }

        /// <summary>
        /// 获取瓦片精灵
        /// </summary>
        public Sprite GetTileSprite(CellType cellType)
        {
            string key = cellType switch
            {
                CellType.Floor => "floor",
                CellType.Wall => "wall",
                CellType.Exit => "exit",
                CellType.Stone => "stone",
                CellType.FixedStone => "fixedStone",
                CellType.Dynamite => "dynamite",
                CellType.CrackedStone => "crackedStone",
                CellType.Box => "box",
                CellType.Cloud => "cloud",
                CellType.Fog => "fog",
                _ => "floor"
            };

            return tileSprites.TryGetValue(key, out Sprite sprite) ? sprite : null;
        }

        /// <summary>
        /// 获取实体精灵
        /// </summary>
        public Sprite GetEntitySprite(string entityType)
        {
            return entitySprites.TryGetValue(entityType, out Sprite sprite) ? sprite : null;
        }

        /// <summary>
        /// 检查是否成功加载
        /// </summary>
        public bool IsLoaded()
        {
            return tileAtlas != null && entityAtlas != null;
        }
    }
}
