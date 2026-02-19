using UnityEngine;
using UnityEditor;
using MazeGame;

namespace MazeEditor
{
    /// <summary>
    /// 渲染诊断工具 - 检查为什么网格没有被渲染
    /// </summary>
    public class RenderingDiagnostics : EditorWindow
    {
        [MenuItem("Tools/Maze/Diagnose Rendering")]
        public static void ShowWindow()
        {
            GetWindow<RenderingDiagnostics>("Rendering Diagnostics");
        }

        private Vector2 scrollPos;

        private void OnGUI()
        {
            GUILayout.Label("渲染诊断工具", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            if (GUILayout.Button("运行诊断", GUILayout.Height(40)))
            {
                RunDiagnostics();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("强制刷新 MazeRenderer", GUILayout.Height(30)))
            {
                RefreshRenderer();
            }
        }

        private void RunDiagnostics()
        {
            System.Text.StringBuilder report = new System.Text.StringBuilder();
            report.AppendLine("=== 渲染诊断报告 ===\n");

            bool hasIssues = false;

            // 1. 检查 GameManager
            GameObject gmObj = GameObject.Find("GameManager");
            if (gmObj == null)
            {
                report.AppendLine("❌ 未找到 GameManager GameObject");
                report.AppendLine("   请先创建 GameManager (Tools → Maze → Setup Game Scene)\n");
                hasIssues = true;
            }
            else
            {
                report.AppendLine("✅ GameManager 存在\n");

                // 检查 MazeRenderer
                var renderer = gmObj.GetComponent<MazeRenderer>();
                if (renderer == null)
                {
                    report.AppendLine("❌ 缺少 MazeRenderer 组件");
                    hasIssues = true;
                }
                else
                {
                    report.AppendLine("✅ MazeRenderer 组件存在");

                    // 检查相机引用
                    var cameraField = typeof(MazeRenderer).GetField("mainCamera",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

                    if (cameraField != null)
                    {
                        Camera cam = cameraField.GetValue(renderer) as Camera;
                        if (cam == null)
                        {
                            report.AppendLine("❌ Main Camera 未分配！");
                            report.AppendLine("   在 MazeRenderer Inspector 中分配 Main Camera\n");
                            hasIssues = true;
                        }
                        else
                        {
                            report.AppendLine($"✅ Main Camera 已分配: {cam.name}");

                            // 检查相机设置
                            if (!cam.orthographic)
                            {
                                report.AppendLine("⚠️  警告: 相机不是正交投影！");
                                report.AppendLine("   运行 Tools → Maze → Setup Camera 修复\n");
                                hasIssues = true;
                            }
                            else
                            {
                                report.AppendLine($"✅ 相机是正交投影，Size = {cam.orthographicSize}");

                                if (cam.orthographicSize < 5 || cam.orthographicSize > 15)
                                {
                                    report.AppendLine($"⚠️  Orthographic Size ({cam.orthographicSize}) 可能不合适");
                                    report.AppendLine("   推荐值: 7-10\n");
                                }
                            }

                            // 检查相机位置
                            if (Mathf.Abs(cam.transform.position.z) < 5)
                            {
                                report.AppendLine($"⚠️  相机 Z 位置太近: {cam.transform.position.z}");
                                report.AppendLine("   推荐值: -10\n");
                                hasIssues = true;
                            }
                        }
                    }

                    // 检查 tile objects (如果游戏正在运行)
                    if (Application.isPlaying)
                    {
                        var tileObjectsField = typeof(MazeRenderer).GetField("tileObjects",
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Instance);

                        if (tileObjectsField != null)
                        {
                            GameObject[,] tiles = tileObjectsField.GetValue(renderer) as GameObject[,];
                            if (tiles != null && tiles.Length > 0)
                            {
                                report.AppendLine($"\n✅ 瓦片对象已创建: {tiles.GetLength(0)}x{tiles.GetLength(1)}");

                                // 检查第一个瓦片
                                if (tiles[0, 0] != null)
                                {
                                    var pos = tiles[0, 0].transform.position;
                                    var scale = tiles[0, 0].transform.localScale;
                                    report.AppendLine($"   第一个瓦片位置: {pos}");
                                    report.AppendLine($"   瓦片缩放: {scale}");

                                    var sr = tiles[0, 0].GetComponent<SpriteRenderer>();
                                    if (sr != null)
                                    {
                                        report.AppendLine($"   SpriteRenderer 存在: sprite={sr.sprite?.name}, color={sr.color}");
                                    }
                                    else
                                    {
                                        report.AppendLine("   ❌ 缺少 SpriteRenderer！");
                                        hasIssues = true;
                                    }
                                }
                            }
                            else
                            {
                                report.AppendLine("\n❌ 瓦片对象未创建！");
                                report.AppendLine("   可能原因:");
                                report.AppendLine("   1. GameState 未初始化");
                                report.AppendLine("   2. Initialize() 未被调用");
                                report.AppendLine("   3. 关卡数据缺失\n");
                                hasIssues = true;
                            }
                        }
                    }
                    else
                    {
                        report.AppendLine("\n⏸️  游戏未运行，无法检查运行时状态");
                        report.AppendLine("   按 Play 键后再次运行诊断\n");
                    }
                }
            }

            // 2. 检查场景中的其他对象
            report.AppendLine("\n=== 场景对象检查 ===");

            var allTiles = GameObject.FindGameObjectsWithTag("Untagged");
            int tileCount = 0;
            foreach (var obj in allTiles)
            {
                if (obj.name.StartsWith("Tile_"))
                {
                    tileCount++;
                }
            }

            if (tileCount > 0)
            {
                report.AppendLine($"✅ 找到 {tileCount} 个瓦片对象");
            }
            else
            {
                report.AppendLine("⚠️  场景中没有瓦片对象");
            }

            // 3. 总结
            report.AppendLine("\n=== 总结 ===");
            if (!hasIssues)
            {
                report.AppendLine("✅ 没有发现明显问题");
                report.AppendLine("\n如果仍然看不到网格，请检查:");
                report.AppendLine("1. Game 窗口（不是 Scene 窗口）");
                report.AppendLine("2. 相机视野是否对准 (0,0,0)");
                report.AppendLine("3. Console 中的错误信息");
            }
            else
            {
                report.AppendLine("⚠️  发现问题，请按照上述提示修复");
            }

            Debug.Log(report.ToString());

            EditorUtility.DisplayDialog(
                "渲染诊断",
                report.ToString(),
                "确定"
            );
        }

        private void RefreshRenderer()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "需要运行模式",
                    "请先按 Play 键启动游戏，然后再点击此按钮。",
                    "确定"
                );
                return;
            }

            GameObject gmObj = GameObject.Find("GameManager");
            if (gmObj == null)
            {
                EditorUtility.DisplayDialog("错误", "未找到 GameManager", "确定");
                return;
            }

            var renderer = gmObj.GetComponent<MazeRenderer>();
            if (renderer == null)
            {
                EditorUtility.DisplayDialog("错误", "未找到 MazeRenderer 组件", "确定");
                return;
            }

            // 强制重新初始化
            var initMethod = typeof(MazeRenderer).GetMethod("Initialize",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);

            var gmComponent = gmObj.GetComponent<GameManager>();
            if (gmComponent != null)
            {
                var stateMethod = typeof(GameManager).GetMethod("GetGameState",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance);

                if (stateMethod != null)
                {
                    var state = stateMethod.Invoke(gmComponent, null);
                    if (state != null && initMethod != null)
                    {
                        initMethod.Invoke(renderer, new object[] { state });
                        Debug.Log("MazeRenderer 已刷新！");
                        EditorUtility.DisplayDialog("成功", "MazeRenderer 已强制刷新", "确定");
                        return;
                    }
                }
            }

            EditorUtility.DisplayDialog("失败", "无法刷新 MazeRenderer", "确定");
        }
    }
}
