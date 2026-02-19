using UnityEngine;
using UnityEditor;
using MazeGame;

/// <summary>
/// 添加 UI 点击修复组件的编辑器工具
/// </summary>
public class UIClickFixerHelper : EditorWindow
{
    [MenuItem("Tools/Maze/Fix UI Clicks")]
    public static void ShowWindow()
    {
        GetWindow<UIClickFixerHelper>("Fix UI Clicks");
    }

    private void OnGUI()
    {
        GUILayout.Label("UI Click Fix Helper", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "这个工具会帮助诊断和修复 UI 按钮无法点击的问题。\n\n" +
            "点击下面的按钮将会：\n" +
            "1. 添加 UIClickFixer 组件到场景\n" +
            "2. 检查 EventSystem 是否存在\n" +
            "3. 检查所有 Canvas 的 GraphicRaycaster\n" +
            "4. 检查所有按钮的 Image 和 raycastTarget\n\n" +
            "请先运行游戏，然后查看 Console 的诊断信息。",
            MessageType.Info
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("Add UIClickFixer to Scene", GUILayout.Height(40)))
        {
            AddClickFixer();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Remove UIClickFixer from Scene", GUILayout.Height(30)))
        {
            RemoveClickFixer();
        }
    }

    private void AddClickFixer()
    {
        // 查找是否已存在
        UIClickFixer existing = FindObjectOfType<UIClickFixer>();
        if (existing != null)
        {
            EditorUtility.DisplayDialog("提示", "UIClickFixer 已经存在于场景中！", "OK");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // 尝试添加到 Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject targetObj;

        if (canvas != null)
        {
            targetObj = canvas.gameObject;
        }
        else
        {
            // 如果没有 Canvas，创建一个新的 GameObject
            targetObj = new GameObject("UIClickFixer");
        }

        targetObj.AddComponent<UIClickFixer>();

        EditorUtility.SetDirty(targetObj);
        Selection.activeGameObject = targetObj;

        EditorUtility.DisplayDialog(
            "成功",
            "UIClickFixer 已添加！\n\n请运行游戏，它会自动诊断并修复问题。\n查看 Console 了解详细信息。",
            "OK"
        );
    }

    private void RemoveClickFixer()
    {
        UIClickFixer[] fixers = FindObjectsOfType<UIClickFixer>();

        if (fixers.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "场景中没有找到 UIClickFixer 组件。", "OK");
            return;
        }

        foreach (var fixer in fixers)
        {
            DestroyImmediate(fixer);
        }

        EditorUtility.DisplayDialog("成功", $"已移除 {fixers.Length} 个 UIClickFixer 组件。", "OK");
    }
}
