using UnityEngine;
using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// Snake spawn data
    /// </summary>
    [System.Serializable]
    public class SnakeSpawn
    {
        public Vector2Int position;
        public Direction direction;

        public SnakeSpawn(Vector2Int pos, Direction dir)
        {
            position = pos;
            direction = dir;
        }
    }

    /// <summary>
    /// Cloud/Fog group data
    /// First position is cloud, rest are fog
    /// </summary>
    [System.Serializable]
    public class CloudFogGroupData
    {
        public List<Vector2Int> positions;

        public CloudFogGroupData()
        {
            positions = new List<Vector2Int>();
        }
    }

    /// <summary>
    /// Level configuration data (from sample/config/mazeLevels.js)
    /// ScriptableObject for easy editing and serialization
    /// </summary>
    [CreateAssetMenu(fileName = "Level", menuName = "Maze/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Start & End Positions")]
        public Vector2Int playerStart = new Vector2Int(9, 11); // col, row
        public Vector2Int exitPos = new Vector2Int(9, 0);

        [Header("Entities")]
        public List<SnakeSpawn> snakes = new List<SnakeSpawn>();
        public List<Vector2Int> pushableStones = new List<Vector2Int>();
        public List<Vector2Int> fixedStones = new List<Vector2Int>();
        public List<Vector2Int> dynamite = new List<Vector2Int>();
        public List<Vector2Int> crackedStones = new List<Vector2Int>();
        public List<Vector2Int> boxes = new List<Vector2Int>();

        [Header("Cloud/Fog Groups")]
        [Tooltip("Each group: first position = cloud, rest = fog")]
        public List<CloudFogGroupData> cloudFogGroups = new List<CloudFogGroupData>();

        /// <summary>
        /// Validate level data
        /// </summary>
        public bool Validate()
        {
            // Check bounds
            if (!IsValidPosition(playerStart) || !IsValidPosition(exitPos))
            {
                Debug.LogError($"Level {name}: Invalid player start or exit position");
                return false;
            }

            // Check all entity positions
            foreach (var snake in snakes)
            {
                if (!IsValidPosition(snake.position))
                {
                    Debug.LogError($"Level {name}: Invalid snake position {snake.position}");
                    return false;
                }
            }

            return true;
        }

        private bool IsValidPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < GridConstants.Width &&
                   pos.y >= 0 && pos.y < GridConstants.Height;
        }
    }
}
