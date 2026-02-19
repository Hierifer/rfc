using UnityEngine;
using UnityEditor;

namespace MazeEditor
{
    /// <summary>
    /// 相机设置辅助工具 - 配置正交相机用于横屏 2D 游戏
    /// </summary>
    public class CameraSetupHelper : EditorWindow
    {
        [MenuItem("Tools/Maze/Setup Camera")]
        public static void ShowWindow()
        {
            GetWindow<CameraSetupHelper>("Camera Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("相机设置工具", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "点击按钮自动配置 Main Camera 为横屏 2D 游戏模式\n" +
                "- 正交投影（Orthographic）\n" +
                "- 适合 19×13 网格\n" +
                "- 居中显示",
                MessageType.Info
            );

            EditorGUILayout.Space();

            if (GUILayout.Button("配置为横屏 2D 相机", GUILayout.Height(40)))
            {
                SetupCamera();
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("推荐设置：", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• Orthographic Size: 7-10");
            EditorGUILayout.LabelField("• Position: (0, 0, -10)");
            EditorGUILayout.LabelField("• Background: 深色 (#111827)");
        }

        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }

            if (mainCamera == null)
            {
                EditorUtility.DisplayDialog(
                    "未找到相机",
                    "场景中没有相机！请先创建一个 Camera。",
                    "确定"
                );
                return;
            }

            // 记录 Undo
            Undo.RecordObject(mainCamera, "Setup Camera for 2D Landscape");

            // 设置为正交投影
            mainCamera.orthographic = true;

            // 设置正交大小
            // 19×13 网格，推荐 Size = 7-8 能完整显示
            mainCamera.orthographicSize = 8f;

            // 设置位置
            mainCamera.transform.position = new Vector3(0, 0, -10);

            // 设置背景色（深色）
            mainCamera.backgroundColor = new Color(0.067f, 0.094f, 0.153f); // #111827

            // 设置 Clear Flags
            mainCamera.clearFlags = CameraClearFlags.SolidColor;

            // 标记场景为已修改
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );

            // 选中相机
            Selection.activeGameObject = mainCamera.gameObject;

            Debug.Log("✅ 相机配置完成！");
            Debug.Log($"  - 正交投影，Size: {mainCamera.orthographicSize}");
            Debug.Log($"  - 位置: {mainCamera.transform.position}");
            Debug.Log($"  - 背景色: {mainCamera.backgroundColor}");

            EditorUtility.DisplayDialog(
                "配置完成",
                "Main Camera 已配置为横屏 2D 模式！\n\n" +
                "如果需要调整显示大小，修改 Orthographic Size：\n" +
                "• Size 7 = 较大显示\n" +
                "• Size 8 = 推荐（默认）\n" +
                "• Size 10 = 较小显示（看到更多区域）",
                "确定"
            );
        }
    }
}
