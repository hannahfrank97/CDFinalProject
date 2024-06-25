using libs.Dialogue;
using Newtonsoft.Json;

namespace libs
{
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
        private int levelTimeSeconds = 0;
        private System.Timers.Timer _timer;
        private int _remainingTime;
        private PlayerSingelton _player;
        private Map map = new Map();
        private List<GameObject> gameObjects = new List<GameObject>();
        private object _lock = new object();
        private object _moveLock = new object(); // Separate lock for movement
        private bool isMoving = false; // Flag to check if a move is in progress
        private DialogueLevel dialogueLevel;

        public bool gameEnd = false;
        public bool IsGameWon()
        {
            return DoorUnlocked == true;
        }

        public bool IsGameLost()
        {
            return _remainingTime <= 0;
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
            // Initialize properties here if needed
            gameObjectFactory = new GameObjectFactory();

            // Added for proper display of game characters
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            _timer = new System.Timers.Timer(1000); // Initialize the timer with 1 second interval
            _timer.Elapsed += OnTimedEvent;
            _player = PlayerSingelton.Instance; // Ensure only one instance of player
        }

        public PlayerSingelton GetFocusedObject()
        {
            return _player;
        }

        public void StartTimer(int initialTime)
        {
            _remainingTime = initialTime;
            _timer.Start();
        }

        private void OnTimedEvent(object? source, System.Timers.ElapsedEventArgs e)
        {
            lock (_lock)
            {
                _remainingTime--;
                if (_remainingTime <= 0)
                {
                    _timer.Stop();
                    // Handle game over logic
                }
                else
                {
                    UpdateTimerDisplay();
                }
            }
        }

        public void UpdateTimerDisplay()
        {
            lock (_lock)
            {
                TimeSpan timeLeft = TimeSpan.FromSeconds(_remainingTime);
                Console.SetCursorPosition(0, 0); // Adjust the position as needed
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Time: " + timeLeft.ToString("mm\\:ss") + "        "); // Extra spaces to clear any previous longer time string
            }
        }

        private void AdjustTime(int timeEffect)
        {
            lock (_lock)
            {
                _remainingTime += timeEffect;
                if (_remainingTime < 0)
                {
                    _remainingTime = 0; // Prevent negative time
                    _timer.Stop();
                    // Handle game over logic if needed
                }
                UpdateTimerDisplay();
            }
        }

        public void RenderGame()
        {
            lock (_lock)
            {
                // Clear the console before rendering the game
                Console.Clear();

                UpdateTimerDisplay(); // Keep the timer updated at the top

                Console.SetCursorPosition(0, 1); // Adjust the position to start rendering below the timer

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(levelName);

                PlaceGameObjects();

                // Render the map
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
                    if(levelName == "Find your phone") Console.WriteLine("Phone collected");
                    else Console.WriteLine("Beer collected");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if(levelName == "Find your phone") Console.WriteLine("Phone not collected");
                    else Console.WriteLine("Beer not collected");
                }

                if (levelSaved != "")
                {
                    Console.WriteLine("Level has been saved under " + levelSaved);
                    levelSaved = "";
                }

                DisplayMessage();
            }
        }

        public void Setup()
        {
            lock (_lock)
            {
                // Reset previous things:
                gameObjects.Clear();
                this.keyCollected = false;
                this.DoorUnlocked = false;

                dynamic gameData = FileHandler.ReadJson();

                // Check if the level is a dialogue level
                if (gameData.levelType == "dialogue")
                {
                    dialogueLevel = new DialogueLevel(JsonConvert.SerializeObject(gameData));
                    dialogueLevel.Run();

                    // Debug output to check level completion
                    Console.WriteLine($"Is Dialogue Level Complete: {dialogueLevel.IsLevelComplete}");

                    // Check if the dialogue level is complete and load the next level if it is
                    if (dialogueLevel.IsLevelComplete)
                    {
                        Console.WriteLine("Loading next level...");
                        LoadNextLevel();
                        return;
                    }
                    
                }

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

                _player = PlayerSingelton.Instance;
                if (!gameObjects.Contains(_player))
                {
                    AddGameObject(_player);
                }
                StartTimer(levelTimeSeconds); // Start the timer when setting up the level
            }
        }

        public void SaveToFile()
        {
            lock (_lock)
            {
                levelSaved = FileHandler.saveGameState(
                    gameObjects,
                    map,
                    Convert.ToInt32(_remainingTime)
                );
            }
        }

        public void CheckForSaveFiles()
        {
            // Implementation for checking for save files
            FileHandler.SaveSelector();
        }

        public bool LoadNextLevel()
        {
            lock (_lock)
            {
                // Implementation for loading the next level
                bool levelLeft = FileHandler.LoadNextLevel();
                if (levelLeft)
                {
                    Setup();
                }
                else{
                    GameOver();
                }
                return levelLeft;
            }
        }

        public int GetLevelInitialTime()
        {
            return levelTimeSeconds;
        }

        public int GetRemainingTime()
        {
            return _remainingTime;
        }

        public void GameOver()
        {
            lock (_lock)
            {
                _timer.Stop();
                gameEnd = true;
            }
        }

        public void CheckCollision()
        {
            lock (_moveLock) // Use a separate lock for movement
            {
                if (isMoving) return; // Skip if a move is already in progress

                isMoving = true; // Set the flag to indicate a move is in progress

                GameObject player = _player;
                GameObject obstacle = map.Get(player.PosY, player.PosX);
                // Move is allowed
                if (obstacle == null || obstacle.Type == GameObjectType.Floor)
                {
                    map.Save();
                }
                else
                {
                    HandleCollision(player, obstacle);
                }

                isMoving = false; // Reset the flag after the move is completed
            }
        }

        private void HandleCollision(GameObject player, GameObject obstacle)
        {
            if (obstacle.Type == GameObjectType.Goal || obstacle.Type == GameObjectType.Uncle)
            {
                if (!keyCollected)
                {
                    player.UndoMove();
                }
                else
                {
                    this.DoorUnlocked = true;
                }
            }
            else if (obstacle.Type == GameObjectType.Phone || obstacle.Type == GameObjectType.Beer)
            {
                this.keyCollected = true;
                obstacle.Color = ConsoleColor.Cyan;
            }
            else if (obstacle.Type == GameObjectType.Wall)
            {
                player.UndoMove();
                return;
            }
            else if (obstacle.Type == GameObjectType.Obstacle)
            {
                HandleObstacleCollision(player, (Obstacle)obstacle);
            }
            else if (obstacle.Type == GameObjectType.Box)
            {
                int boxY = obstacle.PosY + player.getDy();
                int boxX = obstacle.PosX + player.getDx();
                GameObject obstacleObstacle = map.Get(boxY, boxX);

                if (obstacleObstacle.Type == GameObjectType.Wall || obstacleObstacle.Type == GameObjectType.Box)
                {
                    player.UndoMove();
                }
                else
                {
                    obstacle.PosX = boxX;
                    obstacle.PosY = boxY;

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
            lock (_moveLock) // Use a separate lock for movement
            {
                // Display the message associated with the obstacle
                currentMessage = obstacle.Message;

                // Adjust the timer based on the obstacle's TimeEffect
                AdjustTime(obstacle.TimeEffect);

                // Prevent movement into the obstacle if necessary
                player.UndoMove();

                // Obstacle becomes floor after collision
                obstacle.Type = GameObjectType.Floor;
                obstacle.CharRepresentation = " ";

                // Refreshing the spot on the map where the obstacle was
                map.Set(obstacle);

                // Remove the obstacle from the gameObjects list
                gameObjects.Remove(obstacle);
            }
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
            lock (_moveLock) // Use a separate lock for movement
            {
                map.Undo();

                GameObject?[,] gameObjectLayer = map.GetGameObjectLayer();
                if (gameObjectLayer == null)
                    return;

                // Iterate through all objects and update their position
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
                                _player.PosX = x;
                                _player.PosY = y;
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
                _player.setColor(ConsoleColor.Red);
            }
        }

        // Method to create GameObject using the factory from clients
        public GameObject CreateGameObject(dynamic obj)
        {
            return gameObjectFactory.CreateGameObject(obj);
        }

        public void AddGameObject(GameObject gameObject)
        {
            lock (_lock)
            {
                if (gameObject.Type == GameObjectType.Goal)
                    missingGoals++;
                // remove a missing goal if a box is placed on it
                else if (gameObject.Type == GameObjectType.Box && gameObject.Color == ConsoleColor.Green)
                    missingGoals--;

                gameObjects.Add(gameObject);
            }
        }

        private void PlaceGameObjects()
        {
            lock (_lock)
            {
                if (_player != null)
                {
                    map.Set(_player);
                }

                gameObjects.ForEach(
                    delegate (GameObject obj)
                    {
                        if (obj != _player)
                            map.Set(obj);
                    }
                );
            }
        }

        private void DrawObject(GameObject gameObject)
        {
            Console.ResetColor();

            if (gameObject != null)
            {
                if ((gameObject.Type == GameObjectType.Phone || gameObject.Type == GameObjectType.Beer) && keyCollected)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(" ");
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
}
