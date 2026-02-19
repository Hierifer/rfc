using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MazeGame;

/// <summary>
/// Editor tool to setup game UI
/// </summary>
public class UISetupHelper : EditorWindow
{
    [MenuItem("Tools/Maze/Setup Game UI")]
    public static void ShowWindow()
    {
        GetWindow<UISetupHelper>("UI Setup Helper");
    }

    private void OnGUI()
    {
        GUILayout.Label("Game UI Setup", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Game UI", GUILayout.Height(30)))
        {
            CreateGameUI();
        }

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This will create:\n" +
            "- Canvas with UI elements\n" +
            "- Level display (左侧顶部)\n" +
            "- Dynamite counter (左侧)\n" +
            "- UIManager component",
            MessageType.Info
        );
    }

    /// <summary>
    /// 清理旧的 UI 元素，避免重复创建
    /// </summary>
    private void CleanupOldUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            // 删除旧的 LevelSelectPanel
            Transform oldPanel = canvas.transform.Find("LevelSelectPanel");
            if (oldPanel != null)
            {
                Debug.Log("Deleting old LevelSelectPanel");
                DestroyImmediate(oldPanel.gameObject);
            }

            // 删除旧的 UI_Panel
            Transform oldUIPanel = canvas.transform.Find("UI_Panel");
            if (oldUIPanel != null)
            {
                Debug.Log("Deleting old UI_Panel");
                DestroyImmediate(oldUIPanel.gameObject);
            }
        }
    }

    private void CreateGameUI()
    {
        // 先清理旧的 UI 元素，避免重复
        CleanupOldUI();

        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject canvasObj;

        if (canvas == null)
        {
            canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("Canvas created with GraphicRaycaster");
        }
        else
        {
            canvasObj = canvas.gameObject;

            // Ensure GraphicRaycaster exists
            if (canvasObj.GetComponent<GraphicRaycaster>() == null)
            {
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("GraphicRaycaster added to existing Canvas");
            }
        }

        // Create UI Panel (left sidebar)
        GameObject panelObj = new GameObject("UI_Panel");
        panelObj.transform.SetParent(canvasObj.transform, false);

        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(20, -20);
        panelRect.sizeDelta = new Vector2(200, 0);

        // Create Level Text
        GameObject levelTextObj = new GameObject("LevelText");
        levelTextObj.transform.SetParent(panelObj.transform, false);

        RectTransform levelRect = levelTextObj.AddComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0, 1);
        levelRect.anchorMax = new Vector2(0, 1);
        levelRect.pivot = new Vector2(0, 1);
        levelRect.anchoredPosition = new Vector2(0, 0);
        levelRect.sizeDelta = new Vector2(200, 40);

        Text levelText = levelTextObj.AddComponent<Text>();
        levelText.text = "关卡 1/20";
        levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        levelText.fontSize = 32;
        levelText.color = Color.white;
        levelText.alignment = TextAnchor.MiddleLeft;

        // Add outline for better visibility
        Outline levelOutline = levelTextObj.AddComponent<Outline>();
        levelOutline.effectColor = Color.black;
        levelOutline.effectDistance = new Vector2(1, -1);

        // Create Dynamite Text
        GameObject dynamiteTextObj = new GameObject("DynamiteText");
        dynamiteTextObj.transform.SetParent(panelObj.transform, false);

        RectTransform dynamiteRect = dynamiteTextObj.AddComponent<RectTransform>();
        dynamiteRect.anchorMin = new Vector2(0, 1);
        dynamiteRect.anchorMax = new Vector2(0, 1);
        dynamiteRect.pivot = new Vector2(0, 1);
        dynamiteRect.anchoredPosition = new Vector2(0, -50);
        dynamiteRect.sizeDelta = new Vector2(200, 40);

        Text dynamiteText = dynamiteTextObj.AddComponent<Text>();
        dynamiteText.text = "雷管: 0";
        dynamiteText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        dynamiteText.fontSize = 28;
        dynamiteText.color = new Color(0.961f, 0.620f, 0.043f); // Orange color for dynamite
        dynamiteText.alignment = TextAnchor.MiddleLeft;

        // Add outline
        Outline dynamiteOutline = dynamiteTextObj.AddComponent<Outline>();
        dynamiteOutline.effectColor = Color.black;
        dynamiteOutline.effectDistance = new Vector2(1, -1);

        // Create Level Select Button
        GameObject levelSelectButtonObj = new GameObject("LevelSelectButton");
        levelSelectButtonObj.transform.SetParent(panelObj.transform, false);

        RectTransform buttonRect = levelSelectButtonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 1);
        buttonRect.anchorMax = new Vector2(0, 1);
        buttonRect.pivot = new Vector2(0, 1);
        buttonRect.anchoredPosition = new Vector2(0, -100);
        buttonRect.sizeDelta = new Vector2(120, 35);

        Image buttonImage = levelSelectButtonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.3f, 0.4f, 0.9f);
        buttonImage.raycastTarget = true; // 确保可以接收点击

        Button levelSelectBtn = levelSelectButtonObj.AddComponent<Button>();
        ColorBlock colors = levelSelectBtn.colors;
        colors.normalColor = new Color(0.2f, 0.3f, 0.4f, 0.9f);
        colors.highlightedColor = new Color(0.3f, 0.4f, 0.5f, 1f);
        colors.pressedColor = new Color(0.15f, 0.25f, 0.35f, 1f);
        levelSelectBtn.colors = colors;

        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(levelSelectButtonObj.transform, false);

        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;

        Text buttonText = buttonTextObj.AddComponent<Text>();
        buttonText.text = "选关";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;

        // Create Level Select Panel
        GameObject levelSelectPanelObj = new GameObject("LevelSelectPanel");
        levelSelectPanelObj.transform.SetParent(canvasObj.transform, false);

        RectTransform panelPanelRect = levelSelectPanelObj.AddComponent<RectTransform>();
        panelPanelRect.anchorMin = Vector2.zero;
        panelPanelRect.anchorMax = Vector2.one;
        panelPanelRect.sizeDelta = Vector2.zero;

        Image panelImage = levelSelectPanelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        panelImage.raycastTarget = true; // 阻挡背后的点击

        // Create panel content
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(levelSelectPanelObj.transform, false);

        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(600, 400);

        Image contentImage = contentObj.AddComponent<Image>();
        contentImage.color = new Color(0.1f, 0.15f, 0.2f, 1f);
        contentImage.raycastTarget = true; // 确保内容区域可以接收点击

        // Create title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(contentObj.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(580, 40);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "选择关卡";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 36;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;

        // Create close button
        GameObject closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(contentObj.transform, false);

        RectTransform closeRect = closeButtonObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.pivot = new Vector2(1, 1);
        closeRect.anchoredPosition = new Vector2(-10, -10);
        closeRect.sizeDelta = new Vector2(40, 40);

        Image closeImage = closeButtonObj.AddComponent<Image>();
        closeImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        closeImage.raycastTarget = true; // 确保可以接收点击

        Button closeButton = closeButtonObj.AddComponent<Button>();

        // 添加关闭面板脚本
        ClosePanelButton closePanelScript = closeButtonObj.AddComponent<ClosePanelButton>();

        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeButtonObj.transform, false);

        RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.sizeDelta = Vector2.zero;

        Text closeText = closeTextObj.AddComponent<Text>();
        closeText.text = "×";
        closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        closeText.fontSize = 30;
        closeText.color = Color.white;
        closeText.alignment = TextAnchor.MiddleCenter;

        // Create scroll view for level buttons
        GameObject scrollViewObj = new GameObject("ScrollView");
        scrollViewObj.transform.SetParent(contentObj.transform, false);

        RectTransform scrollRect = scrollViewObj.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.pivot = new Vector2(0.5f, 0.5f);
        scrollRect.anchoredPosition = new Vector2(0, -30);
        scrollRect.sizeDelta = new Vector2(-40, -100);

        ScrollRect scrollComponent = scrollViewObj.AddComponent<ScrollRect>();

        // Create viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);

        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;

        Mask viewportMask = viewportObj.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = Color.clear;
        viewportImage.raycastTarget = false; // 不阻挡点击，让点击穿透到按钮

        // Create content container for buttons
        GameObject buttonContainerObj = new GameObject("ButtonContainer");
        buttonContainerObj.transform.SetParent(viewportObj.transform, false);

        RectTransform containerRect = buttonContainerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot = new Vector2(0.5f, 1);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(0, 300);

        GridLayoutGroup gridLayout = buttonContainerObj.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(100, 60);
        gridLayout.spacing = new Vector2(10, 10);
        gridLayout.padding = new RectOffset(10, 10, 10, 10);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 5;

        ContentSizeFitter sizeFitter = buttonContainerObj.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollComponent.content = containerRect;
        scrollComponent.viewport = viewportRect;
        scrollComponent.horizontal = false;
        scrollComponent.vertical = true;

        // Create level button prefab
        GameObject levelButtonPrefab = new GameObject("LevelButtonPrefab");

        RectTransform prefabRect = levelButtonPrefab.AddComponent<RectTransform>();
        prefabRect.sizeDelta = new Vector2(100, 60);

        Image prefabImage = levelButtonPrefab.AddComponent<Image>();
        prefabImage.color = new Color(0.3f, 0.4f, 0.5f, 1f);
        prefabImage.raycastTarget = true; // 确保可以接收点击

        Button prefabButton = levelButtonPrefab.AddComponent<Button>();

        GameObject prefabTextObj = new GameObject("Text");
        prefabTextObj.transform.SetParent(levelButtonPrefab.transform, false);

        RectTransform prefabTextRect = prefabTextObj.AddComponent<RectTransform>();
        prefabTextRect.anchorMin = Vector2.zero;
        prefabTextRect.anchorMax = Vector2.one;
        prefabTextRect.sizeDelta = Vector2.zero;

        Text prefabText = prefabTextObj.AddComponent<Text>();
        prefabText.text = "1";
        prefabText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        prefabText.fontSize = 28;
        prefabText.color = Color.white;
        prefabText.alignment = TextAnchor.MiddleCenter;

        // Deactivate prefab so it doesn't appear in the scene
        levelButtonPrefab.SetActive(false);

        // 确保所有中间对象都是激活的（在隐藏面板之前）
        contentObj.SetActive(true);
        scrollViewObj.SetActive(true);
        viewportObj.SetActive(true);
        buttonContainerObj.SetActive(true);
        Debug.Log("All intermediate UI objects activated");

        // 初始隐藏选关面板（这会使整个层级变为 inactive）
        levelSelectPanelObj.SetActive(false);
        Debug.Log("LevelSelectPanel created and hidden (will be shown when clicking level select button)");

        // Add UIManager to Canvas
        UIManager uiManager = canvasObj.GetComponent<UIManager>();
        if (uiManager == null)
        {
            uiManager = canvasObj.AddComponent<UIManager>();
        }

        // Set references using SerializedObject
        SerializedObject serializedUI = new SerializedObject(uiManager);
        serializedUI.FindProperty("levelText").objectReferenceValue = levelText;
        serializedUI.FindProperty("dynamiteText").objectReferenceValue = dynamiteText;
        serializedUI.FindProperty("levelSelectButton").objectReferenceValue = levelSelectBtn;
        serializedUI.FindProperty("uiPanel").objectReferenceValue = panelObj;
        serializedUI.FindProperty("levelSelectPanel").objectReferenceValue = levelSelectPanelObj;
        serializedUI.FindProperty("levelButtonContainer").objectReferenceValue = buttonContainerObj.transform;
        serializedUI.FindProperty("levelButtonPrefab").objectReferenceValue = levelButtonPrefab;
        serializedUI.ApplyModifiedProperties();

        // Setup close button - 使用 SerializedObject 设置引用
        closePanelScript = closeButtonObj.GetComponent<ClosePanelButton>();
        if (closePanelScript != null)
        {
            SerializedObject serializedCloseBtn = new SerializedObject(closePanelScript);
            serializedCloseBtn.FindProperty("panelToClose").objectReferenceValue = levelSelectPanelObj;
            serializedCloseBtn.ApplyModifiedProperties();
            Debug.Log("Close button configured to close level select panel");
        }

        // Find GameManager and link UIManager
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            SerializedObject serializedGM = new SerializedObject(gameManager);
            serializedGM.FindProperty("uiManager").objectReferenceValue = uiManager;
            serializedGM.ApplyModifiedProperties();
        }

        // Ensure EventSystem exists for UI interactions
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("EventSystem created");
        }

        EditorUtility.SetDirty(canvasObj);
        if (gameManager != null)
        {
            EditorUtility.SetDirty(gameManager);
        }

        Selection.activeGameObject = canvasObj;

        EditorUtility.DisplayDialog("Success", "Game UI created successfully!\n\nCanvas with Level and Dynamite displays has been added.", "OK");
    }
}
