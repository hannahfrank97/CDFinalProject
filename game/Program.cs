using System;
using System.Threading;
using libs;

class Program
{
    private static bool isProcessingKeyPress = false;
    private static readonly object keyPressLock = new object();
    private static GameEngine engine = GameEngine.Instance;
    private static InputHandler inputHandler = InputHandler.Instance;

    static void Main(string[] args)
    {
        // Setup
        Console.CursorVisible = false;

        // Display the main menu
        ShowMainMenu();

        // Allow user to select saved game if they choose to start the game
        engine.CheckForSaveFiles();

        while (engine.LoadNextLevel())
        {
            int initialTime = engine.GetLevelInitialTime();
            engine.StartTimer(initialTime);

            // Start a separate thread for input handling
            Thread inputThread = new Thread(HandleInput);
            inputThread.Start();

            // Main game loop for rendering and game state updates
            while (!engine.IsGameWon() && !engine.IsGameLost())
            {
                engine.RenderGame();

                if (engine.GetRemainingTime() <= 0)
                {
                    engine.GameOver();
                    break;
                }

                Thread.Sleep(50); // Reduce the delay to increase responsiveness
            }

            inputThread.Join(); // Wait for the input thread to finish

            if (engine.IsGameWon())
            {
                engine.RenderGame();
                Console.WriteLine("Level complete!\nPress any key to continue to the next level...");
                Console.ReadKey();
            }
            else if (engine.IsGameLost())
            {
                break;
            }
        }

        engine.RenderGame();
        if (engine.IsGameWon())
            Console.WriteLine("Congrats, you beat all levels!");
        else if (engine.IsGameLost())
            Console.WriteLine("Game over! You really suck at this!");

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void HandleInput()
    {
        while (!engine.IsGameWon() && !engine.IsGameLost())
        {
            if (Console.KeyAvailable)
            {
                lock (keyPressLock) // Ensure only one key press is processed at a time
                {
                    if (!isProcessingKeyPress)
                    {
                        isProcessingKeyPress = true;
                        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                        bool skipCollisionCheck = inputHandler.Handle(keyInfo);
                        // Check collision only if player is not undoing the move
                        if (!skipCollisionCheck)
                            engine.CheckCollision();

                        // Set a short delay to debounce key presses
                        Thread.Sleep(50); // Adjust this value as needed
                        isProcessingKeyPress = false;
                    }
                }
            }
            Thread.Sleep(10); // Reduce this delay to increase responsiveness
        }
    }

    static void ShowMainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Welcome to the Game!");
            Console.WriteLine("1. Start Game");
            Console.WriteLine("2. Instructions");
            Console.WriteLine("3. Exit");

            Console.Write("\nEnter your choice: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    return; // Exit the menu and start the game
                case "2":
                    ShowInstructions();
                    break;
                case "3":
                    Environment.Exit(0); // Exit the application
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    Thread.Sleep(1000); // Wait a bit before showing the menu again
                    break;
            }
        }
    }

    static void ShowInstructions()
    {
        Console.Clear();
        Console.WriteLine("Instructions:");
        Console.WriteLine("1. Use the arrow keys to move.");
        Console.WriteLine("2. Be aware of the time.");
        Console.WriteLine("3. Some obstacles add, other substract some time.");
        Console.WriteLine("4. Click yourself through the dialogues and choose your answers wisely.");
        Console.WriteLine("\nPress any key to return to the main menu...");
        Console.ReadKey();
    }
}
