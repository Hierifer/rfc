using UnityEngine;
using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// Manages level loading and progression
    /// </summary>
    public class LevelManager
    {
        private const int TOTAL_LEVELS = 20;
        private List<LevelData> levels;
        private int currentLevelIndex;

        public int CurrentLevel => currentLevelIndex;
        public int TotalLevels => TOTAL_LEVELS;

        public LevelManager()
        {
            levels = new List<LevelData>();
            currentLevelIndex = 0;
        }

        /// <summary>
        /// Load all level assets from Resources
        /// </summary>
        public void LoadAllLevels()
        {
            levels.Clear();

            for (int i = 1; i <= TOTAL_LEVELS; i++)
            {
                string levelName = $"Levels/Level_{i:D2}";
                LevelData levelData = Resources.Load<LevelData>(levelName);

                if (levelData != null)
                {
                    if (levelData.Validate())
                    {
                        levels.Add(levelData);
                    }
                    else
                    {
                        Debug.LogError($"Level {i} failed validation!");
                    }
                }
                else
                {
                    Debug.LogWarning($"Level {levelName} not found in Resources!");
                }
            }

            Debug.Log($"Loaded {levels.Count}/{TOTAL_LEVELS} levels");
        }

        /// <summary>
        /// Get level data by index
        /// </summary>
        public LevelData GetLevel(int index)
        {
            if (index < 0 || index >= levels.Count)
            {
                Debug.LogError($"Invalid level index: {index}");
                return null;
            }

            return levels[index];
        }

        /// <summary>
        /// Get current level data
        /// </summary>
        public LevelData GetCurrentLevel()
        {
            return GetLevel(currentLevelIndex);
        }

        /// <summary>
        /// Load specific level by index
        /// </summary>
        public void LoadLevel(int index)
        {
            if (index < 0 || index >= levels.Count)
            {
                Debug.LogError($"Cannot load level {index}: out of range");
                return;
            }

            currentLevelIndex = index;
            Debug.Log($"Loaded level {index + 1}/{TOTAL_LEVELS}");
        }

        /// <summary>
        /// Go to next level
        /// </summary>
        public bool NextLevel()
        {
            if (currentLevelIndex < levels.Count - 1)
            {
                currentLevelIndex++;
                return true;
            }

            return false; // No more levels
        }

        /// <summary>
        /// Reset to first level
        /// </summary>
        public void ResetToFirstLevel()
        {
            currentLevelIndex = 0;
        }

        /// <summary>
        /// Check if there's a next level
        /// </summary>
        public bool HasNextLevel()
        {
            return currentLevelIndex < levels.Count - 1;
        }
    }
}
