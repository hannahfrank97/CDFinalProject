using libs;

class Program
{
    static void Main(string[] args)
    {
        //Setup
        Console.CursorVisible = false;
        var engine = GameEngine.Instance;
        var inputHandler = InputHandler.Instance;

        // Allow user to select saved game
        engine.CheckForSaveFiles();

        while (engine.LoadNextLevel())
        {
            // Main game loop
            while (!engine.IsGameWon())
            {
                engine.Render();
                // Handle keyboard input
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                bool skipCollisionCheck = inputHandler.Handle(keyInfo);
                // check collision only if player is not undoing the move
                if (!skipCollisionCheck)
                    engine.CheckCollision();

            }
            engine.Render();
            Console.WriteLine("Level complete!\nPress any key to continue to the next level...");
            Console.ReadKey();
        }
        engine.Render();
        Console.WriteLine("Congrats, you beat all levels!");
    }
}