using UnityEngine;
using UnityEditor;
using MazeGame;

namespace MazeEditor
{
    /// <summary>
    /// 场景设置帮助工具 - 快速创建 GameManager 和配置
    /// </summary>
    public class SceneSetupHelper : EditorWindow
    {
        [MenuItem("Tools/Maze/Setup Game Scene")]
        public static void ShowWindow()
        {
            GetWindow<SceneSetupHelper>("Scene Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("迷宫游戏场景设置", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "点击下面的按钮自动创建和配置 GameManager GameObject",
                MessageType.Info
            );

            EditorGUILayout.Space();

            if (GUILayout.Button("创建 GameManager", GUILayout.Height(40)))
            {
                CreateGameManager();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("检查场景配置", GUILayout.Height(30)))
            {
                CheckSceneSetup();
            }
        }

        private void CreateGameManager()
        {
            // 检查是否已存在
            GameObject existing = GameObject.Find("GameManager");
            if (existing != null)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "GameManager 已存在",
                    "场景中已经有 GameManager，是否要删除并重新创建？",
                    "重新创建",
                    "取消"
                );

                if (overwrite)
                {
                    DestroyImmediate(existing);
                }
                else
                {
                    return;
                }
            }

            // 创建 GameManager GameObject
            GameObject gameManagerObj = new GameObject("GameManager");

            // 添加组件
            GameManager gameManager = gameManagerObj.AddComponent<GameManager>();
            InputManager inputManager = gameManagerObj.AddComponent<InputManager>();
            MazeRenderer mazeRenderer = gameManagerObj.AddComponent<MazeRenderer>();

            // 查找 Main Camera
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }

            // 配置 MazeRenderer (通过反射设置私有字段)
            if (mainCamera != null)
            {
                var field = typeof(MazeRenderer).GetField("mainCamera",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (field != null)
                {
                    field.SetValue(mazeRenderer, mainCamera);
                    Debug.Log("已自动分配 Main Camera");
                }
            }
            else
            {
                Debug.LogWarning("未找到 Main Camera，请手动分配！");
            }

            // 标记场景为已修改
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            // 选中新创建的对象
            Selection.activeGameObject = gameManagerObj;

            EditorUtility.DisplayDialog(
                "创建成功",
                "GameManager 已创建并配置完成！\n\n请检查 MazeRenderer 组件的 Main Camera 是否正确分配。",
                "确定"
            );

            Debug.Log("✅ GameManager 创建成功！");
        }

        private void CheckSceneSetup()
        {
            System.Text.StringBuilder report = new System.Text.StringBuilder();
            report.AppendLine("=== 场景配置检查 ===\n");

            bool allGood = true;

            // 检查 GameManager
            GameObject gmObj = GameObject.Find("GameManager");
            if (gmObj != null)
            {
                report.AppendLine("✅ GameManager GameObject 存在");

                // 检查组件
                var gm = gmObj.GetComponent<GameManager>();
                var im = gmObj.GetComponent<InputManager>();
                var mr = gmObj.GetComponent<MazeRenderer>();

                if (gm != null) report.AppendLine("  ✅ GameManager 组件");
                else { report.AppendLine("  ❌ 缺少 GameManager 组件"); allGood = false; }

                if (im != null) report.AppendLine("  ✅ InputManager 组件");
                else { report.AppendLine("  ❌ 缺少 InputManager 组件"); allGood = false; }

                if (mr != null)
                {
                    report.AppendLine("  ✅ MazeRenderer 组件");

                    // 检查 Main Camera 引用
                    var cameraField = typeof(MazeRenderer).GetField("mainCamera",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

                    if (cameraField != null)
                    {
                        Camera cam = cameraField.GetValue(mr) as Camera;
                        if (cam != null)
                        {
                            report.AppendLine("    ✅ Main Camera 已分配");
                        }
                        else
                        {
                            report.AppendLine("    ⚠️  Main Camera 未分配");
                            allGood = false;
                        }
                    }
                }
                else
                {
                    report.AppendLine("  ❌ 缺少 MazeRenderer 组件");
                    allGood = false;
                }
            }
            else
            {
                report.AppendLine("❌ GameManager GameObject 不存在");
                allGood = false;
            }

            // 检查 Main Camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                report.AppendLine("\n✅ Main Camera 存在");
            }
            else
            {
                report.AppendLine("\n❌ Main Camera 不存在");
                allGood = false;
            }

            // 检查精灵图资源
            report.AppendLine("\n=== 资源检查 ===");

            Texture2D tileAtlas = Resources.Load<Texture2D>("Spirits/tile-atlas-small");
            Texture2D entityAtlas = Resources.Load<Texture2D>("Spirits/entity-atlas-small");

            if (tileAtlas != null)
            {
                report.AppendLine($"✅ tile-atlas-small.png ({tileAtlas.width}x{tileAtlas.height})");
            }
            else
            {
                report.AppendLine("❌ 缺少 tile-atlas-small.png");
                report.AppendLine("   请确保文件在: Assets/Resources/Spirits/");
                allGood = false;
            }

            if (entityAtlas != null)
            {
                report.AppendLine($"✅ entity-atlas-small.png ({entityAtlas.width}x{entityAtlas.height})");
            }
            else
            {
                report.AppendLine("❌ 缺少 entity-atlas-small.png");
                report.AppendLine("   请确保文件在: Assets/Resources/Spirits/");
                allGood = false;
            }

            // 显示报告
            report.AppendLine("\n===================");
            if (allGood)
            {
                report.AppendLine("\n🎉 所有配置正确！可以运行游戏了！");
            }
            else
            {
                report.AppendLine("\n⚠️  发现问题，请按照上述提示修复");
            }

            Debug.Log(report.ToString());

            EditorUtility.DisplayDialog(
                "场景配置检查",
                report.ToString(),
                "确定"
            );
        }
    }
}
