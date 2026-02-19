using UnityEngine;
using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// Main game controller - singleton pattern (from sample/game.js)
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private MazeRenderer mazeRenderer;
        [SerializeField] private UIManager uiManager;

        // Game systems
        private GameState gameState;
        private LevelManager levelManager;
        private PlayerController playerController;
        private SnakeController snakeController;
        private AutoMoveController autoMoveController;
        private SaveManager saveManager;

        // Game timing
        private float gameStartTime;
        private float levelTime;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSystems();
        }

        private void Start()
        {
            LoadAndStartGame();
        }

        private void Update()
        {
            if (gameState == null) return;

            // Update auto-move controller
            autoMoveController?.Update(Time.deltaTime);

            // Update snake movement
            snakeController?.Update(Time.deltaTime);

            // Clean up completed animations
            gameState.CleanupAnimations();

            // Render current state
            mazeRenderer?.Render();
        }

        /// <summary>
        /// Initialize all game systems
        /// </summary>
        private void InitializeSystems()
        {
            gameState = new GameState();
            levelManager = new LevelManager();
            saveManager = new SaveManager();

            playerController = new PlayerController(gameState);
            snakeController = new SnakeController(gameState);
            autoMoveController = new AutoMoveController(gameState, playerController, snakeController);

            // Set up callbacks
            playerController.SetCallbacks(
                OnLevelWin,
                OnLevelLose,
                OnDynamiteChanged
            );

            snakeController.SetGameOverCallback(OnLevelLose);

            Debug.Log("Game systems initialized");
        }

        /// <summary>
        /// Load levels and start the game
        /// </summary>
        private void LoadAndStartGame()
        {
            // Load all level assets
            levelManager.LoadAllLevels();

            // Load saved progress
            int savedLevel = saveManager.LoadProgress();
            levelManager.LoadLevel(savedLevel);

            // Initialize renderer
            if (mazeRenderer != null)
            {
                mazeRenderer.Initialize(gameState);
            }

            // Initialize UI
            if (uiManager != null)
            {
                uiManager.Initialize(levelManager.CurrentLevel, levelManager.TotalLevels, this, saveManager);
            }

            // Start first level
            InitializeCurrentLevel();
        }

        /// <summary>
        /// Initialize the current level
        /// </summary>
        private void InitializeCurrentLevel()
        {
            LevelData levelData = levelManager.GetCurrentLevel();

            if (levelData == null)
            {
                Debug.LogError("Failed to load level data!");
                return;
            }

            // Initialize game state from level
            gameState.InitializeFromLevel(levelData);

            // Reset timing
            gameStartTime = Time.time;

            // Update UI
            if (uiManager != null)
            {
                uiManager.UpdateLevel(levelManager.CurrentLevel);
                uiManager.UpdateDynamite(gameState.dynamiteCount);
            }

            // Save progress
            saveManager.SaveProgress(levelManager.CurrentLevel);

            Debug.Log($"Level {levelManager.CurrentLevel + 1}/{levelManager.TotalLevels} initialized");
        }

        /// <summary>
        /// Handle player input (called from InputManager or UI)
        /// </summary>
        public void HandlePlayerMove(Direction direction)
        {
            if (gameState == null || playerController == null)
                return;

            bool moved = playerController.MovePlayer(direction);

            if (moved)
            {
                // Trigger snake movement after player moves
                snakeController.StartSnakeMovement();
            }
        }

        /// <summary>
        /// Reset current level
        /// </summary>
        public void ResetLevel()
        {
            InitializeCurrentLevel();
            Debug.Log("Level reset");
        }

        /// <summary>
        /// Go to next level
        /// </summary>
        public void NextLevel()
        {
            if (levelManager.NextLevel())
            {
                InitializeCurrentLevel();
            }
            else
            {
                Debug.Log("All levels completed!");
                OnGameComplete();
            }
        }

        /// <summary>
        /// Load specific level
        /// </summary>
        public void LoadLevel(int levelIndex)
        {
            levelManager.LoadLevel(levelIndex);
            InitializeCurrentLevel();
        }

        /// <summary>
        /// Callback when level is won
        /// </summary>
        private void OnLevelWin()
        {
            levelTime = Time.time - gameStartTime;

            // Save completed level
            saveManager.SaveCompletedLevel(levelManager.CurrentLevel);

            // Unlock next level
            if (levelManager.HasNextLevel())
            {
                saveManager.SaveProgress(levelManager.CurrentLevel + 1);
            }

            Debug.Log($"Level {levelManager.CurrentLevel + 1} completed in {FormatTime(levelTime)}!");

            // Auto-advance to next level after a short delay
            StartCoroutine(AutoAdvanceToNextLevel());
        }

        /// <summary>
        /// Coroutine to automatically advance to the next level after a delay
        /// </summary>
        private System.Collections.IEnumerator AutoAdvanceToNextLevel()
        {
            // Wait for 1.5 seconds to show the win state
            yield return new WaitForSeconds(1.5f);

            // Load next level
            NextLevel();
        }

        /// <summary>
        /// Callback when level is lost
        /// </summary>
        private void OnLevelLose()
        {
            Debug.Log("Game Over! Snake caught you!");

            // TODO: Show lose dialog
        }

        /// <summary>
        /// Callback when dynamite count changes
        /// </summary>
        private void OnDynamiteChanged(int count)
        {
            Debug.Log($"Dynamite count: {count}");

            // Update UI
            if (uiManager != null)
            {
                uiManager.UpdateDynamite(count);
            }
        }

        /// <summary>
        /// Callback when all levels are completed
        /// </summary>
        private void OnGameComplete()
        {
            Debug.Log("Congratulations! All levels completed!");

            // TODO: Show completion dialog
        }

        /// <summary>
        /// Format time in MM:SS format
        /// </summary>
        private string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            return $"{minutes}:{seconds:D2}";
        }

        /// <summary>
        /// Get current game state (for debugging/UI)
        /// </summary>
        public GameState GetGameState()
        {
            return gameState;
        }

        /// <summary>
        /// Get level manager (for UI)
        /// </summary>
        public LevelManager GetLevelManager()
        {
            return levelManager;
        }

        /// <summary>
        /// Start auto-move along a path (called from InputManager)
        /// </summary>
        public void StartAutoMove(List<Vector2Int> path)
        {
            autoMoveController?.StartAutoMove(path);
        }

        /// <summary>
        /// Stop auto-move (called from InputManager or when player manually moves)
        /// </summary>
        public void StopAutoMove()
        {
            autoMoveController?.StopAutoMove();
        }

        private void OnDestroy()
        {
            if (mazeRenderer != null)
            {
                mazeRenderer.Clear();
            }
        }
    }
}
