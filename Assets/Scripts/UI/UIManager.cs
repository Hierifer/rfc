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
                Debug.Log("Setting up level select button listener");
                levelSelectButton.onClick.RemoveAllListeners(); // Clear existing listeners
                levelSelectButton.onClick.AddListener(ShowLevelSelect);
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
        /// Show level select panel
        /// </summary>
        public void ShowLevelSelect()
        {
            Debug.Log("ShowLevelSelect called");

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

            // Clear existing buttons
            foreach (Transform child in levelButtonContainer)
            {
                Destroy(child.gameObject);
            }

            // Get completed levels
            HashSet<int> completedLevels = saveManager != null ? saveManager.GetCompletedLevels() : new HashSet<int>();

            // Create level buttons
            for (int i = 0; i < totalLevels; i++)
            {
                int levelIndex = i; // Capture for closure
                GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer);
                buttonObj.SetActive(true); // Ensure button is active

                // Setup button text
                Text buttonText = buttonObj.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    bool isCompleted = completedLevels.Contains(levelIndex);
                    buttonText.text = isCompleted ? $"✓ {levelIndex + 1}" : $"{levelIndex + 1}";
                }
                else
                {
                    Debug.LogWarning($"Button {i} has no Text component");
                }

                // Setup button click
                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => OnLevelSelected(levelIndex));
                }
                else
                {
                    Debug.LogWarning($"Button {i} has no Button component");
                }
            }

            Debug.Log($"Created {totalLevels} level buttons");

            // Show panel
            levelSelectPanel.SetActive(true);
        }

        /// <summary>
        /// Hide level select panel
        /// </summary>
        public void HideLevelSelect()
        {
            if (levelSelectPanel != null)
            {
                levelSelectPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Handle level selection
        /// </summary>
        private void OnLevelSelected(int levelIndex)
        {
            HideLevelSelect();

            if (gameManager != null)
            {
                gameManager.LoadLevel(levelIndex);
            }
        }
    }
}
