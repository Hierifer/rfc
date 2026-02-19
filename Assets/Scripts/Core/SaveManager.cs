using UnityEngine;
using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// Handles save/load of game progress using PlayerPrefs (from sample/game.js)
    /// </summary>
    public class SaveManager
    {
        private const string PROGRESS_KEY = "maze_game_progress";
        private const string COMPLETED_LEVELS_KEY = "maze_game_completed_levels";

        /// <summary>
        /// Save current level progress
        /// </summary>
        public void SaveProgress(int levelIndex)
        {
            if (levelIndex >= 0 && levelIndex < 20)
            {
                PlayerPrefs.SetInt(PROGRESS_KEY, levelIndex);
                PlayerPrefs.Save();
                Debug.Log($"Progress saved: Level {levelIndex + 1}");
            }
        }

        /// <summary>
        /// Load saved progress
        /// Returns 0 (first level) if no save exists
        /// </summary>
        public int LoadProgress()
        {
            int saved = PlayerPrefs.GetInt(PROGRESS_KEY, 0);

            // Validate range
            if (saved < 0 || saved >= 20)
            {
                saved = 0;
            }

            Debug.Log($"Progress loaded: Level {saved + 1}");
            return saved;
        }

        /// <summary>
        /// Save a completed level
        /// </summary>
        public void SaveCompletedLevel(int levelIndex)
        {
            HashSet<int> completed = GetCompletedLevels();

            if (!completed.Contains(levelIndex))
            {
                completed.Add(levelIndex);
                SaveCompletedLevels(completed);
                Debug.Log($"Level {levelIndex + 1} marked as completed");
            }
        }

        /// <summary>
        /// Get all completed levels
        /// </summary>
        public HashSet<int> GetCompletedLevels()
        {
            string saved = PlayerPrefs.GetString(COMPLETED_LEVELS_KEY, "");
            HashSet<int> completed = new HashSet<int>();

            if (!string.IsNullOrEmpty(saved))
            {
                try
                {
                    string[] parts = saved.Split(',');
                    foreach (string part in parts)
                    {
                        if (int.TryParse(part, out int level))
                        {
                            completed.Add(level);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse completed levels: {e.Message}");
                }
            }

            return completed;
        }

        /// <summary>
        /// Save completed levels set
        /// </summary>
        private void SaveCompletedLevels(HashSet<int> completed)
        {
            List<string> parts = new List<string>();

            foreach (int level in completed)
            {
                parts.Add(level.ToString());
            }

            string data = string.Join(",", parts);
            PlayerPrefs.SetString(COMPLETED_LEVELS_KEY, data);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Check if a level is completed
        /// </summary>
        public bool IsLevelCompleted(int levelIndex)
        {
            return GetCompletedLevels().Contains(levelIndex);
        }

        /// <summary>
        /// Clear all save data (for testing)
        /// </summary>
        public void ClearAllData()
        {
            PlayerPrefs.DeleteKey(PROGRESS_KEY);
            PlayerPrefs.DeleteKey(COMPLETED_LEVELS_KEY);
            PlayerPrefs.Save();
            Debug.Log("All save data cleared");
        }
    }
}
