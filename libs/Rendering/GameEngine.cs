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
        private GameObject? _focusedObject;
        private Map map = new Map();
        private List<GameObject> gameObjects = new List<GameObject>();

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
        }

        public void StartTimer(int initialTime)
        {
            _remainingTime = initialTime;
            _timer.Start();
        }

        private void OnTimedEvent(object? source, System.Timers.ElapsedEventArgs e)
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

        public void UpdateTimerDisplay()
        {
            TimeSpan timeLeft = TimeSpan.FromSeconds(_remainingTime);
            Console.SetCursorPosition(0, 0); // Adjust the position as needed
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Time: " + timeLeft.ToString("mm\\:ss") + "        "); // Extra spaces to clear any previous longer time string
        }

        public void RenderGame()
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
                Console.WriteLine("Key collected");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Key not collected");
            }

            if (levelSaved != "")
            {
                Console.WriteLine("Level has been saved under " + levelSaved);
                levelSaved = "";
            }

            DisplayMessage();
        }

        public GameObject GetFocusedObject()
        {
            return _focusedObject;
        }

        public void Setup()
        {
            // Reset previous things:
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
            StartTimer(levelTimeSeconds); // Start the timer when setting up the level
        }

        public void SaveToFile()
        {
            levelSaved = FileHandler.saveGameState(
                gameObjects,
                map,
                Convert.ToInt32(_remainingTime)
            );
        }

        public void CheckForSaveFiles()
        {
            // Implementation for checking for save files
            FileHandler.SaveSelector();
        }

        public bool LoadNextLevel()
        {
            // Implementation for loading the next level
            bool levelLeft = FileHandler.LoadNextLevel();
            if (levelLeft)
                Setup();
            return levelLeft;
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
            _timer.Stop();
            // Additional game over logic if needed
        }

        public void CheckCollision()
        {
            GameObject player = _focusedObject;
            GameObject obstacle = map.Get(player.PosY, player.PosX);
            // Move is allowed
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
            // Handle collision with an Obstacle (e.g., old toast)
            else if (obstacle.Type == GameObjectType.Obstacle)
            {
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
                    // Do not move the player
                    _focusedObject.UndoMove();
                    return;
                }
                else
                {
                    // Move the box
                    obstacle.PosX = boxX;
                    obstacle.PosY = boxY;

                    // This also works if we move box from target to target, because we are smart ppl
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

            // Obstacle becomes floor after collision
            obstacle.Type = GameObjectType.Floor;
            obstacle.CharRepresentation = ' ';

            // Refreshing the spot on the map where the obstacle was
            map.Set(obstacle);

            // Remove the obstacle from the gameObjects list
            gameObjects.Remove(obstacle);
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
}
