using System.Reflection.Metadata.Ecma335;

namespace libs;

using System;
using System.Diagnostics;
using System.Dynamic;
using Newtonsoft.Json;

public static class FileHandler
{
    private static string path = "../Games/";
    private static int level = -1;
    private static bool saveLoaded = false;
    private static string[] files;
    private static readonly string envVar = "LEVELS_PATH";

    static FileHandler()
    {
        Initialize();
    }

    public static bool IsLevelsLeft()
    {
        return level < files.Length;
    }

    private static void Initialize()
    {
        if (Environment.GetEnvironmentVariable(envVar) != null)
        {
            path = Environment.GetEnvironmentVariable(envVar);
        }

        // Check if environment variable is set
        if (Directory.Exists(path + "Levels/"))
        {
            files = Directory.GetFiles(path + "Levels/");
            Array.Sort(files);
        }
        else
        {
            throw new DirectoryNotFoundException($"Directory not found at path: {path}");
        }
    }

    public static void SaveSelector()
    {
        string[] saveFiles = Directory
            .GetFiles(path + "/Saves/")
            .OrderBy(filePath => filePath)
            .ToArray();
        if (saveFiles.Length == 0)
        {
            return;
        }

        Console.Clear();
        Console.WriteLine("Would you like to load a game save?");
        for (int i = 0; i < saveFiles.Length; i++)
        {
            string fileName = Path.GetFileName(saveFiles[i]); // Extracting only the file name
            Console.WriteLine($"{i + 1}: {fileName}");
        }

        bool validInput = false;
        while (!validInput)
        {
            try
            {
                Console.Write(
                    "\nEnter the number of the save file you would like to load(1 / "
                        + saveFiles.Length
                        + ") or 0 to just play: "
                );
                int saveFileNumber = Convert.ToInt32(Console.ReadLine());
                if (saveFileNumber == 0)
                {
                    validInput = true;
                    Console.Clear();
                    return;
                }
                if (saveFileNumber > 0 && saveFileNumber <= saveFiles.Length)
                {
                    validInput = true;
                    string saveFile = saveFiles[saveFileNumber - 1];
                    dynamic saveFileContent = ReadJson(saveFile);
                    level = saveFileContent.levelNumber;
                    files[level] = saveFiles[saveFileNumber - 1];
                    saveLoaded = true;
                    Console.Clear();
                }
                else
                {
                    Console.WriteLine(
                        "Invalid input. Please enter a number between 1 and " + saveFiles.Length
                    );
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    public static bool LoadNextLevel()
    {
        if (saveLoaded)
        {
            saveLoaded = false;
            return true;
        }
        level++;
        return IsLevelsLeft();
    }

    public static dynamic ReadJson()
    {
        string levelPath = files[level];
        return ReadJson(levelPath);
    }

    public static dynamic ReadJson(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new InvalidOperationException(
                "JSON file path not provided in environment variable"
            );
        }

        try
        {
            string jsonContent = File.ReadAllText(path);
            dynamic jsonData = JsonConvert.DeserializeObject(jsonContent);
            return jsonData;
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"JSON file not found at path: {path}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading JSON file: {ex.Message}");
        }
    }

    public static void WriteJson(object data, string filePath)
    {
        try
        {
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, jsonString);
            Console.WriteLine($"JSON data written to file: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing JSON data to file: {ex.Message}");
        }
    }

    /**
        * Save the current game state to a JSON file
        * @param gameObjects List of GameObjects
        * @param map Map object
        * @return string File path of the saved game state
        */
    public static string saveGameState(List<GameObject> gameObjects, Map map, int timeLeft)
    {
        // get the number of current save files
        int saveFiles = Directory.GetFiles(path + "/Saves/").Length;
        string saveFilesString = (saveFiles + 1).ToString("D3");
        string levelNumberString = (level + 1).ToString("D3");

        string fileName = "Save_" + saveFilesString + "_Level_" + levelNumberString + ".json";

        // Construct the file path with leading zeros in the saveFiles part
        string filePath = Path.Combine(path, "Saves", fileName);

        //dynamic saveFileContent = ReadJson();

        dynamic jsonContent = new ExpandoObject();

        dynamic jsonMap = new ExpandoObject();
        jsonMap.height = map.MapHeight;
        jsonMap.width = map.MapWidth;
        jsonContent.levelNumber = level;
        jsonContent.levelName = map.LevelName;
        jsonContent.map = jsonMap;
        jsonContent.time = timeLeft;

        List<dynamic> jsonGameObjects = new List<dynamic>();
        foreach (var gameObject in gameObjects)
        {
            dynamic jsonGameObject = new ExpandoObject();
            jsonGameObject.Type = Enum.GetName(typeof(GameObjectType), gameObject.Type);
            jsonGameObject.Color = gameObject.Color;
            jsonGameObject.PosX = gameObject.PosX;
            jsonGameObject.PosY = gameObject.PosY;

            if (gameObject.Type == GameObjectType.Obstacle)
            {
                jsonGameObject.TimeEffect = ((Obstacle)gameObject).TimeEffect;
                jsonGameObject.Message = ((Obstacle)gameObject).Message;
            }

            jsonGameObjects.Add(jsonGameObject);
        }
        jsonContent.gameObjects = jsonGameObjects;

        WriteJson(jsonContent, filePath);
        return fileName;
    }
}
