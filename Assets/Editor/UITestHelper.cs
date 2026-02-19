using UnityEngine;
using UnityEditor;
using MazeGame;

/// <summary>
/// UI 功能测试工具
/// </summary>
public class UITestHelper : EditorWindow
{
    [MenuItem("Tools/Maze/Test UI Functions")]
    public static void ShowWindow()
    {
        GetWindow<UITestHelper>("UI Test Helper");
    }

    private void OnGUI()
    {
        GUILayout.Label("UI 功能测试", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("请先进入 Play 模式才能测试！", MessageType.Warning);
            return;
        }

        EditorGUILayout.HelpBox(
            "测试 UI 各项功能：\n" +
            "1. 检查 UI 设置\n" +
            "2. 测试显示选关弹窗\n" +
            "3. 测试隐藏选关弹窗\n" +
            "4. 测试跳转到指定关卡",
            MessageType.Info
        );

        EditorGUILayout.Space();

        // 检查 UI 设置
        if (GUILayout.Button("1. 检查 UI 设置", GUILayout.Height(35)))
        {
            CheckUISetup();
        }

        EditorGUILayout.Space();

        // 测试显示选关弹窗
        if (GUILayout.Button("2. 显示选关弹窗", GUILayout.Height(35)))
        {
            TestShowLevelSelect();
        }

        EditorGUILayout.Space();

        // 检查按钮可见性
        if (GUILayout.Button("2b. 检查按钮可见性", GUILayout.Height(35)))
        {
            CheckButtonVisibility();
        }

        EditorGUILayout.Space();

        // 测试隐藏选关弹窗
        if (GUILayout.Button("3. 隐藏选关弹窗 (通过 UIManager)", GUILayout.Height(35)))
        {
            TestHideLevelSelect();
        }

        EditorGUILayout.Space();

        // 测试关闭按钮
        if (GUILayout.Button("3b. 测试关闭按钮", GUILayout.Height(35)))
        {
            TestCloseButton();
        }

        EditorGUILayout.Space();

        // 测试跳转关卡
        GUILayout.Label("测试跳转到关卡：");
        if (GUILayout.Button("跳转到关卡 1", GUILayout.Height(30)))
        {
            TestLoadLevel(0);
        }
        if (GUILayout.Button("跳转到关卡 5", GUILayout.Height(30)))
        {
            TestLoadLevel(4);
        }
        if (GUILayout.Button("跳转到关卡 10", GUILayout.Height(30)))
        {
            TestLoadLevel(9);
        }
    }

    private void CheckUISetup()
    {
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManager not found!");
            EditorUtility.DisplayDialog("错误", "场景中没有找到 UIManager！", "OK");
            return;
        }

        Debug.Log("=== UI Setup Check ===");
        uiManager.TestButton();
        EditorUtility.DisplayDialog("提示", "检查完成，请查看 Console 了解详细信息。", "OK");
    }

    private void TestShowLevelSelect()
    {
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            EditorUtility.DisplayDialog("错误", "UIManager not found!", "OK");
            return;
        }

        Debug.Log("=== Testing ShowLevelSelect ===");
        uiManager.ShowLevelSelect();
        EditorUtility.DisplayDialog("提示", "选关弹窗应该已显示。\n请检查游戏画面。", "OK");
    }

    private void CheckButtonVisibility()
    {
        Debug.Log("=== Checking Button Visibility ===");

        // 查找 LevelSelectPanel
        GameObject panel = GameObject.Find("LevelSelectPanel");
        if (panel == null)
        {
            EditorUtility.DisplayDialog("错误", "找不到 LevelSelectPanel！", "OK");
            return;
        }

        Debug.Log($"LevelSelectPanel found, active: {panel.activeSelf}, activeInHierarchy: {panel.activeInHierarchy}");

        // 查找 ButtonContainer
        Transform container = panel.transform.Find("Content/ScrollView/Viewport/ButtonContainer");
        if (container == null)
        {
            Debug.LogError("找不到 ButtonContainer！检查层级结构...");
            // 尝试递归查找
            container = FindChildRecursive(panel.transform, "ButtonContainer");
        }

        if (container == null)
        {
            EditorUtility.DisplayDialog("错误", "找不到 ButtonContainer！\n请查看 Console 了解层级结构。", "OK");
            DebugHierarchy(panel.transform, 0);
            return;
        }

        Debug.Log($"ButtonContainer found: {container.name}, active: {container.gameObject.activeSelf}, activeInHierarchy: {container.gameObject.activeInHierarchy}");
        Debug.Log($"ButtonContainer child count: {container.childCount}");

        int visibleCount = 0;
        int invisibleCount = 0;

        foreach (Transform child in container)
        {
            bool visible = child.gameObject.activeInHierarchy;
            if (visible)
                visibleCount++;
            else
                invisibleCount++;

            Debug.Log($"  Button: {child.name}, active: {child.gameObject.activeSelf}, activeInHierarchy: {visible}");
        }

        string message = $"按钮总数: {container.childCount}\n" +
                        $"可见: {visibleCount}\n" +
                        $"不可见: {invisibleCount}\n\n" +
                        $"查看 Console 了解详细信息。";

        EditorUtility.DisplayDialog("按钮可见性检查", message, "OK");
    }

    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    private void DebugHierarchy(Transform root, int depth)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log($"{indent}{root.name} (active: {root.gameObject.activeSelf})");

        foreach (Transform child in root)
        {
            DebugHierarchy(child, depth + 1);
        }
    }

    private void TestHideLevelSelect()
    {
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            EditorUtility.DisplayDialog("错误", "UIManager not found!", "OK");
            return;
        }

        Debug.Log("=== Testing HideLevelSelect ===");
        uiManager.HideLevelSelect();
        EditorUtility.DisplayDialog("提示", "选关弹窗应该已隐藏。\n请检查游戏画面。", "OK");
    }

    private void TestCloseButton()
    {
        ClosePanelButton closeBtn = FindObjectOfType<ClosePanelButton>();
        if (closeBtn == null)
        {
            Debug.LogError("ClosePanelButton not found in scene!");
            EditorUtility.DisplayDialog("错误", "没有找到 ClosePanelButton 组件！\n请先重新创建 UI。", "OK");
            return;
        }

        Debug.Log("=== Testing Close Button ===");

        // 检查引用
        var serializedObj = new UnityEditor.SerializedObject(closeBtn);
        var panelProp = serializedObj.FindProperty("panelToClose");

        if (panelProp.objectReferenceValue == null)
        {
            Debug.LogError("Close button's panelToClose reference is null!");
            EditorUtility.DisplayDialog("错误", "关闭按钮的面板引用为空！\n请先重新创建 UI。", "OK");
            return;
        }

        Debug.Log($"Close button panel reference: {panelProp.objectReferenceValue.name}");

        // 模拟点击
        var button = closeBtn.GetComponent<UnityEngine.UI.Button>();
        if (button != null)
        {
            button.onClick.Invoke();
            EditorUtility.DisplayDialog("提示", "已模拟点击关闭按钮。\n请查看 Console 和游戏画面。", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("错误", "关闭按钮没有 Button 组件！", "OK");
        }
    }

    private void TestLoadLevel(int levelIndex)
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            EditorUtility.DisplayDialog("错误", "GameManager not found!", "OK");
            return;
        }

        Debug.Log($"=== Testing LoadLevel({levelIndex}) ===");
        gameManager.LoadLevel(levelIndex);
        EditorUtility.DisplayDialog("提示", $"正在加载关卡 {levelIndex + 1}...\n请查看 Console 和游戏画面。", "OK");
    }
}
