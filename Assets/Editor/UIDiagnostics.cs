using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using MazeGame;

/// <summary>
/// Diagnostic tool for UI setup
/// </summary>
public class UIDiagnostics : EditorWindow
{
    [MenuItem("Tools/Maze/Diagnose UI")]
    public static void ShowWindow()
    {
        GetWindow<UIDiagnostics>("UI Diagnostics");
    }

    private void OnGUI()
    {
        GUILayout.Label("UI Diagnostics", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        if (GUILayout.Button("Check UI Setup", GUILayout.Height(30)))
        {
            CheckUISetup();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Test Show Level Select", GUILayout.Height(30)))
        {
            TestShowLevelSelect();
        }
    }

    private void CheckUISetup()
    {
        UIManager uiManager = FindObjectOfType<UIManager>();

        if (uiManager == null)
        {
            EditorUtility.DisplayDialog("Error", "UIManager not found in scene!", "OK");
            return;
        }

        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("UI Setup Report:");
        report.AppendLine();

        // Check serialized fields using reflection
        var type = typeof(UIManager);
        var levelTextField = type.GetField("levelText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dynamiteTextField = type.GetField("dynamiteText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var levelSelectButtonField = type.GetField("levelSelectButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var uiPanelField = type.GetField("uiPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var levelSelectPanelField = type.GetField("levelSelectPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var levelButtonContainerField = type.GetField("levelButtonContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var levelButtonPrefabField = type.GetField("levelButtonPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        report.AppendLine(CheckField("Level Text", levelTextField?.GetValue(uiManager)));
        report.AppendLine(CheckField("Dynamite Text", dynamiteTextField?.GetValue(uiManager)));
        report.AppendLine(CheckField("Level Select Button", levelSelectButtonField?.GetValue(uiManager)));
        report.AppendLine(CheckField("UI Panel", uiPanelField?.GetValue(uiManager)));
        report.AppendLine(CheckField("Level Select Panel", levelSelectPanelField?.GetValue(uiManager)));
        report.AppendLine(CheckField("Level Button Container", levelButtonContainerField?.GetValue(uiManager)));
        report.AppendLine(CheckField("Level Button Prefab", levelButtonPrefabField?.GetValue(uiManager)));

        Debug.Log(report.ToString());
        EditorUtility.DisplayDialog("UI Diagnostics", report.ToString(), "OK");
    }

    private string CheckField(string fieldName, object value)
    {
        if (value == null || (value is UnityEngine.Object obj && obj == null))
        {
            return $"❌ {fieldName}: NOT ASSIGNED";
        }
        return $"✓ {fieldName}: OK";
    }

    private void TestShowLevelSelect()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Error", "Please enter Play mode first!", "OK");
            return;
        }

        UIManager uiManager = FindObjectOfType<UIManager>();

        if (uiManager == null)
        {
            EditorUtility.DisplayDialog("Error", "UIManager not found!", "OK");
            return;
        }

        uiManager.ShowLevelSelect();
        Debug.Log("ShowLevelSelect called manually");
    }
}
