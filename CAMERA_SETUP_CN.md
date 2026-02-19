# 相机设置指南 - 横屏居中显示

## 🎯 问题说明

如果你看到：
- ❌ 游戏画面不在屏幕中央
- ❌ 网格显示不完整
- ❌ 瓦片过大或过小

那么需要正确配置相机！

---

## 🚀 快速修复（30秒）

### 方法 1: 自动配置（推荐）

1. Unity 菜单栏 → **Tools → Maze → Setup Camera**
2. 点击 **"配置为横屏 2D 相机"**
3. 完成！🎉

### 方法 2: 手动配置

选中 **Main Camera**，在 Inspector 中设置：

```
Camera 组件
├─ Projection: Orthographic ⭐
├─ Size: 8 ⭐
├─ Position: (0, 0, -10)
├─ Background:
│   └─ Color: #111827 (深灰色)
└─ Clear Flags: Solid Color
```

---

## 📐 尺寸调整

修改 **Orthographic Size** 来调整显示大小：

| Size 值 | 效果 | 适用场景 |
|--------|------|---------|
| **7** | 较大，网格占满屏幕 | 小屏幕设备 |
| **8** | ✅ 推荐，居中显示 | 通用设置 |
| **10** | 较小，能看到更多 | 大屏幕/调试 |

**提示**: 数值越小，物体显示越大

---

## 🖥️ 横屏 vs 竖屏

### 横屏模式（默认）

```
Camera Aspect: 16:9 或 16:10
网格: 19 宽 × 13 高
适合: 电脑、平板横屏
```

相机会自动计算并居中显示网格。

### 如果要支持竖屏

修改 `PlayerSettings`:
1. Edit → Project Settings → Player
2. Resolution and Presentation
3. Default Orientation: Portrait

**注意**: 竖屏时可能需要调整 UI 布局

---

## 🔍 调试检查

### 检查相机设置

选中 Main Camera，确认：
- ✅ Projection = **Orthographic**
- ✅ Size = **7-10** 之间
- ✅ Position Z = **-10** (负数！)

### 检查 Game 窗口

点击 **Game** 标签（不是 Scene），检查：
- ✅ Aspect 设置为 **Free Aspect** 或 **16:9**
- ✅ 能看到完整的网格

### Console 日志检查

运行游戏时查看 Console，应该看到：
```
Camera size: 14.22x8, Tile size: 0.xxx, Grid offset: (...)
```

如果看到这行，说明计算正确！

---

## 🐛 常见问题

### Q1: 游戏画面在左上角

**原因**: 相机坐标系统错误

**解决方案**:
1. 确认相机 Position = **(0, 0, -10)**
2. 运行 Tools → Maze → Setup Camera

### Q2: 瓦片太小/太大

**原因**: Orthographic Size 不合适

**解决方案**:
- 太小 → 增加 Size (如 8 → 10)
- 太大 → 减小 Size (如 8 → 7)

### Q3: 网格没有居中

**原因**: MazeRenderer 的坐标计算问题

**解决方案**:
1. 确认已使用最新版本的 MazeRenderer.cs
2. 检查 Main Camera 是否分配给 MazeRenderer
3. 查看 Console 日志中的 Grid offset 值

### Q4: 只能看到一部分网格

**原因**: 相机 Size 太小

**解决方案**:
- 增加 Orthographic Size 到 8-10

---

## 📊 技术细节

### 坐标系统说明

游戏使用 **世界坐标系统**，不依赖屏幕像素：

```
世界坐标原点 (0, 0) = 屏幕中心
网格左下角 = (-gridWidth/2, -gridHeight/2)
网格右上角 = (+gridWidth/2, +gridHeight/2)
```

### 计算公式

```csharp
// 相机世界空间尺寸
cameraHeight = orthographicSize * 2
cameraWidth = cameraHeight * aspect

// 瓦片大小
worldTileSize = min(
    cameraWidth / 19,   // 宽度限制
    cameraHeight / 13   // 高度限制
) * 0.9  // 留出边距

// 居中偏移
gridOffset = (
    -gridWidth / 2,
    -gridHeight / 2
)
```

### 自适应屏幕

系统会根据屏幕宽高比自动调整：

- **横屏 (16:9)**: 以高度为基准，左右留白
- **竖屏 (9:16)**: 以宽度为基准，上下留白
- **方屏 (1:1)**: 均匀缩放

---

## ✅ 验证清单

设置完成后检查：

- [ ] Main Camera 是正交投影
- [ ] Orthographic Size = 7-10
- [ ] Position = (0, 0, -10)
- [ ] MazeRenderer 已分配 Main Camera
- [ ] Game 窗口能看到完整网格
- [ ] 网格在屏幕中央
- [ ] Console 无错误

全部勾选 = 配置正确！🎉

---

## 🎮 测试运行

1. 按 **Play** 键
2. 观察 Game 窗口
3. 网格应该：
   - ✅ 在屏幕正中央
   - ✅ 完整显示（19×13 格子）
   - ✅ 大小适中（不太大不太小）

如果一切正常，用 **WASD** 移动测试！

---

**配置愉快！** 🚀

有问题？运行 **Tools → Maze → Setup Camera** 自动修复！
