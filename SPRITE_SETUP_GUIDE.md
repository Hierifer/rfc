# 精灵图设置指南 / Sprite Setup Guide

## ✅ 已完成的工作

1. **精灵图集加载器** (`SpriteAtlasLoader.cs`)
   - 自动从图集中切割单个精灵
   - 支持 tile 图集（1024x1024, 4x4 grid）
   - 支持 entity 图集（512x512, 2x2 grid）

2. **MazeRenderer 更新**
   - 使用精灵图集渲染瓦片
   - 使用精灵图集渲染实体（玩家、蛇）
   - 自动降级到纯色（如果精灵未加载）

3. **文件位置**
   - 精灵图已复制到 `Assets/Resources/Spirits/`
   - 运行时通过 `Resources.Load<Texture2D>()` 加载

## 🔧 Unity 编辑器设置步骤

### 步骤 1: 配置精灵图导入设置

1. 在 Unity Project 窗口中找到这两个文件：
   - `Assets/Resources/Spirits/tile-atlas-small.png`
   - `Assets/Resources/Spirits/entity-atlas-small.png`

2. **点击 `tile-atlas-small.png`**，在 Inspector 中设置：
   ```
   Texture Type: Sprite (2D and UI)
   Sprite Mode: Single
   Pixels Per Unit: 256 （重要！）
   Filter Mode: Point (no filter) （保持像素风格）
   Compression: None
   ```
   点击 **Apply**

3. **点击 `entity-atlas-small.png`**，在 Inspector 中设置：
   ```
   Texture Type: Sprite (2D and UI)
   Sprite Mode: Single
   Pixels Per Unit: 256 （重要！）
   Filter Mode: Point (no filter)
   Compression: None
   ```
   点击 **Apply**

### 步骤 2: 验证设置

在 Inspector 中确认：
- ✅ Read/Write Enabled: **勾选**（让代码可以读取像素）
- ✅ Generate Mip Maps: **不勾选**
- ✅ Max Size: **2048** 或更高

### 步骤 3: 测试运行

1. 打开场景（如果还没有场景，先设置 GameManager）
2. 按 **Play** 键
3. 检查 Console 是否有以下日志：
   ```
   Loaded tile atlas: 1024x1024
   Loaded entity atlas: 512x512
   ```

4. 如果看到错误信息，检查：
   - 文件是否在 `Assets/Resources/Spirits/` 文件夹中
   - 文件名是否正确（`tile-atlas-small.png` 和 `entity-atlas-small.png`）

## 📋 精灵图集映射表

### Tile 图集 (4x4 网格, 每格 256x256)

| 位置 (col, row) | 精灵名称 | 游戏中用途 |
|----------------|---------|----------|
| (1, 0) | floor | 地板 |
| (2, 0) | wall | 墙壁 |
| (3, 0) | exit | 出口 |
| (0, 1) | stone | 可推石头 |
| (1, 1) | fixedStone | 固定石头 |
| (2, 1) | dynamite | 炸药 |
| (3, 1) | crackedStone | 裂缝石头 |
| (0, 2) | box | 箱子 |
| (2, 2) | cloud | 云 |
| (3, 2) | fog | 雾 |

### Entity 图集 (2x? 网格, 每格 256x256)

| 位置 (col, row) | 精灵名称 | 游戏中用途 |
|----------------|---------|----------|
| (0, 0) | player | 玩家 |
| (0, 2) | snake | 蛇 |

## 🐛 常见问题

### 问题 1: Console 显示 "Failed to load tile-atlas-small.png"

**原因**: 文件不在 Resources 文件夹中

**解决方案**:
```bash
# 确认文件存在
ls Assets/Resources/Spirits/

# 应该看到:
# entity-atlas-small.png
# tile-atlas-small.png
```

### 问题 2: 游戏运行但看不到精灵

**原因**: 可能是图集切割配置错误

**解决方案**:
1. 检查 SpriteAtlasLoader.cs 中的常量：
   ```csharp
   TILE_ATLAS_SIZE = 1024
   TILE_GRID_SIZE = 256
   ```
2. 确认图片实际尺寸匹配（在 Inspector 中查看）

### 问题 3: 精灵显示模糊

**原因**: Filter Mode 设置错误

**解决方案**:
1. 选中精灵图
2. 设置 Filter Mode = **Point (no filter)**
3. 点击 Apply

### 问题 4: 精灵上下颠倒

**原因**: Unity 纹理坐标从底部开始，而图集可能从顶部开始

**解决方案**: 已在 `SpriteAtlasLoader.cs` 中处理：
```csharp
// Y 坐标翻转
int y = tileAtlas.height - (row + 1) * TILE_GRID_SIZE;
```

## 🎨 自定义精灵图

如果你想使用自己的精灵图：

1. **保持图集尺寸**:
   - Tile 图集: 1024x1024 (4x4 grid)
   - Entity 图集: 512x512 或 1024x1024

2. **保持精灵位置映射**: 参考上面的映射表

3. **替换文件**:
   ```bash
   # 替换为你的新图集
   cp your-tile-atlas.png Assets/Resources/Spirits/tile-atlas-small.png
   cp your-entity-atlas.png Assets/Resources/Spirits/entity-atlas-small.png
   ```

4. **重新导入**: 在 Unity 中右键 → Reimport

## 📝 代码结构说明

### SpriteAtlasLoader.cs

- **LoadAtlases()**: 从 Resources 加载图集纹理
- **SliceAllSprites()**: 根据预定义映射切割精灵
- **GetTileSprite(CellType)**: 获取瓦片精灵
- **GetEntitySprite(string)**: 获取实体精灵

### MazeRenderer.cs (已更新部分)

- **Initialize()**: 创建 SpriteAtlasLoader 实例并加载
- **UpdateTiles()**: 使用精灵图集渲染瓦片（自动降级到纯色）
- **CreateEntityObject()**: 创建实体对象，优先使用精灵
- **CreateTileEntityObject()**: 为石头/箱子创建使用 tile 精灵的对象

## ✅ 验证清单

运行游戏前确认：

- [ ] 精灵图在 `Assets/Resources/Spirits/` 文件夹中
- [ ] 精灵图 Import Settings 正确（Sprite 2D, PPU=256, Point filter）
- [ ] Console 显示 "Loaded tile atlas" 和 "Loaded entity atlas"
- [ ] 游戏场景中能看到瓦片和实体
- [ ] 没有 "Failed to load" 错误信息

## 🎮 效果预览

正确设置后，你应该看到：

1. **地板和墙壁**: 使用精灵图集中的纹理，而非纯色
2. **玩家**: 显示为精灵图集中的玩家图标
3. **蛇**: 显示为精灵图集中的蛇图标
4. **石头/箱子**: 使用 tile 图集中的对应精灵
5. **其他物品**: 炸药、裂缝石头、云雾等都使用精灵

如果精灵未加载成功，系统会自动降级到纯色显示，游戏仍可正常运行。

---

**祝你游戏开发愉快！🎮**
