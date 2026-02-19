# 渲染问题排查指南

## ❌ 问题：网格没有被渲染出来

如果你看到空白屏幕或黑屏，按照以下步骤排查：

---

## 🔍 快速诊断（1分钟）

### 步骤 1: 运行自动诊断

Unity 菜单栏 → **Tools → Maze → Diagnose Rendering**

点击 **"运行诊断"** 按钮

系统会自动检查并告诉你问题所在！

### 步骤 2: 查看诊断报告

报告会显示所有问题，例如：
- ❌ Main Camera 未分配
- ❌ 相机不是正交投影
- ❌ 瓦片对象未创建

按照报告中的提示逐一修复。

---

## 🛠️ 常见问题及解决方案

### 问题 1: Main Camera 未分配

**症状**: 控制台显示 "Main Camera not assigned"

**解决方案**:
1. 选中 GameManager GameObject
2. 在 Inspector 找到 **Maze Renderer** 组件
3. 将 **Main Camera** 拖拽到 "Main Camera" 字段
4. 或使用工具: **Tools → Maze → Setup Game Scene**

### 问题 2: 相机设置错误

**症状**: 看到部分网格或网格太大/太小

**解决方案**:
1. **Tools → Maze → Setup Camera**
2. 点击 "配置为横屏 2D 相机"

手动设置：
```
Main Camera Inspector:
├─ Projection: Orthographic ⭐
├─ Size: 8
└─ Position: (0, 0, -10)
```

### 问题 3: 游戏未启动

**症状**: 诊断报告说 "游戏未运行"

**解决方案**:
1. 按 **Play** 键启动游戏
2. 检查 **Game 窗口**（不是 Scene 窗口！）
3. 查看 Console 中的日志

### 问题 4: 关卡数据缺失

**症状**: Console 显示 "Failed to load level"

**解决方案**:
1. **Tools → Maze → Convert JS Levels**
2. 点击 **"Create Test Level"**
3. 确认 `Assets/Resources/Levels/` 中有关卡文件

### 问题 5: worldTileSize 未初始化

**症状**: Console 显示 "worldTileSize is not initialized"

**解决方案**:
1. 确认 Main Camera 已正确分配
2. 确认相机是正交投影
3. 重启 Unity 编辑器
4. 删除 GameManager 重新创建

### 问题 6: 看不到任何东西（黑屏）

**可能原因**:

#### A. 在错误的窗口查看
- ❌ Scene 窗口 - 这是编辑视图
- ✅ **Game 窗口** - 这是游戏视图

解决：点击 Unity 顶部的 **Game** 标签页

#### B. 相机背景是黑色
检查 Main Camera:
```
Background: #111827 (深灰色，不是纯黑)
```

#### C. 瓦片在相机视野外
检查相机位置：
```
Position: (0, 0, -10)  ← Z 必须是负数！
```

#### D. 瓦片太小看不见
增加 Orthographic Size:
```
Size: 7-10  (推荐 8)
```

---

## 📊 检查清单

运行游戏前确认：

- [ ] **GameManager 存在** - Hierarchy 中有 GameManager GameObject
- [ ] **组件完整** - GameManager、InputManager、MazeRenderer 都已添加
- [ ] **相机已分配** - MazeRenderer 的 Main Camera 字段不为空
- [ ] **相机设置正确** - Orthographic, Size=8, Position=(0,0,-10)
- [ ] **关卡已创建** - Resources/Levels/ 中有 Level_Test.asset
- [ ] **精灵图已加载** - Resources/Spirits/ 中有图集文件
- [ ] **在 Game 窗口查看** - 不是 Scene 窗口

全部勾选 = 应该能看到网格！

---

## 🔬 高级诊断

### 查看 Console 日志

运行游戏时，Console 应该显示：

```
✅ 正常日志:
Game systems initialized
Loaded tile atlas: 1024x1024
Loaded entity atlas: 512x512
Camera size: 14.22x8, Tile size: 0.585, Grid offset: (-5.55, -3.80)
Creating grid: 19x13, worldTileSize=0.585
Grid created successfully! First tile at: (...)
Loaded level 1/20

❌ 错误日志:
Main Camera not assigned to MazeRenderer!
worldTileSize is not initialized!
Failed to load level data!
```

根据日志信息对症下药。

### 检查 Hierarchy 层级

运行游戏后，Hierarchy 应该有：

```
Hierarchy (Play Mode)
├─ Main Camera
└─ GameManager
    ├─ Tile_0_0
    ├─ Tile_0_1
    ├─ ... (247 个瓦片)
    ├─ Player
    └─ Snake_0 (如果有蛇)
```

如果看不到 Tile_ 开头的对象，说明网格创建失败。

### 检查 Inspector

运行时选中任意 `Tile_X_Y` 对象，检查：

```
Transform
├─ Position: 应该在 (-10, -7) 到 (10, 7) 范围内
└─ Scale: 应该接近 (0.5-0.8, 0.5-0.8, 1)

Sprite Renderer
├─ Sprite: 应该有精灵或白色方块
├─ Color: 应该不是全透明 (Alpha > 0)
└─ Sorting Layer: Default, Order = 0
```

---

## 🆘 仍然无法解决？

### 1. 完全重置

```bash
# 在 Unity 中：
1. 停止游戏 (按 Play 键)
2. 删除 GameManager GameObject
3. Tools → Maze → Setup Game Scene
4. 点击 "创建 GameManager"
5. Tools → Maze → Setup Camera
6. 点击 "配置为横屏 2D 相机"
7. Tools → Maze → Convert JS Levels
8. 点击 "Create Test Level"
9. 按 Play 键
```

### 2. 查看详细日志

在运行游戏时，仔细查看 Console 中的每一条日志，特别是：
- 红色错误（Error）
- 黄色警告（Warning）

每条错误都会告诉你哪里有问题。

### 3. 使用诊断工具

**运行模式下**:
- Tools → Maze → Diagnose Rendering
- 点击 "强制刷新 MazeRenderer"

### 4. 检查 Unity 版本

确认 Unity 版本兼容：
- Unity 2021.3 LTS 或更高
- Unity 2022.3 LTS（推荐）

### 5. 重新导入脚本

右键点击 `Assets/Scripts` → **Reimport All**

等待编译完成（查看右下角进度条）。

---

## ✅ 成功标志

正确配置后，你应该看到：

1. **Game 窗口显示**:
   - 深灰色背景
   - 19×13 的网格（瓦片或精灵）
   - 居中显示
   - 可以看到所有格子

2. **Console 日志**:
   ```
   Camera size: ...
   Creating grid: 19x13
   Grid created successfully!
   ```

3. **Hierarchy**:
   - 247 个 Tile 对象 (13×19)
   - 都是 GameManager 的子对象

4. **可以移动**:
   - 按 WASD 键
   - 玩家（蓝色圆圈或精灵）移动

看到这些 = 渲染成功！🎉

---

## 📞 获取帮助

如果仍然无法解决：

1. 运行 **Tools → Maze → Diagnose Rendering**
2. 复制完整的诊断报告
3. 复制 Console 中的所有错误信息
4. 截图 Game 窗口、Scene 窗口、Inspector
5. 查看 GitHub Issues 或寻求帮助

**记住**: 大部分问题都是因为 Main Camera 未分配或相机设置错误！

祝你好运！🚀
