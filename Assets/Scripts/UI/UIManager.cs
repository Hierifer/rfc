using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// UI Manager for displaying game information
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Text levelText;
        [SerializeField] private Text dynamiteText;
        [SerializeField] private Button levelSelectButton;

        [Header("UI Container")]
        [SerializeField] private GameObject uiPanel;

        [Header("Level Select Panel")]
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private Transform levelButtonContainer;
        [SerializeField] private GameObject levelButtonPrefab;

        private int currentLevel;
        private int totalLevels;
        private int dynamiteCount;
        private GameManager gameManager;
        private SaveManager saveManager;

        /// <summary>
        /// Initialize UI with level information
        /// </summary>
        public void Initialize(int levelIndex, int totalLevelCount, GameManager manager, SaveManager saveManager)
        {
            Debug.Log("UIManager Initialize called");

            this.gameManager = manager;
            this.saveManager = saveManager;
            totalLevels = totalLevelCount;

            UpdateLevel(levelIndex);
            UpdateDynamite(0);

            // Setup level select button
            if (levelSelectButton != null)
            {
                Debug.Log($"Setting up level select button listener. Button active: {levelSelectButton.gameObject.activeInHierarchy}, Interactable: {levelSelectButton.interactable}");
                levelSelectButton.onClick.RemoveAllListeners(); // Clear existing listeners
                levelSelectButton.onClick.AddListener(() =>
                {
                    Debug.Log("Level select button clicked!");
                    ShowLevelSelect();
                });

                // Ensure button is interactable
                levelSelectButton.interactable = true;
            }
            else
            {
                Debug.LogError("Level select button is null!");
            }

            // Ensure UI is visible
            if (uiPanel != null)
            {
                uiPanel.SetActive(true);
            }

            // Hide level select panel initially
            if (levelSelectPanel != null)
            {
                levelSelectPanel.SetActive(false);
                Debug.Log("Level select panel hidden initially");
            }
            else
            {
                Debug.LogError("Level select panel is null!");
            }
        }

        /// <summary>
        /// Update level display
        /// </summary>
        public void UpdateLevel(int levelIndex)
        {
            currentLevel = levelIndex + 1; // Convert 0-based to 1-based

            if (levelText != null)
            {
                levelText.text = $"关卡 {currentLevel}/{totalLevels}";
            }
        }

        /// <summary>
        /// Update dynamite count display
        /// </summary>
        public void UpdateDynamite(int count)
        {
            dynamiteCount = count;

            if (dynamiteText != null)
            {
                dynamiteText.text = $"雷管: {dynamiteCount}";
            }
        }

        /// <summary>
        /// Show win message
        /// </summary>
        public void ShowWinMessage()
        {
            // TODO: Show win dialog or animation
            Debug.Log("Level Complete!");
        }

        /// <summary>
        /// Show lose message
        /// </summary>
        public void ShowLoseMessage()
        {
            // TODO: Show lose dialog
            Debug.Log("Game Over!");
        }

        /// <summary>
        /// Test method for debugging button issues
        /// </summary>
        public void TestButton()
        {
            Debug.Log("=== BUTTON TEST ===");
            Debug.Log($"levelSelectButton is null: {levelSelectButton == null}");
            if (levelSelectButton != null)
            {
                Debug.Log($"Button GameObject: {levelSelectButton.gameObject.name}");
                Debug.Log($"Button active: {levelSelectButton.gameObject.activeInHierarchy}");
                Debug.Log($"Button interactable: {levelSelectButton.interactable}");
                Debug.Log($"Button onClick listener count: {levelSelectButton.onClick.GetPersistentEventCount()}");
            }
            Debug.Log($"levelSelectPanel is null: {levelSelectPanel == null}");
            Debug.Log($"levelButtonContainer is null: {levelButtonContainer == null}");
            Debug.Log($"levelButtonPrefab is null: {levelButtonPrefab == null}");
        }

        /// <summary>
        /// Show level select panel
        /// </summary>
        public void ShowLevelSelect()
        {
            Debug.Log("=== ShowLevelSelect called ===");

            if (levelSelectPanel == null)
            {
                Debug.LogError("Level select panel is null!");
                return;
            }

            if (levelButtonContainer == null)
            {
                Debug.LogError("Level button container is null!");
                return;
            }

            if (levelButtonPrefab == null)
            {
                Debug.LogError("Level button prefab is null!");
                return;
            }

            Debug.Log($"levelButtonPrefab active state: {levelButtonPrefab.activeSelf}");
            Debug.Log($"levelButtonContainer: {levelButtonContainer.name}, child count: {levelButtonContainer.childCount}");
            Debug.Log($"levelButtonContainer active: {levelButtonContainer.gameObject.activeSelf}");

            // 检查整个层级的激活状态
            Transform current = levelButtonContainer;
            while (current != null)
            {
                Debug.Log($"  Hierarchy: {current.name}, active: {current.gameObject.activeSelf}, activeInHierarchy: {current.gameObject.activeInHierarchy}");
                current = current.parent;
            }

            Debug.Log($"levelSelectPanel active before show: {levelSelectPanel.activeSelf}");

            // Clear existing buttons
            int childCount = levelButtonContainer.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Destroy(levelButtonContainer.GetChild(i).gameObject);
            }
            Debug.Log($"Cleared {childCount} existing buttons");

            // Get completed levels
            HashSet<int> completedLevels = saveManager != null ? saveManager.GetCompletedLevels() : new HashSet<int>();
            Debug.Log($"Completed levels: {completedLevels.Count}");

            // Create level buttons
            for (int i = 0; i < totalLevels; i++)
            {
                int levelIndex = i; // Capture for closure
                GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer, false);
                buttonObj.SetActive(true); // Ensure button is active
                buttonObj.name = $"LevelButton_{levelIndex + 1}";

                // 确保 RectTransform 正确
                RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.localScale = Vector3.one;
                    Debug.Log($"Button {levelIndex + 1} created: pos={buttonRect.anchoredPosition}, size={buttonRect.sizeDelta}");
                }
                else
                {
                    Debug.LogWarning($"Button {i} has no RectTransform!");
                }

                // Setup button text
                Text buttonText = buttonObj.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    bool isCompleted = completedLevels.Contains(levelIndex);
                    buttonText.text = isCompleted ? $"✓ {levelIndex + 1}" : $"{levelIndex + 1}";
                    Debug.Log($"Button {levelIndex + 1} text: {buttonText.text}");
                }
                else
                {
                    Debug.LogWarning($"Button {i} has no Text component");
                }

                // Setup button click
                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    // 确保按钮可交互
                    button.interactable = true;

                    // 添加点击监听器
                    button.onClick.AddListener(() =>
                    {
                        Debug.Log($"Level button {levelIndex + 1} clicked!");
                        OnLevelSelected(levelIndex);
                    });

                    Debug.Log($"Button {levelIndex + 1} setup complete, interactable: {button.interactable}");
                }
                else
                {
                    Debug.LogWarning($"Button {i} has no Button component");
                }

                // 确保按钮的 Image 可以接收射线
                UnityEngine.UI.Image buttonImage = buttonObj.GetComponent<UnityEngine.UI.Image>();
                if (buttonImage != null)
                {
                    buttonImage.raycastTarget = true;
                }
            }

            Debug.Log($"=== Created {totalLevels} level buttons ===");
            Debug.Log($"Final button container child count: {levelButtonContainer.childCount}");

            // 确保所有父级对象都激活
            current = levelButtonContainer;
            while (current != null && current != levelSelectPanel.transform)
            {
                if (!current.gameObject.activeSelf)
                {
                    Debug.LogWarning($"Activating inactive parent: {current.name}");
                    current.gameObject.SetActive(true);
                }
                current = current.parent;
            }

            // Show panel (这会激活整个层级)
            levelSelectPanel.SetActive(true);
            Debug.Log($"Level select panel shown, active: {levelSelectPanel.activeSelf}");

            // 再次检查按钮的可见性
            Debug.Log($"=== Checking button visibility after panel shown ===");
            foreach (Transform child in levelButtonContainer)
            {
                Debug.Log($"  Child: {child.name}, active: {child.gameObject.activeSelf}, activeInHierarchy: {child.gameObject.activeInHierarchy}");
            }

            if (levelButtonContainer.childCount > 0 && !levelButtonContainer.GetChild(0).gameObject.activeInHierarchy)
            {
                Debug.LogError("❌ Buttons are still not active in hierarchy after showing panel!");
                Debug.LogError($"ButtonContainer activeInHierarchy: {levelButtonContainer.gameObject.activeInHierarchy}");
            }
            else
            {
                Debug.Log($"✓ Buttons are active and visible");
            }
        }

        /// <summary>
        /// Hide level select panel
        /// </summary>
        public void HideLevelSelect()
        {
            Debug.Log("=== HideLevelSelect called ===");

            if (levelSelectPanel != null)
            {
                Debug.Log($"Panel before: active={levelSelectPanel.activeSelf}, name={levelSelectPanel.name}");
                levelSelectPanel.SetActive(false);
                Debug.Log($"Panel after: active={levelSelectPanel.activeSelf}");

                // 强制检查
                if (levelSelectPanel.activeSelf)
                {
                    Debug.LogError("Panel is still active after SetActive(false)!");
                }
                else
                {
                    Debug.Log("✓ Level select panel successfully hidden");
                }
            }
            else
            {
                Debug.LogError("levelSelectPanel reference is null!");
            }
        }

        /// <summary>
        /// Handle level selection
        /// </summary>
        private void OnLevelSelected(int levelIndex)
        {
            Debug.Log($"=== OnLevelSelected called: Level {levelIndex + 1} ===");

            HideLevelSelect();

            if (gameManager != null)
            {
                Debug.Log($"Loading level {levelIndex}...");
                gameManager.LoadLevel(levelIndex);
            }
            else
            {
                Debug.LogError("GameManager is null!");
            }
        }
    }
}
