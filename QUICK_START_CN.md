# 快速开始指南 - Unity 迷宫逃脱游戏

## 🎯 当前状态

✅ **核心系统已实现** (100%)
- 网格管理、游戏状态、寻路系统
- 玩家移动、石头推动、蛇 AI
- 精灵图集渲染系统
- 关卡加载、存档系统

✅ **精灵图集已集成**
- Tile 图集: 地板、墙壁、石头、物品等
- Entity 图集: 玩家、蛇等角色

## 🚀 30 秒快速启动

### 1. 配置精灵图（重要！）

在 Unity Project 窗口：
1. 找到 `Assets/Resources/Spirits/tile-atlas-small.png`
2. 在 Inspector 中设置：
   - Texture Type: **Sprite (2D and UI)**
   - Pixels Per Unit: **256**
   - Filter Mode: **Point (no filter)**
   - 点击 **Apply**

3. 对 `entity-atlas-small.png` 重复相同设置

### 2. 设置场景

1. 创建空 GameObject → 命名为 "GameManager"
2. 添加组件：
   - `GameManager.cs`
   - `InputManager.cs`
   - `MazeRenderer.cs`
3. 在 MazeRenderer Inspector 中：
   - 拖拽 Main Camera 到 "Main Camera" 字段
   - UI Width 保持 140

### 3. 创建测试关卡

菜单栏 → **Tools → Maze → Convert JS Levels**
- 点击 **"Create Test Level"**

### 4. 运行游戏

按 **Play** 键！

**操作方式**:
- **WASD** 或 **方向键**: 移动
- **R**: 重置关卡
- **N**: 下一关（测试用）

## 📖 详细文档

- **完整实现文档**: `UNITY_IMPLEMENTATION.md`
- **精灵图设置**: `SPRITE_SETUP_GUIDE.md`

## 🎮 游戏机制

- 推石头 → 滑动直到碰到障碍
- 收集炸药 → 炸开裂缝石头
- 踩云 → 整组云雾消失
- 避开蛇 → 被抓就输了
- 到达绿色出口 → 过关！

## 🐛 遇到问题？

1. 检查 Console 是否有错误
2. 确认精灵图在 `Assets/Resources/Spirits/`
3. 确认 Main Camera 已分配给 MazeRenderer
4. 查看 `SPRITE_SETUP_GUIDE.md` 排查精灵问题

祝游戏开发愉快！🎉
