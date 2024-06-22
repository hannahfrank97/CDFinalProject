﻿using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Nodes;

namespace libs;

using System.Security.Cryptography;
using Newtonsoft.Json;

public sealed class GameEngine
{
    private static GameEngine? _instance;
    private IGameObjectFactory gameObjectFactory;
    private int missingGoals = 0;
    private string levelName = "";
    private string levelSaved = "";
    private bool keyCollected = false;

    private bool DoorUnlocked = false;

    private string currentMessage = "";

    private DateTime startTime = DateTime.Now;
    private int levelTimeSeconds = 0;

    public bool IsGameWon()
    {
        //return missingGoals == 0;
        return DoorUnlocked == true;
    }

    public static GameEngine Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameEngine();
            }
            return _instance;
        }
    }

    private GameEngine()
    {
        //INIT PROPS HERE IF NEEDED
        gameObjectFactory = new GameObjectFactory();

        //Added for proper display of game characters
        Console.OutputEncoding = System.Text.Encoding.UTF8; // Ensure console encoding is set to UTF-8
    }

    private GameObject? _focusedObject;

    private Map map = new Map();

    private List<GameObject> gameObjects = new List<GameObject>();

    public Map GetMap()
    {
        return map;
    }

    public GameObject GetFocusedObject()
    {
        return _focusedObject;
    }

    public void CheckForSaveFiles()
    {
        FileHandler.SaveSelector();
    }

    public bool LoadNextLevel()
    {
        bool levelLeft = FileHandler.LoadNextLevel();
        if (levelLeft)
            Setup();
        return levelLeft;
    }

    public void Setup()
    {
        // reset previous things:
        gameObjects.Clear();
        this.keyCollected = false;
        this.DoorUnlocked = false;

        dynamic gameData = FileHandler.ReadJson();

        map.MapWidth = gameData.map.width;
        map.MapHeight = gameData.map.height;

        map.Reset();

        levelName = gameData.levelName;
        map.LevelName = levelName;
        levelTimeSeconds = gameData.time;
        missingGoals = 0;

        foreach (var gameObject in gameData.gameObjects)
        {
            AddGameObject(CreateGameObject(gameObject));
        }

        _focusedObject = gameObjects.OfType<PlayerSingelton>().First();
        startTime = DateTime.Now;
    }

    public TimeSpan timeLeft()
    {
        return startTime.AddSeconds(levelTimeSeconds).Subtract(DateTime.Now);
    }

    public bool IsGameLost()
    {
        return timeLeft().TotalSeconds <= 0;
    }

    public void SaveToFile()
    {
        levelSaved = FileHandler.saveGameState(
            gameObjects,
            map,
            Convert.ToInt32(timeLeft().TotalSeconds)
        );
    }

    public void Render()
    {
        //Clean the map
        Console.Clear();

        PlaceGameObjects();

        // Write how much time is left:
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Time: " + timeLeft().ToString("mm\\:ss"));

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(levelName);
        //Render the map
        for (int i = 0; i < map.MapHeight; i++)
        {
            for (int j = 0; j < map.MapWidth; j++)
            {
                DrawObject(map.Get(i, j));
            }
            Console.WriteLine();
        }

        if (keyCollected)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Key collected");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Key not collected");
        }

        // if (missingGoals > 0)
        //     Console.WriteLine(missingGoals + " goal" + (missingGoals == 1 ? "s" : "") + " Missing");
        // else
        //     Console.WriteLine("All goals are filled");

        if (levelSaved != "")
        {
            Console.WriteLine("Level has beeb saved under " + levelSaved);

            levelSaved = "";
        }

        DisplayMessage();
    }

    public void CheckCollision()
    {
        // NEW COLLITION CHECK

        GameObject player = _focusedObject;
        GameObject obstacle = map.Get(player.PosY, player.PosX);
        // move is allowed
        if (obstacle == null || obstacle.Type == GameObjectType.Floor)
        {
            map.Save();
            return;
        }

        if (obstacle.Type == GameObjectType.Goal)
        {
            if (!keyCollected)
            {
                _focusedObject.UndoMove();
            }
            else
            {
                this.DoorUnlocked = true;
            }
        }

        // Handle collision with a key
        if (obstacle.Type == GameObjectType.Key)
        {
            this.keyCollected = true;
            obstacle.Color = ConsoleColor.Cyan;
        }

        if (obstacle.Type == GameObjectType.Wall)
        {
            _focusedObject.UndoMove();
            return;
        }
        // Handle collision with an Obstacle (eg.: old toast)
        else if (obstacle.Type == GameObjectType.Obstacle)
        {
            // Handle collision with an Obstacle
            HandleObstacleCollision(player, (Obstacle)obstacle);
        }
        else if (obstacle.Type == GameObjectType.Box)
        {
            int boxY = obstacle.PosY + player.getDy();
            int boxX = obstacle.PosX + player.getDx();
            GameObject obstacleObstacle = map.Get(boxY, boxX);

            if (
                obstacleObstacle.Type == GameObjectType.Wall
                || obstacleObstacle.Type == GameObjectType.Box
            )
            {
                // do not move the player
                _focusedObject.UndoMove();
                return;
            }
            else
            {
                // move the box
                obstacle.PosX = boxX;
                obstacle.PosY = boxY;

                // this also works if we move box from target to target, because we are smart ppl
                if (obstacle.Color == ConsoleColor.Green)
                {
                    obstacle.Color = ConsoleColor.Yellow;
                    missingGoals++;
                }
                if (map.Get(boxY, boxX).Type == GameObjectType.Goal)
                {
                    missingGoals--;
                    obstacle.Color = ConsoleColor.Green;
                }
            }
        }
        map.Save();
    }

    private void HandleObstacleCollision(GameObject player, Obstacle obstacle)
    {
        // Display the message associated with the obstacle
        currentMessage = obstacle.Message;

        // Optionally, handle time effects or other game state changes
        levelTimeSeconds += obstacle.TimeEffect;

        // Prevent movement into the obstacle if necessary
        player.UndoMove();

        //Obstacle becomes floor after collision
        obstacle.Type = GameObjectType.Floor;
        obstacle.CharRepresentationString = " ";

        //Refreshing the spot on the map where the obstacle was
        map.Set(obstacle);

        //Remove the obstacle from the gameObjects list
        gameObjects.Remove(obstacle);

        //Clearing the message after displaying it
        //currentMessage = "";
    }

    private void DisplayMessage()
    {
        if (!string.IsNullOrEmpty(currentMessage))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(currentMessage);
            Console.ResetColor();
            currentMessage = ""; // Reset after displaying to prevent repeat display
        }
    }

    public void Undo()
    {
        map.Undo();

        GameObject?[,] gameObjectLayer = map.GetGameObjectLayer();
        if (gameObjectLayer == null)
            return;

        // iterate through all objects and update their position
        for (int y = 0; y < gameObjectLayer.GetLength(0); y++)
        for (int x = 0; x < gameObjectLayer.GetLength(1); x++)
            if (gameObjectLayer[y, x] != null)
                if (gameObjectLayer[y, x].Type == GameObjectType.Box)
                {
                    gameObjectLayer[y, x].PosX = x;
                    gameObjectLayer[y, x].PosY = y;
                    gameObjectLayer[y, x].setColor(ConsoleColor.Yellow);
                }
                else if (gameObjectLayer[y, x].Type == GameObjectType.Player)
                {
                    _focusedObject.PosX = x;
                    _focusedObject.PosY = y;
                }

        // Update the missing boxes
        List<Goal> goals = gameObjects.OfType<Goal>().ToList();
        missingGoals = goals.Count;

        foreach (var goal in goals)
            if (gameObjectLayer[goal.PosY, goal.PosX].Type == GameObjectType.Box)
            {
                gameObjectLayer[goal.PosY, goal.PosX].setColor(ConsoleColor.Green);
                missingGoals--;
            }

        // Update the focused object color to show that move was undone
        _focusedObject.setColor(ConsoleColor.Red);
    }

    // Method to create GameObject using the factory from clients
    public GameObject CreateGameObject(dynamic obj)
    {
        return gameObjectFactory.CreateGameObject(obj);
    }

    public void AddGameObject(GameObject gameObject)
    {
        if (gameObject.Type == GameObjectType.Goal)
            missingGoals++;
        // remove a missing goal if a box is placed on it
        else if (gameObject.Type == GameObjectType.Box && gameObject.Color == ConsoleColor.Green)
            missingGoals--;

        gameObjects.Add(gameObject);
    }

    private void PlaceGameObjects()
    {
        map.Set(_focusedObject);
        gameObjects.ForEach(
            delegate(GameObject obj)
            {
                if (obj != _focusedObject)
                    map.Set(obj);
            }
        );
    }

    private void DrawObject(GameObject gameObject)
    {
        Console.ResetColor();

        if (gameObject != null)
        {
            if (gameObject.Type == GameObjectType.Key && keyCollected)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(' ');
                return;
            }
            Console.ForegroundColor = gameObject.Color;
            Console.Write(gameObject.CharRepresentation);
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(' ');
        }
    }
}
