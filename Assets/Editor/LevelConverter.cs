using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using MazeGame;
using MiniJSON;

/// <summary>
/// JSON data structures for level parsing
/// </summary>
[System.Serializable]
public class JsonPosition
{
    public int row;
    public int col;
}

[System.Serializable]
public class JsonSnake
{
    public int row;
    public int col;
    public string direction;
}

[System.Serializable]
public class JsonLevel
{
    public JsonPosition player;
    public JsonPosition exit;
    public List<JsonSnake> snakes;
    public List<JsonPosition> pushableStones;
    public List<JsonPosition> fixedStones;
    public List<JsonPosition> dynamite;
    public List<JsonPosition> crackedStones;
    public List<JsonPosition> boxes;
    public List<List<JsonPosition>> cloudFogGroups;
}

[System.Serializable]
public class JsonLevelsContainer
{
    
}

/// <summary>
/// Editor tool to convert JSON level data to Unity ScriptableObjects
/// </summary>
public class LevelConverter : EditorWindow
{
    private string jsonFilePath = "Assets/Resources/mazeLevels.json";

    [MenuItem("Tools/Maze/Convert Levels")]
    public static void ShowWindow()
    {
        GetWindow<LevelConverter>("Level Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Maze Level Converter", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        jsonFilePath = EditorGUILayout.TextField("JSON File Path:", jsonFilePath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Convert All Levels", GUILayout.Height(30)))
        {
            ConvertLevels();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Test Level"))
        {
            CreateTestLevel();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "JSON Format:\n" +
            "{\n" +
            "  \"levels\": [\n" +
            "    {\n" +
            "      \"player\": { \"row\": 11, \"col\": 9 },\n" +
            "      \"exit\": { \"row\": 0, \"col\": 9 },\n" +
            "      \"snakes\": [...],\n" +
            "      \"pushableStones\": [...],\n" +
            "      \"fixedStones\": [...],\n" +
            "      \"dynamite\": [...],\n" +
            "      \"crackedStones\": [...],\n" +
            "      \"boxes\": [...],\n" +
            "      \"cloudFogGroups\": [[...]]\n" +
            "    }\n" +
            "  ]\n" +
            "}",
            MessageType.Info
        );
    }

    private void ConvertLevels()
    {
        // Handle both absolute and relative paths
        string fullPath;
        if (jsonFilePath.StartsWith("Assets/"))
        {
            fullPath = Path.Combine(Application.dataPath, jsonFilePath.Substring("Assets/".Length));
        }
        else
        {
            fullPath = Path.Combine(Application.dataPath, "..", jsonFilePath);
        }

        if (!File.Exists(fullPath))
        {
            EditorUtility.DisplayDialog("Error", $"File not found: {fullPath}\n\nPlease create a JSON file with level data.", "OK");
            return;
        }

        string jsonContent = File.ReadAllText(fullPath);

        try
        {
            
            var dict = MiniJSON.Json.Deserialize(jsonContent) as Dictionary<string, object>;
            
            if (dict == null || dict.Count == 0)
            {
                 EditorUtility.DisplayDialog("Error", "No levels found in JSON file or failed to parse.", "OK");
                 return;
            }

            // Iterate through dictionary keys
            int successCount = 0;
            // Sort keys by level number
            var sortedKeys = new List<string>(dict.Keys);
            sortedKeys.Sort((a, b) => {
                int nA = int.Parse(a.Replace("level", ""));
                int nB = int.Parse(b.Replace("level", ""));
                return nA.CompareTo(nB);
            });

            foreach (var key in sortedKeys)
            {
                if (!key.StartsWith("level")) continue;

                int levelNum = int.Parse(key.Replace("level", ""));
                
                // Convert the object back to JSON for JsonUtility to parse into strong type
                string levelJson = MiniJSON.Json.Serialize(dict[key]);
                JsonLevel jsonLevel = JsonUtility.FromJson<JsonLevel>(levelJson);
                
                CreateLevelAsset(levelNum, jsonLevel);
                successCount++;
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Converted {successCount} levels successfully!", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to parse JSON:\n{e.Message}\n{e.StackTrace}", "OK");
            Debug.LogError($"JSON Parse Error: {e}");
        }
    }

    private void CreateLevelAsset(int levelNumber, JsonLevel jsonLevel)
    {
        LevelData level = ScriptableObject.CreateInstance<LevelData>();

        // Initialize all lists
        level.snakes = new List<SnakeSpawn>();
        level.pushableStones = new List<Vector2Int>();
        level.fixedStones = new List<Vector2Int>();
        level.dynamite = new List<Vector2Int>();
        level.crackedStones = new List<Vector2Int>();
        level.boxes = new List<Vector2Int>();
        level.cloudFogGroups = new List<CloudFogGroupData>();

        // Parse player and exit (required fields)
        if (jsonLevel.player != null)
        {
            level.playerStart = new Vector2Int(jsonLevel.player.col, jsonLevel.player.row);
        }
        else
        {
            Debug.LogWarning($"Level {levelNumber}: Missing player position, using default");
            level.playerStart = new Vector2Int(9, 11);
        }

        if (jsonLevel.exit != null)
        {
            level.exitPos = new Vector2Int(jsonLevel.exit.col, jsonLevel.exit.row);
        }
        else
        {
            Debug.LogWarning($"Level {levelNumber}: Missing exit position, using default");
            level.exitPos = new Vector2Int(9, 0);
        }

        // Parse snakes
        if (jsonLevel.snakes != null)
        {
            foreach (var snake in jsonLevel.snakes)
            {
                Direction dir = snake.direction switch
                {
                    "up" => Direction.Up,
                    "down" => Direction.Down,
                    "left" => Direction.Left,
                    "right" => Direction.Right,
                    _ => Direction.Right
                };
                level.snakes.Add(new SnakeSpawn(new Vector2Int(snake.col, snake.row), dir));
            }
        }

        // Parse pushable stones
        if (jsonLevel.pushableStones != null)
        {
            foreach (var pos in jsonLevel.pushableStones)
            {
                level.pushableStones.Add(new Vector2Int(pos.col, pos.row));
            }
        }

        // Parse fixed stones
        if (jsonLevel.fixedStones != null)
        {
            foreach (var pos in jsonLevel.fixedStones)
            {
                level.fixedStones.Add(new Vector2Int(pos.col, pos.row));
            }
        }

        // Parse dynamite
        if (jsonLevel.dynamite != null)
        {
            foreach (var pos in jsonLevel.dynamite)
            {
                level.dynamite.Add(new Vector2Int(pos.col, pos.row));
            }
        }

        // Parse cracked stones
        if (jsonLevel.crackedStones != null)
        {
            foreach (var pos in jsonLevel.crackedStones)
            {
                level.crackedStones.Add(new Vector2Int(pos.col, pos.row));
            }
        }

        // Parse boxes
        if (jsonLevel.boxes != null)
        {
            foreach (var pos in jsonLevel.boxes)
            {
                level.boxes.Add(new Vector2Int(pos.col, pos.row));
            }
        }

        // Parse cloud/fog groups
        if (jsonLevel.cloudFogGroups != null)
        {
            foreach (var group in jsonLevel.cloudFogGroups)
            {
                if (group == null || group.Count == 0)
                    continue;

                CloudFogGroupData groupData = new CloudFogGroupData();
                groupData.positions = new List<Vector2Int>();

                foreach (var pos in group)
                {
                    groupData.positions.Add(new Vector2Int(pos.col, pos.row));
                }

                level.cloudFogGroups.Add(groupData);
            }
        }

        // Ensure Resources/Levels directory exists
        string directory = "Assets/Resources/Levels";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }

        // Create or overwrite asset
        string assetPath = $"{directory}/Level_{levelNumber:D2}.asset";

        // Check if asset already exists
        LevelData existingAsset = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
        if (existingAsset != null)
        {
            // Update existing asset
            EditorUtility.CopySerialized(level, existingAsset);
            EditorUtility.SetDirty(existingAsset);
            Debug.Log($"Updated level {levelNumber}: player={level.playerStart}, exit={level.exitPos}, snakes={level.snakes.Count}, stones={level.pushableStones.Count + level.fixedStones.Count}");
        }
        else
        {
            // Create new asset
            AssetDatabase.CreateAsset(level, assetPath);
            Debug.Log($"Created level {levelNumber}: player={level.playerStart}, exit={level.exitPos}, snakes={level.snakes.Count}, stones={level.pushableStones.Count + level.fixedStones.Count}");
        }
    }

    private void CreateTestLevel()
    {
        LevelData level = ScriptableObject.CreateInstance<LevelData>();

        // Initialize all lists
        level.snakes = new List<SnakeSpawn>();
        level.pushableStones = new List<Vector2Int>();
        level.fixedStones = new List<Vector2Int>();
        level.dynamite = new List<Vector2Int>();
        level.crackedStones = new List<Vector2Int>();
        level.boxes = new List<Vector2Int>();
        level.cloudFogGroups = new List<CloudFogGroupData>();

        // Set basic level data
        level.playerStart = new Vector2Int(9, 11);
        level.exitPos = new Vector2Int(9, 0);

        // Add a simple test configuration
        level.fixedStones.Add(new Vector2Int(8, 5));
        level.fixedStones.Add(new Vector2Int(10, 5));
        level.pushableStones.Add(new Vector2Int(9, 6));

        // Ensure Resources/Levels directory exists
        string directory = "Assets/Resources/Levels";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh();
        }

        string assetPath = $"{directory}/Level_Test.asset";
        AssetDatabase.CreateAsset(level, assetPath);
        AssetDatabase.Refresh();

        Debug.Log($"Created test level: {assetPath}");
        EditorUtility.DisplayDialog("Success", "Test level created at:\n" + assetPath, "OK");
    }
}
